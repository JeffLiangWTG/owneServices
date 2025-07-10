using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[ModuleID(ModuleId.ProcessTasks)]
	public class WorkItemProcessTaskCollection : ProcessManagement.Business.WorkItemProcessTaskCollection
	{
		public WorkItemProcessTaskCollection(NewWorkItem workItem)
			: base(workItem)
		{
		}

		public WorkItemProcessTaskCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new WorkItemProcessTask this[int index]
		{
			get { return (WorkItemProcessTask)base[index]; }
		}

		public new WorkItemProcessTask AddNew()
		{
			return (WorkItemProcessTask)base.AddNew();
		}

		public override ProcessTaskCollectionView Tasks
		{
			get
			{
				if (tasks == null)
				{
					tasks = new WorkItemProcessTaskCollectionView(this);
					Parent.RegisterEditableChildObject(tasks);
				}
				return tasks;
			}
		}

		ProcessTaskCollectionView tasks;
	}
}

