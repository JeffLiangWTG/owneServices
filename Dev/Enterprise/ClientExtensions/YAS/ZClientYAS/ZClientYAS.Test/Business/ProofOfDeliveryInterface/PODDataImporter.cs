using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.Integration;
using Enterprise.Client.YAS.Testing;

namespace Enterprise.Client.YAS.Business.ProofOfDeliveryInterface.Testing
{
	sealed class PODDataImporterTest : TestCaseWithFactory
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEndToEnd()
		{
			testHelper.SetValidRegistryProofOfDeliveryInterface();

			var shipment = testHelper.CreateForwardingShipment("YATA5685851");
			AssertEquals("PRE: Shipment not delivered.", 0, shipment.DeliveryConfirms.Count);
			AssertNotEquals("PRE: Shipment type is not ASM", Core.Constants.ShipmentTypes.AssemblyMaster, shipment.JS_ShipmentType);
			AssertNotEquals("PRE: Shipment has packlines", 0, shipment.OuterPackLines.Count);
			testHelper.SharedFactory.Save();

			var importer = new PODDataImporter();
			importer.ImportData(YASTestHelper.TestFiles.ProofOfDelivery.CorrectSample, testHelper.Notifications, SourceInfo.EmptySourceInfo);
			Assert("Shipment delivered.", testHelper.Notifications.AsString.Contains("Shipment YATA5685851 is now delivered."));
			AssertEquals("No notification errors.", false, testHelper.Notifications.HasErrors);
		}

		YASTestHelper testHelper;

		protected override void SetUp()
		{
			base.SetUp();
			testHelper = new YASTestHelper(Factory);
		}

		protected override void TearDown()
		{
			base.TearDown();
			testHelper.TidyUp();
		}
	}
}
