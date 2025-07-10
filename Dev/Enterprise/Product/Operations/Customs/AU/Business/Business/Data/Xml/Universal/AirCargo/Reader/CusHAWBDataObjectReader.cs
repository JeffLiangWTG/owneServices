using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBDataObjectReader : DataTransfer.Universal.AirManifest.CusHAWBDataObjectReader<CusMAWB, CusHAWB, AirManifestDataObjectReaderHelper>
	{
		public CusHAWBDataObjectReader(Shipment shipmentDataObject, Shipment mawbDataObject, IXmlImportLogger logger, AirManifestDataObjectReaderHelper helper, CusMAWB mawb, CusHAWB masterHouse, bool isHVLV, bool singleHAWBCheck = false)
			: base(shipmentDataObject, mawbDataObject, logger, helper, mawb, masterHouse, isHVLV, singleHAWBCheck)
		{
		}

		protected override void FillCountrySpecificDetails(CusHAWB hawb)
		{
			base.FillCountrySpecificDetails(hawb);
			var hawbRow = GetColumnIndexer(hawb);
			if (dataObject.ShipmentSubType != null)
			{
				SetValue(hawbRow, CusHAWBSchema.CS_IsSpecialReporter, dataObject.ShipmentSubType.GetCodeAsUpperCase() == AirCargo.Constants.ShipmentSubTypes.IsSpecialReporter);
			}
			ZBool? isSelfAssessedClearance = null;
			if (dataObject.MessageSubType != null)
			{
				isSelfAssessedClearance = dataObject.MessageSubType.GetCodeAsUpperCase() == JobDeclaration.MessageSubType.SelfAssessedClearance;
			}

			if (!isSelfAssessedClearance.HasValue && !hawbRow.GetValue(CusHAWBSchema.CS_IsMasterHouse))
			{
				var sacDecider = new SACDecider(factory.BOFactory, hawb.GoodsValueInLocalCurrency, hawb.GoodsDescription);
				isSelfAssessedClearance = sacDecider.IsValidForSAC;
			}
			SetValue(hawbRow, CusHAWBSchema.CS_IsSelfAssessedClearance, isSelfAssessedClearance);

			if (dataObject.PaymentMethod.TryGetCodeAsUpperCase(out var paymentType))
			{
				SetValue(hawbRow, CusHAWBSchema.CS_FreightPrepaidCollect, new CMRUtilities().ConvertOldAirCargoPaymentType(paymentType));
			}
			else if (dataObject.ShipmentIncoTerm.TryGetCodeAsUpperCase(out var incoTerm))
			{
				var prepaidCollect = IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, incoTerm);
				switch (prepaidCollect)
				{
					case Enterprise.Core.Constants.PaymentType.Collect:
						paymentType = CMRMethodsOfPayment.Codes.Collect;
						break;
					case Enterprise.Core.Constants.PaymentType.Prepaid:
						paymentType = CMRMethodsOfPayment.Codes.PrepaidBySeller;
						break;
				}

				SetValue(hawbRow, CusHAWBSchema.CS_FreightPrepaidCollect, paymentType);
			}

			SetValue(hawbRow, CusHAWBSchema.CS_ShipmentType, dataObject.ShipmentType);
			SetValue(hawbRow, CusHAWBSchema.CS_IsPersonalEffects, dataObject.IsPersonalEffects);

			SetValue(hawbRow, CusHAWBSchema.CS_WarehouseLocation, dataObject.WarehouseLocation);
			SetValue(hawbRow, CusHAWBSchema.CS_FolioReference, dataObject.Folio);
			SetValue(hawbRow, CusHAWBSchema.CS_ChargableWeight, dataObject.ActualChargeable);
			SetValue(hawbRow, CusHAWBSchema.CS_RS_NK_ServiceLevel, dataObject.ServiceLevel);

			if (!hawb.IsInDatabase || !HasCargoReportingMessage(hawb.PK))
			{
				SetValue(hawbRow, CusHAWBSchema.CS_CustomsStatus, dataObject.ConsolidatedCargoStatus);
			}

			if (dataObject.NoteCollection != null)
			{
				new NotesCollectionReader(dataObject.NoteCollection, logger, factory, hawb).ReadIntoCollection();
			}
			PopulateWorkflowCustomFields(hawb, dataObject);
		}

		protected override OrgAddress GetMatchedOrgAddress(OrganizationAddress orgAddressData, BusinessObjectFactory boFactory)
		{
			return orgAddressData.GetMatchingOrgAddressUsingCodeOrAddress1(boFactory);
		}

		protected override void UpdateConsignorWithMatchedOrgAddress(IColumnIndexer hawbRow, OrgAddress orgAddressBO, ZString? contactName)
		{
			SetValue(hawbRow, CusHAWBSchema.CS_OA_ConsignorAddress, orgAddressBO.PK);
		}

		protected override void UpdateConsigneeWithMatchedOrgAddress(IColumnIndexer hawbRow, OrgAddress orgAddressBO, ZString? contactName)
		{
			SetValue(hawbRow, CusHAWBSchema.CS_OA_ConsigneeAddress, orgAddressBO.PK);
		}

		protected override void AdditionalConsigneeUpdate(IColumnIndexer houseBill, OrganizationAddress address, OrgAddress orgAddress)
		{
			var cidConsignee = address?.RegistrationNumberCollection?.FirstOrDefault(number => number.Type.Code == (ZString?)OrgCusCode.CodeTypes.CustomsClientID);
			if (cidConsignee != null)
			{
				SetValue(houseBill, CusHAWBSchema.CS_ConsigneeIdentifier, cidConsignee.Value);
				SetValue(houseBill, CusHAWBSchema.CS_RN_NKConsigneeCountry, cidConsignee.CountryOfIssue);
				SetValue(houseBill, CusHAWBSchema.CS_ConsigneeBusinessNumber, ZString.Empty);
			}
			else
			{
				var abnConsignee = address?.RegistrationNumberCollection?.FirstOrDefault(number => number.Type.Code == (ZString?)OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber);
				if (abnConsignee != null)
				{
					SetValue(houseBill, CusHAWBSchema.CS_ConsigneeBusinessNumber, abnConsignee.Value);
					SetValue(houseBill, CusHAWBSchema.CS_RN_NKConsigneeCountry, abnConsignee.CountryOfIssue);
					SetValue(houseBill, CusHAWBSchema.CS_ConsigneeIdentifier, ZString.Empty);
				}
			}
		}

		protected override void AdditionalConsignorUpdate(IColumnIndexer houseBill, OrganizationAddress address, OrgAddress orgAddress)
		{
			var cidConsignor = address?.RegistrationNumberCollection?.FirstOrDefault(number => number.Type.Code == (ZString?)OrgCusCode.CodeTypes.CustomsClientID);
			if (cidConsignor != null)
			{
				SetValue(houseBill, CusHAWBSchema.CS_ConsignorIdentifier, cidConsignor.Value);
				SetValue(houseBill, CusHAWBSchema.CS_RN_NKConsignorCountry, cidConsignor.CountryOfIssue);
			}

			if (isHVLV)
			{
				var vendorId = dataObject.VendorIdentifier ?? ZString.Empty;
				if (vendorId.IsEmpty)
				{
					vendorId = CargoHelper.GetConsignorVendorId(address);
					if (vendorId.IsEmpty && orgAddress != null)
					{
						vendorId = CargoHelper.GetConsignorVendor(orgAddress.Header);
					}

					if (vendorId.IsEmpty)
					{
						vendorId = mawbDataObject?.VendorIdentifier ?? ZString.Empty;
						if (vendorId.IsEmpty)
						{
							var hvlvConsignorAddressData = mawbDataObject?.OrganizationAddressCollection?.FirstOrDefault(GetConsignorAddressTypesInPreferredOrder());
							if (hvlvConsignorAddressData != null)
							{
								vendorId = CargoHelper.GetConsignorVendorId(hvlvConsignorAddressData);
								if (vendorId.IsEmpty)
								{
									var hvlvConsignorOrgAddress = GetOrgAddressBO(hvlvConsignorAddressData, logger.TopLevelDataContext);
									if (hvlvConsignorOrgAddress != null)
									{
										vendorId = CargoHelper.GetConsignorVendor(hvlvConsignorOrgAddress.Header);
									}
								}
							}
						}
					}
				}

				SetValue(houseBill, CusHAWBSchema.CS_VendorIdentifier, vendorId);
			}
		}

		bool HasCargoReportingMessage(ZGuid hawbPK)
		{
			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, hawbPK);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.CMR);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, new ZString[] { CMRMessage.CMRMessageTypes.AIRCR, CMRMessage.CMRMessageTypes.CARST });
			return factory.LoadTop1<EDIMessage>(query) != null;
		}

		protected override DataTransfer.Universal.AirManifest.CusHAWBDataObjectReader<CusMAWB, CusHAWB, AirManifestDataObjectReaderHelper> GetNewCusHAWBDataObjectReader(Shipment shipmentDataObject, CusHAWB masterhouse)
		{
			return new CusHAWBDataObjectReader(shipmentDataObject, mawbDataObject, logger, helper, mawb, masterhouse, isHVLV, singleHAWBCheck);
		}

		protected override bool CheckUpdateHAWBDataIsAllowed(CusHAWB hawb)
		{
			var result = true;
			if (!IsNewBO)
			{
				if (IsMessagingActive(hawb))
				{
					result = false;
					logger.LogBoth(LogType.Warning, Res.GetString("C6826241-768C-4E95-98EC-F1EEA6DBE8B1", "{0} data will not be updated as it has active messaging.", hawb.HumanReadableName));
				}
			}
			return result;
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(CusHAWB hawb)
		{
			var result = base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(hawb);
			if (result.IsEmpty && IsMessagingActive(hawb) && (dataObject.SubShipmentCollection == null || dataObject.SubShipmentCollection.Count == 0))
			{
				result = Res.GetString("CC2B060E-F70D-4851-BBA5-10EFDB0B0A5A", "Messaging is active for {0}", hawb.HumanReadableName);
			}
			return result;
		}

		public static bool IsMessagingActive(CusHAWB cusHAWB)
		{
			return
				cusHAWB != null &&
				cusHAWB.IsInDatabase &&
				!CMRStatusHelper.IsMessagingBeingNotLodged(cusHAWB.CMRMessageStatus.Code);
		}
	}
}
