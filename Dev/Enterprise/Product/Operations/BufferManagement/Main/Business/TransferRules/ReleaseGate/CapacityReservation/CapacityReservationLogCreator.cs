using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	/// <summary>
	/// Creates log messages used for informing people about the result of an attempted workflow release. This information is shown to users in various places in the user interface, so should be translated.
	/// Please round numbers to 2 decimal places, and use FormatToTwoDecimalPlaces extension method to avoid redundant trailing zeroes.
	/// </summary>
	static class CapacityReservationLogCreator
	{
		internal static string CreateLogMessageForReport(CapacityReservationReport report, BMComponent buffer, bool wasWorkflowReleased, bool includeOtherResourcesConsideredMessage = false)
		{
			var message = new StringBuilder();

			if (wasWorkflowReleased)
			{
				message.Append(Res.GetString("0e86cff4-39da-4fd1-bd13-6dd7aa424d3c", "Released [{0}] into [{1}] at {2} UTC.",
					/*0*/ report.WorkflowBeingConsideredForRelease.Description,
					/*1*/ buffer.FC_Name,
					/*2*/ ZDateTime.UtcNow.ToLongTimeString()));
				message.Append(" ");
			}

			message.Append(GetReleaseTypeMessage(report));

			foreach (var line in report.ResourceCapacityReportLines.OrderByDescending(l => l.TotalCapacityHoursRequired).ThenBy(l => l.Resource.GS_FullName))
			{
				message.AppendLine();
				message.Append('\t');

				AppendResourceCapacityLineDetails(message, line, includeOtherResourcesConsideredMessage);
			}

			foreach (var line in report.CapabilityTasksWithNoResourcesReportLines.OrderByDescending(l => l.CapacityHoursRequired).ThenBy(l => l.Capability.G4_Description))
			{
				message.AppendLine();
				message.Append('\t');

				AppendCapabilityWithNoResourcesLineDetails(message, line);
			}

			return message.ToString();
		}

		internal static string CreateLogMessageForReportLine(CapacityReservationReportLine line)
		{
			var availableCapacity = line.AvailableCapacityAtThisPointInQueue_ForDisplayOnly;
			var remainingCapacity = availableCapacity - line.TotalCapacityHoursRequired;

			var message = new StringBuilder();
			message.Append(GetReleasedType(line));
			message.Append(", ");
			message.Append(GetDescription(line.Report.WorkflowBeingConsideredForRelease));
			message.Append(", ");

			message.Append(Res.GetString("cd1e59a7-3ec2-42ad-adb8-46ec3c58505f", "Available Capacity: {0} hours", availableCapacity.FormatWithNoMoreThanTwoDecimalPlaces()));
			message.Append(", ");

			message.Append(Res.GetString("f638b085-97ed-4eda-8354-e365c60c9c37", "Reservation: {0} hours", line.TotalCapacityHoursRequired.FormatWithNoMoreThanTwoDecimalPlaces()));
			message.Append(", ");

			if (line.Report.WorkflowInvolvesNominatedCCR && !line.IsNominatedCCRInConstrainedModeReleaseGroup)
			{
				message.Append(Res.GetString("e14363e9-e7f4-4b71-a404-b19dd794f7a4", "Remaining Capacity: {0} hours (for work involving a CCR)", remainingCapacity.FormatWithNoMoreThanTwoDecimalPlaces()));
			}
			else
			{
				message.Append(Res.GetString("a20b20e2-6782-4dd8-a4af-193fecba3b6c", "Remaining Capacity: {0} hours", remainingCapacity.FormatWithNoMoreThanTwoDecimalPlaces()));
			}

			if (line.OtherResourcesConsidered.Any())
			{
				message.Append(",");
				ApppendOtherResourcesConsideredMessage(line, message);
			}

			return message.ToString();
		}

		#region Implementation

		public static string GetManuallyReleasedMessage()
		{
			return Res.GetString("fdd0db6b-3171-4e21-89f0-79a99f74238a", "It was manually released by [{0}].", GlbStaff.CurrentUser.GS_FullName);
		}

		public static string GetNonReleaseGateTransferMessage()
		{
			return Res.GetString("e1b8286e-36f3-45e3-b66c-9466c9c64911", "It was moved into the component without considering capacity.");
		}

		static string GetReleaseTypeMessage(CapacityReservationReport report)
		{
			switch (report.BufferReleaseOutcome)
			{
				case BufferReleaseOutcome.BlockedByMissingResource:
					return Res.GetString("872cbdcb-8672-4574-af30-118710f534f1", "Cannot release this workflow as one or more tasks are assigned to the following staff members which cannot be found: {0}", string.Join(", ", report.MissingResourceCodes));

				case BufferReleaseOutcome.BlockedByResourceCapacity:
					return Res.GetString("4e822dc4-1f07-4835-a9de-791bef44aacf", "There is insufficient capacity to release this workflow. The required capacity has been reserved within this Release Gate run to prevent other workflows later in the queue jumping in front of this one inappropriately. Details of relevant resource capacity are listed below.");

				case BufferReleaseOutcome.BlockedByResourceLeave:
					return Res.GetString("13ebdd19-3488-46ac-97d8-619a31219088", "One or more resources are on leave, and work cannot yet be released to them. Details of relevant resources are listed below.");

				case BufferReleaseOutcome.Releasable:
					return Res.GetString("c0d2d8f3-56ce-45fd-94bd-ed14b5a6bbba", "It was considered releasable based on the capacity of the resources listed below.");

				case BufferReleaseOutcome.ReleasableByCCPMSchedule:
					return Res.GetString("6dce52bc-ce58-4bf3-b602-ac3626b2ffb0", "It was considered releasable based on its approved CCPM schedule. Details of relevant resource capacity are listed below.");

				case BufferReleaseOutcome.ManuallyReleased:
					return GetManuallyReleasedMessage();

				case BufferReleaseOutcome.NonReleaseGateTransfer:
					return GetNonReleaseGateTransferMessage();

				case BufferReleaseOutcome.BlockedByWorkflowEmptiness:
					return Res.GetString("f8d3b57f-c0f3-4287-ab35-7a347c7975a3", "There are no assigned tasks in this workflow.");

				default:
					throw new ArgumentException(FormattableString.Invariant($"BufferReleaseOutcome [{report.BufferReleaseOutcome}] is invalid"));
			}
		}

		static void AppendResourceCapacityLineDetails(StringBuilder message, CapacityReservationReportLine line, bool includeOtherResourcesConsideredMessage)
		{
			var fullName = line.Resource.GS_FullName;
			var hoursRequired = line.TotalCapacityHoursRequired.FormatWithNoMoreThanTwoDecimalPlaces();
			var availableCapacity = line.AvailableCapacityAtThisPointInQueue_ForDisplayOnly.FormatWithNoMoreThanTwoDecimalPlaces();
			var placeInQueue = line.PlaceInQueue != null ? NumberFormatter.ToOrdinalString(line.PlaceInQueue.Value) : null;

			if (line.ResourceUsedNonCCRTemporaryOverloadMultiplier)
			{
				if (line.PlaceInQueue != null)
				{
					message.Append(Res.GetString("507ff24e-aca4-4aef-b5d7-998ccd6796cb", "{0}: required {1} hours, currently has {2} hours of available capacity (for work involving a CCR) at {3} place in the queue for this resource.",
						fullName, hoursRequired, availableCapacity, placeInQueue));
				}
				else
				{
					message.Append(Res.GetString("29649c6e-aece-49c5-84b5-4ed715d4f35d", "{0}: required {1} hours, currently has {2} hours of available capacity (for work involving a CCR).",
						fullName, hoursRequired, availableCapacity));
				}
			}
			else
			{
				if (line.PlaceInQueue != null)
				{
					message.Append(Res.GetString("391b4376-2e65-456c-b7a7-6fe36f4cf685", "{0}: required {1} hours, currently has {2} hours of available capacity at {3} place in the queue for this resource.",
						fullName, hoursRequired, availableCapacity, placeInQueue));
				}
				else
				{
					message.Append(Res.GetString("e19ca417-fa66-4482-a0a0-14cd2be7a2b0", "{0}: required {1} hours, currently has {2} hours of available capacity.",
						fullName, hoursRequired, availableCapacity));
				}
			}

			if (line.ThisResourcesLeaveBlocksWorkflowRelease)
			{
				message.Append(" ");
				message.Append(Res.GetString("e69382e4-0112-49c2-b342-9e6884fc4f7b", "On leave until {0} UTC. The earliest work can be released is {1} UTC. This is determined using the {2} registry item.",
					line.ResourceReturningFromLeaveTime.ToBestReadableDateTimeString(),
					line.EarliestTimeWorkCanBeReleasedToResource.ToBestReadableDateTimeString(),
					BMSRegistry.Instance.ResourceLeaveWindowforReleasingWork.Caption));
			}

			if (includeOtherResourcesConsideredMessage)
			{
				ApppendOtherResourcesConsideredMessage(line, message);
			}

			if (line.DirectlyAssignedTasks.Count > 0)
			{
				message.AppendLine();
				message.Append("\t\t");
				message.Append(Res.GetString("a7a10501-3aa3-4096-a9cb-ecf93fb18af4", "Tasks assigned:") + " ");
				message.Append(GetTaskNumberAndEstimateText(line.DirectlyAssignedTasks));
			}

			if (line.CapabilityTasks.Count > 0)
			{
				foreach (var group in line.CapabilityTasks.GroupBy(t => t.RequiredCapabilityName))
				{
					message.AppendLine();
					message.Append("\t\t");
					message.Append(Res.GetString("de2402f0-b40d-48b6-9358-d4b2a603dba5", "Tasks requiring capability [{0}]:", group.Key) + " ");
					message.Append(GetTaskNumberAndEstimateText(group));
				}
			}
		}

		static void AppendCapabilityWithNoResourcesLineDetails(StringBuilder message, MissingCapabilityCapacityReservationReportLine line)
		{
			message.Append(Res.GetString("d3c9ac15-f2e2-4208-b597-2a81b7d98b61", "Capability [{0}]: required {1} hours, but there are currently no working resources who possess this capability.",
				/*0*/ line.Capability.G4_Description,
				/*1*/ line.CapacityHoursRequired.FormatWithNoMoreThanTwoDecimalPlaces()
				));

			foreach (var group in line.CapabilityTasks.GroupBy(t => t.RequiredCapabilityName))
			{
				message.AppendLine();
				message.Append("\t\t");
				message.Append(Res.GetString("147e1f8f-e9b2-4cad-a894-418543e0b19a", "Tasks requiring this capability:") + " ");
				message.Append(GetTaskNumberAndEstimateText(group));
			}
		}

		static void ApppendOtherResourcesConsideredMessage(CapacityReservationReportLine line, StringBuilder message)
		{
			if (line.OtherResourcesConsidered.Any())
			{
				message.Append(" ");
				message.Append(Res.GetString("777A1365-8F96-4C50-9841-5B2BA2F29144", "Other resources considered: {0}.", string.Join(", ", line.OtherResourcesConsidered.Select(r => r.GS_FullName).OrderBy(s => s))));
			}
		}

		static string GetTaskNumberAndEstimateText(IEnumerable<TaskLogDetails> tasks)
		{
			return string.Join(", ", tasks.Select(t => FormattableString.Invariant($"{t.TaskID} ({Utilities.Round(t.StandardEstimateHours, 2).FormatWithNoMoreThanTwoDecimalPlaces()} {TimeConstants.TimeStrings.Hours})"))); // There's nothing to translate on this line.
		}

		static string GetReleasedType(CapacityReservationReportLine line)
		{
			var wasReleased = line.Report.WasWorkflowReleased;
			var didThisResourceBlockRelease = !wasReleased && line.ThisResourceBlocksWorkflowRelease;

			return wasReleased ? Res.GetString("71e53c55-2300-4e7f-99aa-e4cc580c2a47", "RELEASED")
				: didThisResourceBlockRelease ? Res.GetString("e0001009-3684-415c-b4ba-6ac4bd3051a7", "BLOCKED")
				: Res.GetString("f15a7e87-0046-4ab9-962e-f2c7583c35ff", "PASSED (but not released)");
		}

		static string GetDescription(IWorkflow workflow)
		{
			return FormattableString.Invariant($"{workflow.JobDescription} - {workflow.Description}"); // There's nothing to translate on this line.
		}

		#endregion
	}
}
