using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class TransactionAllocationAndPosterCreator : ITransactionAllocationAndPosterCreator
	{
		public IProcessor CreateTransactionAllocationAndPoster(IWorkflowProvider provider) => new TransactionAllocationAndPoster(provider);
	}
}
