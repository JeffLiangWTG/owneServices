using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business
{
	public class OrgAddressReceivablesAutoCompleteHelper : OrgAddressAutoCompleteHelper
	{
		public OrgAddressReceivablesAutoCompleteHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery GetListFilter(ZString key)
		{
			var query = new ZDBOnlyQuery(typeof(OrgAddress));
			query.AddToFilter(OrgAddressSchema.OA_OH, ParentPK);

			var textSubQuery = GetTextSubQuery(key);
			query.AddToFilter(textSubQuery);

			var receivableAddressesSubQuery = OrgAddressQueryHelper.GetAddressTypeSubQuery(OrgAddressType.Receivables);
			query.AddSubQuery(receivableAddressesSubQuery, JoinCondition.And);

			return query;
		}
	}
}
