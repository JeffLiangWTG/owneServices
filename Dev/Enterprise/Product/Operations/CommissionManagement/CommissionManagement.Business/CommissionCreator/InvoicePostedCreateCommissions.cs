using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.CommissionManagement.Business
{
	public class InvoicePostedCreateCommissions : IInvoicePostedCreateCommissions
	{
		public InvoicePostedCreateCommissions(ICommissionableTransaction transaction)
		{
			this.transaction = transaction;
		}

		readonly ICommissionableTransaction transaction;

		public void PostQueueItemOrCreateCommissions()
		{
			if (OrganisationsDataRegistry.Instance.RunCommissionCreationInBackground.Value && CommissionCreatorOverride == null)
			{
				var factory = transaction.Factory;
				var queueItem = factory.New<OrgCommissionCalculationQueue>();
				queueItem.CAQ_Operation = OrgCommissionCalculationQueueOperationCodeList.Codes.Creation;

				queueItem.CAQ_AH = transaction.PK;
			}
			else
			{
				CreateCommissions();
			}
		}

		public void CreateCommissions()
		{
			var commissionCreator = GetCommissionCreator();
			if (commissionCreator != null)
			{
				commissionCreator.CreateCommissions();
			}
		}

		ICommissionCreator GetCommissionCreator()
		{
			return CommissionCreatorOverride ?? ObjectFactory.Get<ICommissionCreatorProvider>().GetCommissionCreator(transaction);
		}
		public ICommissionCreator CommissionCreatorOverride { get; set; }
	}
}
