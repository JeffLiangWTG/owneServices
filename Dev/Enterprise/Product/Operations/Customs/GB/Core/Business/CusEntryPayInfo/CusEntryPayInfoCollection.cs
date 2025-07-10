using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business
{
	public class CusEntryPayInfoCollection : BusinessObjectCollection<CusEntryPayInfo>
	{
		public CusEntryPayInfoCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
		public CusEntryPayInfoCollection(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryPayInfo));
			var subQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), JobDeclarationSchema.JE_ClusterKey);
			subQuery.AddToFilter(JobDeclarationSchema.JE_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			query.AddSubQuery(CusEntryPayInfoSchema.C9_ClusterKey, subQuery, JoinCondition.And);
			return query;
		}
	}
}
