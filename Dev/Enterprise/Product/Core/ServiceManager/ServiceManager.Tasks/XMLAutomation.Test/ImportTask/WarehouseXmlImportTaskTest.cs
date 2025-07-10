using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class WarehouseXmlImportTaskTest : TestCaseWithFactory
	{
		public void TestImporterType()
		{
			Assert("Importer Type", Task.GetImporterType() == typeof(WarehouseDocketDataImporter));
		}

		public void TestTaskDescription()
		{
			AssertEquals("Task description", Res.GetString("024b1195-7015-4bcb-a096-f429f1ac9334", "Warehouse XML Import"), Task.TaskDescription);
		}

		readonly TestWarehouseXmlImportTask Task = new TestWarehouseXmlImportTask();

		class TestWarehouseXmlImportTask : WarehouseXmlImportTask
		{
			public TestWarehouseXmlImportTask()
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
