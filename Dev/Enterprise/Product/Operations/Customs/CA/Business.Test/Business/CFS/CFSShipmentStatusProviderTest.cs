using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Moq;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CFSShipmentStatusProviderTest : Freight.CFS.Business.Testing.CFSShipmentStatusProviderTest
	{
		public void TestStatus()
		{
			var shipment = Factory.New<GatePassShipment>();
			var provider = new CFSShipmentStatusProviderForTesting(shipment);

			AssertEquals("ShortStatus", "", provider.ShortStatus);
			AssertEquals("Status", "Not Sent", provider.Status);

			var helper = new DeclarationTestHelper(Factory, true);
			var releaseStatus = new ReleaseStatus((EDIReleaseMessage)helper.GetEDIReleaseResponseMessage("37132536987", "1"));

			CFSShipmentRNSStatusProviderTest.AddAWOMessages(shipment);

			AssertEquals("ShortStatus", "AWO", provider.ShortStatus);
			AssertEquals("Status", "Awaiting Warehouse Arrival Certification Message Original", provider.Status);
			AssertEquals("DetailsFromMessages", "", provider.DetailsFromMessages);

			CFSShipmentRNSStatusProviderTest.AddCLRMessages(shipment);

			AssertEquals("ShortStatus", "CLR", provider.ShortStatus);
			AssertEquals("Status", "4 - Goods Released", provider.Status);
			AssertEquals("DetailsFromMessages", "HTML INTERPRETATION", provider.DetailsFromMessages);
		}

		public void TestStatusClass()
		{
			var shipment = Factory.New<GatePassShipment>();
			var provider = new CFSShipmentStatusProviderForTesting(shipment);

			AssertEquals("StatusClass", StatusClass.Held, provider.StatusClass);

			provider.ProcessingIndicator_Exposed = "4";
			AssertEquals("StatusClass", StatusClass.Clear, provider.StatusClass);

			provider.ProcessingIndicator_Exposed = "23";
			AssertEquals("StatusClass", StatusClass.Clear, provider.StatusClass);

			provider.ProcessingIndicator_Exposed = "6";
			AssertEquals("StatusClass", StatusClass.Warning, provider.StatusClass);

			provider.ProcessingIndicator_Exposed = "1";
			AssertEquals("StatusClass", StatusClass.Held, provider.StatusClass);
		}

		public void TestCanSaveAndPrint()
		{
			var shipment = Factory.New<GatePassShipment>();
			var provider = new CFSShipmentStatusProviderForTesting(shipment);
			AssertEquals("precondition", false, shipment.HasErrors);

			var mock = new Mock<ISaveAndPrintUI>();

			provider.ProcessingIndicator_Exposed = "4";
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));

			provider.ProcessingIndicator_Exposed = "23";
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));

			string message = string.Format("The release status of this shipment is {0}.", provider.Status);

			provider.ProcessingIndicator_Exposed = "5";
			mock.Setup(m => m.ShowError(message));
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ProcessingIndicator_Exposed = "7";
			mock.Setup(m => m.ShowError(message));
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ProcessingIndicator_Exposed = "9";
			mock.Setup(m => m.ShowError(message));
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ProcessingIndicator_Exposed = "34";
			mock.Setup(m => m.ShowError(message));
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ProcessingIndicator_Exposed = "6";
			mock.Setup(m => m.Ask(string.Format("{0} Continue with Contingency Release?", message))).Returns(false);
			AssertEquals(false, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ProcessingIndicator_Exposed = "";
			mock.Setup(m => m.ShowWarning(message));
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();

			provider.ProcessingIndicator_Exposed = "1";
			mock.Setup(m => m.ShowWarning(message));
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
			mock.VerifyAll();
		}

		#region Implementation
		class CFSShipmentStatusProviderForTesting : CFSShipmentStatusProvider
		{
			public CFSShipmentStatusProviderForTesting(CFSShipment shipment)
				: base(shipment)
			{
			}

			public ZString ProcessingIndicator_Exposed = ZString.Empty;
			protected override ZString processingIndicator
			{
				get
				{
					return ProcessingIndicator_Exposed;
				}
			}
		}

		protected override string GetCountryCode()
		{
			return Enterprise.Core.Constants.CountryCodes.Canada;
		}
		#endregion
	}
}
