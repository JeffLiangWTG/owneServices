using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;

namespace Enterprise.BufferManagement.Business
{
	class ChangeTimeSlotReassignmentStrategy : TicketReassignmentStrategy
	{
		internal ChangeTimeSlotReassignmentStrategy(TicketReassignmentParameters parameters)
			: base(parameters)
		{
		}

		#region TicketReassignmentStrategy Overrides

		protected override bool CanReassign()
		{
			if (Parameters.DestinationSectionViewModel.IsBuffer && !Env.Security.WorkflowHeadersDragDropInBuffer.IsAllowed)
			{
				Env.Security.WorkflowHeadersDragDropInBuffer.ShowError();

				return false;
			}
			else if (Parameters.DestinationSectionViewModel.IsBucket && !Env.Security.WorkflowHeadersDragDropInBucket.IsAllowed)
			{
				Env.Security.WorkflowHeadersDragDropInBucket.ShowError();

				return false;
			}
			else
			{
				return Parameters.IsMovingWithinChannelToDifferentTimeSlot;
			}
		}

		protected override string GetReassignmentMessage()
		{
			var resourceString = DataBoundResourceStrings.GetDataForProperty(typeof(ProcessHeader), GetPropertyName());
			var propertyInfo = GetRelevantDatePropertyInfo(Parameters.Workflow);

			if (propertyInfo.BizObj is ProcessJobHeader)
			{
				return Res.GetString("f8189ac8-85db-4d15-a1b2-adeac209ba7e", "This will change the {0} of the job-level workflow to {1}.", resourceString.Caption, NewValue);
			}
			else
			{
				return Res.GetString("e2d15e05-ee19-4803-be28-1f2b76ab19da", "This will change the {0} of the workflow to {1}.", resourceString.Caption, NewValue);
			}
		}

		protected override string GetReassignmentCaption()
		{
			return Res.GetString("84479b51-1d57-42ec-8542-cee8f88b1824", "Change Time Slot");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "This is a drag/drop operation controller. There's definitely a GUI attached.")]
		protected override CrossChannelTaskAssignments ShowDialog(string message, string caption)
		{
			var context = CreateContext(caption);
			var result = Globals.Message.ShowOrDefault(context, message); // This is a drag/drop operation controller. There's definitely a GUI attached.

			if (result == ZDialogResult.OK)
			{
				PerformReassignment(null);

				return AllowedReassignmentOptionFlag;
			}
			else
			{
				return CrossChannelTaskAssignments.None;
			}
		}

		DialogDefaultContext CreateContext(string caption)
		{
			var contextText = Parameters.DestinationSectionViewModel.IsBuffer ? "BUFFER" : Parameters.DestinationSectionViewModel.TimeProgressionField;
			var contextBlob = DialogDefaultContext.ToZBlob(contextText);
			var contextGUID = Parameters.DestinationSectionViewModel.IsBuffer ? DialogDefaultContextGUIDForBuffers : DialogDefaultContextGUID;

			return new DialogDefaultContext(contextGUID, caption, ZMessageBoxButtons.OKCancel, ZMessageBoxIcon.Information, contextBlob);
		}

		protected override void PerformReassignment(ProcessTask task)
		{
			var workflowsAffected = new ProcessHeader[] { Parameters.Workflow };
			if (Parameters.ShowJobCards)
			{
				var jobHeader = Parameters.Workflow.JobHeader;
				workflowsAffected = jobHeader.ProcessHeaders.Where(h => Parameters.SourceSectionViewModel.AllShownComponentPKs.Contains(h.FH_FC_CurrentComponent)).ToArray();
			}

			foreach (ProcessHeader workflow in workflowsAffected)
			{
				var propertyInfo = GetRelevantDatePropertyInfo(workflow);

				if (NewValue.IsValid)
				{
					propertyInfo.Value = NewValue;
				}
			}
		}

		protected override bool ShouldReturnTicketToOriginalLocation()
		{
			return true;
		}

		protected override CrossChannelTaskAssignments GetAllowedReassignmentOptionFlags()
		{
			return AllowedReassignmentOptionFlag;
		}

		const CrossChannelTaskAssignments AllowedReassignmentOptionFlag = CrossChannelTaskAssignments.WorkflowBetweenTimeSlot;

