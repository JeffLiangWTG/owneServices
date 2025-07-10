using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.ASYCUDA.Business
{
	[SystemDefinedValues]
	public class TransportMean : Transport
	{
		public TransportMean(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new partial class Schema : Transport.Schema
		{
			public const string VehicleCountry = "VehicleCountry";
			public const string TruckKind = "TruckKind";

			public const int VehicleCountryMaxLength = 2;
			public const int TruckKindMaxLength = 4;
		}

		[ResourceStringData("Enterprise.Customs.ASYCUDA.Business.TransportMean.JW_LegOrder", Caption = "Sequence Number", ShortCaption = "Seq.")]
		public override ZByte JW_LegOrder { get => base.JW_LegOrder; set => base.JW_LegOrder = value; }

		[ResourceStringData("Enterprise.Customs.ASYCUDA.Business.TransportMean.JW_ETA", Caption = "ETA")]
		public override ZDateTime JW_ETA { get => base.JW_ETA; set => base.JW_ETA = value; }

		[ResourceStringData("Enterprise.Customs.ASYCUDA.Business.TransportMean.JW_ATD", Caption = "ATD")]
		public override ZDateTime JW_ATD { get => base.JW_ATD; set => base.JW_ATD = value; }

		[ResourceStringData("Enterprise.Customs.ASYCUDA.Business.TransportMean.JW_RL_NKDiscPort", Caption = "Arrival Port")]
		public override ZString JW_RL_NKDiscPort { get => base.JW_RL_NKDiscPort; set => base.JW_RL_NKDiscPort = value; }

		[ResourceStringData("Enterprise.Customs.ASYCUDA.Business.TransportMean.JW_Vessel", Caption = "Vehicle Registration ID", ShortCaption = "Vehicle ID")]
		public override ZString JW_Vessel { get => base.JW_Vessel; set => base.JW_Vessel = value; }

		[ResourceStringData("Enterprise.Customs.ASYCUDA.Business.TransportMean.VehicleRegistrationCountry", Caption = "Vehicle Registration Country", ShortCaption = "Vehicle Country")]
		[MaxLength(Schema.VehicleCountryMaxLength)]
		[List(nameof(Lookups) + "." + nameof(TransportMeanLookups.VehicleCountryList))]
		public ZString VehicleCountry
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.VehicleCountry); }
			set
			{
				var oldValue = VehicleCountry;
				CheckMaximumLength(VehicleCountryInfo, value);
				this.SetSystemDefinedValue(Schema.VehicleCountry, value);
				VehicleCountryInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo VehicleCountryInfo => GetZPropertyInfo(Schema.VehicleCountry);

		[ResourceStringData("Enterprise.Customs.ASYCUDA.Business.TransportMean.TruckKind", Caption = "Truck Type")]
		[MaxLength(Schema.TruckKindMaxLength)]
		[List(nameof(Lookups) + "." + nameof(TransportMeanLookups.TruckKindList))]
		public ZString TruckKind
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.TruckKind); }
			set
			{
				var oldValue = TruckKind;
				CheckMaximumLength(TruckKindInfo, value);
				this.SetSystemDefinedValue(Schema.TruckKind, value);
				TruckKindInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo TruckKindInfo => GetZPropertyInfo(Schema.TruckKind);

		public new TransportMeanLookups Lookups => (TransportMeanLookups)base.Lookups;

		protected override JobConsolTransportLookups GetNewLookups() => new TransportMeanLookups(this);
	}
}
