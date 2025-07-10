using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class DynamicTransactionCreatorAP : DynamicTransactionCreatorSingleLedger
	{
		public DynamicTransactionCreatorAP(OrganizationSubBalanceCollection subBalances, ZGuid primaryOrg, BusinessObjectFactory factory)
			: base(subBalances, primaryOrg, factory)
		{
		}

		#region APTransferCreator

		protected override TransferCreator Creator
		{
			get
			{
				if (fCreator == null)
				{
					fCreator = new APTransferCreator(Factory);
				}
				return fCreator;
			}
		}

		#endregion
	}
}
