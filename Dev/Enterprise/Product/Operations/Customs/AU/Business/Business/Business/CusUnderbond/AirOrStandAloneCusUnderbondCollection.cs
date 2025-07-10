using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirOrStandAloneCusUnderbondCollection : BusinessObjectCollection<CusUnderbond>
	{
		public AirOrStandAloneCusUnderbondCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = base.CreateRelationshipFilter();
			filter.AddToFilter(CusUnderbondSchema.C4_ApplicationCode, Customs.Business.CusUnderbondApplicationCodeList.Codes.AUUnderbond);
			ZQuery parentFilter = new ZQuery(CusUnderbondSchema.C4_ParentTableCode, CusMAWBSchema.Constants.Prefix);
			parentFilter.AddToFilter(JoinCondition.Or, CusUnderbondSchema.C4_ParentTableCode, SQLComparisonOperator.Equal, ZString.Empty);

			filter.AddToFilter(parentFilter);

			return filter;
		}
	}
}
