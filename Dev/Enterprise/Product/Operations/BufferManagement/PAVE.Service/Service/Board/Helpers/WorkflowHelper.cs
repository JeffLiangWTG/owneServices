using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Module;
using Enterprise.VisualBoards.Business;
using Enterprise.Workflow.Integration;
using Model = CargoWise.PAVE.Common.Model;
using WorkflowDTO = CargoWise.PAVE.Common.DTO.WorkflowDTO;

namespace Enterprise.BufferManagement.Service
{
	public static class WorkflowHelper
	{
		#region Model To DTO

		internal static IEnumerable<WorkflowDTO> ToWorkfloDTOs(this IEnumerable<Model.Workflow> workflows,
			IWorkflowRepository workflowTepository, IEnumerable<Model.Task> tasksInChannels)
		{
			var workflowPKs = workflows.Select(workflow => workflow.PK).ToArray();
			var tasksWorkflows = workflows.Where(wf => tasksInChannels.Any(t => t.WorkflowPK == wf.PK));
			var taskWorkflowDTOs = tasksWorkflows.Select(workflow => ToWorkflowDTO(workflow, workflowPKs, workflowTepository));
			var parentPKs = taskWorkflowDTOs.Where(w => w.ParentPK.HasValue).Select(w => w.ParentPK);
			var parentWorkflows = workflows.Where(wf => parentPKs.Contains(wf.PK));
			var parentWorflowDTOs = parentWorkflows.Select(p => ToWorkflowDTO(p, workflowPKs, workflowTepository));

			return workflows.Select(workflow => ToWorkflowDTO(workflow, workflowPKs, workflowTepository));
		}

		static WorkflowDTO ToWorkflowDTO(Model.Workflow workflow, Guid[] workflowPKs, IWorkflowRepository workflowTepository)
		{
			Guid? parentPK = GetParentPK(workflow.ParentPK, workflowPKs, workflowTepository);

			return new WorkflowDTO
			{
				PK = workflow.PK,
				JobPK = workflow.JobPK,
				ComponentPK = workflow.ComponentPK,
				ParentPK = parentPK,
				#region SuppressResourceStringsCheckRegion 
				Properties = new Dictionary<string, object>
				{
					{ "status", workflow.Status },
					{ "description", workflow.Description },
				}
				#endregion
			};
		}

		static Guid? GetParentPK(Guid? parentPK, Guid[] workflowPKs, IWorkflowRepository workflowRepository)
		{
			if (parentPK == null)
			{
				return null;
			}

			if (workflowPKs.Contains(parentPK.Value))
			{
				return parentPK;
			}

			var parent = workflowRepository.Get(parentPK.Value);

			if (parent?.ParentPK == null)
			{
				return null;
			}

			return GetParentPK(parent.ParentPK, workflowPKs, workflowRepository);
		}

		#endregion

		#region BusinessObject to DTO

		internal static IEnumerable<WorkflowDTO> ToWorkflowsDTO(this IEnumerable<ProcessHeader> workflows, Func<IDictionary<ZGuid, decimal>> getIndexes, bool loadParentWorkflowPK, PropertyCache cache, Dictionary<string, BMControlCustomisation> layouts)
		{
			var indexes = getIndexes?.Invoke();
			var workflowArray = workflows.ToArray();
			var workflowPKs = workflowArray.Select(workflow => workflow.PK).ToArray();

			return workflowArray.Select(workflow => new WorkflowDTO()
			{
				PK = workflow.PK.ToGuid(),
				JobPK = workflow.FH_ParentId.ToGuid(),
				ComponentPK = workflow.CurrentComponent.PK.ToGuid(),
				Index = indexes != null ? indexes[workflow.PK] : 0,
				ParentPK = loadParentWorkflowPK ? GetParentWorkflowPK(workflow, workflowPKs) : null,
				Properties = workflow.LoadProperties(cache, CustomisedLayoutHelper.GetLayoutForRelatedJobType(layouts, workflow))
			}).ToArray();
		}

		static Dictionary<string, object> LoadProperties(this ProcessHeader workflow, PropertyCache cache, BMControlCustomisation layout)
		{
			if (layout != null)
			{
				var properties = PropertyLoaderHelper.LoadPropertiesForLayout(workflow, layout, PropertySourceList.Codes.Workflow, cache);
				PropertyLoaderHelper.AddCompulsoryWorkflowProperties(workflow, properties);
				return properties;
			}
			#region SuppressResourceStringsCheckRegion 
			return new Dictionary<string, object>
				{
					{ "description", workflow.FH_CompletionStatement },
					{ "status", workflow.FH_Status }
				};
			#endregion
		}

		static Guid? GetParentWorkflowPK(ProcessHeader workflow, ZGuid[] workflowPKs)
		{
			if (workflow == null || !workflow.IterationLinks.Cast<IProcessTaskIterationLink>().Any(iteration => iteration.P9I_LinkType == QualityIterationFilterTypeList.Codes.QualityIteration))
			{
				return null;
			}

			var parentWorkflowPK = workflow.WorkflowParent?.PK;

			if (parentWorkflowPK == null)
			{
				return null;
			}

			if (workflowPKs.Contains(parentWorkflowPK.Value))
			{
				return parentWorkflowPK.Value.ToGuid();
			}
			else
			{
				return GetParentWorkflowPK(workflow.WorkflowParent, workflowPKs);
			}
		}

		#endregion
	}
}
