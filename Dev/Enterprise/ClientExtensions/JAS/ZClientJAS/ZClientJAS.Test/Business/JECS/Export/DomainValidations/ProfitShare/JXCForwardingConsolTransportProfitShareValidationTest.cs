using System;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	internal class JXCForwardingConsolTransportProfitShareValidationTest : JXCForwardingConsolTransportValidationTest
	{
		public void TestValidateJW_ETD()
		{
			Transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			Transport.Validation.ValidateJW_ETD();
			AssertHasNotEnteredJXCWarning(Transport.JW_ETDInfo);
			Transport.JW_ETD = ZDateTime.Now;
			AssertHasNoJXCWarnings(Transport.JW_ETDInfo);
		}

		public void TestShouldNotValidateJW_ETDIfDoesNotRequireValidation()
		{
			Transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			Transport newTransport = Consol.Transports.AddNew();
			newTransport.JW_TransportMode = Core.Constants.TransportModes.Air;
			JXCForwardingConsolTransportValidation validation = new JXCForwardingConsolTransportValidation(Transport);
			Transport.JW_ETD = ZDateTime.Empty;
			Transport.Validation.ValidateJW_ETD();
			Assert("Sanity check", !validation.RequiresValidation(Core.Constants.TransportModes.Air));
			AssertHasNoJXCWarnings("Should not be validated, not air transport", Transport.JW_ETDInfo);
			newTransport.JW_LegOrder = 1;
			Transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			Transport.JW_LegOrder = 2;
			Assert("Sanity check", !validation.RequiresValidation(Core.Constants.TransportModes.Air));
			Transport.Validation.ValidateJW_ETD();
			AssertHasNoJXCWarnings("Should not be validated, not the first air transport", Transport.JW_ETDInfo);
		}

		protected override Type ValidationTypeToTest
		{
			get
			{
				return typeof(JXCForwardingConsolTransportProfitShareValidation);
			}
		}
	}
}
