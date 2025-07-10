using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class ShipmentXmlImportTaskTest : TestCaseWithFactory
	{
		public void TestImporterType()
		{
			Assert("Importer Type", Task.GetImporterType() == typeof(ShipmentDataImporter));
		}

		public void TestTaskDescription()
		{
			AssertEquals("Task description", Res.GetString("1b91ff4c-2079-4d9d-80d6-f35c5acb1853", "Shipment XML Import"), Task.TaskDescription);
		}

		readonly TestShipmentXmlImportTask Task = new TestShipmentXmlImportTask();

		class TestShipmentXmlImportTask : ShipmentXmlImportTask
		{
			public TestShipmentXmlImportTask()
				: base(new NotificationBuffer())
			{ }

			public Type GetImporterType()
			{
				Type result = null;

				DataImporter importer = NewImporter();

				if (importer != null)
				{
					result = importer.GetType();
				}

				return result;
			}
		}
	}
}
