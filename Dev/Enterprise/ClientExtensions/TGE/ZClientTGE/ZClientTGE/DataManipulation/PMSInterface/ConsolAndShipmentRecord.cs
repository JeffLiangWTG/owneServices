
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DataTransfer.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.TGE.PMS
{
	public class ConsolAndShipmentRecord : FlatFileDataRow
	{
		public ConsolAndShipmentRecord(FlatFileDataRow dataRow) : base(dataRow)
		{
		}

		public ConsolAndShipmentRecord(string[] dataRow) : base(dataRow)
		{
		}

		public ConsolAndShipmentRecord(int fieldCount) : base(fieldCount)
		{
		}

		#region Shipment Field Property

		public ZString HAWB
		{
			get { return GetField(ShipmentSchema.HAWB); }
		}

		public Xsd.DimensionValue TotalOuterPacks
		{
			get { return Xsd.DimensionValue.FromAmountAndUnit(GetFieldAsZInt(ShipmentSchema.Qty), "PLT"); }
		}

		public Xsd.FinancialValue CustomValue
		{
			get
			{
				ZDecimal customsValue = GetFieldAsZDecimal(ShipmentSchema.CustomValue);
				ZString currency = GetField(ShipmentSchema.CustomsCurr);
				return Xsd.FinancialValue.FromAmountAndCurrencyCode(customsValue, currency);
			}
		}

		public IReadOnlyList<string> OrderReferences
		{
			get { return new string[] { GetField(ShipmentSchema.OrderNo).ToString() }; }
		}

		public ZString GoodsDescriptions
		{
			get { return GetField(ShipmentSchema.DescOfGoods); }
		}

		public uint NumberOfPacks
		{
			get { return (uint)((int)GetFieldAsZInt(ShipmentSchema.Qty)); }
		}

		public ZDecimal Weight
		{
			get { return GetFieldAsZDecimal(ShipmentSchema.ActualWgt); }
		}

		public ZDecimal CustomsValue
		{
			get { return GetFieldAsZDecimal(ShipmentSchema.CustomValue); }
		}

		public ZString Currency
		{
			get { return GetField(ShipmentSchema.CustomsCurr); }
		}

		public ZString CAN
		{
			get
			{
				ZString result = GetField(ShipmentSchema.CAN).Trim();

				if (result.Length > 3)
				{
					result = CMRExportExemptionCodes.Get3CharCode(result);
				}

				return result;
			}
		}

		public Xsd.UNLOCO Destination
		{
			get
			{
				ZString destCountry = GetField(ShipmentSchema.HAWBDestCountry);
				ZString destPort = GetField(ShipmentSchema.HAWBDestPort);

				if (!destCountry.IsEmpty && !destPort.IsEmpty)
				{
					return Xsd.UNLOCO.FromPortCode(Factory, destCountry + destPort);
				}

				return PortOfDischarge;
			}
		}

		public Xsd.UNLOCO Origin
		{
			get
			{
				ZString originCountry = GetField(ShipmentSchema.HAWBOriginCountry);
				ZString originPort = GetField(ShipmentSchema.HAWBOriginPort);

				if (!originCountry.IsEmpty && !originPort.IsEmpty)
				{
					return Xsd.UNLOCO.FromPortCode(Factory, originCountry + originPort);
				}

				return PortOfLoading;
			}
		}

		public ZString CountryOfOrigin
		{
			get { return GetField(ShipmentSchema.HAWBOriginCountry); }
		}

		#region Shipper Field Property

		public ZString ShipperCode
		{
			get { return FixCode(GetField(ShipmentSchema.ShipperCode)); }
		}

		public ZString ShipperName
		{
			get { return GetField(ShipmentSchema.ShipperName); }
		}

		public ZString ShipperAddressLine1
		{
			get { return GetField(ShipmentSchema.ShipperAddress1); }
		}

		public ZString ShipperAddressLine2
		{
			get { return GetField(ShipmentSchema.ShipperAddress2); }
		}

		public ZString ShipperAddressLine3
		{
			get { return GetField(ShipmentSchema.ShipperAddress3); }
		}

		public ZString ShipperPhoneNo
		{
			get { return GetField(ShipmentSchema.ShipperPhone); }
		}

		public ZString ShipperPostCode
		{
			get { return GetField(ShipmentSchema.ShipperPostcode); }
		}

		public ZString ShipperABN
		{
			get { return GetField(ShipmentSchema.ShipperABN); }
		}

		#endregion

		#region Consignee Field Property

		public ZString ConsigneeCode
		{
			get { return FixCode(GetField(ShipmentSchema.ConsigneeCode)); }
		}

		public ZString ConsigneeName
		{
			get { return GetField(ShipmentSchema.ConsigneeName); }
		}

		public ZString ConsigneeAddressLine1
		{
			get { return GetField(ShipmentSchema.ConsigneeAddress1); }
		}

		public ZString ConsigneeAddressLine2
		{
			get { return GetField(ShipmentSchema.ConsigneeAddress2); }
		}

		public ZString ConsigneeAddressLine3
		{
			get { return GetField(ShipmentSchema.ConsigneeAddress3); }
		}

		public ZString ConsigneePhoneNo
		{
			get { return GetField(ShipmentSchema.ConsigneePhone); }
		}

		public ZString ConsigneePostCode
		{
			get { return GetField(ShipmentSchema.ConsigneePostcode); }
		}

		public Xsd.UNLOCO ConsigneeCity
		{
			get { return Xsd.UNLOCO.FromPortCode(Factory, GetField(ShipmentSchema.ConsigneeCity)); }
		}

		#endregion

		#endregion

		#region Consol Field Property

		public ZString MAWB
		{
			get { return GetField(ConsolSchema.MAWB); }
		}

		public ZDateTime ConsolDate
		{
			get
			{
				ZDateTime result = GetFieldAsZDateTime(ConsolSchema.ConsolDate, ShortDateFormat);
				return result.IsValid ? result : ZDateTime.Now;
			}
		}

		public Xsd.ConsolType ConsolType
		{
			get { return GetConsolType(GetField(ConsolSchema.ConsolType)); }
		}

		public Xsd.ContainerMode ContainerMode
		{
			get { return GetContainerMode(GetField(ConsolSchema.ConsolMode)); }
		}

		public ZString SendingAgent
		{
			get { return GetField(ConsolSchema.Agent); }
		}

		public Xsd.UNLOCO PortOfLoading
		{
			get { return Xsd.UNLOCO.FromPortCode(Factory, GetField(ConsolSchema.OriginCountry) + GetField(ConsolSchema.OriginPort)); }
		}

		public Xsd.UNLOCO PortOfDischarge
		{
			get { return Xsd.UNLOCO.FromPortCode(Factory, GetField(ConsolSchema.DestCountry) + GetField(ConsolSchema.DestPort)); }
		}

		public ZDateTime ETD
		{
			get { return GetFieldAsZDateTime(ConsolSchema.ETD, LongDateTimeFormat); }
		}

		public ZDateTime ETA
		{
			get { return GetFieldAsZDateTime(ConsolSchema.ETA, LongDateTimeFormat); }
		}

		public ZString FlightCarrier
		{
			get { return GetField(ConsolSchema.Flight1Carrier); }
		}

		public ZString CarrierCode
		{
			get { return GetField(ConsolSchema.AirlineCode); }
		}

		public ZString FlightNo
		{
			get { return GetField(ConsolSchema.Flight1Number); }
		}

		public ZString CreditorCode
		{
			get { return FixCode(GetField(ConsolSchema.FrtCreditorCode)); }
		}

		public ZString ReceivingAgent
		{
			get { return GetField(ConsolSchema.ReceivingAgent); }
		}

		#endregion

		#region Implementation

		#region Factory

		public BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}
				return fFactory;
			}
		}

		BusinessObjectFactory fFactory;

		#endregion

		#region GetContainerType

		Xsd.ContainerMode GetContainerMode(ZString mode)
		{
			Xsd.ContainerMode result = new Xsd.ContainerMode();
			switch (mode)
			{
				default:
				case "LOOSE":
				case "LSE":
					result = Xsd.ContainerMode.LSE;
					break;

				case "ULD":
				case "Unit Load Device":
					result = Xsd.ContainerMode.ULD;
					break;

				case "BCN":
				case "Buyer's Consolidation":
					result = Xsd.ContainerMode.BCN;
					break;

				case "OTH":
				case "OTHER":
					result = Xsd.ContainerMode.OTH;
					break;
			}

			return result;
		}

		#endregion

		#region GetConsolType

		Xsd.ConsolType GetConsolType(ZString consolType)
		{
			Xsd.ConsolType result = new Xsd.ConsolType();

			switch (consolType)
			{
				default:
				case "Agent2Agent":
				case "AGT":
					result = Xsd.ConsolType.Agent;
					break;

				case "Co_load":
				case "CLD":
					result = Xsd.ConsolType.CoLoad;
					break;

				case "Direct":
				case "DRT":
					result = Xsd.ConsolType.Direct;
					break;

				case "Charter":
				case "CHT":
					result = Xsd.ConsolType.Charter;
					break;

				case "Other":
				case "OTH":
					result = Xsd.ConsolType.Other;
					break;
			}

			return result;
		}

		#endregion

		#endregion

		const string LongDateTimeFormat = "yyyyMMddHHmmss";
		const string ShortDateFormat = "yyyyMMdd";

		#region ConsolSchema

		public static class ConsolSchema
		{
			public const int Branch = 0;
			public const int ConsolDate = 1;
			public const int ConsolType = 2;
			public const int ConsolMode = 3;
			public const int OriginCountry = 4;
			public const int OriginPort = 5;
			public const int DestCountry = 6;
			public const int DestPort = 7;
			public const int ETD = 8;
			public const int ETA = 9;
			public const int ATD = 10;
			public const int Agent = 11;
			public const int Flight1Carrier = 13;
			public const int Flight1Number = 14;
			public const int Flight1Dest = 15;
			public const int MAWB = 16;
			public const int DischargeCountry = 17;
			public const int DischargePort = 18;
			public const int AirlineCode = 19;
			public const int FrtCreditorCode = 20;
			public const int ReceivingAgent = 21;
			public const int CurrencyCode = 22;
		}

		#endregion

		#region ShipmentSchema

		public static class ShipmentSchema
		{
			public const int ShipperCode = 23;
			public const int ShipperName = 24;
			public const int ShipperAddress1 = 25;
			public const int ShipperAddress2 = 26;
			public const int ShipperAddress3 = 27;
			public const int ShipperPostcode = 28;
			public const int ShipperPhone = 29;
			public const int ShipperABN = 30;
			public const int ConsigneeCode = 31;
			public const int ConsigneeName = 32;
			public const int ConsigneeAddress1 = 33;
			public const int ConsigneeAddress2 = 34;
			public const int ConsigneeAddress3 = 35;
			public const int ConsigneePostcode = 36;
			public const int ConsigneePhone = 37;
			public const int ConsigneeCity = 38;
			public const int BillCode = 39;
			public const int DelivAgent = 40;
			public const int SalesPerson = 41;
			public const int OpsPerson = 42;
			public const int AsAgreed = 44;
			public const int OrderNo = 45;
			public const int HAWB = 46;
			public const int HAWBOriginCountry = 47;
			public const int HAWBOriginPort = 48;
			public const int HAWBDestCountry = 49;
			public const int HAWBDestPort = 50;
			public const int HAWBETA = 51;
			public const int Qty = 52;
			public const int PackCode = 53;
			public const int ServiceLevel = 54;
			public const int INCOTerm = 55;
			public const int CustomValue = 57;
			public const int CustomsCurr = 58;
			public const int DescOfGoods = 59;
			public const int ActualWgt = 60;
			public const int ActualVol = 61;
			public const int Chgwgt = 62;
			public const int HAWBKgRate = 63;
			public const int AirPrepaid = 64;
			public const int FrtCurr = 65;
			public const int FrtPrepaidAmt = 66;
			public const int CAN = 67;
			public const int Type = 68;
		}

		#endregion

		ZString FixCode(ZString code)
		{
			return (code.ToUpper() == MiscCode) ? ZString.Empty : code.ToUpper();
		}

		internal const string MiscCode = "MISC";
	}
}
