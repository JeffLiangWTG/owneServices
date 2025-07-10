using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public abstract class DynamicTransactionCreatorDoubleLedger : DynamicTransactionCreator
	{
		public DynamicTransactionCreatorDoubleLedger(OrganizationSubBalanceCollection aRSubBals,
			OrganizationSubBalanceCollection aPSubBals, ZGuid primaryOrg, BusinessObjectFactory factory)
			: base(factory)
		{
			fPrimaryOrg = primaryOrg;
		}

		public override IMatchingCollection CreateTransactions()
		{
			IMatchingCollection dynamicTransactions = new IMatchingCollection(Factory);

			foreach (OrganizationSubBalance subBal in OtherLedgerSubBalances)
			{
				Contra dynamicContra = DynamicContraCreator.CreateContra(fPrimaryOrg, subBal);
				dynamicTransactions.Add(dynamicContra);
			}
			foreach (OrganizationSubBalance subBal in PrimaryLedgerSubBalances)
			{
				if (subBal.Organization != fPrimaryOrg)
				{
					Transfer dynamicTransfer = DynamicTransferCreator.CreateTransfer(fPrimaryOrg, subBal);
					dynamicTransactions.Add(dynamicTransfer);
				}
			}
			return dynamicTransactions;
		}

		#region DynamicContraCreator

		public ContraCreator DynamicContraCreator
		{
			get
			{
				if (fDynamicContraCreator == null)
				{
					fDynamicContraCreator = new ContraCreator(Factory);
				}
				return fDynamicContraCreator;
			}
		}
		protected ContraCreator fDynamicContraCreator;

		#endregion

		protected abstract TransferCreator DynamicTransferCreator { get; }
		protected TransferCreator fDynamicTransferCreator;

		protected OrganizationSubBalanceCollection PrimaryLedgerSubBalances;
		protected OrganizationSubBalanceCollection OtherLedgerSubBalances;

		readonly ZGuid fPrimaryOrg;
	}
}
