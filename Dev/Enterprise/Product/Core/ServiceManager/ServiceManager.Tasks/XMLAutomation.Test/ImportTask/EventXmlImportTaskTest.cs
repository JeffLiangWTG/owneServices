using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class EventXmlImportTaskTest : TestCaseWithFactory
	{
		public void TestImporterType()
		{
			Assert("Importer Type", Task.GetImporterType() == typeof(XmlDataImporter));
		}

		public void TestTaskDescription()
		{
			AssertEquals("Task description", Res.GetString("49d89e6a-d3bc-41f8-bc41-48d72755a0e1", "Event XML Import"), Task.TaskDescription);
		}

		readonly TestEventXmlImportTask Task = new TestEventXmlImportTask();

		class TestEventXmlImportTask : EventXmlImportTask
		{
			public TestEventXmlImportTask()
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
