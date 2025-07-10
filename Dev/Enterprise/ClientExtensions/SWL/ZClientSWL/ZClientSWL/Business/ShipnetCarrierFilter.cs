using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.SWL.Business
{
	public class ShipnetCarrierFilter
	{
		public ZQuery GetFilter()
		{
			ZQuery filter = new ZQuery();
			filter.DefaultJoinCondition = JoinCondition.Or;
			DynamicBusinessObjectCollection shipnetCarrierPKCollection = GetShipnetCarrierPKCollection();
			if (shipnetCarrierPKCollection.Count == 0)
			{
				filter.IsNoResultQuery = true;
			}
			else
			{
				foreach (DynamicBusinessObject shipnetCarrierPK in shipnetCarrierPKCollection)
				{
					filter.AddToFilter(OrgHeaderSchema.PK, (ZGuid)shipnetCarrierPK[OrgCompanyDataSchema.OB_OH]);
				}
			}
			return filter;
		}

		public DynamicBusinessObjectCollection GetShipnetCarrierPKCollection()
		{
			DynamicBusinessObjectCollection result = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			string sD_NameName = "@SD_Name";
			string sD_OwnerName = "@SD_Owner";
			result.Load(GetShipnetCarrierSqlQuery(sD_NameName, sD_OwnerName), GetShipnetCarrierSqlQueryParameters(sD_NameName, sD_OwnerName));
			return result;
		}

		string GetShipnetCarrierSqlQuery(string sD_NameName, string sD_OwnerName)
		{
			return string.Format(@"SELECT DISTINCT {0}
FROM {1} 
WHERE {2} IN (SELECT {3}
		FROM {4} 
		WHERE {5} = {7}
		AND {6} = {8})",
			OrgCompanyDataSchema.Constants.OB_OH,
			OrgCompanyDataSchema.Constants.TableName,
			OrgCompanyDataSchema.Constants.PK,
			StmDataSchema.Constants.SD_DepartmentGuid,
			StmDataSchema.Constants.TableName,
			StmDataSchema.Constants.SD_Name,
			StmDataSchema.Constants.SD_Owner,
			sD_NameName,
			sD_OwnerName);
		}

		ZSqlParameterCollection GetShipnetCarrierSqlQueryParameters(string sD_NameName, string sD_OwnerName)
		{
			ZSqlParameterCollection result = new ZSqlParameterCollection(ZSqlParameter.New(sD_NameName, "ShipnetSetupRaw", StmDataSchema.SD_Name));
			result.Add(sD_OwnerName, Env.CurrentCompany.PK, StmDataSchema.SD_Owner);
			return result;
		}
	}
}
