using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class LocalCartageXmlImportTaskTest : TestCaseWithFactory
	{
		public void TestImporterType()
		{
			Assert("Importer Type", Task.GetImporterType() == typeof(LocalCartageDataImporter));
		}

		public void TestTaskDescription()
		{
			AssertEquals("Task description", Res.GetString("562a2cce-008d-4b0a-840e-cc889fc91a7e", "Port Transport XML Import"), Task.TaskDescription);
		}

		readonly TestLocalCartageXmlImportTask Task = new TestLocalCartageXmlImportTask();

		class TestLocalCartageXmlImportTask : LocalCartageXmlImportTask
		{
			public TestLocalCartageXmlImportTask()
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
