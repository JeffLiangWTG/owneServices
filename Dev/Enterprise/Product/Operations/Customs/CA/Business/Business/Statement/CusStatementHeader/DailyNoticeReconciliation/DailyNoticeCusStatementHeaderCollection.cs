using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	[ModuleID(ModuleId.CADailyNoticeReconciliation)]
	public sealed class DailyNoticeCusStatementHeaderCollection : DependentBusinessObjectCollection<CusStatementHeader, CusStatementHeader>
	{
		public DailyNoticeCusStatementHeaderCollection(CusStatementHeader master)
			: base(master)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, ZBool.False);
			result.AddToFilter(CusStatementHeaderSchema.B2_StatementType, SQLComparisonOperator.NotEqual, CusStatementHeaderTypes.Codes.RSF);

			return result;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var header = (CusStatementHeader)child;
			header.B2_IsMonthlyStatement = false;
		}

		protected override bool AllowNewCore => false;

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusStatementHeaderSchema.B2_B2_PeriodicStatement;
	}
}
