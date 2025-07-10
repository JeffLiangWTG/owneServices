using System;
using System.Collections.Generic;
using System.Drawing;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.DocumentWrappers.Freight;
using Enterprise.DocumentWrappers.Freight.Forwarding;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.DocumentWrappers;
using Constants = Enterprise.Core.Constants;
using ForwardingShipment = Enterprise.Freight.Forwarding.Business.ForwardingShipment;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocForwardingShipment : DocShipment, IDocForwardingShipment, IRequestForMissingDocuments, IShipperDepartureNotice, IDocJobDetail, IBusinessObjectForCustomFieldsOverridable
	{
		#region Constructors

		protected DocForwardingShipment(ForwardingShipment shipment, BusinessObjectFactory factoryToWrap)
			: base(shipment, factoryToWrap)
		{
		}

		public static DocForwardingShipment New(ForwardingShipment shipment, BusinessObjectFactory factoryToWrap)
		{
			DocForwardingShipment result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(shipment, factoryToWrap);
			}
			else if (shipment != null)
			{
				result = new DocForwardingShipment(shipment, factoryToWrap);
			}

			return result;
		}

		public static new DocForwardingShipment New(DocumentShipment documentShipment, BusinessObjectFactory factoryToWrap)
		{
			DocForwardingShipment wrapper = null;

			if (documentShipment != null && documentShipment.Shipment != null)
			{
				wrapper = New((ForwardingShipment)documentShipment.Shipment, factoryToWrap);
				if (documentShipment.DataContext == Constants.DataContext.Shipment)
				{
					wrapper.IsBillOfLading = documentShipment.IsBillOfLading;
				}
			}

			return wrapper;
		}

		public static new DocForwardingShipment New(BusinessObjectFactory factory, ZGuid pK)
		{
			return New(factory.Load<ForwardingShipment>(pK), factory);
		}

		protected new delegate DocForwardingShipment NewDelegate(ForwardingShipment shipment, BusinessObjectFactory factoryToWrap);
		protected new static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		#region Approved Known Shipper

		public ZBool IsApprovedKnownShipper
		{
			get
			{
				return CommonShipment.JS_InspectionTypeCode == "APP";
			}
		}

		#endregion

		#region Menu Filter Fields

		#region Country Specific Filter Fields

		#region China
		public ZBool IsChina // Replacing old "CNCTY" meaningless filter code
		{
			get
			{
				return (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China);
			}
		}

		public ZBool IsChinaAndIsAir // Replacing old "CNAIR" meaningless filter code
		{
			get
			{
				return (IsChina && CommonShipment.IsAir);
			}
		}

		public ZBool IsChinaAndIsSea // Replacing old "CNSEA" meaningless filter code
		{
			get
			{
				return (IsChina && CommonShipment.IsSea);
			}
		}
		#endregion

		#endregion

		#endregion

		#region ForwardingShipment

		public new ForwardingShipment CommonShipment
		{
			get { return (ForwardingShipment)base.CommonShipment; }
		}

		#endregion

		#region CurrentConsol

		public new DocForwardingConsol CurrentConsol
		{
			get
			{
				if (fCurrentConsol == null)
				{
					if (base.CurrentConsol != null && base.CurrentConsol.WrappedObject.GetType().IsAssignableFrom(typeof(ForwardingConsol)))
					{
						CurrentConsol = DocForwardingConsol.New((ForwardingConsol)base.CurrentConsol.WrappedObject, Factory);
					}
					else if (CommonShipment.CurrentConsolForDocuments != null)
					{
						CurrentConsol = DocForwardingConsol.New(CommonShipment.CurrentConsolForDocuments, Factory);
					}
				}
				return fCurrentConsol;
			}
			set
			{
				fCurrentConsol = value;
				base.CurrentConsol = value;
			}
		}

		DocForwardingConsol fCurrentConsol;
		#endregion

		#region ShipmentConsols

		protected override DocBaseConsolCollection GetDocNewConsolCollection(ConsolCollection consols, BusinessObjectFactory factory)
		{
			return new DocForwardingConsolCollection(CommonShipment.Consols, factory);
		}

		#endregion

		#region Orders

		public ZString FirstOrderNumber
		{
			get
			{
				ZString result = ZString.Empty;

				if (Orders.Count > 0)
				{
					DocOrder firstOrder = Orders[0];

					if (firstOrder != null)
					{
						result = firstOrder.OrderNumber;
					}
				}

				return result;
			}
		}

		public ZString OrderReference
		{
			get
			{
				return CommonShipment.DocsAndCartage.JP_OrderItemsAsString;
			}
		}

		public override ZString OrderNumbers
		{
			get
			{
				ZString result = ZString.Empty;

				foreach (DocOrder order in Orders)
				{
					result += order.OrderNumber + ",";
				}

				if (result.IsEmpty)
				{
					result = OrderReference;
				}

				return result.TrimEndIncludingWhiteSpace(',');
			}
		}

		public ZString OrderNumbersForInvoice
		{
			get
			{
				return OrderNumbers;
			}
		}

		public ZString Note
		{
			get
			{
				return (!OrderNumbers.IsEmpty) ? new ZString(Res.GetString("9263702d-5f37-45f8-bad8-456c189022da", "Order Numbers: {0}", OrderNumbers)) : ZString.Empty;
			}
		}

		public ZString ClientOwnerOrderReferenceIncludingRelatedShipments
		{
			get
			{
				ZString result = ClientOwnerOrderReference;
				foreach (DocShipment coloadShipment in ColoadShipments)
				{
					if (!result.IsEmpty && result[result.Length - 1] != ' ')
					{
						result += ",";
					}

					result += coloadShipment.OrderNumbers;
				}

				return result;
			}
		}

		#endregion

		#region Heading

		public ZString HBManifestHeading
		{
			get
			{
				ZString result = (TransportMode == Core.Constants.TransportModes.Air) ? "HAWB:" : "HBL:";

				if (CommonShipment.IsCoLoadMaster || CommonShipment.IsBlindCoLoadMaster)
				{
					result = "CoL " + result;
				}
				return result;
			}
		}

		#endregion

		#region CoLoadShipments

		protected override DocShipment GetNewDocShipment(BusinessObjectFactory factory, ZGuid shipmenPK)
		{
			return DocForwardingShipment.New(CommonShipment.Factory, shipmenPK);
		}

		public ZString GoodsValueOfColoadShipmentsGroupedByCurrency
		{
			get
			{
				ZString result = ZString.Empty;
				Dictionary<string, ZDecimal> valueByCurrency = new Dictionary<string, ZDecimal>();

				foreach (DocShipment shipment in ColoadShipments)
				{
					string currencyCode = shipment.GoodsCurr != null ? shipment.GoodsCurr.Code : ZString.Empty;

					if (!valueByCurrency.ContainsKey(currencyCode))
					{
						valueByCurrency.Add(currencyCode, shipment.GoodsValue);
					}
					else
					{
						valueByCurrency[currencyCode] += shipment.GoodsValue;
					}
				}

				foreach (var entry in valueByCurrency)
				{
					result += entry.Value.ToString(2) + " " + entry.Key + System.Environment.NewLine;
				}

				return result;
			}
		}

		#endregion

		#region Transhipment

		public DocDocAddress ExportTranshipmentPackDepotAddress
		{
			get
			{
				if (CommonShipment.Consols.Count == 1)
				{
					return ImportReleaseDepot;
				}
				else
				{
					if (ExportTranshipmentConsol != null)
					{
						return ExportTranshipmentConsol.PackDepotAddress;
					}

					return null;
				}
			}
		}

		#endregion

		#region Bill of Lading

		public DocBillOfLading BillOfLading
		{
			get
			{
				if (fBillOfLading == null)
				{
					fBillOfLading = new DocBillOfLading(this);
				}

				return fBillOfLading;
			}
		}

		public ZString PlaceOfReceiptForBOL
		{
			get
			{
				ZString receipt = OriginLoco != null ? OriginLoco.PortNameAndCountryNameInEnglish : ZString.Empty;

				return receipt.ToUpper();
			}
		}

		public ZString PlaceOfDeliveryForBOL
		{
			get
			{
				ZString delivery = DestinationLoco != null ? DestinationLoco.PortNameAndCountryNameInEnglish : ZString.Empty;

				return delivery.ToUpper();
			}
		}

		public ZString FreightPayableAtForBOL
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsManufacturerBillOfLading && DestinationLoco != null)
				{
					result = DestinationLoco.PortNameAndCountryNameInEnglish;
				}
				else if (IsPrepaid && OriginLoco != null)
				{
					result = OriginLoco.PortNameAndCountryNameInEnglish;
				}
				else if (IsCollect && DestinationLoco != null)
				{
					result = DestinationLoco.PortNameAndCountryNameInEnglish;
				}

				return result.ToUpper();
			}
		}

		public Image HBLLogo
		{
			get
			{
				if (fHBLLogo == null || fHBLLogo.IsDisposed())
				{
					fHBLLogo = BillOfLading.Logo;
				}

				return fHBLLogo;
			}
		}

		Image fHBLLogo;

		#endregion

		#region ZBool

		public ZBool ShowChargesOnBookingConfirmation
		{
			get { return Env.Registry.ShowChargesOnForwardingBookingConfirmation; }
		}

		#endregion

		#region FirstExportConsol

		public DocForwardingConsol FirstExportConsol
		{
			get
			{
				ForwardingConsol[] consols = (ForwardingConsol[])CommonShipment.Consols.ToArray(typeof(ForwardingConsol));
				MovementLegComparer.SortMovementLegsByPorts(consols);

				foreach (ForwardingConsol consol in consols)
				{
					if (consol.IsExport())
					{
						return DocForwardingConsol.New(consol, Factory);
					}
				}

				return null;
			}
		}

		#endregion

		#region LastImportConsol

		ForwardingConsol LastImportConsolObject
		{
			get
			{
				if (CommonShipment.Consols.Count > 0)
				{
					ForwardingConsol[] consols = (ForwardingConsol[])CommonShipment.Consols.ToArray(typeof(ForwardingConsol));
					MovementLegComparer.SortMovementLegsByPorts(consols);

					for (int index = consols.Length - 1; index >= 0; index--)
					{
						if (consols[index].IsImport())
						{
							return consols[index];
						}
					}
				}

				return null;
			}
		}

		public DocForwardingConsol LastImportConsol
		{
			get { return DocForwardingConsol.New(LastImportConsolObject, Factory); }
		}

		#endregion

		#region LastConsol

		ForwardingConsol LastConsolObject
		{
			get
			{
				if (CommonShipment.Consols.Count > 1)
				{
					ForwardingConsol[] consols = (ForwardingConsol[])CommonShipment.Consols.ToArray(typeof(ForwardingConsol));
					MovementLegComparer.SortMovementLegsByPorts(consols);

					return consols[consols.Length - 1];
				}
				else if (CommonShipment.Consols.Count == 1)
				{
					return CommonShipment.Consols[0];
				}

				return null;
			}
		}

		public DocForwardingConsol LastConsol
		{
			get { return DocForwardingConsol.New(LastConsolObject, Factory); }
		}

		#endregion

		#region PreAlertReferenceHeading

		public override ZString PreAlertReferenceHeading
		{
			get { return Res.GetString("9eb593db-3296-4967-a5ef-14b9074b960d", "ORDER NUMBERS / REFERENCE"); }
		}

		#endregion

		#region PreAlertReference

		public override ZString PreAlertReference
		{
			get
			{
				DocBaseJobDeclaration declaration = DeclarationForCurrentBranch;

				if (declaration == null)
				{
					return OrderNumbers;
				}
				else
				{
					if (!OrderNumbers.Contains(declaration.OwnerRef))
					{
						return OrderNumbers + " " + declaration.OwnerRef;
					}
					else
					{
						return OrderNumbers;
					}
				}
			}
		}

		#endregion

		#region North Port Delivery Order

		public NorthPortDeliveryOrder NPDeliveryOrder
		{
			get
			{
				if (fNPDeliveryOrder == null)
				{
					fNPDeliveryOrder = new NorthPortDeliveryOrder(this);
				}

				return fNPDeliveryOrder;
			}
		}

		#endregion

		#region Container Liablility Statement

		public ZString ContainerLiabiltyStatementWarningText
		{
			get { return DocumentsDataRegistry.Instance.ContainerLiabilityStatementLiabilityWarningText.Value; }
		}

		#endregion

		#region Request for Profit Share

		public ZString RequestForProfitShareOpeningText
		{
			get { return DocumentsDataRegistry.Instance.RequestForProfitShareOpeningText_Shipment.Value; }
		}

		public ZString RequestForProfitShareDocumentHeader
		{
			get { return Res.GetString("397ad2ee-bdd6-4304-bf7e-0f8cd0992716", "Request for Shipment Profit Share Credit Note"); }
		}

		public ZString AWBSecurityDeclarationOpeningText
		{
			get { return Env.Registry.AWBSecurityDeclaration.OpeningText; }
		}

		#endregion

		#region Inbond Transit Details (US, Import)

		public ZString InbondTransitNumber
		{
			get { return InbondTransitUS != null ? InbondTransitUS.CE_EntryNum : ZString.Empty; }
		}

		public ZDateTime InbondTransitIssueDate
		{
			get { return InbondTransitUS != null ? InbondTransitUS.CE_IssueDate : ZDateTime.Empty; }
		}

		public ZString InbondTransitIssuePlace
		{
			get { return InbondTransitUS != null ? InbondTransitUS.CE_EntryLineReference : ZString.Empty; }
		}

		CusEntryNumber InbondTransitUS
		{
			get
			{
				CusEntryNumber result = null;

				ZQuery query = new ZQuery(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);
				query.AddToFilter(CusEntryNumSchema.CE_EntryType, UnitedStatesAdditionalReferenceNumberTypes.Codes.IT);
				CusEntryNumber[] results = (CusEntryNumber[])CommonShipment.Numbers.Find(query);

				if (results.Length > 0)
				{
					result = results[0];
				}
				else if (LastImportConsolObject != null)
				{
					results = (CusEntryNumber[])LastImportConsolObject.Numbers.Find(query);
					result = results.Length > 0 ? results[0] : null;
				}

				return result;
			}
		}

		#endregion

		#region Icelandic

		public DocAddress CustomsHouse
		{
			get
			{
				if (!fCustomsHouseLoaded)
				{
					fCustomsHouse = LoadCustomsHouse(CustomsHouseCode);
					fCustomsHouseLoaded = true;
				}

				return fCustomsHouse;
			}
		}
		DocAddress fCustomsHouse;
		bool fCustomsHouseLoaded;

		public DocAddress PreviousCustomsHouse
		{
			get
			{
				if (!fPreviousCustomsHouseLoaded)
				{
					fPreviousCustomsHouse = LoadCustomsHouse(PreviousCustomsHouseCode);
					fPreviousCustomsHouseLoaded = true;
				}

				return fPreviousCustomsHouse;
			}
		}
		DocAddress fPreviousCustomsHouse;
		bool fPreviousCustomsHouseLoaded;

		DocAddress LoadCustomsHouse(ZString customsHouseCode)
		{
			DocAddress result = null;
			ZQuery query = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.IcelandCodeTypes.CustomsOfficeCode);
			query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, customsHouseCode);
			OrgCusCode coc = Factory.LoadTop1<OrgCusCode>(query);
			if (coc != null)
			{
				result = coc.PremisesAddress != null ?
					DocAddress.New(coc.PremisesAddress, Factory)
					:
					DocAddress.New(coc.Header.MainAddress, Factory);
			}

			return result;
		}

		public ZString CustomsHouseCode
		{
			get
			{
				if (!fCustomsHouseCodeLoaded)
				{
					ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, IcelandForwardingShipmentSupport.CustomsOfficeCode);
					query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Iceland);
					CusEntryNumber[] results = (CusEntryNumber[])CommonShipment.Numbers.Find(query);
					fCustomsHouseCode = results.Length > 0 ? results[0].CE_EntryNum : ZString.Empty;
					fCustomsHouseCodeLoaded = true;
				}

				return fCustomsHouseCode;
			}
		}

		ZString fCustomsHouseCode;
		bool fCustomsHouseCodeLoaded;

		public ZString PreviousCustomsHouseCode
		{
			get { return CommonShipment.PreviousCOC; }
		}

		public DocOrganisation PreviousConsignorConsignee
		{
			get { return DocOrganisation.New(IsImport ? CommonShipment.PreviousConsignee : CommonShipment.PreviousConsignor, Factory); }
		}

		public ZString PreviousConsignorConsigneeName
		{
			get { return IsImport ? CommonShipment.PreviousConsigneeName : CommonShipment.PreviousConsignorName; }
		}

		public ZString CRNCarrierNumer
		{
			get
			{
				ZString result = ZString.Empty;
				if (GlbBranch.CurrentBranch.Country.Code == Core.Constants.CountryCodes.Iceland)
				{
					Sendingarnumer sc = new Sendingarnumer(CommonShipment.CustomsEntryNumber);
					result = sc.CarrierNumber;
				}
				return result;
			}
		}

		public ZString CRNWithSpaces
		{
			get
			{
				if (GlbBranch.CurrentBranch.Country.Code == Core.Constants.CountryCodes.Iceland)
				{
					Sendingarnumer sc = new Sendingarnumer(CommonShipment.CustomsEntryNumber, " ");
					return sc.CodeWithoutCheckDigit;
				}
				else
				{
					return CommonShipment.CustomsEntryNumber;
				}
			}
		}

		public ZString CRNWOCheckDigit
		{
			get
			{
				if (GlbBranch.CurrentBranch.Country.Code == Core.Constants.CountryCodes.Iceland)
				{
					Sendingarnumer sc = new Sendingarnumer(CommonShipment.CustomsEntryNumber);
					return sc.CodeWithoutCheckDigit;
				}

				return CommonShipment.CustomsEntryNumber;
			}
		}

		#endregion

		#region  VAT numbers for consignor, consignee, notify party
		public ZString ConsigneeRequiredTaxNumber
		{
			get
			{
				if (Consignee != null && Consignee.OrgHeader != null && !Consignee.OrgHeader.IsMiscellaneous
					&& DestinationLoco != null && !DestinationLoco.CountryCode.IsEmpty)
				{
					if (DestinationLoco.CountryCode == Core.Constants.CountryCodes.India)
					{
						return string.Join(System.Environment.NewLine, RequiredTaxNumbers.GetRequiredTaxNumberWithTypes(DestinationLoco.CountryCode, string.Empty, Consignee.OrgHeader, RequiredTaxNumbers.TaxOrgType.Consignee, RequiredTaxNumbers.DocumentType.General));
					}

					return RequiredTaxNumbers.GetRequiredTaxNumberWithType(DestinationLoco.CountryCode, string.Empty, Consignee.OrgHeader, RequiredTaxNumbers.TaxOrgType.Consignee, RequiredTaxNumbers.DocumentType.General);
				}

				return ZString.Empty;
			}
		}

		public ZString ConsignorRequiredTaxNumber
		{
			get
			{
				if (Consignor != null && Consignor.OrgHeader != null && !Consignor.OrgHeader.IsMiscellaneous
					&& OriginLoco != null && !OriginLoco.CountryCode.IsEmpty)
				{
					if (DestinationLoco == null || DestinationLoco.CountryCode.IsEmpty)
					{
						return RequiredTaxNumbers.GetRequiredTaxNumberWithType(ZString.Empty, OriginLoco.CountryCode, Consignor.OrgHeader, RequiredTaxNumbers.TaxOrgType.Shipper, RequiredTaxNumbers.DocumentType.General);
					}

					return RequiredTaxNumbers.GetRequiredTaxNumberWithType(DestinationLoco.CountryCode, OriginLoco.CountryCode, Consignor.OrgHeader, RequiredTaxNumbers.TaxOrgType.Shipper, RequiredTaxNumbers.DocumentType.General);
				}

				return ZString.Empty;
			}
		}

		public ZString NotifyPartyRequiredTaxNumber
		{
			get
			{
				if (NotifyParty != null && NotifyParty.Organisation != null && NotifyParty.Organisation.OrgHeader != null && !NotifyParty.Organisation.OrgHeader.IsMiscellaneous
					 && DestinationLoco != null && !DestinationLoco.CountryCode.IsEmpty)
				{
					return RequiredTaxNumbers.GetRequiredTaxNumberWithType(DestinationLoco.CountryCode, String.Empty, NotifyParty.Organisation.OrgHeader, RequiredTaxNumbers.TaxOrgType.AlsoNotify, RequiredTaxNumbers.DocumentType.General);
				}

				return ZString.Empty;
			}
		}

		#endregion

		#region China Specific

		//Shi Lian Dan Number
		public ZString ShippingOrderNumber
		{
			get
			{
				ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber);
				query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				CusEntryNumber[] results = (CusEntryNumber[])CommonShipment.Numbers.Find(query);
				return results.Length > 0 ? results[0].CE_EntryNum : ZString.Empty;
			}
		}

		#endregion

		#region ACI Zones (US)

		public override ZString ShipperACIZone
		{
			get { return CommonShipment.JS_Calc_ACIConsignorOriginZone; }
		}

		public override ZString ConsigneeACIZone
		{
			get { return CommonShipment.JS_Calc_ACIConsigneeDestinationZone; }
		}

		#endregion

		#region IBODocDataProvider Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Menu Item Name")]
		protected override string[] ImageNamesToRemove
		{
			get
			{
				string[] result = Array.Empty<string>();

				if (IsBillOfLading)
				{
					result = BillOfLading.ImageNamesToRemove;
				}
				else if (MenuTitle.Contains("SHI LIAN DAN", StringComparison.OrdinalIgnoreCase))
				{
					result = new string[] { "DockReceipt.FaceImage" };
				}
				else if (MenuTitle.ToString().Equals("CARGO INSPECTION REQUEST", StringComparison.OrdinalIgnoreCase))
				{
					result = new string[] { "CargoInspectionRequest.FaceImage" };
				}
				return result;
			}
		}

		#endregion

		#region IBusinessObjectForCustomFieldsOverridable Members

		protected override BusinessObject BusinessObjectForCustomFields => businessObjectForCustomFieldsOverride ?? base.BusinessObjectForCustomFields;
		BusinessObject businessObjectForCustomFieldsOverride;

		void IBusinessObjectForCustomFieldsOverridable.OverrideBusinessObjectForCustomFields(BusinessObject businessObject)
		{
			businessObjectForCustomFieldsOverride = businessObject;
		}

		#endregion
	}
}
