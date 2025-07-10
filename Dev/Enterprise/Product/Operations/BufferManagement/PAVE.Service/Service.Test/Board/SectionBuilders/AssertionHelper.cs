using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using DTOs = CargoWise.PAVE.Common.DTO;

namespace Enterprise.BufferManagement.Service.Test
{
	internal static class AssertionHelper
	{
		#region Job

		public static void AssertJobs(IEnumerable<IWorkflowProvider> workflowProviders, IEnumerable<JobDTO> taskDTOs)
		{
			Assertion.AssertEquals("Number of jobs", workflowProviders.Count(), taskDTOs.Count());

			foreach (var workflowProvider in workflowProviders)
			{
				var taskDTO = taskDTOs.SingleOrDefault(t => t.PK == workflowProvider.PK.ToGuid());
				Assertion.AssertNotNull(taskDTO);

				AssertJob(workflowProvider, taskDTO);
			}
		}

		public static void AssertJob(IWorkflowProvider workflowProvider, JobDTO jobDTO)
		{
			var codeDescription = workflowProvider as ICodeDescription;

			AssertionWithHtml.CombineAssertions(() =>
			{
				Assertion.AssertEquals("PK", workflowProvider.PK.ToGuid(), jobDTO.PK);

				Assertion.AssertEquals("job should have 2 properties", 2, jobDTO.Properties.Count);

				Assertion.AssertEquals("code", codeDescription?.Code, jobDTO.Properties["code"]);
				Assertion.AssertEquals("description", codeDescription?.Description, jobDTO.Properties["description"]);
			});
		}

		#endregion

		#region Tag

		public static void AssertTags(IEnumerable<TagMagnitude> tagMagnitudes, IEnumerable<TagDTO> tagDTOs)
		{
			Assertion.AssertEquals("Number of tag", tagMagnitudes.Count(), tagDTOs.Count());

			foreach (var tagMagnitude in tagMagnitudes)
			{
				var tagDTO = tagDTOs.SingleOrDefault(t => t.PK == tagMagnitude.PK.ToGuid());
				Assertion.AssertNotNull(tagDTO);

				AssertTag(tagMagnitude, tagDTO);
			}
		}

		public static void AssertTag(TagMagnitude tagMagnitude, TagDTO tagDTO)
		{
			var color = tagMagnitude.GetColor();
			var colorHex = !color.IsEmpty && color.IsKnownColor ? color.ToHex() : null;

			#region borderStyle

			TagBorderStyle? borderStyle = null;

			if (!tagMagnitude.ApplyColorToBackground && tagMagnitude.ApplyColorToBorder && tagMagnitude.BorderStyle != ZString.Empty)
			{
				borderStyle = SizedButtonBorderStyle.FromCodeToTagBorderStyle(tagMagnitude.BorderStyle);
			}

			#endregion

			#region borderStyle

			TagBorderSize? borderSize = null;

			if (!tagMagnitude.ApplyColorToBackground && tagMagnitude.ApplyColorToBorder && tagMagnitude.BorderStyle != ZString.Empty)
			{
				var sizedButtonBorderStyle = SizedButtonBorderStyle.FromCode(tagMagnitude.BorderStyle);
				borderSize = sizedButtonBorderStyle.IsLarge ? TagBorderSize.Large : TagBorderSize.Small;
			}

			#endregion

			#region colorApplicationStyle

			var colorApplicationStyle = TagColorApplicationStyle.None;

			if (tagMagnitude.ApplyColorToBackground)
			{
				colorApplicationStyle = TagColorApplicationStyle.ApplyToBackground;
			}

			if (tagMagnitude.ApplyColorToBorder && tagMagnitude.BorderStyle != ZString.Empty)
			{
				colorApplicationStyle = TagColorApplicationStyle.ApplyToBorder;
			}

			#endregion

			AssertionWithHtml.CombineAssertions(() =>
			{
				Assertion.AssertEquals("PK", tagMagnitude.PK.ToGuid(), tagDTO.PK);
				Assertion.AssertEquals("Code", tagMagnitude.TGM_Code, tagDTO.Code);
				Assertion.AssertEquals("Color", colorHex, tagDTO.Color);
				Assertion.AssertEquals("Description", tagMagnitude.Description, tagDTO.Description);
				Assertion.AssertEquals("BorderSize", borderSize, tagDTO.BorderSize);
				Assertion.AssertEquals("BorderStyle", borderStyle, tagDTO.BorderStyle);
				Assertion.AssertEquals("ColorApplicationStyle", colorApplicationStyle, tagDTO.ColorApplicationStyle);
			});
		}

