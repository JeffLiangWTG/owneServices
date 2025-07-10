using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BMNCNShapeValidation : AutoBMNCNShapeValidation
	{
		public BMNCNShapeValidation(AutoBMNCNShape parent)
			: base(parent)
		{
		}

		new BMNCNShape Parent
		{
			get { return (BMNCNShape)base.Parent; }
		}

		protected override void CheckBNS_Name()
		{
			base.CheckBNS_Name();

			MandatoryValidation.CheckEntered(Parent.BNS_NameInfo);
		}

		protected override void CheckBNS_ShapeType()
		{
			base.CheckBNS_ShapeType();
			ListValidation.ErrorIfInvalidCode(Parent.BNS_ShapeTypeInfo);
		}

		protected override void CheckBNS_JobType()
		{
			base.CheckBNS_JobType();
			ListValidation.ErrorIfInvalidCode(Parent.BNS_JobTypeInfo);
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			return info.Name != BMNCNShapeSchema.Constants.BNS_RelatedEntityID;
		}

		public void ValidateActive()
		{
			ValidateCalculatedProperty(Parent.ActiveInfo);
		}

		protected void CheckActive()
		{
			if (Parent.IsBufferShape && !Parent.Active)
			{
				Parent.ActiveInfo.AddError(BufferNotAcceptedErrorMessage);
			}
		}

		public static string BufferNotAcceptedErrorMessage => Res.GetString("49d6b136-ba9e-41fa-ae07-81561c784c1b", "This buffer has not been accepted yet. Please accept the buffer or hide it from the diagram.");

		#region Date Violations

		internal void ValidateDoNotStartBeforeDateViolation()
		{
			var doNotStartBeforeOnWorkflow = false;
			var doNotStartBeforeOnJob = false;

			if (Parent.ScheduledStartTimeUtc.IsValid)
			{
				var processHeader = Parent.ProcessHeader;
				if (processHeader != null && processHeader.ApplicableDoNotStartBeforeDateUtc.IsValid && processHeader.ApplicableDoNotStartBeforeDateUtc > Parent.ScheduledStartTimeUtc)
				{
					doNotStartBeforeOnWorkflow = processHeader.IsWorkflow;
					doNotStartBeforeOnJob = !doNotStartBeforeOnWorkflow;
				}
			}

			Parent.ToggleRowMessageErrorSafe(doNotStartBeforeOnWorkflow, DoNotStartBeforeDateOnWorkflowViolationMessage);
			Parent.ToggleRowMessageErrorSafe(doNotStartBeforeOnJob, DoNotStartBeforeDateOnJobViolationMessage);
		}

		static string DoNotStartBeforeDateOnWorkflowViolationMessage
		{
			get { return Res.GetString("22e5e08e-d465-4b43-a334-9f3a13c4da9b", "This shape is scheduled to start before the Earliest Start Date of its workflow. This could lead to delays."); }
		}

		static string DoNotStartBeforeDateOnJobViolationMessage
		{
			get { return Res.GetString("8a35b459-0001-49f4-a9b0-b2531c7e5d3f", "This shape is scheduled to start before the Earliest Start Date of its job. This could lead to delays."); }
		}

		public void ValidateScheduledFinishTimeUtc()
		{
			ValidateCalculatedProperty(Parent.ScheduledFinishTimeUtcInfo);
		}

		protected void CheckScheduledFinishTimeUtc()
		{
			if (Parent.ScheduledFinishTimeUtc.IsValid &&
				Parent.ScheduledFinishTimeUtc < Parent.ScheduledStartTimeUtc)
			{
				Parent.ScheduledFinishTimeUtcInfo.AddError(Res.GetString("570E850B-4EB3-464A-9112-17DDFF89C528", "Scheduled Finish Date cannot be less than Scheduled Start Date."));
			}
			ValidateScheduledStartTimeUtc();
		}

		public void ValidateScheduledStartTimeUtc()
		{
			ValidateCalculatedProperty(Parent.ScheduledStartTimeUtcInfo);
		}

		protected void CheckScheduledStartTimeUtc()
		{
			if (Parent.ScheduledStartTimeUtc.IsValid &&
				Parent.ScheduledStartTimeUtc > Parent.ScheduledFinishTimeUtc)
			{
				Parent.ScheduledStartTimeUtcInfo.AddError(Res.GetString("FD0903BA-D6D7-4746-9523-E8259B476E8A", "Scheduled Start Date cannot be greater than Scheduled Finish Date."));
			}
			ValidateScheduledFinishTimeUtc();
		}

		#endregion

		#region LinkedDiagramSchedulePropagation

		public void ValidateLinkedDiagramSchedulePropagation()
		{
			var shouldHaveMessage = Parent.ShouldPropagateScheduleToLinkedDiagramNow();
			var message = Res.GetString("633eec4c-7390-4064-b83c-08d3a99b78e0", "When saving this diagram, the Scheduled Start and Scheduled Finish of this shape will be propagated to the diagram to which this shape is linked.");

			Parent.ToggleRowMessageErrorSafe(shouldHaveMessage, message);

			var shouldHavePinnedShapesWarning = Parent.PropagationWouldChangePinnedShapeSchedules();
			var warning = Res.GetString("616efcea-a082-4df0-9a2c-1d388b449dbd", "The linked diagram has one or more pinned shapes. When propagating schedules, the pinned shapes will remain in the same position, but the Scheduled Start time will change. Please ensure this is appropriate and consider adjusting the pinned shape positions after the schedules have propagated.");

			Parent.ToggleRowWarningSafe(shouldHavePinnedShapesWarning, warning);
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateActive();
			ValidateDoNotStartBeforeDateViolation();
			ValidateScheduledFinishTimeUtc();
			ValidateScheduledStartTimeUtc();
			ValidateLinkedDiagramSchedulePropagation();
		}
	}
}
