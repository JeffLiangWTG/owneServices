using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class OrderCsvImportTaskTest : TestCaseWithFactory
	{
		public void TestImporterType()
		{
			Assert("Importer Type", Task.GetImporterType() == typeof(CsvOrderDataImporter));
		}

		public void TestTaskDescription()
		{
			AssertEquals(Res.GetString("83cc8665-1022-44d5-a4bf-ccfcf9bf1e28", "Order CSV Import"), Task.TaskDescription);
		}

		readonly OrderCsvImportTaskForTest Task = new OrderCsvImportTaskForTest();

		class OrderCsvImportTaskForTest : OrderCsvImportTask
		{
			public OrderCsvImportTaskForTest()
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
