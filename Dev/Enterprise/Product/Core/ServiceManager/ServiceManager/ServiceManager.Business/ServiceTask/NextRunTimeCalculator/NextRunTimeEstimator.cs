using CargoWise.EntityFramework;

namespace Enterprise.ServiceManager.Business
{
	public class NextRunTimeEstimator : NonPersistentBusinessObject
	{
		public NextRunTimeEstimator(StmServiceTask serviceTask)
		{
			this.serviceTask = serviceTask;
		}

		public override bool HasChanges => false;

		#region Next Run Time List

		public NextRunTimeRecordCollection NextRunTimeList
		{
			get
			{
				nextRunTimeCollection ??= new NextRunTimeRecordCollection();

				if (reload && !serviceTask.HasErrors)
				{
					nextRunTimeCollection.Load(serviceTask);
					reload = false;
				}

				return nextRunTimeCollection;
			}
		}

		NextRunTimeRecordCollection nextRunTimeCollection;

		public ZPropertyInfo NextRunTimeInfo => serviceTask.SST_NextRunTimeInfo;

		public void ReloadNextRunTimeList()
		{
			reload = true;
			if (nextRunTimeCollection != null && !serviceTask.HasErrors)
			{
				NextRunTimeList.RefreshBinding();
			}
		}

		bool reload = true;

		#endregion

		readonly StmServiceTask serviceTask;
	}
}

