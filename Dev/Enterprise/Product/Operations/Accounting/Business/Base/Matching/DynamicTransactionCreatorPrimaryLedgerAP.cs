using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class DynamicTransactionCreatorPrimaryLedgerAP : DynamicTransactionCreatorDoubleLedger
	{
		public DynamicTransactionCreatorPrimaryLedgerAP(OrganizationSubBalanceCollection aRSubBals,
			OrganizationSubBalanceCollection aPSubBals, ZGuid primaryOrg, BusinessObjectFactory factory) : base(aRSubBals, aPSubBals, primaryOrg, factory)
		{
			PrimaryLedgerSubBalances = aPSubBals;
			OtherLedgerSubBalances = aRSubBals;
		}

		protected override TransferCreator DynamicTransferCreator
		{
			get
			{
				if (fDynamicTransferCreator == null)
				{
					fDynamicTransferCreator = new APTransferCreator(Factory);
				}
				return fDynamicTransferCreator;
			}
		}
	}
}
