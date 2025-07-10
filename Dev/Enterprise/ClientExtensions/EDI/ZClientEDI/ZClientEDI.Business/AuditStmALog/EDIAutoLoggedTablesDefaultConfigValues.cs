using System.Collections.Generic;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI
{
	public class EDIAutoLoggedTablesDefaultConfigValues : AutoLoggedTablesDefaultConfigValues
	{
		public override IDictionary<string, (bool IsEnabledForADD, bool IsEnabledForEDT, bool IsEnabledForDEL)> GetDefaultConfigurationValues()
		{
			return new Dictionary<string, (bool, bool, bool)>(base.GetDefaultConfigurationValues())
			{
				[IncidentManagementGroupSchema.Constants.TableName] = (false, false, false),
			};
		}
	}
}
