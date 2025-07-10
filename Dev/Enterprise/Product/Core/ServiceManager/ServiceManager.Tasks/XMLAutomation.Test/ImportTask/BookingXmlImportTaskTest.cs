using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class BookingXmlImportTaskTest : TestCaseWithFactory
	{
		public void TestImporterType()
		{
			Assert("Importer Type", Task.GetImporterType() == typeof(BookingDataImporter));
		}

		public void TestTaskDescription()
		{
			AssertEquals("Task description", Res.GetString("65f04cfc-09f8-4fe4-a956-777f36111548", "Booking XML Import"), Task.TaskDescription);
		}

		readonly TestBookingXmlImportTask Task = new TestBookingXmlImportTask();

		class TestBookingXmlImportTask : BookingXmlImportTask
		{
			public TestBookingXmlImportTask()
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
