using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.UniversalDataBuss.Management.ShipmentProcessing
{
	public class JobTrackerService : IService
	{
		public JobTrackerService(BusinessObjectFactory factory)
		{
			jobs = new List<IJobHeader>();
			this.factory = factory;
		}

		readonly List<IJobHeader> jobs;
		readonly BusinessObjectFactory factory;

		public int JobCount
		{
			get { return jobs.Count; }
		}

		public void Add(IJobHeader disposable)
		{
			if (disposable != null)
			{
				jobs.Add(disposable);
				factory.SubscribeForDispose(disposable);
			}
		}

		public void Clear()
		{
			jobs.Clear();
		}

		public void DeleteNewJobs()
		{
			foreach (var job in jobs)
			{
				if (!job.IsInDatabase)
				{
					job.Delete();
				}
			}
			Clear();
		}
	}
}
