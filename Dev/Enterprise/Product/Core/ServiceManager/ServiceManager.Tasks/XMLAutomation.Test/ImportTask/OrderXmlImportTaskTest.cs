using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class OrderXmlImportTaskTest : TestCaseWithFactory
	{
		public void TestImporterType()
		{
			Assert("Importer Type", Task.GetImporterType() == typeof(OrderXmlDataImporter));
		}

		public void TestTaskDescription()
		{
			AssertEquals("Task description", Res.GetString("cd5e6379-0260-48ef-9656-e518e7550035", "Order XML Import"), Task.TaskDescription);
		}

		readonly TestOrderXmlImportTask Task = new TestOrderXmlImportTask();

		class TestOrderXmlImportTask : OrderXmlImportTask
		{
			public TestOrderXmlImportTask()
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
