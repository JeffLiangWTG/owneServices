using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	[SystemDefinedValues]
	[RemovalType(RemovalCode)]
	public class TranshipmentRemoval : CusUnderbond, ILicenseRestrictionIndProvider, IOnwardCarrierProvider, Integration.Customs.GB.CCSUK.ICusUnderbond_TranshipmentRemoval
	{
		public TranshipmentRemoval(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public const string RemovalCode = "TSR";

		public new class Schema : Customs.Business.CusUnderbond.Schema
		{
			public const string ValueOfGoods = "ValueOfGoods";
		}

		public override ZString RemovalTypeHuman
		{
			get { return "Transhipment Removal"; }
		}

		protected override Customs.Business.CusUnderbondValidation GetNewValidation()
		{
			return new TranshipmentRemovalValidation(this);
		}

		public new TranshipmentRemovalLookups Lookups
		{
			get { return (TranshipmentRemovalLookups)base.Lookups; }
		}

		protected override Customs.Business.CusUnderbondLookups GetNewLookups()
		{
			return new TranshipmentRemovalLookups(this);
		}

		public ZString TranshipmentEntryNumber
		{
			get { return C4_UnderbondBySeaLloydsIMONum; }
			set { C4_UnderbondBySeaLloydsIMONum = value.SubstringSafe(0, C4_UnderbondBySeaLloydsIMONumInfo.MaxLength); }
		}
		public ZPropertyInfo TranshipmentEntryNumberInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(TranshipmentEntryNumber), x => C4_UnderbondBySeaLloydsIMONumInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(TranshipmentRemovalLookups.OnwardModeList))]
		public ZString OnwardMode
		{
			get { return OnwardModeCore; }
			set { OnwardModeCore = value; }
		}
		public ZPropertyInfo OnwardModeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(OnwardMode), x => C4_ModeOfMovementInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(TranshipmentRemovalLookups.PortOfShipmentList))]
		[MaxLength(3)]
		public ZString PortOfShipment
		{
			get { return PortOfShipmentCore; }
			set { PortOfShipmentCore = value; }
		}
		public ZPropertyInfo PortOfShipmentInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(PortOfShipment), x => C4_RL_NKTranshipDestPortInfo); }
		}

		public ZString CountryOfDestination { get { return CountryOfDestinationCore; } }

		[List(nameof(Lookups) + "." + nameof(TranshipmentRemovalLookups.Carriers))]
		public ZString OnwardCarrier
		{
			get { return OnwardCarrierCore; }
			set { OnwardCarrierCore = value; }
		}
		public ZPropertyInfo OnwardCarrierInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(OnwardCarrier), x => C4_FlightNoInfo); }
		}

		public ZString OnwardAirWaybillNumber
		{
			get { return OnwardAirWaybillNumberCore; }
			set { OnwardAirWaybillNumberCore = value; }
		}
		public ZPropertyInfo OnwardAirWaybillNumberInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(OnwardAirWaybillNumber), x => C4_MAWBInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(TranshipmentRemovalLookups.YesNoList))]
		public ZString LicenseRestrictionInd
		{
			get { return LicenseRestrictionIndCore; }
			set { LicenseRestrictionIndCore = value; }
		}

		public ZPropertyInfo LicenseRestrictionIndInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(LicenseRestrictionInd), x => C4_UnderbondBySeaVoyageInfo); }
		}

		public override string ToString()
		{
			var table = new HtmlTableCreator();
			WriteCorePropertiesForInterpretation(table);
			table.WriteRow("Port of Shipment", PortOfShipment);
			table.WriteRow("Country of Destination", CountryOfDestination);
			table.WriteRow("Goods' Value", ValueOfGoods.ToStringTrimZeros() + CurrencyCode);
			table.WriteRow("Onward Carrier", OnwardCarrier);
			table.WriteRow("Onward Waybill", OnwardAirWaybillNumber);
			table.WriteRow("Licence/Restricted Indicator", LicenseRestrictionInd);
			table.WriteRow("Onward Mode of Transport", OnwardMode);

			return table.ToHtml();
		}

		public override ZString Description
		{
			get { return AirportOrCountryOfDestination.IsEmpty ? "Transhipment removal" : "Transhipment removal to " + AirportOrCountryOfDestination; }
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			using (GetValidationSuspender())
			{
				OnwardMode = "40";
				PortOfShipment = "LHR";
				AirportOrCountryOfDestination = "AUSYD";
			}
		}
#endif

		public ZDecimal ValueOfGoods
		{
			get => this.GetSystemDefinedValue<ZDecimal>(Customs.Business.GenAddOnHelper.ValueOfGoodsCodeType);
			set
			{
				var oldValue = ValueOfGoods;
				this.SetSystemDefinedValue(Customs.Business.GenAddOnHelper.ValueOfGoodsCodeType, value);
				ValueOfGoodsInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ValueOfGoodsInfo => GetZPropertyInfo(Schema.ValueOfGoods);

		[List(nameof(Lookups) + "." + nameof(TranshipmentRemovalLookups.Currencies))]
		[MaxLength(3)]
		public ZString CurrencyCode
		{
			get
			{
				if (genAddOnForCurrencyCode == null || genAddOnForCurrencyCode.IsDeleted)
				{
					Customs.Business.GenAddOnHelper.FindOrMakeNewAddOn(Customs.Business.GenAddOnHelper.CurrencyCodeType, this, out genAddOnForCurrencyCode);
				}
				return genAddOnForCurrencyCode.XA_Data;
			}
			set
			{
				if (value.IsEmpty && genAddOnForCurrencyCode != null)
				{
					genAddOnForCurrencyCode.Delete();
				}
				else
				{
					if (genAddOnForCurrencyCode == null || genAddOnForCurrencyCode.IsDeleted)
					{
						Customs.Business.GenAddOnHelper.FindOrMakeNewAddOn(Customs.Business.GenAddOnHelper.CurrencyCodeType, this, out genAddOnForCurrencyCode);
					}
					genAddOnForCurrencyCode.XA_Data = value;
				}
			}
		}
		GenAddOnColumn genAddOnForCurrencyCode;

		public ZPropertyInfo CurrencyCodeInfo
		{
			get
			{
				if (genAddOnForCurrencyCode == null)
				{
					Customs.Business.GenAddOnHelper.FindOrMakeNewAddOn(Customs.Business.GenAddOnHelper.CurrencyCodeType, this, out genAddOnForCurrencyCode);
				}
				return GetWrappedZPropertyInfo(nameof(CurrencyCode), x => genAddOnForCurrencyCode.XA_DataInfo);
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			C4_ApplicationCode = Enterprise.Customs.Business.CusUnderbondApplicationCodeList.Codes.GBTranshipmentRemoval;
		}
	}
}
