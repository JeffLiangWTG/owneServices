using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class DailyStatementCollectionProvider : StatementCollectionProvider
	{
		public DailyStatementCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		internal override ZQuery GetAdditionalQuery()
		{
			var result = new ZQuery();
			result.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, false);
			return result;
		}
	}
}
