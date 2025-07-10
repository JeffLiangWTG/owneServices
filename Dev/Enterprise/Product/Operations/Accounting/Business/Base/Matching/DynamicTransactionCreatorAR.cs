using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class DynamicTransactionCreatorAR : DynamicTransactionCreatorSingleLedger
	{
		public DynamicTransactionCreatorAR(OrganizationSubBalanceCollection subBalances, ZGuid primaryOrg, BusinessObjectFactory factory) : base(subBalances, primaryOrg, factory)
		{
		}

		#region ARTransferCreator

		protected override TransferCreator Creator
		{
			get
			{
				if (fCreator == null)
				{
					fCreator = new ARTransferCreator(Factory);
				}
				return fCreator;
			}
		}

		#endregion
	}
}
