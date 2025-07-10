using System.Collections.Generic;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class BatchExportDirector : XMLDirector
	{
		public BatchExportDirector(INotifications notifications)
			: base(notifications)
		{
			ExportTasks.AddRange(new AccountingExportTasksCreator(notifications, Factory).XMLTasks);
		}

		protected override void RunCore(CancellationToken token)
		{
			foreach (XMLTask task in ExportTasks)
			{
				token.ThrowIfCancellationRequested();
				SqlApplicationLock mutex;
				if (Db.Connection.TryGetLock("BatchExportDirector:" + task.UniqueIdentifier, out mutex))
				{
					using (mutex)
					{
						task.Run();
						Notify.Clear();
					}
				}
			}
		}

#if DEBUG
		internal
#endif
 List<XMLTask> ExportTasks
		{
			get { return exportTasks ?? (exportTasks = new List<XMLTask>()); }
		}
		List<XMLTask> exportTasks;

		BusinessObjectFactory factory;

		public BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
	}
}
