using System.Data;
using System.Web;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public class OrgAddressReceivablesFilterBusinessObject : AutoSimpleFilterBusinessObject
	{
		public OrgAddressReceivablesFilterBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		ZGuid ParentPK
		{
			get
			{
				var queryParam = HttpContext.Current.Request[ZFilterPage.ParentPKQuery];
				if (queryParam != null)
				{
					return new ZGuid(queryParam);
				}
				return ZGuid.Empty;
			}
		}

		public override ZQuery Filter
		{
			get
			{
				var query = new ZDBOnlyQuery(typeof(OrgAddress));
				query.AddToFilter(OrgAddressSchema.OA_OH, ParentPK);

				var textSubQuery = GetTextSubQuery();
				query.AddToFilter(textSubQuery);

				var receivableAddressesSubQuery = OrgAddressQueryHelper.GetAddressTypeSubQuery(OrgAddressType.Receivables);
				query.AddSubQuery(receivableAddressesSubQuery, JoinCondition.And);

				return query;
			}
		}

		ZQuery GetTextSubQuery()
		{
			var @operator = Contains ? SQLComparisonOperator.Contains : SQLComparisonOperator.StartsWith;

			var subQuery = new ZQuery();
			subQuery.AddToFilter(OrgAddressSchema.OA_Address1, @operator, SoughtText);
			subQuery.AddToFilter(JoinCondition.Or, OrgAddressSchema.OA_Address2, @operator, SoughtText);
			subQuery.AddToFilter(JoinCondition.Or, OrgAddressSchema.OA_City, @operator, SoughtText);
			subQuery.AddToFilter(JoinCondition.Or, OrgAddressSchema.OA_RL_NKRelatedPortCode, @operator, SoughtText);
			subQuery.AddToFilter(JoinCondition.And, OrgAddressSchema.OA_IsActive, ZBool.True);

			return subQuery;
		}
	}
}
