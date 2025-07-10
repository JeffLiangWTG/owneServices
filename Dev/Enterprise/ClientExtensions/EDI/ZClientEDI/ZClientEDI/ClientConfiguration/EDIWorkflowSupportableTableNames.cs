using System.Linq;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client
{
	class EDIWorkflowSupportableTableNames : WorkflowSupportableTableNames
	{
		public override string[] GetTableNames()
		{
			return base.GetTableNames()
				.Concat(new[]
				{
					IncidentMainSchema.Constants.TableName,
					IncidentManagementGroupSchema.Constants.TableName,
					IncidentTriageSchema.Constants.TableName,
				}).ToArray();
		}
	}
}
