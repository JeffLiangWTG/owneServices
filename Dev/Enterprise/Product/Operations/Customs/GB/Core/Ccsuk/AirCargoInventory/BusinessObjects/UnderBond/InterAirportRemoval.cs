using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CodeDescriptionPairLists;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	[RemovalType(RemovalCode)]
	public class InterAirportRemoval : CusUnderbond, INewShedProvider, ILicenseRestrictionIndProvider, IOnwardCarrierProvider, Integration.Customs.GB.CCSUK.ICusUnderbond_InterAirportRemoval
	{
		public InterAirportRemoval(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public new InterAirportRemovalLookups Lookups
		{
			get { return (InterAirportRemovalLookups)base.Lookups; }
		}

		protected override Customs.Business.CusUnderbondLookups GetNewLookups()
		{
			return new InterAirportRemovalLookups(this);
		}

		protected override void SetPKAndDefaults()
		{
			base.SetPKAndDefaults();
			OnwardMode = ModesOfTransportCodes.Codes.Air;
		}

		protected override Customs.Business.CusUnderbondValidation GetNewValidation()
		{
			return new InterAirportRemovalValidation(this);
		}

		public const string RemovalCode = "IAR";

		public override ZString RemovalTypeHuman
		{
			get { return "Inter-Airport Removal"; }
		}

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(InterAirportRemovalLookups.AirportsOfDestinationList))]
		public ZString AirportOfDestination
		{
			get { return AirportOrCountryOfDestination; }
			set { AirportOrCountryOfDestination = value; }
		}
		public ZPropertyInfo AirportOfDestinationInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(AirportOfDestination), x => C4_RL_NKDischargePortInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(InterAirportRemovalLookups.OnwardModeList))]
		public ZString OnwardMode
		{
			get { return OnwardModeCore; }
			set { OnwardModeCore = value; }
		}
		public ZPropertyInfo OnwardModeInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(OnwardMode), x => C4_ModeOfMovementInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(InterAirportRemovalLookups.ShedsList))]
		public ZString NewShedId
		{
			get { return NewShedIdCore; }
			set { NewShedIdCore = value; }
		}
		public ZPropertyInfo NewShedIdInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(NewShedId), x => C4_DischargePremiseIDInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(InterAirportRemovalLookups.Carriers))]
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

		[List(nameof(Lookups) + "." + nameof(InterAirportRemovalLookups.YesNoList))]
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
			table.WriteRow("New Shed", NewShedId);
			table.WriteRow("Onward Carrier", OnwardCarrier);
			table.WriteRow("Onward Waybill", OnwardAirWaybillNumber);
			table.WriteRow("Licence/Restricted Indicator", LicenseRestrictionInd);
			table.WriteRow("Onward Mode of Transport", OnwardMode);

			return table.ToHtml();
		}

		public override ZString Description
		{
			get { return new ZString(AirportOrCountryOfDestination + NewShedId).IsEmpty ? "Inter-airport removal" : "Inter-airport removal to " + AirportOrCountryOfDestination + NewShedId; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			C4_ApplicationCode = Enterprise.Customs.Business.CusUnderbondApplicationCodeList.Codes.GBInterAirportRemoval;
		}
	}
}
