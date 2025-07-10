using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class WarehouseIFSXmlImportTaskTest : TestCaseWithFactory
	{
		#region TetstNotificationGroup

		public void TestNotificationGroup()
		{
			AssertEquals(NotificationDataRegistry.Instance.WarehouseImportNotificationGroup, Task.NotificationGroup);
		}

		#endregion

		#region TestTaskDescription

		public void TestTaskDescription()
		{
			AssertEquals("Warehouse IFS XML Import", Task.TaskDescription);
		}

		#endregion

		#region TestImporterType

		public void TestImporterType()
		{
			AssertEquals(typeof(WarehouseIFSDataImporter), Task.NewImporter().GetType());
		}

		#endregion

		#region Implementation

		WarehouseIFSXmlImportTaskForTest Task
		{
			get { return task ?? (task = new WarehouseIFSXmlImportTaskForTest()); }
		}

		WarehouseIFSXmlImportTaskForTest task;

		#region class WarehouseCartageXmlImportTaskForTest

		class WarehouseIFSXmlImportTaskForTest : WarehouseIFSXmlImportTask
		{
			public WarehouseIFSXmlImportTaskForTest()
				: base(new NotificationBuffer())
			{
			}

			public new DataImporter NewImporter()
			{
				return base.NewImporter();
			}
		}

		#endregion

		#endregion

	}
}
