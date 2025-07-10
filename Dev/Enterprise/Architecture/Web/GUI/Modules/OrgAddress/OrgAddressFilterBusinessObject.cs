using System.Data;
using System.Web;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public class OrgAddressFilterBusinessObject : AutoSimpleFilterBusinessObject
	{
		public OrgAddressFilterBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		ZGuid ParentPK
		{
			get
			{
				string queryParam = HttpContext.Current.Request[ZFilterPage.ParentPKQuery];
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
				ZQuery query = new ZQuery();
				query.AddToFilter(OrgAddressSchema.OA_OH, ParentPK);

				SQLComparisonOperator @operator = Contains ? SQLComparisonOperator.Contains : SQLComparisonOperator.StartsWith;

				ZQuery subQuery = new ZQuery();
				subQuery.AddToFilter(OrgAddressSchema.OA_Address1, @operator, SoughtText.SubstringSafe(0, OrgAddressSchema.OA_Address1.MaxLength));
				subQuery.AddToFilter(JoinCondition.Or, OrgAddressSchema.OA_Address2, @operator, SoughtText.SubstringSafe(0, OrgAddressSchema.OA_Address2.MaxLength));
				subQuery.AddToFilter(JoinCondition.Or, OrgAddressSchema.OA_City, @operator, SoughtText.SubstringSafe(0, OrgAddressSchema.OA_City.MaxLength));
				subQuery.AddToFilter(JoinCondition.Or, OrgAddressSchema.OA_RL_NKRelatedPortCode, @operator, SoughtText.SubstringSafe(0, OrgAddressSchema.OA_RL_NKRelatedPortCode.MaxLength));
				subQuery.AddToFilter(JoinCondition.And, OrgAddressSchema.OA_IsActive, ZBool.True);
				query.AddToFilter(subQuery);

				return query;
			}
		}
	}
}
