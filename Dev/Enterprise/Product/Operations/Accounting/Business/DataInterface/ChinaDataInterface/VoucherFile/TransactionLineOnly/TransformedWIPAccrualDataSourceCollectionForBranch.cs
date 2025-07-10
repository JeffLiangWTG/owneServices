using System.Collections.Generic;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class TransformedWIPAccrualDataSourceCollectionForBranch : WIPAccrualDataSourceCollection
	{
		public TransformedWIPAccrualDataSourceCollectionForBranch(IEnumerable<VoucherDataSource> collection)
		{
			voucherDataSources = new SortableList();
			voucherDataSources.AddRange(collection);
		}

		readonly SortableList voucherDataSources;

		public override int GetCount()
		{
			return voucherDataSources.Count;
		}

		public override VoucherDataSource GetVoucherData(int index)
		{
			return voucherDataSources[index];
		}
	}
}