		#endregion

		#region Task

		public static void AssertTasks(IEnumerable<ProcessTask> processTasks, IEnumerable<DTOs.TaskDTO> taskDTOs)
		{
			Assertion.AssertEquals("Number of task", processTasks.Count(), taskDTOs.Count());

			foreach (var processTask in processTasks)
			{
				var taskDTO = taskDTOs.SingleOrDefault(t => t.PK == processTask.PK.ToGuid());
				Assertion.AssertNotNull(taskDTO);

				AssertTask(processTask, taskDTO);
			}
		}

		public static void AssertTask(ProcessTask processTask, DTOs.TaskDTO taskDTO, IEnumerable<Guid> tagPKs = null)
		{
			var tagsPKs = tagPKs ?? Enumerable.Empty<Guid>();
			AssertionWithHtml.CombineAssertions(() =>
			{
				Assertion.AssertEquals("PK", processTask.PK.ToGuid(), taskDTO.PK);
				Assertion.AssertContainsExactElementsInAnyOrder("TagPKs", tagsPKs, taskDTO.TagPKs);
				Assertion.AssertEquals("WorkflowPK", processTask.P9_FH_ProcessHeader.ToGuid(), taskDTO.WorkflowPK);
				Assertion.AssertEquals("CapabilityPK", processTask.P9_G4_RequiredCapability.IsValid ? (Guid?)processTask.P9_G4_RequiredCapability.ToGuid() : null, taskDTO.CapabilityPK);

				Assertion.AssertEquals("task should have 11 properties", 11, taskDTO.Properties.Count);

				Assertion.AssertEquals("type", processTask.P9_Type, taskDTO.Properties["type"]);
				Assertion.AssertEquals("note", processTask.P9_CardNote, taskDTO.Properties["note"]);
				Assertion.AssertEquals("status", processTask.P9_Status, taskDTO.Properties["status"]);
				Assertion.AssertEquals("isStartable", processTask.IsStartable(), taskDTO.Properties["isStartable"]);
				Assertion.AssertEquals("description", processTask.LongDescription, taskDTO.Properties["description"]);
				Assertion.AssertEquals("resourceCode", processTask.AssignedStaffMember?.GS_Code, taskDTO.Properties["resourceCode"]);
				Assertion.AssertEquals("estimateVariationFactor", processTask.P9_EstimateVariationFactor, taskDTO.Properties["estimateVariationFactor"]);
				Assertion.AssertEquals("lowEstimatedMinutes", Utilities.Round(processTask.LowEstimatedDurationHours * 60, 0), taskDTO.Properties["lowEstimatedMinutes"]);
				Assertion.AssertEquals("standardEstimatedMinutes", Utilities.Round(processTask.StandardEstimateHours * 60, 0), taskDTO.Properties["standardEstimatedMinutes"]);
				Assertion.AssertEquals("highEstimatedMinutes", Utilities.Round(processTask.HighEstimatedDurationHours * 60, 0), taskDTO.Properties["highEstimatedMinutes"]);
				Assertion.AssertEquals("estimatedTimeToCompleteMinutes", Utilities.Round(processTask.EstimatedTimeToCompleteHours * 60, 0), taskDTO.Properties["estimatedTimeToCompleteMinutes"]);
			});
		}

		#endregion

		#region Workflow

