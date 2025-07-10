using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class CusStatementHeaderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CusStatementHeaderFetchStrategy(CusStatementHeader statementHeader)
			: base(statementHeader)
		{
		}

		new CusStatementHeader BusinessObject
		{
			get { return (CusStatementHeader)base.BusinessObject; }
		}
		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(CusStatementLineSchema.B3_B2, BusinessObject.PK);
		}
		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			Factory.AddFetchHint(OrgHeaderSchema.PK, BusinessObject.B2_OH_Importer);
			Factory.AddFetchHint(StmNoteSchema.ST_ParentID, BusinessObject.PK);

			foreach (var column in columns)
			{
				if (column.ColumnName == nameof(CusStatementHeader.TotalVATBaseAmountForVATReport) || column.ColumnName == nameof(CusStatementHeader.FirstLine) + "+" + nameof(CusStatementLine.FormattedNumber))
				{
					Factory.AddFetchHint(typeof(CusStatementLine), CusStatementLineSchema.B3_B2, BusinessObject.PK);
				}
			}
		}
	}
}
