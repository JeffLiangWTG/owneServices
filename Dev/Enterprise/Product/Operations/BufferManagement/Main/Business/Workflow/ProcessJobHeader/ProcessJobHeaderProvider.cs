using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.Business
{
	class ProcessJobHeaderProvider : IProcessJobHeaderProvider
	{
		#region IProcessJobHeaderProvider Members

		IProcessJobHeader IProcessJobHeaderProvider.GetForParent(IWorkflowProviderCore parent, BusinessObjectFactory factory, bool addDefaultProcessHeaderIfNone, bool checkTemplates)
		{
			if (BufferManagementEnabledForWorkflowProvider(parent, factory))
			{
				return ProcessJobHeaderProvider.GetForParent(parent, factory, addDefaultProcessHeaderIfNone, checkTemplates);
			}
			else
			{
				return null;
			}
		}

		IProcessJobHeader IProcessJobHeaderProvider.GetForParentWithoutCreation(IWorkflowProviderCore parent, BusinessObjectFactory factory)
		{
			if (BufferManagementEnabledForWorkflowProvider(parent, factory))
			{
				return ProcessJobHeaderProvider.GetForParentWithoutCreation(parent, factory);
			}
			else
			{
				return null;
			}
		}

		bool IProcessJobHeaderProvider.BufferManagementEnabledForWorkflowProvider(IWorkflowProviderCore workflowProvider, BusinessObjectFactory factory)
		{
			return ProcessJobHeaderProvider.BufferManagementEnabledForWorkflowProvider(workflowProvider, factory);
		}

		IProcessJobHeader IProcessJobHeaderProvider.GetForTemplate(IProcessTaskTemplate template, IWorkflowProviderCore parent)
		{
			var workflowProvider = (IWorkflowProvider)parent;
			var concreteTemplate = (ProcessTaskTemplate)template;

			if (BufferManagementEnabledForWorkflowProvider(parent, concreteTemplate.Factory))
			{
				return ProcessJobHeader.GetForTemplate(concreteTemplate, workflowProvider);
			}
			else
			{
				return null;
			}
		}

		bool IProcessJobHeaderProvider.SupportsPAVE(string workflowType, BusinessObjectFactory factory)
		{
			return BMSRegistryProvider.IsBufferManagementEnabled && BMSystem.GetSystemForWorkflowType(workflowType, factory) != null;
		}

		IProcessHeader IProcessJobHeaderProvider.GetDefaultWorkflowForTask(IWorkflowTask task)
		{
			var template = (task as TemplateProcessTask)?.Parent;

			if (template != null)
			{
				var workflows = template.ProcessHeaders.Cast<IProcessHeader>().Where(x => x.IsWorkflow).Take(2).ToArray();

				if (workflows.Length == 1)
				{
					return workflows[0];
				}
			}
			else
			{
				var parent = task.GetJob() as IWorkflowProvider;

				if (parent != null && !(parent is ProcessTask))
				{
					var processJobHeader = ProcessJobHeaderProvider.GetForParent(parent, task.Factory);
					var workflows = processJobHeader?.GetWorkflowsEvenIfListChangedEventsDelayed();

					if (workflows != null && workflows.Count == 1)
					{
						var workflow = workflows.First();
						if (workflow.IsDefaultWorkflow)
						{
							return workflow;
						}
					}
				}
			}

			return null;
		}

		IProcessHeaderCollection IProcessJobHeaderProvider.GetWorkflowsForParent(IWorkflowProviderCore parent, BusinessObjectFactory factory)
		{
			var jobHeader = ProcessJobHeaderProvider.GetForParent(parent, factory, false);
			if (jobHeader != null)
			{
				return jobHeader.ProcessHeaders;
			}
			else
			{
				return new ProcessHeaderCollection(factory, ZQuery.NoResultQuery);
			}
		}

		IProcessHeaderCollection IProcessJobHeaderProvider.GetWorkflowsForProcessTaskCollection(IProcessTaskCollection parent, BusinessObjectFactory factory)
		{
			var taskCollection = parent as ProcessTaskCollection;

			var task = taskCollection[0] as IWorkflowTask;
			var parentJob = task.GetJob() as IWorkflowProvider;

			ProcessJobHeader jobHeader = ProcessJobHeaderProvider.GetForParent(parentJob, factory, false);

			if (jobHeader != null)
			{
				return jobHeader.ProcessHeaders;
			}
			else
			{
				return new ProcessHeaderCollection(factory, ZQuery.NoResultQuery);
			}
		}

		#endregion

		#region Implementation

		internal static ProcessJobHeader GetForParent(IWorkflowProviderCore parent, BusinessObjectFactory factory, bool addDefaultProcessHeaderIfNone = true, bool checkTemplates = true)
		{
			Argument.NotNull(parent, nameof(parent));

			if (BufferManagementEnabledForWorkflowProvider(parent, factory))
			{
				var workflowProvider = (IWorkflowProvider)parent;
				return ProcessJobHeader.GetForParent(workflowProvider, factory, addDefaultProcessHeaderIfNone, checkTemplates);
			}
			else
			{
				return null;
			}
		}

		internal static ProcessJobHeader GetForParentWithoutCreation(IWorkflowProviderCore parent, BusinessObjectFactory factory)
		{
			Argument.NotNull(parent, nameof(parent));

			if (BufferManagementEnabledForWorkflowProvider(parent, factory))
			{
				var workflowProvider = (IWorkflowProvider)parent;
				return ProcessJobHeader.GetForParentWithoutCreation(workflowProvider, factory);
			}
			else
			{
				return null;
			}
		}

		internal static bool BufferManagementEnabledForWorkflowProvider(IWorkflowProviderCore workflowProvider, BusinessObjectFactory factory)
		{
			if (BMSRegistryProvider.IsBufferManagementEnabled)
			{
				var system = BMSystem.GetSystemForWorkflowProvider(workflowProvider, factory);
				return (system != null && system.RelatedWorkflowTypes.Any(t => t.FSW_WorkflowType == workflowProvider.WorkflowType && t.FSW_IsActive));
			}

			return false;
		}

		#endregion
	}
}
