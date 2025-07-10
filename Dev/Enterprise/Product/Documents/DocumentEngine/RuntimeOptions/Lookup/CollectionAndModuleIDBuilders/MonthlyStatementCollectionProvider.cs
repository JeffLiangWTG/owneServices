using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class MonthlyStatementCollectionProvider : StatementCollectionProvider
	{
		public MonthlyStatementCollectionProvider(BusinessObjectFactory businessObjectFactory) : base(businessObjectFactory)
		{
		}

		internal override ZQuery GetAdditionalQuery()
		{
			var result = new ZQuery();
			result.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, true);
			return result;
		}
	}
}
