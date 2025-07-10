using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Business
{
	public class StmMenuTemplatePivotBaseStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public StmMenuTemplatePivotBaseStrategy(StmMenuTemplatePivotBase businessObject) : base(businessObject) { }

		StmMenuTemplatePivotBase stmMenuTemplatePivot => BusinessObject as StmMenuTemplatePivotBase;

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			foreach (var column in columns)
			{
				if (string.IsNullOrEmpty(column.TableName))
				{
					if (column.ColumnName == nameof(StmMenuTemplatePivotBase.SO_Name))
					{
						Factory.AddFetchHint(StmTemplateSchema.PK, stmMenuTemplatePivot.SI_SO);
					}
				}
			}
		}
	}
}