		public static void AssertWorkflows(IEnumerable<ProcessHeader> processHeaders, IEnumerable<DTOs.WorkflowDTO> workflowDTOs)
		{
			Assertion.AssertEquals("Number of workflows", processHeaders.Count(), workflowDTOs.Count());

			foreach (var processHeader in processHeaders)
			{
				var workflowDTO = workflowDTOs.SingleOrDefault(t => t.PK == processHeader.PK.ToGuid());
				Assertion.AssertNotNull(workflowDTO);

				AssertWorkflow(processHeader, workflowDTO);
			}
		}

		public static void AssertWorkflow(ProcessHeader processHeader, DTOs.WorkflowDTO workflowDTO)
		{
			AssertionWithHtml.CombineAssertions(() =>
			{
				Assertion.AssertEquals("PK", processHeader.PK.ToGuid(), workflowDTO.PK);
				Assertion.AssertEquals("JobPK", processHeader.FH_ParentId.ToGuid(), workflowDTO.JobPK);
				Assertion.AssertEquals("ComponentPK", processHeader.CurrentComponent.PK.ToGuid(), workflowDTO.ComponentPK);
				Assertion.AssertEquals("ParentPK", processHeader.WorkflowParent?.PK.ToGuid(), workflowDTO.ParentPK);

				Assertion.AssertEquals("Workflow should have 2 properties", 2, workflowDTO.Properties.Count);
				Assertion.AssertEquals("status", processHeader.FH_Status, workflowDTO.Properties["status"]);
				Assertion.AssertEquals("description", processHeader.FH_CompletionStatement, workflowDTO.Properties["description"]);
			});
		}

		#endregion

		#region Capability

		public static void AssertCapabilities(IEnumerable<GlbCapability> glbCapabilities, IEnumerable<CapabilityDTO> capabilityDTOs)
		{
			Assertion.AssertEquals("Number of capabilities", glbCapabilities.Count(), capabilityDTOs.Count());

			foreach (var glbCapability in glbCapabilities)
			{
				var taskDTO = capabilityDTOs.SingleOrDefault(t => t.PK == glbCapability.PK.ToGuid());
				Assertion.AssertNotNull(taskDTO);

				AssertCapability(glbCapability, taskDTO);
			}
		}

		public static void AssertCapability(GlbCapability capability, CapabilityDTO capabilityDTO)
		{
			var codeDescription = capability as ICodeDescription;

			AssertionWithHtml.CombineAssertions(() =>
			{
				Assertion.AssertEquals("PK", capability.PK.ToGuid(), capabilityDTO.PK);
				Assertion.AssertEquals("Code", capability.G4_Code, capabilityDTO.Code);
				Assertion.AssertEquals("Name", capability.G4_Description, capabilityDTO.Name);
			});
		}

		#endregion

		#region CustomisedLayouts

		public static void AssertDTOProperties(IDictionary<string, object> expected, IDictionary<string, object> actual)
		{
			var diagnosticInfo = new ZStringBuilder();
			diagnosticInfo.AppendLine();
			diagnosticInfo.AppendLine();
			diagnosticInfo.AppendLine("Expected:");
			int i = 0;

			foreach (var expectedProperty in expected)
			{
				diagnosticInfo.AppendLine($"[{i++}]: {expectedProperty}");
			}

			diagnosticInfo.AppendLine();
			diagnosticInfo.AppendLine("Expected:");
			i = 0;

			foreach (var actualProperty in actual)
			{
				diagnosticInfo.AppendLine($"[{i++}]: {actualProperty}");
			}

			Assertion.AssertContainsExactElementsInAnyOrder($"Property names should match {diagnosticInfo.ToString()}", expected.Select(pair => pair.Key), actual.Select(pair => pair.Key));
			Assertion.AssertContainsExactElementsInAnyOrder($"Property value types should match {diagnosticInfo.ToString()}", expected.Select(pair => pair.Value?.GetType()), actual.Select(pair => pair.Value?.GetType()));
			Assertion.AssertContainsExactElementsInAnyOrder($"Property values should match {diagnosticInfo.ToString()}", expected.Select(pair => pair.Value), actual.Select(pair => pair.Value));
		}

		#endregion
	}
}
