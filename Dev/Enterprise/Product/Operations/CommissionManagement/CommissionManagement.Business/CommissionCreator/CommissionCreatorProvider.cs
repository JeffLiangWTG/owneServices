using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionCreatorProvider : ICommissionCreatorProvider
	{
		public ICommissionCreator GetCommissionCreator(ICommissionableTransaction transaction)
		{
			if (transaction.AH_IsCancelled && transaction.AH_TransactionBelongsToGroup.IsValid)
			{
				return new ReversalTransactionCommissionCreator(transaction);
			}
			else if (!transaction.IsJobRelated)
			{
				return NonJobRelatedTransactionCommissionCreator.New(transaction);
			}
			else
			{
				return new JobRelatedTransactionCommissionCreator(transaction, null);
			}
		}

		public IReversalTransactionCommissionCreator GetReversalTransactionCommissionCreator(ICommissionableTransaction transaction)
			=> new ReversalTransactionCommissionCreator(transaction);

		public IJobClosedCommissionCreator GetJobClosedCommissionCreator(IJobHeader jobHeader, ZDateTime jobCloseTime)
			=> new JobClosedCommissionCreator(jobHeader, jobCloseTime);

		public ICommissionRegenerator GetCommissionRegenerator(IJobHeader jobHeader, BusinessObjectFactory factory, ILogger logger = null)
			=> new JobCommissionRegenerator(jobHeader, factory, logger);

		public ICommissionRegenerator GetCommissionRegenerator(ICommissionableTransaction transaction, BusinessObjectFactory factory, ILogger logger = null)
			=> new TransactionCommissionRegenerator(transaction, factory, logger);
	}
}
