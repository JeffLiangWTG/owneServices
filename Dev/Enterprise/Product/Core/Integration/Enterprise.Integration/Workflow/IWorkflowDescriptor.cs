using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Integration
{
	public interface IWorkflowDescriptor
	{
		string Code { get; }
		IMultilingualString Description { get; }
		Type WorkflowProviderType { get; }
		string[] GetPropertiesThatAffectWorkflow();
		bool GetLogIsValidForDateDefaulting(IStmALog log);
		BusinessObject[] GetUDFMacroDataContext(ITriggerConditions workflowItem, BusinessObject parent);

		#region For Test
#if DEBUG

		BusinessObject GetBizOForTest(BusinessObjectFactory factory);

#endif
		#endregion
	}
}
