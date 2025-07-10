using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class DataImportFlagChangerTest : TestCaseWithDummy
	{
		public void TestDataImportFlagChanged()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			Assert("Pre-condition", !((ISupportDataImporting)consol).IsImportingData);
			using (new DataImportFlagChanger(consol))
			{
				Assert("Should be set to true", ((ISupportDataImporting)consol).IsImportingData);
			}

			Assert("Should be set back to false", !((ISupportDataImporting)consol).IsImportingData);
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			((ISupportDataImporting)shipment).IsImportingData = true;
			Assert("Pre-condition", ((ISupportDataImporting)shipment).IsImportingData);
			using (new DataImportFlagChanger(shipment))
			{
				Assert("Should still be true", ((ISupportDataImporting)shipment).IsImportingData);
			}

			Assert("Should still be true", ((ISupportDataImporting)shipment).IsImportingData);
		}

		[ExpectNoExceptions]
		public void TestShouldNotBlowUpIfBizOIsNotISupportDataImporting()
		{
			using (new DataImportFlagChanger(Dummy))
			{
			}
		}
	}
}
