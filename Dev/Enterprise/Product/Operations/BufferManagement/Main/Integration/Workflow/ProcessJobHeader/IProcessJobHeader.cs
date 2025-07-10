using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.Integration
{
	public interface IProcessJobHeader : IProcessHeader
	{
		IProcessHeaderCollection ProcessHeaders { get; }
		void ApplyTemplate(IProcessTaskTemplate template, IWorkflowTemplateApplicationParameters parameters = null);
		void ApplyReleaseGroupRules();
		IEnumerable<IProcessHeader> ApplyTemplate(IProcessTaskTemplate template, Predicate<IProcessHeader> templateProcessHeaderPredicate, IWorkflowTemplateApplicationParameters parameters = null);
		IProcessHeader ApplyTemplateWorkflow(IProcessTaskTemplate template, IProcessHeader templateWorkflow, IWorkflowTemplateApplicationParameters parameters = null);
		IProcessHeader CreateQualityIterationWorkflow(IProcessHeader triggerWorkflow, string iterationType = "");
		void RefreshWorkflowsStatus();
	}
}
