using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IReOpenClosedJobDataProvider : IFactoryProvider
	{
		IReadOnlyCollection<Job> GetAllJobs();

		string JobReopenLogText();
	}
}
