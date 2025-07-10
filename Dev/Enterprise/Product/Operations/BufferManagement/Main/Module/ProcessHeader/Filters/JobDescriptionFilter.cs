using System;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Module
{
	public class JobDescriptionFilter : JobDetailFilter
	{
		public JobDescriptionFilter(GetList workflowTypeListDelegate)
			: base(ProcessHeader.ModuleFilterConstants.JobDescription, GetColumnNameQuery, workflowTypeListDelegate)
		{
			MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|JobDescription", "Job Description");
		}

		static string GetColumnNameQuery(Type workflowType)
		{
			return JobTextPropertyFilter.SafeDescriptionPropertyNameFromType(workflowType);
		}
	}
}
