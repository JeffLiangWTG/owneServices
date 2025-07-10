using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.JXC;
using Enterprise.DocumentEngine.MacroValueProviders.Utilities;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business
{
	public class ExportHouseBillOfLading
	{
		public ExportHouseBillOfLading(IJXCExportHeader headerData, JASForwardingShipment shipment)
		{
			CheckForNullHeaderAndShipment(headerData, shipment);
			this.HeaderData = headerData;
			this.Shipment = shipment;
		}

		#region Properties

		public JASForwardingConsol Consol
		{
			get { return HeaderData as JASForwardingConsol; }
		}

		#region Identitifer

		public ZString OceanHouseBillOfLadingNo
		{
			get { return Shipment.JS_HouseBill; }
		}

		public ZString OceanBillOfLadingNo
		{
			get { return (Consol != null) ? Consol.JK_MasterBillNum : ZString.Empty; }
		}

		#endregion

		#region Port of Loading & Discharge

		public ZString PortOfLoadingCode
		{
			get { return (PortOfLoading != null) ? PortOfLoading.Code : ZString.Empty; }
		}

		public virtual ZString PortOfLoadingName
		{
			get { return (PortOfLoading != null) ? PortOfLoading.RL_PortName : ZString.Empty; }
		}

		public ZString PortOfDischargeCode
		{
			get { return (PortOfDischarge != null) ? PortOfDischarge.Code : ZString.Empty; }
		}

		public virtual ZString PortOfDischargeName
		{
			get { return (PortOfDischarge != null) ? PortOfDischarge.RL_PortName : ZString.Empty; }
		}

		public RefUNLOCO PortOfLoading
		{
			get { return (Consol != null) ? Consol.LoadPort : Shipment.Origin; }
		}

		public RefUNLOCO PortOfDischarge
		{
			get { return (Consol != null) ? Consol.DischargePort : Shipment.Destination; }
		}

		#endregion

		#region Place of Receipt & Delivery

		public virtual ZString PlaceOfReceipt
		{
			get { return (Shipment.Origin != null) ? Shipment.Origin.RL_PortName : ZString.Empty; }
		}

		public virtual ZString PlaceOfDelivery
		{
			get { return (Shipment.Destination != null) ? Shipment.Destination.RL_PortName : ZString.Empty; }
		}

		#endregion

		#region Shipping Line details

		public virtual ZString ShippingLineName
		{
			get { return (ShippingLine != null) ? ShippingLine.OH_FullNameTruncated : ZString.Empty; }
		}

		public virtual ZString ShippingLineAddress1
		{
			get { return (ShippingLineAddress != null) ? ShippingLineAddress.OA_Address1 : ZString.Empty; }
		}

		public virtual ZString ShippingLineAddress2
		{
			get { return (ShippingLineAddress != null) ? ShippingLineAddress.OA_Address2 : ZString.Empty; }
		}

		public virtual ZString ShippingLineCity
		{
			get { return (ShippingLineAddress != null) ? ShippingLineAddress.OA_City : ZString.Empty; }
		}

		public ZString ShippingLinePortCode
		{
			get { return (ShippingLine != null) ? ShippingLine.OH_RL_NKClosestPort : ZString.Empty; }
		}

		public ZString ShippingLineSSLCode
		{
			get { return (ShippingLine != null) ? ShippingLine.JASWWMappedCode : ZString.Empty; }
		}

		JASOrgHeader ShippingLine
		{
			get { return (Consol != null) ? Consol.ShippingLine : null; }
		}

		OrgAddress ShippingLineAddress
		{
			get { return (ShippingLine != null) ? ShippingLine.MainAddress : null; }
		}

		#endregion

		#region Shipper details

		public virtual ZString ShipperAccount
		{
			get { return (Shipment.Consignor != null) ? Shipment.Consignor.OH_Code : ZString.Empty; }
		}

		public virtual ZString ShipperName
		{
			get { return (Shipment.Consignor != null) ? Shipment.Consignor.OH_FullNameTruncated : ZString.Empty; }
		}

		public virtual ZString ShipperAddress1
		{
			get { return (ShipperAddress != null) ? ShipperAddress.OA_Address1 : ZString.Empty; }
		}

		public virtual ZString ShipperAddress2
		{
			get { return (ShipperAddress != null) ? ShipperAddress.OA_Address2 : ZString.Empty; }
		}

		public virtual ZString ShipperCity
		{
			get { return (ShipperAddress != null) ? ShipperAddress.OA_City : ZString.Empty; }
		}

		public virtual ZString ShipperState
		{
			get { return (ShipperAddress != null) ? ShipperAddress.OA_State : ZString.Empty; }
		}

		public virtual ZString ShipperPostalCode
		{
			get { return (ShipperAddress != null) ? ShipperAddress.OA_PostCode : ZString.Empty; }
		}

		public ZString ShipperPortCode
		{
			get { return (ShipperClosestPort != null) ? ShipperClosestPort.Code : ZString.Empty; }
		}

		public ZString ShipperCountryCode
		{
			get { return ShipperClosestPort != null ? ShipperClosestPort.RL_RN_NKCountryCode : ZString.Empty; }
		}

		public ZString ShipperEmail
		{
			get { return (ShipperAddress != null) ? ShipperAddress.OA_Email : ZString.Empty; }
		}

		public virtual ZString ShipperPhone
		{
			get { return (ShipperAddress != null) ? ShipperAddress.OA_Phone : ZString.Empty; }
		}

		OrgAddress ShipperAddress
		{
			get { return (Shipment.Consignor != null) ? Shipment.Consignor.MainAddress : null; }
		}

		RefUNLOCO ShipperClosestPort
		{
			get { return (Shipment.Consignor != null) ? Shipment.Consignor.ClosestPort : null; }
		}

		#endregion

		#region Consignee details

		public virtual ZString ConsigneeAccount
		{
			get { return (Shipment.Consignee != null) ? Shipment.Consignee.OH_Code : ZString.Empty; }
		}

		public virtual ZString ConsigneeName
		{
			get { return (Shipment.Consignee != null) ? Shipment.Consignee.OH_FullNameTruncated : ZString.Empty; }
		}

		public virtual ZString ConsigneeAddress1
		{
			get { return (ConsigneeAddress != null) ? ConsigneeAddress.OA_Address1 : ZString.Empty; }
		}

		public virtual ZString ConsigneeAddress2
		{
			get { return (ConsigneeAddress != null) ? ConsigneeAddress.OA_Address2 : ZString.Empty; }
		}

		public virtual ZString ConsigneeCity
		{
			get { return (ConsigneeAddress != null) ? ConsigneeAddress.OA_City : ZString.Empty; }
		}

		public virtual ZString ConsigneeState
		{
			get { return (ConsigneeAddress != null) ? ConsigneeAddress.OA_State : ZString.Empty; }
		}

		public virtual ZString ConsigneePostalCode
		{
			get { return (ConsigneeAddress != null) ? ConsigneeAddress.OA_PostCode : ZString.Empty; }
		}

		public ZString ConsigneePortCode
		{
			get { return (ConsigneeClosestPort != null) ? ConsigneeClosestPort.Code : ZString.Empty; }
		}

		public ZString ConsigneeCountryCode
		{
			get { return ConsigneeClosestPort != null ? ConsigneeClosestPort.RL_RN_NKCountryCode : ZString.Empty; }
		}

		public ZString ConsigneeEmail
		{
			get { return (ConsigneeAddress != null) ? ConsigneeAddress.OA_Email : ZString.Empty; }
		}

		public virtual ZString ConsigneePhone
		{
			get { return (ConsigneeAddress != null) ? ConsigneeAddress.OA_Phone : ZString.Empty; }
		}

		OrgAddress ConsigneeAddress
		{
			get { return (Shipment.Consignee != null) ? Shipment.Consignee.MainAddress : null; }
		}

		RefUNLOCO ConsigneeClosestPort
		{
			get { return (Shipment.Consignee != null) ? Shipment.Consignee.ClosestPort : null; }
		}

		#endregion

		#region Delivery

		public virtual ZString DeliveryAgentName
		{
			get { return (DeliveryAgent != null) ? DeliveryAgent.OH_FullNameTruncated : ZString.Empty; }
		}

		public virtual ZString DeliveryAgentAddress1
		{
			get { return (DeliveryAgentAddress != null) ? DeliveryAgentAddress.OA_Address1 : ZString.Empty; }
		}

		public virtual ZString DeliveryAgentAddress2
		{
			get { return (DeliveryAgentAddress != null) ? DeliveryAgentAddress.OA_Address2 : ZString.Empty; }
		}

		public virtual ZString DeliveryAgentCity
		{
			get { return (DeliveryAgentAddress != null) ? DeliveryAgentAddress.OA_City : ZString.Empty; }
		}

		public ZString DeliveryAgentPortCode
		{
			get { return (DeliveryAgent != null) ? DeliveryAgent.OH_RL_NKClosestPort : ZString.Empty; }
		}

		public virtual ZString DeliveryInstructions
		{
			get
			{
				ZString result = "";
				StmNote[] notes = Shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description);
				if (notes.Length > 0)
				{
					result = notes[0].ST_NoteText;
				}
				return result;
			}
		}

		OrgHeader DeliveryAgent
		{
			get
			{
				OrgHeader result = null;
				if (Shipment.DeliveryAgent != null)
				{
					result = Shipment.DeliveryAgent;
				}
				else
				{
					result = HeaderData.ReceivingForwarder;
				}
				return result;
			}
		}

		OrgAddress DeliveryAgentAddress
		{
			get { return (DeliveryAgent != null) ? DeliveryAgent.MainAddress : null; }
		}

		#endregion

		#region Notify Party

		public virtual ZString NotifyPartyName
		{
			get { return (Shipment.NotifyPartyDocumentaryAddress != null) ? Shipment.NotifyPartyDocumentaryAddress.E2_Contact : ZString.Empty; }
		}

		public virtual ZString NotifyPartyAddress1
		{
			get { return (Shipment.NotifyPartyDocumentaryAddress != null) ? Shipment.NotifyPartyDocumentaryAddress.E2_Address1 : ZString.Empty; }
		}

		public virtual ZString NotifyPartyAddress2
		{
			get { return (Shipment.NotifyPartyDocumentaryAddress != null) ? Shipment.NotifyPartyDocumentaryAddress.E2_Address2 : ZString.Empty; }
		}

		public virtual ZString NotifyPartyCity
		{
			get { return (Shipment.NotifyPartyDocumentaryAddress != null) ? Shipment.NotifyPartyDocumentaryAddress.E2_City : ZString.Empty; }
		}

		public ZString NotifyPartyEmail
		{
			get { return (Shipment.NotifyPartyDocumentaryAddress != null) ? Shipment.NotifyPartyDocumentaryAddress.E2_Email : ZString.Empty; }
		}

		public virtual ZString NotifyPartyPhone
		{
			get { return (Shipment.NotifyPartyDocumentaryAddress != null) ? Shipment.NotifyPartyDocumentaryAddress.E2_Phone : ZString.Empty; }
		}

		#endregion

		#region Freight Charges

		public ZString PaymentTerm
		{
			get { return Shipment.JS_PaymentTerm; }
		}

		public ZDecimal TotalFreightAmount
		{
			get { return Shipment.TotalFreightRevenue; }
		}

		public ZString Currency
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency.RX_Code; }
		}

		#endregion

		#region Vessel

		public virtual ZString VesselName
		{
			get { return (Vessel != null) ? Vessel.RV_Code : ZString.Empty; }
		}

		public ZString VesselLloydsCode
		{
			get { return (Vessel != null) ? Vessel.RV_LloydsNumber : ZString.Empty; }
		}

		public ZString VesselCountryCode
		{
			get { return (Vessel != null && Vessel.CountryOfReg != null) ? Vessel.CountryOfReg.Code : ZString.Empty; }
		}

		RefVessel Vessel
		{
			get { return (Consol != null) ? Consol.Vessel : null; }
		}

		#endregion

		#region Voyage

		public virtual ZString VoyageNumber
		{
			get { return (Consol != null) ? Consol.JK_JX_JV_VoyageFlight : ZString.Empty; }
		}

		#endregion

		#region OnBoardDate

		public ZDateTime OnBoardDate
		{
			get { return Shipment.JS_ShippedOnBoardDate; }
		}

		#endregion

		#region Special Instructions

		public ZString SpecialInstructions
		{
			get
			{
				ZString result = "";
				StmNote[] notes = Shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.SpecialInstructions.Description);
				if (notes.Length > 0)
				{
					result = notes[0].ST_NoteText;
				}
				return result;
			}
		}

		#endregion

		#region Handling Instructions

		public virtual ZString HandlingInstructions
		{
			get
			{
				ZString result = "";
				StmNote[] notes = Shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
				if (notes.Length > 0)
				{
					result = notes[0].ST_NoteText;
				}
				return result;
			}
		}

		#endregion

		#region FreightInfo

		public ZInt TotalNoOfPackages
		{
			get { return Shipment.JS_OuterPacks; }
		}

		public ZString PackageType
		{
			get { return Shipment.JS_F3_NKPackType; }
		}

		public virtual ZString TotalNoOfPackagesAndUnitInWords
		{
			get
			{
				ZString result = ZString.Empty;
				if (TotalNoOfPackages > 0)
				{
					result = NumberToString_EN.ConvertNumberToWords(TotalNoOfPackages).ToUpper();
					result += " " + Shipment.Lookups.JS_PackType_List.GetDescriptionFromCode(PackageType).ToString().ToUpper() + "(S)";
				}
				return result;
			}
		}

		public ZDecimal GrossWeightInKilograms
		{
			get { return Shipment.GrossWeightInKilograms; }
		}

		public ZDecimal MeasurementInCubicMetres
		{
			get { return Shipment.MeasurementInCubicMetres; }
		}

		public ZDecimal FreightRate
		{
			get
			{
				ZDecimal result = Shipment.JS_UnitFreightRate;
				if (!Shipment.JS_RX_NKFrtRateCurrency.IsEmpty && Shipment.JS_RX_NKFrtRateCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					ZQuery filter = new ZQuery(RefExchangeRateSchema.RE_ExRateType, Core.Constants.ExchangeRateTypes.Code.SellRate);
					filter.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now);
					filter.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.Now);
					IEnumerable<RefExchangeRate> rates = Shipment.FrtRateCurrency.ExchangeRates.Find(filter);
					if (rates.Any())
					{
						ZDecimal sellRate = rates.First().RE_SellRate;
						result *= sellRate;
					}
				}
				return result;
			}
		}

		public virtual ZString GoodsDescription
		{
			get
			{
				ZString result = Shipment.DetailedGoodsDescriptionNoteText;
				return !result.IsEmpty ? result : Shipment.JS_GoodsDescription;
			}
		}

		public virtual ZString HarmonisedCommodityCode
		{
			get { return (Shipment.OuterPackLines.Count > 0) ? Shipment.OuterPackLines[0].JL_HarmonisedCode : ZString.Empty; }
		}

		public ZInt NoOfContainers
		{
			get { return Shipment.Containers.Count(); }
		}

		#endregion

		#region Date and Place of Issue

		public ZDateTime DateOfIssue
		{
			get { return Shipment.JS_HouseBillIssueDate; }
		}

		public virtual ZString PlaceOfIssue
		{
			get
			{
				ZString result = "";

				if (PortOfLoading != null)
				{
					result = PortOfLoading.RL_PortName;
					if (PortOfLoading.Country != null)
					{
						result += ", " + PortOfLoading.Country.RN_DescMultilingual;
					}
				}
				return result;
			}
		}

		#endregion

		#region Marks and Numbers

		public virtual ZString MarksAndNumbers
		{
			get
			{
				ZString result = "";
				StmNote[] notes = Shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.MarksAndNumbers.Description);
				if (notes.Length > 0)
				{
					result = notes[0].ST_NoteText;
				}
				return result;
			}
		}

		#endregion

		#region Payable By

		public virtual ZString FreightPayableBy
		{
			get { return (Shipment.IsPrepaid) ? "ORIGIN" : "DESTINATION"; }
		}

		#endregion

		#region NoOfBillOfLadings

		public ZByte NoOfBillOfLadings
		{
			get { return Shipment.JS_NoOriginalBills; }
		}

		#endregion

		#endregion

		void CheckForNullHeaderAndShipment(IJXCExportHeader headerData, JASForwardingShipment shipment)
		{
			if (headerData == null)
			{
				throw new InvalidOperationException("HeaderData cannot be null");
			}

			if (shipment == null)
			{
				throw new InvalidOperationException("Shipment cannot be null");
			}
		}

		public readonly IJXCExportHeader HeaderData;
		public readonly JASForwardingShipment Shipment;
	}
}
