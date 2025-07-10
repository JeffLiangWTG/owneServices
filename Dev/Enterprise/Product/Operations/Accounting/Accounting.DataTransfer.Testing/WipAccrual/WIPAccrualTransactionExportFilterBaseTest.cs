using System;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.DataTransfer.Testing;

namespace Enterprise.Accounting.DataTransfer.WipsAndAccruals.Testing
{
	public abstract class WIPAccrualTransactionExportFilterBaseTest : TransactionExportFilterTestBase
	{
		protected override Type GetBizoTypeForTableName()
		{
			return typeof(WIP);
		}

		protected override int ExpectedNumberOfHighWaterMarkParams
		{
			get
			{
				return FilterProvider.AtLeastOneTypeOfWIPAccrualPostingIsSelected ? 1 : 0;
			}
		}
	}
}
