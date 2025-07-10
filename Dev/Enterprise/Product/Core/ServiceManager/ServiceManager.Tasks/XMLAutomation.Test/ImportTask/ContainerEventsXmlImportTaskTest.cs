using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class ContainerEventsXmlImportTaskTest : TestCaseWithFactory
	{
		public void TestImporterType()
		{
			Assert("Importer Type", Task.GetImporterType() == typeof(ContainerEventsDataImporter));
		}

		public void TestTaskDescription()
		{
			AssertEquals("Task description", Res.GetString("657a7cd7-804d-41f0-a1a1-8960a6dd6727", "Container Events XML Import"), Task.TaskDescription);
		}

		readonly TestContainerEventsXmlImportTask Task = new TestContainerEventsXmlImportTask();

		class TestContainerEventsXmlImportTask : ContainerEventsXmlImportTask
		{
			public TestContainerEventsXmlImportTask()
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
