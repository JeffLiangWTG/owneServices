using CargoWise.Application;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Integration
{
	public interface IConsolBatchPostingDirector
	{
		void RunBatchPosting(JobInvoicingPostingOption postingOption, IJobCostingPlugIn[] consolCollection);
	}

	public class ConsolBatchPostingDirectorCreator
	{
		public ConsolBatchPostingDirectorCreator()
		{
		}

		public IConsolBatchPostingDirector GetNewConsolBatchPostingDirector()
		{
			return ObjectFactory.Get<IConsolBatchPostingDirector>();
		}
	}
}
