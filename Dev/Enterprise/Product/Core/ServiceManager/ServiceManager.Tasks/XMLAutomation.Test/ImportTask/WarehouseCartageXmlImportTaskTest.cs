using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class WarehouseCartageXmlImportTaskTest : TestCaseWithFactory
	{
		#region TestTaskDescription

		public void TestTaskDescription()
		{
			AssertEquals(Res.GetString("892b0272-a2c5-4d5d-99c0-a9b41c255d0b", "Warehouse Port Transport XML Import"), Task.TaskDescription);
		}

		#endregion

		#region TestImporterType

		public void TestImporterType()
		{
			AssertEquals(true, Task.NewImporter() is WarehouseCartageDataImporter);
		}

		#endregion

		#region Implementation

		WarehouseCartageXmlImportTaskForTest Task
		{
			get { return task ?? (task = new WarehouseCartageXmlImportTaskForTest()); }
		}

		WarehouseCartageXmlImportTaskForTest task;

		#endregion

		#region class WarehouseCartageXmlImportTaskForTest

		class WarehouseCartageXmlImportTaskForTest : WarehouseCartageXmlImportTask
		{
			public WarehouseCartageXmlImportTaskForTest()
				: base(new NotificationBuffer())
			{
			}

			public new DataImporter NewImporter()
			{
				return base.NewImporter();
			}
		}

		#endregion
	}
}
