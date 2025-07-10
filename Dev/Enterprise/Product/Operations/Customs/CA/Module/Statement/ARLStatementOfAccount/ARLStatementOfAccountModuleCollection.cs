using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Module
{
	sealed class ARLStatementOfAccountModuleCollection : StatementModuleCollection
	{
		public ARLStatementOfAccountModuleCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, true);
			result.AddToFilter(CusStatementHeaderSchema.B2_StatementType, SQLComparisonOperator.NotEqual, CusStatementHeaderTypes.Codes.RSF);
			return result;
		}
	}
}
