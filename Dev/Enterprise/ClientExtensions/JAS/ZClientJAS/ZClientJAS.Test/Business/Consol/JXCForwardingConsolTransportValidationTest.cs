using System;
using Enterprise.Freight.Business;
using Moq;
using Moq.Protected;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	internal class JXCForwardingConsolTransportValidationTest : JXCValidationTestCase
	{
		public void TestAutoValidationType()
		{
			JXCForwardingConsolTransportValidation validation = (JXCForwardingConsolTransportValidation)Activator.CreateInstance(ValidationTypeToTest, new object[] { Transport });
			AssertEquals(validation.GetType(), validation.AutoValidationType);
		}

		public void TestRequiresValidation_Air()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			Transport transport = consol.Transports[0];
			JXCForwardingConsolTransportValidation validation = new JXCForwardingConsolTransportValidation(transport);
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_LegOrder = 1;
			Assert("Transport is the first air Transport, should be true", validation.RequiresValidation(Core.Constants.TransportModes.Air));
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Sanity check", consol.Transports.MostInterestingTransport, transport);
			Assert("Transport is the Main Transport, should be true", validation.RequiresValidation(Core.Constants.TransportModes.Air));
			Transport anotherTransport = consol.Transports.AddNew();
			JXCForwardingConsolTransportValidation anotherValidation = new JXCForwardingConsolTransportValidation(anotherTransport);
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			anotherTransport.JW_TransportMode = Core.Constants.TransportModes.Air;
			anotherTransport.JW_LegOrder = 2;
			Assert("Transport is the first air Transport, should be true", validation.RequiresValidation(Core.Constants.TransportModes.Air));
			Assert("Not the first air transport, should be false", !anotherValidation.RequiresValidation(Core.Constants.TransportModes.Air));
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			Assert("Transport is not an air Transport, should be false", !validation.RequiresValidation(Core.Constants.TransportModes.Air));
			Assert("AnotherTransport is the first air Transport, should be true", anotherValidation.RequiresValidation(Core.Constants.TransportModes.Air));
		}

		public void TestRequiresValidation_Sea()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.Transports.RemoveAll();
			Transport transport = consol.Transports[0];
			JXCForwardingConsolTransportValidation validation = new JXCForwardingConsolTransportValidation(transport);
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_LegOrder = 1;
			Assert("Transport is the first sea Transport, should be true", validation.RequiresValidation(Core.Constants.TransportModes.Sea));
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Sanity check", consol.Transports.MostInterestingTransport, transport);
			Assert("Transport is the Main Transport, should be true", validation.RequiresValidation(Core.Constants.TransportModes.Sea));
			Transport anotherTransport = consol.Transports.AddNew();
			JXCForwardingConsolTransportValidation anotherValidation = new JXCForwardingConsolTransportValidation(anotherTransport);
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			anotherTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			anotherTransport.JW_LegOrder = 2;
			Assert("Transport is the first sea Transport, should be true", validation.RequiresValidation(Core.Constants.TransportModes.Sea));
			Assert("Not the first sea transport, should be false", !anotherValidation.RequiresValidation(Core.Constants.TransportModes.Sea));
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			Assert("Transport is not a sea Transport, should be false", !validation.RequiresValidation(Core.Constants.TransportModes.Sea));
			Assert("AnotherTransport is the first sea Transport, should be true", anotherValidation.RequiresValidation(Core.Constants.TransportModes.Sea));
		}

		public void TestValidateAll()
		{
			var validationMock = new Mock<JXCForwardingConsolTransportValidation>(new object[] { Transport });
			validationMock.CallBase = true;
			var validation = validationMock.Object;
			validationMock.Protected().Setup("CheckJW_ETD");
			validationMock.Protected().Setup("CheckJW_VoyageFlight");
			validationMock.Protected().Setup("CheckJW_Vessel");
			AssertNoExceptionThrown(() => validation.ValidateAll());
			validationMock.VerifyAll();
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			Factory.Validation.MainGroup.RegisterValidationType(typeof(Transport), ValidationTypeToTest);
		}

		protected virtual Type ValidationTypeToTest
		{
			get
			{
				return typeof(JXCForwardingConsolTransportValidation);
			}
		}

		protected JASForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.New<JASForwardingConsol>();
				}

				return fConsol;
			}
		}

		protected Transport Transport
		{
			get
			{
				if (fTransport == null)
				{
					fTransport = Consol.Transports[0];
				}

				return fTransport;
			}
		}

		JASForwardingConsol fConsol;
		Transport fTransport;
		#endregion
	}
}
