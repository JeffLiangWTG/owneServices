using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.UPE.Business.DataImport;
using Enterprise.Customs.Business;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.SGAccess
{
	public class SGDecisionSupporter
	{
		public bool ImportThisShipment(BusinessObjectFactory factory, ZString masterBillNumber, ZString flightNo, ZString matchingReference, ZString origin, ZString destination)
		{
			Argument.NotNull(factory, "factory");

			decisionReason = ZString.Empty;
			var result = true;
			if (!matchingReference.IsEmpty)
			{
				if (StopImportOfBillIfMatchingBillFound)
				{
					result = !HasMatchingBill(factory, matchingReference);
				}
				if (result)
				{
					var matchedBill = GetMatchingBill(factory, masterBillNumber, flightNo, matchingReference);
					if (matchedBill != null)
					{
						if (AllowBillUpdatesDuringMulitpleLevel1Loads)
						{
							if (matchedBill.HasManifestBeenSubmittedToCustomsIncludingChildren)
							{
								result = false;
								decisionReason = Res.GetString("1140B66E-614C-49BE-BE42-E50C9EAD3216", "Shipment already exists and has been submitted to Customs");
							}
						}
						else
						{
							result = false;
							decisionReason = Res.GetString("78493CCA-642A-41C2-AFF2-20518CF7C9AF", "Shipment already exists and registry to allow bill updates is disabled");
						}
					}
				}
			}
			if (result && IsTranshipment(origin, destination))
			{
				result = false;
			}
			return result;
		}

		public bool IsTradeNet(Shipment subShipment, OrgHeader consignee, BusinessObjectFactory factory)
		{
			Argument.NotNull(subShipment, "subShipment");

			decisionReason = ZString.Empty;
			var result = IsShipmentDirectDelivery(consignee) || IsShipmentInFreeTradeZone(subShipment) || IsDutiableGoods(subShipment) || DoesShipmentHaveControlledGoodsOrStopWord(subShipment);
			if (!result && IsShipmentHighValue(subShipment, factory))
			{
				result = IsShipmentInterbankGiro(consignee) || IsMajorExporter(consignee);
				if (!result)
				{
					result = true;
					decisionReason = Res.GetString("B15EBA28-0696-4F8A-99B7-85BE681824AD", "Shipment is high value normal goods");
				}
			}
			return result;
		}

		public bool ShouldCycleNumberBeAutoNominated(Shipment subShipment, OrgHeader consignee)
		{
			Argument.NotNull(subShipment, "subShipment");

			return IsShipmentDirectDelivery(consignee)
					|| IsShipmentInFreeTradeZone(subShipment)
					|| IsShipmentAlternateBroker(subShipment, consignee);
		}

		public static ZDecimal GetCurrentSGCustomsDeminimusValue(BusinessObjectFactory factory) => new RefCusTaxOrFee.Loader(factory).LoadMostRecentEffectiveDeminimusOfCountry(Core.Constants.CountryCodes.Singapore);

		public static ZDecimal GetSGCustomsExportDeminimusValue(BusinessObjectFactory factory) => new RefCusTaxOrFee.Loader(factory).LoadExportDeminimusOfCountry(Core.Constants.CountryCodes.Singapore);

		public const string MC = "MC";
		public const string ME = "ME";

		public AsycudaBill GetMatchingBill(BusinessObjectFactory factory, ZString masterBillNumber, ZString flightNo, ZString matchingReference)
		{
			AsycudaBill result = null;
			var headers = LoadManifestHeadersForMasterBillAndFlightNo(factory, masterBillNumber, flightNo);

			foreach (var header in headers)
			{
				result = header.Bills.Cast<AsycudaBill>().FirstOrDefault(
					x => IsMatchingBill(x, matchingReference)
					&& IsWithinShipmentNumberRecyclePeriod(x));

				if (result != null)
				{
					break;
				}
			}
			return result;
		}

		public IEnumerable<AsycudaManifestHeader> LoadManifestHeadersForMasterBillAndFlightNo(BusinessObjectFactory factory, ZString masterBillNumber, ZString flightNo)
		{
			return factory.GetCachedValue(string.Join("|", "UPS|Level1|GlobalManifest", masterBillNumber, flightNo), () =>
			{
				var manifestQuery = Customs.ASYCUDA.Business.AsycudaManifestHeaderHelper.GetManifestHeadersQuery(masterBillNumber);
				manifestQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_IsActive, true);
				manifestQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_Voyage, flightNo);

				return factory.Load<Integration.Customs.ASYCUDA.IAsycudaManifestHeader>(manifestQuery).OfType<AsycudaManifestHeader>();
			});
		}

		public bool IsMajorExporter(OrgHeader consignee)
		{
			var partyStatus = GetOrgCusCode(consignee, OrgCusCode.SingaporeCodeTypes.PartyStatusType)?.OK_CustomsRegNo ?? ZString.Empty;
			var result = partyStatus == YesNoList.Codes.Yes;
			if (result)
			{
				decisionReason = Res.GetString("345622B0-EF53-4518-B1C2-682F9E6A9698", "Shipment is high value and consignee has the major exporter indicator");
			}
			return result;
		}

		public ZString DecisionReason => decisionReason;
		ZString decisionReason;

		#region Implementation

		bool IsWithinShipmentNumberRecyclePeriod(AsycudaBill bill)
		{
			var result = bill.ABL_IsActive;
			if (result)
			{
				var matchingNumberRecyclePeriod = ShipmentReferenceNumberRecyclePeriod;
				if (matchingNumberRecyclePeriod > 0)
				{
					result = bill.ABL_SystemCreateTimeUtc.Date >= ZDateTime.Now.AddMonths(-matchingNumberRecyclePeriod).Date;
				}
			}
			return result;
		}

		bool IsMatchingBill(AsycudaBill bill, ZString matchingReference) => bill.ABL_BillNumber == matchingReference;

		bool IsShipmentHighValue(Shipment subShipment, BusinessObjectFactory factory)
		{
			var result = false;
			var customsValue = subShipment.CommercialInfo?.CommercialChargeCollection?.FirstOrDefault(x => x.ChargeType != null && x.ChargeType.Code.HasValue && x.ChargeType.Code.Value == Level1DataFileImporterForSGAccess.Constants.ChargeType.CustomsValue);
			if (customsValue != null && customsValue.Amount.HasValue)
			{
				var isExport = subShipment.PortOfLoading != null && subShipment.PortOfLoading.Code.Value.StartsWith(Core.Constants.CountryCodes.Singapore);
				var deminimusValue = isExport ? GetSGCustomsExportDeminimusValue(factory) : GetCurrentSGCustomsDeminimusValue(factory);
				result = customsValue.Amount.Value > deminimusValue;
			}

			return result;
		}

		bool IsShipmentInFreeTradeZone(Shipment subShipment)
		{
			var consigneeDetails = subShipment.OrganizationAddressCollection?.FirstOrDefault(x => x.AddressType.HasValue && x.AddressType.Value == nameof(MasterFiles.Integration.DocAddressType.ConsigneeDocumentaryAddress));
			var consigneeCountry = (consigneeDetails?.Country?.Code.HasValue ?? false) ? consigneeDetails.Country.Code.Value : ZString.Empty;
			var consigneePostcode = (consigneeDetails?.Postcode.HasValue ?? false) ? consigneeDetails.Postcode.Value : ZString.Empty;
			ZInt postcode;
			var result = false;

			if (consigneeCountry == Core.Constants.CountryCodes.Singapore && !consigneePostcode.IsEmpty && ZInt.TryParse(consigneePostcode, out postcode))
			{
				var postCodeRanges = StopPostcodeRangesForSGFreeTradeZones;
				foreach (var rangeString in postCodeRanges)
				{
					var rangeValues = rangeString.Split(new string[] { " to " }, StringSplitOptions.RemoveEmptyEntries);
					ZInt minValue;
					ZInt maxValue;
					if (rangeValues.Length == 2 && ZInt.TryParse(rangeValues[0], out minValue) && ZInt.TryParse(rangeValues[1], out maxValue) && postcode.IsInRange(minValue, maxValue))
					{
						decisionReason = Res.GetString("55308423-7D9F-4BA8-91B5-6F486F982200", "Shipment is within the Free Trade Zone");
						result = true;
						break;
					}
				}
			}
			return result;
		}

		bool DoesShipmentHaveControlledGoodsOrStopWord(Shipment subShipment)
		{
			var organizationAddresses = subShipment.OrganizationAddressCollection;
			if (organizationAddresses != null && (ConsigneeOrganizationContainsStopWords(organizationAddresses) || ConsignorOrganizationContainsStopWords(organizationAddresses)))
			{
				decisionReason = Res.GetString("3EE12B09-D5AA-4C85-9A09-2226F42CB60F", "Shipment consignee or consignor details contained a stop word");
				return true;
			}
			if (subShipment.PackingLineCollection != null)
			{
				var stopPhrasesGoodsDescription = StopPhrasesForSGGoodsDescription;
				foreach (var packingLineHeader in subShipment.PackingLineCollection)
				{
					if (StopPhraseMatcher.FindMatchingStopPhrasesPluralized(packingLineHeader.GoodsDescription ?? ZString.Empty, stopPhrasesGoodsDescription).Any())
					{
						decisionReason = Res.GetString("D0447199-A87F-4C8A-9C8C-534742DEC03F", "Shipment contains a pack line with goods description details matching a stop word");
						return true;
					}
					if (packingLineHeader.PackingLineCollection != null)
					{
						foreach (var packingLineDetail in packingLineHeader.PackingLineCollection)
						{
							if (packingLineDetail != null && packingLineDetail.AddInfoGroupCollection != null)
							{
								var packingLineAddInfoGroup = packingLineDetail.AddInfoGroupCollection.FirstOrDefault(x => x.Type != null && x.Type.Code.HasValue && x.Type.Code.Value == Level1DataFileImporterForSGAccess.Constants.AddInfoConstants.PackedItem.PackingItemAddInfoType);

								if (packingLineAddInfoGroup != null && packingLineAddInfoGroup.AddInfoCollection != null)
								{
									var controlledGoodsLine = packingLineAddInfoGroup.AddInfoCollection.FirstOrDefault(
										x => x.Key.HasValue && x.Key.Value == Level1DataFileImporterForSGAccess.Constants.AddInfoConstants.PackedItem.GoodsType &&
										x.Value.HasValue && x.Value.Value == Level1DataFileImporterForSGAccess.Constants.GoodsType.ControlledGoods);
									if (controlledGoodsLine != null)
									{
										decisionReason = Res.GetString("B35F21ED-ED3D-4C5E-B703-585DCFD52E06", "Shipment contained a pack line with controlled goods");
										return true;
									}
								}
							}
						}
					}
				}
			}
			return false;
		}

		bool IsShipmentDirectDelivery(OrgHeader consignee)
		{
			var result = GetOrgCusCode(consignee, OrgCusCode.SingaporeCodeTypes.DirectDelivery) != null;
			if (result)
			{
				decisionReason = Res.GetString("E68FB64B-4231-4CD5-A227-A5998C2CE601", "Shipment consignee has the Direct Delivery indicator");
			}
			return result;
		}

		bool IsShipmentInterbankGiro(OrgHeader consignee)
		{
			var result = GetOrgCusCode(consignee, OrgCusCode.SingaporeCodeTypes.InterbankGIRO) != null;
			if (result)
			{
				decisionReason = Res.GetString("2CF35AAA-2BEA-4644-977D-50F30468AD43", "Shipment is high value and consignee has the Interbank Giro indicator");
			}
			return result;
		}

		bool IsShipmentAlternateBroker(Shipment subShipment, OrgHeader consignee)
		{
			var result = false;

			var transportMode = subShipment.TransportMode?.Code.GetValueOrDefault() ?? ZString.Empty;
			if (!transportMode.IsEmpty)
			{
				result = consignee?.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, transportMode, ZString.Empty) != null
						|| consignee?.GetRelatedParty(RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.PickupAndDelivery, transportMode, ZString.Empty) != null;

				if (result)
				{
					decisionReason = Res.GetString("9707E843-95CB-4A09-A8B1-06F97B3529CF", "Shipment consignee has the Alternate Broker indicator");
				}
			}

			return result;
		}

		OrgCusCode GetOrgCusCode(OrgHeader org, ZString code)
		{
			OrgCusCode result = null;
			if (org != null && !code.IsEmpty)
			{
				orgCusCodeDictionary = orgCusCodeDictionary ?? new Dictionary<ZString, OrgCusCode>();
				var key = org.PK.ToString() + code;
				if (!orgCusCodeDictionary.TryGetValue(key, out result))
				{
					result = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(code, Core.Constants.CountryCodes.Singapore);
					orgCusCodeDictionary.Add(key, result);
				}
			}
			return result;
		}
		Dictionary<ZString, OrgCusCode> orgCusCodeDictionary;

		bool IsTranshipment(ZString origin, ZString destination)
		{
			var result = false;
			if (FilterSGTranshipments)
			{
				if (!destination.IsEmpty && !origin.IsEmpty &&
					!destination.StartsWith(Core.Constants.CountryCodes.Singapore, StringComparison.OrdinalIgnoreCase) &&
					!origin.StartsWith(Core.Constants.CountryCodes.Singapore, StringComparison.OrdinalIgnoreCase))
				{
					decisionReason = Res.GetString("{E4E67386-E4DE-46F7-8A05-C6343EF33485}", "Shipment is a transhipment");
					result = true;
				}
			}
			return result;
		}

		bool IsDutiableGoods(Shipment subShipment)
		{
			var result = false;
			ZDecimal dutyAmount;
			if (subShipment.EntryInstructionCollection != null)
			{
				foreach (var entryInstruction in subShipment.EntryInstructionCollection)
				{
					var dutyAddInfo = entryInstruction?.AddInfoCollection?.FirstOrDefault(x => x.Key.HasValue && x.Key.Value == Level1DataFileImporterForSGAccess.Constants.AddInfoConstants.BillCountry.DutyAmount);
					if (dutyAddInfo != null && dutyAddInfo.Value.HasValue && ZDecimal.TryParse(dutyAddInfo.Value, out dutyAmount) && !dutyAmount.IsEmpty)
					{
						decisionReason = Res.GetString("594D6B43-EC9E-4B3E-94B0-986CC9EB92C6", "Shipment contains dutiable goods");
						result = true;
						break;
					}
				}
			}
			return result;
		}

		bool ConsigneeOrganizationContainsStopWords(List<OrganizationAddress> organizations)
		{
			return DoesOrganizationHaveStopWords(organizations, nameof(MasterFiles.Integration.DocAddressType.ConsigneeDocumentaryAddress), StopPhrasesForSGConsigneeName, StopPhrasesForSGConsigneeAddress, StopPhrasesForSGConsigneeAccountNum);
		}

		bool ConsignorOrganizationContainsStopWords(List<OrganizationAddress> organizations)
		{
			return DoesOrganizationHaveStopWords(organizations, nameof(MasterFiles.Integration.DocAddressType.ConsignorDocumentaryAddress), StopPhrasesForSGConsignorName, StopPhrasesForSGConsignorAddress, StopPhrasesForSGConsignorAccountNum);
		}

		bool DoesOrganizationHaveStopWords(List<OrganizationAddress> organizations, ZString addressType, string[] stopPhrasesCompanyName, string[] stopPhrasesAddress, string[] stopPhrasesAccountNum)
		{
			var result = false;
			var org = organizations.FirstOrDefault(x => x.AddressType.HasValue && x.AddressType.Value == addressType);
			var orgAccountNum = org?.RegistrationNumberCollection?.FirstOrDefault(x => x.Type.Code.Value == UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber && x.CountryOfIssue.Code.Value == Core.Constants.CountryCodes.Singapore)?.Value ?? ZString.Empty;
			if (org != null &&
				(StopPhraseMatcher.FindMatchingStopPhrasesExact(org.CompanyName ?? ZString.Empty, stopPhrasesCompanyName).Any() ||
				StopPhraseMatcher.FindMatchingStopPhrasesExact(org.Address1 ?? ZString.Empty, stopPhrasesAddress).Any() ||
				StopPhraseMatcher.FindMatchingStopPhrasesExact(org.Address2 ?? ZString.Empty, stopPhrasesAddress).Any() ||
				StopPhraseMatcher.FindMatchingStopPhrasesExact(orgAccountNum, stopPhrasesAccountNum).Any()))
			{
				result = true;
			}
			return result;
		}

		bool HasMatchingBill(BusinessObjectFactory factory, ZString matchingReference)
		{
			var result = factory.GetCachedValue(matchingReference, () =>
			{
				var housebillQuery = GetHouseBillQuery(matchingReference, ShipmentReferenceNumberRecyclePeriod);
				return factory.Exists(CargoWise.Application.ObjectFactory.GetType<Integration.Customs.ASYCUDA.IAsycudaBill>(), housebillQuery);
			});

			if (result)
			{
				decisionReason = Res.GetString("476B1023-08AF-4F8E-B059-D566D8BD59D2", "Matching Housebill located in the system within the Shipment Number Recycle Period");
			}

			return result;
		}

		public static ZQuery GetHouseBillQuery(ZString billNumber, int shipmentReferenceNumberRecyclePeriod)
		{
			var housebillQuery = new ZDBOnlyQuery(typeof(Customs.ManifestBase.AsycudaBill));
			housebillQuery.AddToFilter(AsycudaBillSchema.ABL_BillNumber, billNumber);
			housebillQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, AsycudaBill.ChildBolCode);
			housebillQuery.AddToFilter(AsycudaBillSchema.ABL_IsActive, true);

			if (shipmentReferenceNumberRecyclePeriod > 0)
			{
				housebillQuery.AddToFilter(AsycudaBillSchema.ABL_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Now.AddMonths(-shipmentReferenceNumberRecyclePeriod));
			}

			var manifestSubquery = new ZDBOnlySubQuery(typeof(AsycudaManifestHeader), AsycudaManifestHeaderSchema.PK);
			manifestSubquery.AddToFilter(AsycudaManifestHeaderSchema.AMA_IsActive, true);
			housebillQuery.AddSubQuery(AsycudaBillSchema.ABL_AMA, manifestSubquery, JoinCondition.And);

			return housebillQuery;
		}

		#endregion

		#region Registry Cache

		bool StopImportOfBillIfMatchingBillFound
		{
			get
			{
				if (!stopImportOfBillIfMatchingBillFound.HasValue)
				{
					stopImportOfBillIfMatchingBillFound = UPEDataRegistry.Instance.StopImportOfBillIfMatchingBillFound;
				}
				return stopImportOfBillIfMatchingBillFound.Value;
			}
		}
		bool? stopImportOfBillIfMatchingBillFound;

		bool AllowBillUpdatesDuringMulitpleLevel1Loads
		{
			get
			{
				if (!allowBillUpdatesDuringMulitpleLevel1Loads.HasValue)
				{
					allowBillUpdatesDuringMulitpleLevel1Loads = UPEDataRegistry.Instance.AllowBillUpdatesDuringMulitpleLevel1Loads;
				}
				return allowBillUpdatesDuringMulitpleLevel1Loads.Value;
			}
		}
		bool? allowBillUpdatesDuringMulitpleLevel1Loads;

		string[] StopPostcodeRangesForSGFreeTradeZones
		{
			get
			{
				if (stopPostcodeRangesForSGFreeTradeZones == null)
				{
					stopPostcodeRangesForSGFreeTradeZones = UPEDataRegistry.Instance.StopPostcodeRangesForSGFreeTradeZones;
				}
				return stopPostcodeRangesForSGFreeTradeZones;
			}
		}
		string[] stopPostcodeRangesForSGFreeTradeZones;

		string[] StopPhrasesForSGGoodsDescription
		{
			get
			{
				if (stopPhrasesForSGGoodsDescription == null)
				{
					stopPhrasesForSGGoodsDescription = UPEDataRegistry.Instance.StopPhrasesForSGGoodsDescription;
				}
				return stopPhrasesForSGGoodsDescription;
			}
		}
		string[] stopPhrasesForSGGoodsDescription;

		string[] StopPhrasesForSGConsigneeAccountNum
		{
			get
			{
				if (stopPhrasesForSGConsigneeAccountNum == null)
				{
					stopPhrasesForSGConsigneeAccountNum = UPEDataRegistry.Instance.StopPhrasesForSGConsigneeAccountNum;
				}
				return stopPhrasesForSGConsigneeAccountNum;
			}
		}
		string[] stopPhrasesForSGConsigneeAccountNum;

		string[] StopPhrasesForSGConsigneeAddress
		{
			get
			{
				if (stopPhrasesForSGConsigneeAddress == null)
				{
					stopPhrasesForSGConsigneeAddress = UPEDataRegistry.Instance.StopPhrasesForSGConsigneeAddress;
				}
				return stopPhrasesForSGConsigneeAddress;
			}
		}
		string[] stopPhrasesForSGConsigneeAddress;

		string[] StopPhrasesForSGConsigneeName
		{
			get
			{
				if (stopPhrasesForSGConsigneeName == null)
				{
					stopPhrasesForSGConsigneeName = UPEDataRegistry.Instance.StopPhrasesForSGConsigneeName;
				}
				return stopPhrasesForSGConsigneeName;
			}
		}
		string[] stopPhrasesForSGConsigneeName;

		string[] StopPhrasesForSGConsignorAddress
		{
			get
			{
				if (stopPhrasesForSGConsignorAddress == null)
				{
					stopPhrasesForSGConsignorAddress = UPEDataRegistry.Instance.StopPhrasesForSGConsignorAddress;
				}
				return stopPhrasesForSGConsignorAddress;
			}
		}
		string[] stopPhrasesForSGConsignorAddress;

		string[] StopPhrasesForSGConsignorAccountNum
		{
			get
			{
				if (stopPhrasesForSGConsignorAccountNum == null)
				{
					stopPhrasesForSGConsignorAccountNum = UPEDataRegistry.Instance.StopPhrasesForSGConsignorAccountNum;
				}
				return stopPhrasesForSGConsignorAccountNum;
			}
		}
		string[] stopPhrasesForSGConsignorAccountNum;

		string[] StopPhrasesForSGConsignorName
		{
			get
			{
				if (stopPhrasesForSGConsignorName == null)
				{
					stopPhrasesForSGConsignorName = UPEDataRegistry.Instance.StopPhrasesForSGConsignorName;
				}
				return stopPhrasesForSGConsignorName;
			}
		}
		string[] stopPhrasesForSGConsignorName;

		int ShipmentReferenceNumberRecyclePeriod
		{
			get
			{
				if (!shipmentReferenceNumberRecyclePeriod.HasValue)
				{
					shipmentReferenceNumberRecyclePeriod = UPEDataRegistry.Instance.ShipmentReferenceNumberRecyclePeriod;
				}
				return shipmentReferenceNumberRecyclePeriod.Value;
			}
		}
		int? shipmentReferenceNumberRecyclePeriod;

		bool FilterSGTranshipments
		{
			get
			{
				if (!filterSGTranshipments.HasValue)
				{
					filterSGTranshipments = UPEDataRegistry.Instance.FilterSGTranshipments;
				}
				return filterSGTranshipments.Value;
			}
		}
		bool? filterSGTranshipments;

		#endregion

	}
}
