using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class TransportMeanRoadValidation : Freight.Business.TransportRoadValidation
	{
		public TransportMeanRoadValidation(TransportMean transport) : base(transport)
		{
			Argument.NotNull(transport, "Transport");
		}

		public new TransportMean Parent => (TransportMean)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateVehicleCountry();
			ValidateTruckKind();
		}

		public void ValidateVehicleCountry()
		{
			((IValidationInternals)this).Validate(Parent.VehicleCountryInfo, () => { CheckVehicleCountry(); });
		}

		public void ValidateTruckKind()
		{
			((IValidationInternals)this).Validate(Parent.TruckKindInfo, () => { CheckTruckKind(); });
		}

		protected override void CheckJW_RL_NKDiscPort()
		{
			base.CheckJW_RL_NKDiscPort();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.JW_RL_NKDiscPortInfo);
		}

		protected override void CheckJW_Vessel()
		{
			base.CheckJW_Vessel();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.JW_VesselInfo);
		}

		void CheckVehicleCountry()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.VehicleCountryInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.VehicleCountryInfo);
		}

		void CheckTruckKind()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.TruckKindInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.TruckKindInfo);
		}
	}
}
