using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class CusEntryHeaderFetchStrategy : Customs.Business.FetchStrategies.CusEntryHeaderFetchStrategy
	{
		public CusEntryHeaderFetchStrategy(CusEntryHeader cusEntryLine)
			: base(cusEntryLine)
		{
		}

		CusEntryHeader Header
		{
			get { return (CusEntryHeader)base.BusinessObject; }
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			foreach (var column in columns)
			{
				var columnName = column.ColumnName;
				switch (columnName)
				{
					case CusEntryHeader.Schema.Freight:
					case CusEntryHeader.Schema.Insurance:
					case CusEntryHeader.Schema.TotalPackages:
						Factory.AddFetchHint(CusEntryLineSchema.CL_CH, Header.PK);
						break;
					case CusEntryHeader.Schema.TotalAmountPayable:
						Factory.AddFetchHint(CusEntryHeaderChargesSchema.C1_CH, Header.PK);
						break;
				}
			}
		}
	}
}
