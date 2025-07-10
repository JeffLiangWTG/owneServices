using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class DynamicTransactionCreatorPrimaryLedgerAR : DynamicTransactionCreatorDoubleLedger
	{
		public DynamicTransactionCreatorPrimaryLedgerAR(OrganizationSubBalanceCollection aRSubBals,
			OrganizationSubBalanceCollection aPSubBals, ZGuid primaryOrg, BusinessObjectFactory factory)
			: base(aRSubBals, aPSubBals, primaryOrg, factory)
		{
			PrimaryLedgerSubBalances = aRSubBals;
			OtherLedgerSubBalances = aPSubBals;
		}

		protected override TransferCreator DynamicTransferCreator
		{
			get
			{
				if (fDynamicTransferCreator == null)
				{
					fDynamicTransferCreator = new ARTransferCreator(Factory);
				}
				return fDynamicTransferCreator;
			}
		}
	}
}
