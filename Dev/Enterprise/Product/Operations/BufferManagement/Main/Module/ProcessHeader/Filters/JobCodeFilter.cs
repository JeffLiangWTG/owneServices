using System;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Module
{
	public class JobCodeFilter : JobDetailFilter
	{
		public JobCodeFilter(GetList workflowTypeListDelegate)
			: base(ProcessHeader.ModuleFilterConstants.JobCode, GetColumnNameQuery, workflowTypeListDelegate)
		{
			MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|JobCode", "Job Code");
		}

		static string GetColumnNameQuery(Type workflowType)
		{
			return JobTextPropertyFilter.SafeCodePropertyNameFromType(workflowType);
		}
	}
}
