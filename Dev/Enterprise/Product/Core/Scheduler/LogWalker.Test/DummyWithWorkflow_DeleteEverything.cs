using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.LogWalker.Test
{
	sealed class DummyWithWorkflow_DeleteEverything : DummyWithWorkflow, ICustomProcessTaskHandlerProvider
	{
		public DummyWithWorkflow_DeleteEverything(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public IProcessTaskHandler GetHandler(IStmALog log)
		{
			return new ProcessTaskHandler(this);
		}

		class ProcessTaskHandler : IProcessTaskHandler
		{
			readonly DummyWithWorkflow_DeleteEverything dummyWithWorkflow_DeleteEverything;

			public ProcessTaskHandler(DummyWithWorkflow_DeleteEverything dummyWithWorkflow_DeleteEverything)
			{
				this.dummyWithWorkflow_DeleteEverything = dummyWithWorkflow_DeleteEverything;
			}

			public void Fire()
			{
			}

			public void Withdraw()
			{
				foreach (StmALog log in dummyWithWorkflow_DeleteEverything.Logs.GetAllLogs().ToArray())
				{
					if (!log.IsInDatabase && !log.IsDeleted)
					{
						log.Delete();
					}
				}
			}
		}
	}
}
