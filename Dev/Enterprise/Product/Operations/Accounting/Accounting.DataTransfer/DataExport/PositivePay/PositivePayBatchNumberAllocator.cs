using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;

namespace Enterprise.Accounting.DataTransfer.DataExport
{
	public class PositivePayBatchNumberAllocator : SaveInTransactionActionWithMainConnection
	{
		ZInt NewBatchNumber;

		protected override IChangedTableNames SaveInTransaction()
		{
			AccountingNumberFountainNonVoucher numberFountainWrapper = AccountingNumberFountainWrapperFactory.Instance.PositivePayFileExportBatchNumber;
			NewBatchNumber = ZInt.Parse(numberFountainWrapper.GetNext(this.connected));
			return ChangedTableNames.Empty;
		}

		public ZInt GetNewBatchNumber()
		{
			BusinessObjectFactory.SaveTogether(this);
			return NewBatchNumber;
		}
	}
}
