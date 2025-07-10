using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Integration
{
	public interface IJobBatchPostingDirector
	{
		void RunBatchPosting(JobInvoicingPostingOption postingOption, IJobInvoicingPlugIn[] consolCollection, ZString postedObjectName);

		void RunBatchPosting(JobInvoicingPostingOption postingOption, BusinessObject[] consolCollection, ZString postedObjectName);
	}

	public class JobBatchPostingDirectorCreator
	{
		public IJobBatchPostingDirector GetNewJobBatchPostingDirector()
		{
			return (IJobBatchPostingDirector)Activator.CreateInstance(ObjectFactory.GetType<IJobBatchPostingDirector>());
		}
	}
}
