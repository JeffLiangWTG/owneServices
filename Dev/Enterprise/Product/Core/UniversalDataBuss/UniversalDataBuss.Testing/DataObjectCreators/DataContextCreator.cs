using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Testing
{
	public static class DataContextCreator
	{
		public static IDataContextDataObject Create(DataContextType type, string contextKey,
			string enterpriseID = "EDI", string serverID = "DAT",
			string companyCode = null, string eventBranchCode = null, string eventDepartmentCode = null)
		{
			var dataTargets = CreateDataTargets((type, contextKey));
			return Create(dataTargets, enterpriseID, serverID, companyCode, eventBranchCode, eventDepartmentCode);
		}

		public static IDataContextDataObject Create(List<DataTarget> dataTargetCollection,
			string enterpriseID = "EDI", string serverID = "DAT",
			string companyCode = null, string eventBranchCode = null, string eventDepartmentCode = null)
		{
			var dataContext = new DataContext();
			dataContext.DataTargetCollection = dataTargetCollection;

			dataContext.EnterpriseID = enterpriseID;
			dataContext.ServerID = serverID;

			if (companyCode != null)
			{
				dataContext.Company = new Company { Code = companyCode };
			}

			if (eventBranchCode != null)
			{
				dataContext.EventBranch = new DataObjects.Branch { Code = eventBranchCode };
			}

			if (eventDepartmentCode != null)
			{
				dataContext.EventDepartment = new DataObjects.Department { Code = eventDepartmentCode };
			}
			return dataContext;
		}

		public static List<DataTarget> CreateDataTargets(params (DataContextType type, string contextKey)[] targets)
		{
			var result = new List<DataTarget>();
			foreach (var target in targets)
			{
				result.Add(new DataTarget { Type = target.type.ToString(), Key = target.contextKey });
			}
			return result;
		}
	}
}
