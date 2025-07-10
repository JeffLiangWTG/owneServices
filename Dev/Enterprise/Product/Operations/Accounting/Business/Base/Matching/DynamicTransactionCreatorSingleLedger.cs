using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public abstract class DynamicTransactionCreatorSingleLedger : DynamicTransactionCreator
	{
		public DynamicTransactionCreatorSingleLedger(OrganizationSubBalanceCollection orgSubBalances, ZGuid primaryOrg, BusinessObjectFactory factory)
			: base(factory)
		{
			SubBalances = orgSubBalances;
			PrimaryOrganization = primaryOrg;
		}

		protected abstract TransferCreator Creator { get; }
		protected TransferCreator fCreator;
		protected OrganizationSubBalanceCollection SubBalances;
		readonly ZGuid PrimaryOrganization;

		// creates balancing Transfers using the OrganizationSubBalances in SubBalances
		public override IMatchingCollection CreateTransactions()
		{
			IMatchingCollection newTransactions = new IMatchingCollection(Factory);

			foreach (OrganizationSubBalance subBalance in SubBalances)
			{
				if (subBalance.Amount != 0 && subBalance.Organization != PrimaryOrganization)
				{
					Transfer dynamicTransfer = Creator.CreateTransfer(PrimaryOrganization, subBalance);
					newTransactions.Add(dynamicTransfer);
				}
			}
			return newTransactions;
		}
	}
}
