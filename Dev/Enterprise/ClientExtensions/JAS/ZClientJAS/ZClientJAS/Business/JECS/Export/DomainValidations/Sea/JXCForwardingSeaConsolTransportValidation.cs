using System;
using Enterprise.Freight.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations
{
	public class JXCForwardingSeaConsolTransportValidation : JXCForwardingConsolTransportValidation
	{
		public JXCForwardingSeaConsolTransportValidation(Transport transport)
			: base(transport)
		{
		}

		public override Type AutoValidationType
		{
			get { return typeof(JXCForwardingSeaConsolTransportValidation); }
		}

		protected override void CheckJW_VoyageFlight()
		{
			if (RequiresValidation(Core.Constants.TransportModes.Sea))
			{
				ValidationHelper.AddJXCWarningIfNotEntered(Parent.JW_VoyageFlightInfo);
			}
		}

		protected override void CheckJW_Vessel()
		{
			if (RequiresValidation(Core.Constants.TransportModes.Sea))
			{
				ValidationHelper.AddJXCWarningIfNotEntered(Parent.JW_VesselInfo);
				bool isCoLoad = Consol != null && Consol.IsCoLoad;
				if (isCoLoad && Parent.Vessel != null && Parent.Vessel.RV_RN_NKCountryOfReg.IsEmpty)
				{
					ValidationHelper.AddJXCWarning(Parent.JW_VesselInfo, "Vessel Country is not specified. Please modify the Vessel details to include Country of Registration");
				}
			}
		}
	}
}
