using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Module
{
	sealed class CSARevenueSummaryFormModuleCollection : StatementModuleCollection
	{
		public CSARevenueSummaryFormModuleCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(CusStatementHeaderSchema.B2_StatementType, CusStatementHeaderTypes.Codes.RSF);
			return result;
		}
	}
}
