using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class ScheduleXmlImportTaskTest : TestCaseWithFactory
	{
		public void TestImporterType()
		{
			Assert("Importer Type", Task.GetImporterType() == typeof(XmlDataImporter));
		}

		public void TestTaskDescription()
		{
			AssertEquals("Task description", Res.GetString("cac96e9c-5f35-4018-8e47-4beb7a8ea40b", "Schedule XML Import"), Task.TaskDescription);
		}

		readonly TestScheduleXmlImportTask Task = new TestScheduleXmlImportTask();

		class TestScheduleXmlImportTask : ScheduleXmlImportTask
		{
			public TestScheduleXmlImportTask()
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
