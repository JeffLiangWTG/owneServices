using System.Data;
using System.Web;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public class OrgContactFilterBusinessObject : AutoSimpleFilterBusinessObject
	{
		public OrgContactFilterBusinessObject(BusinessObjectFactory factory, DataRow row)
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

		#region Filter

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = new ZQuery();
				query.DefaultJoinCondition = JoinCondition.And;

				query.AddToFilter(OrgContactSchema.OC_OH, ParentPK);

				SQLComparisonOperator @operator = Contains ? SQLComparisonOperator.Contains : SQLComparisonOperator.StartsWith;

				ZQuery subQuery = new ZQuery();
				subQuery.AddToFilter(OrgContactSchema.OC_ContactName, @operator, SoughtText);
				subQuery.AddToFilter(JoinCondition.Or, OrgContactSchema.OC_Title, @operator, SoughtText);
				subQuery.AddToFilter(JoinCondition.And, OrgContactSchema.OC_IsActive, ZBool.True);
				query.AddToFilter(subQuery);

				return query;
			}
		}

		#endregion

	}
}
