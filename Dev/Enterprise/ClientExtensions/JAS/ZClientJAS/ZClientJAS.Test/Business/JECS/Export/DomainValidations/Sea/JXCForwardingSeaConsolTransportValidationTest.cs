using System;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	internal class JXCForwardingSeaConsolTransportValidationTest : JXCForwardingConsolTransportValidationTest
	{
		public void TestValidateJW_VoyageFlight()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.OHBLFieldBoundaries.VoyageSSELNumberMaxLength, JobConsolTransportSchema.JW_VoyageFlight);
			Transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			Transport.JW_VoyageFlight = "VOY123";
			AssertHasNoJXCWarnings(Transport.JW_VoyageFlightInfo);
			Transport.JW_VoyageFlight = "";
			AssertHasNotEnteredJXCWarning(Transport.JW_VoyageFlightInfo);
		}

		public void TestShouldNotValidateJW_VoyageFlightIfDoesNotRequireValidation()
		{
			Transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			Transport newTransport = Consol.Transports.AddNew();
			JXCForwardingConsolTransportValidation validation = new JXCForwardingConsolTransportValidation(Transport);
			newTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			Transport.JW_VoyageFlight = ZString.Empty;
			Transport.Validation.ValidateJW_VoyageFlight();
			Assert("Sanity check", !validation.RequiresValidation(Core.Constants.TransportModes.Sea));
			AssertHasNoJXCWarnings("Should not be validated, not sea transport", Transport.JW_VoyageFlightInfo);
			newTransport.JW_LegOrder = 1;
			Transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			Transport.JW_LegOrder = 2;
			Assert("Sanity check", !validation.RequiresValidation(Core.Constants.TransportModes.Sea));
			Transport.Validation.ValidateJW_VoyageFlight();
			AssertHasNoJXCWarnings("Should not be validated, not the first air transport", Transport.JW_VoyageFlightInfo);
		}

		public void TestValidateJW_Vessel()
		{
			Transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			Transport.JW_Vessel = "HAHA";
			AssertHasNoJXCWarnings(Transport.JW_VesselInfo);
			Transport.JW_Vessel = "";
			AssertHasNotEnteredJXCWarning(Transport.JW_VesselInfo);
		}

		public void TestValidateJW_VesselForCoLoadConsol()
		{
			Transport newTransport = Factory.New<JASForwardingConsol>().Transports.AddNew();
			newTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			newTransport.JW_Vessel = "HAHA";
			AssertHasNoJXCWarnings("Not attached to a consol, cannot determine whether consol is co-load. Should not throw exception", newTransport.JW_VesselInfo);
			Transport.JW_Vessel = "HAHA";
			AssertHasNoJXCWarnings("Not a co-load consol, should not validate country code", Transport.JW_VesselInfo);
			Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			Transport.Validation.ValidateJW_Vessel();
			AssertHasNoJXCWarnings("Vessel is invalid, cannot validate. Should not throw exception", Transport.JW_VesselInfo);
			RefVessel newVessel = Factory.New<RefVessel>();
			newVessel.RV_Code = "HAHA";
			Transport.Validation.ValidateJW_Vessel();
			AssertHasJXCWarning(Transport.JW_VesselInfo, JXCConstants.JXCWarningPrefix + "Vessel Country is not specified. Please modify the Vessel details to include Country of Registration");
			newVessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Spain;
			Transport.Validation.ValidateJW_Vessel();
			AssertHasNoJXCWarnings(Transport.JW_VesselInfo);
		}

		public void TestShouldNotValidateJW_VesselIfDoesNotRequireValidation()
		{
			Transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			Transport newTransport = Consol.Transports.AddNew();
			JXCForwardingConsolTransportValidation validation = new JXCForwardingConsolTransportValidation(Transport);
			newTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			Transport.JW_Vessel = ZString.Empty;
			Transport.Validation.ValidateJW_Vessel();
			Assert("Sanity check", !validation.RequiresValidation(Core.Constants.TransportModes.Sea));
			AssertHasNoJXCWarnings("Should not be validated, not sea transport", Transport.JW_VesselInfo);
			newTransport.JW_LegOrder = 1;
			Transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			Transport.JW_LegOrder = 2;
			Assert("Sanity check", !validation.RequiresValidation(Core.Constants.TransportModes.Sea));
			Transport.Validation.ValidateJW_Vessel();
			AssertHasNoJXCWarnings("Should not be validated, not the first air transport", Transport.JW_VesselInfo);
		}

		protected override Type ValidationTypeToTest
		{
			get
			{
				return typeof(JXCForwardingSeaConsolTransportValidation);
			}
		}
	}
}