		#endregion

		#region Implementation

		static ZGuid DialogDefaultContextGUID => new ZGuid("a1f3ce29-5c13-4fc7-a414-87f3db06a6c7");
		static ZGuid DialogDefaultContextGUIDForBuffers => new ZGuid("2c8b56e4-2f04-4394-868a-cd64a5d40e0c");

		string GetPropertyName()
		{
			switch (Parameters.DestinationSectionViewModel.TimeProgressionField)
			{
				case TimeProgressionFieldList.Codes.AgreedDeliveryDate:
					return nameof(ProcessHeader.AgreedDeliveryDateLocal);

				case TimeProgressionFieldList.Codes.DoNotStartBeforeDate:
					return nameof(ProcessHeader.DoNotStartBeforeDateLocal);

				case TimeProgressionFieldList.Codes.TransferTime:
					return nameof(ProcessHeader.LastTransferDateLocal);

				default:
					throw new InvalidOperationException("Invalid TimeProgressionField: " + Parameters.DestinationSectionViewModel.TimeProgressionField);
			}
		}

		ZDateTime NewValue
		{
			get
			{
				if (_newValue == null)
				{
					var workflow = Parameters.Workflow;
					var section = workflow.Factory.Load<BMBoardSection>(Parameters.DestinationSectionViewModel.SectionPK);
					var context = WorkingTimeContext.Create(section);
					var workTimeArithmetic = context.GetWorkTimeArithmetic(workflow.Factory);
					var now = context.GetCurrentLocalTime(workflow.Factory);
					var timeIndex = Parameters.DestinationCell.TimeIndex + 0.5; // The 0.5 places it halfway between slots to avoid a few seconds passing and the ticket moving to the next slot.

					var workflowComponent = workflow.CurrentComponent;
					var timePerCell = workflowComponent.IsBuffer
						? ComponentGridHelper.GetTimePerCellForBuffer(section.SectionConfiguration, workflowComponent).ToTimeSpan() // The workflow could be in an additional component rather than the primary component, and they could have different time spans.
						: Parameters.DestinationCell.TimeInCell;

					var timeDifferenceHoursFromFirstSlot = timeIndex * timePerCell.TotalHours;

					if (Parameters.DestinationSectionViewModel.TimeProgressionMode == TimeProgressionModeList.Codes.Age)
					{
						timeDifferenceHoursFromFirstSlot *= -1;

						if (section.SectionConfiguration.IsBuffer)
						{
							timeDifferenceHoursFromFirstSlot += workflow.CalculatePenetrationMinutesWithoutAging() / 60.0;
						}
					}

					_newValue = workTimeArithmetic.GetDateTimeInWorkingHoursFutureOrPast(now.ToDateTime(), timeDifferenceHoursFromFirstSlot);
				}

				return _newValue.Value;
			}
		}

		ZDateTime? _newValue;

		ZPropertyInfo GetRelevantDatePropertyInfo(ProcessHeader workflow)
		{
			var field = Parameters.DestinationSectionViewModel.TimeProgressionField;
			var directPropertyInfo = GetDatePropertyInfo(workflow, field);

			if (!directPropertyInfo.Value.IsValid && workflow.IsWorkflow)
			{
				var jobHeaderPropertyInfo = GetDatePropertyInfo(workflow.JobHeader, field);

				if (jobHeaderPropertyInfo.Value.IsValid)
				{
					return jobHeaderPropertyInfo;
				}
			}

			return directPropertyInfo;
		}

		static ZPropertyInfo GetDatePropertyInfo(ProcessHeader workflow, string timeProgressionField)
		{
			switch (timeProgressionField)
			{
				case TimeProgressionFieldList.Codes.AgreedDeliveryDate:
					return workflow.AgreedDeliveryDateLocalInfo;

				case TimeProgressionFieldList.Codes.DoNotStartBeforeDate:
					return workflow.DoNotStartBeforeDateLocalInfo;

				case TimeProgressionFieldList.Codes.TransferTime:
					return workflow.LastTransferDateLocalInfo;

				default:
					throw new InvalidOperationException("Cannot drag/drop when TimeProgressionField is " + timeProgressionField);
			}
		}

		#endregion
	}
}
