using CargoWise.EntityFramework;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class CusDecHouseBillFetchStrategy : BaseBillFetchStrategy
	{
		public CusDecHouseBillFetchStrategy(Bill bill)
			: base(bill)
		{
		}

		Bill HouseBill
		{
			get { return (Bill)base.BusinessObject; }
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			foreach (var column in columns)
			{
				var columnName = column.ColumnName;
				switch (columnName)
				{
					case Bill.Schema.HBSplitDecReasonRemark:
						Factory.AddFetchHint(StmNoteSchema.ST_ParentID, HouseBill.PK);
						break;
				}
			}
		}
	}
}
