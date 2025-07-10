using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class ShapeNetworkEntityValidation : ZValidation
	{
		public ShapeNetworkEntityValidation(ShapeNetworkEntity parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly ShapeNetworkEntity parent;
		ShapeNetworkEntity Parent => parent;

		public override Type AutoValidationType
		{
			get { return typeof(ShapeNetworkEntity); }
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();

			ValidateName();
			ValidateScrollPosition();
			ValidateScale();
			ValidateResolutionIncrement();
			ValidateAgreedDeliveryDateViolation();
			ValidateScheduleDateViolations();
			ValidateIsCyclic();

			if (BMSRegistry.Instance.AutomaticallyValidateWorkflowLoopsOnSave.Value)
			{
				ValidateProcessHeaderLoops();
			}

			ValidateLevelingRuleViolations();
			ValidateLinkedDiagramConflictsFound();
			ValidateShouldShowNonScheduledSection();
		}

		public void ValidateName()
		{
			ValidateCalculatedProperty(Parent.NameInfo);
			ValidateBackInTimeHiddenDependencies();
		}

		protected void CheckName()
		{
			if (Parent.Shape.IsBufferShape)
			{
				if (Parent.Root.Descendants(new ShapeNetworkEntityDescendantsStrategy()).Select(s => s.Shape).Where(s => s.IsBufferShape && string.Equals(s.BNS_Name, Parent.Name, StringComparison.OrdinalIgnoreCase)).Take(2).Count() == 2)
				{
					Parent.NameInfo.AddError(Res.GetString("638c1328-9a28-4b74-a85b-b44a1f0f6122", "There is another buffer with the same name in this diagram."));
				}
			}
		}

		public void ValidateScrollPosition()
		{
			ValidateCalculatedProperty(Parent.ScrollPositionInfo);
		}

		protected void CheckScrollPosition()
		{
			if (!Parent.ScrollPositionInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.ScrollPositionInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ScrollPositionInfo);
			}
		}

		public void ValidateResolutionIncrement()
		{
			ValidateCalculatedProperty(Parent.ResolutionIncrementInfo);
		}

		protected void CheckResolutionIncrement()
		{
			if (Parent.IsScaled && Parent.IsRoot)
			{
				MandatoryValidation.CheckEntered(Parent.ResolutionIncrementInfo);
				TypeValidation.CheckValidZDateTimeWithoutRange(Parent.ResolutionIncrementInfo);

				if (Parent.ResolutionIncrement.GetMinutesFromDateTimeSpan() <= 0)
				{
					Parent.ResolutionIncrementInfo.AddError(Res.GetString("AA1950B8-F167-4316-9AEE-64F8B4ACEB45", "Please enter a Resolution Increment greater than 0."));
				}
			}
		}

		public void ValidateScale()
		{
			ValidateCalculatedProperty(Parent.ScaleInfo);
		}

		protected void CheckScale()
		{
			if (Parent.IsScaled && Parent.IsRoot)
			{
				MandatoryValidation.CheckEntered(Parent.ScaleInfo);
				TypeValidation.CheckValidZDateTimeWithoutRange(Parent.ScaleInfo, Res.GetString("46cdcf18-9914-4179-a01d-3976c1e71374", "Scale. The required format is h:mm"));

				if (Parent.Scale.GetMinutesFromDateTimeSpan() <= 0)
				{
					Parent.ScaleInfo.AddError(Res.GetString("81B66A02-5DC5-455E-B9F8-C14FA84C9395", "Please enter a Scale greater than 0."));
				}
			}
		}

		public void ValidateShouldShowNonScheduledSection()
		{
			if (Parent.IsScaled && Parent.ShapeAsRootDiagram != null && !Parent.ShouldShowNonScheduledSection)
			{
				Parent.ToggleRowErrorSafe(Parent.Network.Shapes.Any(x => x.IsNonScheduled), DiagramHasNonScheduledShapes);
			}
		}

		#region Relationship Validations

		internal void ValidateRelationships()
		{
			ValidateBackInTimeHiddenDependencies();
			ValidateBackInTimeArrows();
		}

		public void ValidateBackInTimeHiddenDependencies()
		{
			if (Parent.Owner == null && Parent.IsScaled)
			{
				var descendants = DescendantsWithParent().Where(t => t.RelatedEntityPK.IsValid).Distinct().ToArray();

				var shapesThatNeedWarnings = new HashSet<ShapeNetworkEntity>();

				var descendantsByWorkflow = new Dictionary<ZGuid, ShapeNetworkEntity>();
				foreach (var shape in descendants)
				{
					ShapeNetworkEntity existingShape;
					if (descendantsByWorkflow.TryGetValue(shape.RelatedEntityPK, out existingShape))
					{
						descendantsByWorkflow[shape.RelatedEntityPK] = shape.X + shape.Width > existingShape.X + existingShape.Width ? shape : existingShape;
					}
					else
					{
						descendantsByWorkflow[shape.RelatedEntityPK] = shape;
					}
				}

				foreach (ProcessHeaderLink link in descendantsByWorkflow.Values.SelectMany(d => d.HiddenRelationships))
				{
					var fromShape = descendantsByWorkflow.ContainsKey(link.FP_FH_HeaderFrom) ? descendantsByWorkflow[link.FP_FH_HeaderFrom] : null;
					var toShape = descendantsByWorkflow.ContainsKey(link.FP_FH_HeaderTo) ? descendantsByWorkflow[link.FP_FH_HeaderTo] : null;

					if (fromShape != null && toShape != null && fromShape.RelatedEntityPK != toShape.RelatedEntityPK && fromShape.X + fromShape.Width > toShape.X)
					{
						shapesThatNeedWarnings.UnionWith(new[]
						{
							fromShape, toShape
						});
					}
				}

				foreach (var shape in descendantsByWorkflow.Values)
				{
					shape.ToggleRowWarningSafe(shapesThatNeedWarnings.Contains(shape), BackInTimeHiddenDependencyMessage);
				}
			}
		}

		IEnumerable<ShapeNetworkEntity> DescendantsWithParent()
		{
			return Parent.Descendants().Append(Parent).Cast<ShapeNetworkEntity>().Where(s => s.RelatedEntityPK != ZGuid.Empty);
		}

		internal void ValidateBackInTimeArrows()
		{
			foreach (var arrow in Parent.DependencyAttachments)
			{
				arrow.Validation.ValidateBackInTimeArrows();
			}
		}

		internal void ValidateAgreedDeliveryDateViolation()
		{
			var agreedDeliveryDateViolationWarning = false;
			var agreedDeliveryViolationOnWorkflow = false;
			var agreedDeliveryViolationOnJob = false;

			var sheduledFinishTime = Parent.Shape.ScheduledFinishTimeUtc;
			if (sheduledFinishTime.IsValid)
			{
				var processHeader = Parent.ProcessHeader;
				if (processHeader != null && processHeader.ApplicableAgreedDeliveryDateUtc.IsValid)
				{
					if (processHeader.ApplicableAgreedDeliveryDateUtc < sheduledFinishTime)
					{
						agreedDeliveryDateViolationWarning = true;
					}
					else if (Parent.Network.Shapes.Any(s => s.IsBufferShape)) // This doesn't need to be O(n).
					{
						var shapeOffsetPlusReasonableBufferPenetration = CCPMConstants.ReasonableCompletionBufferPenetrationPercentage * (Parent.X + Parent.Width);
						var offsetConverter = new ShapeOffsetToDateConverter(Parent.Root, WorkingTimeContext.Create(Parent.Shape), Parent.Factory);
						var timeSinceProjectStart = offsetConverter.GetTimeInUtcForScalePosition(shapeOffsetPlusReasonableBufferPenetration);

						if (timeSinceProjectStart.IsValid && timeSinceProjectStart > processHeader.ApplicableAgreedDeliveryDateUtc)
						{
							agreedDeliveryViolationOnWorkflow = processHeader.IsWorkflow;
							agreedDeliveryViolationOnJob = !agreedDeliveryViolationOnWorkflow;
						}
					}
				}
			}

			Parent.ToggleRowWarningSafe(agreedDeliveryDateViolationWarning, AgreedDeliveryDateViolationMessage);
			Parent.ToggleRowMessageErrorSafe(agreedDeliveryViolationOnWorkflow, AgreedDeliveryDateOnWorkflowPlusBufferViolationMessage);
			Parent.ToggleRowMessageErrorSafe(agreedDeliveryViolationOnJob, AgreedDeliveryDateOnJobPlusBufferViolationMessage);
		}

		internal void ValidateIsCyclic()
		{
			var schedule = Parent.Schedule;
			Parent.ToggleRowErrorSafe(schedule != null && schedule.IsCyclic, ShapeHasCircularDependencies);
		}

		public void ValidateProcessHeaderLoops()
		{
			var diagramShape = Parent.AsShape().RootShape;

			if (!diagramShape.IsInDatabase && Parent.ProcessHeader != null)
			{
				foreach (var link in Parent.ProcessHeader?.LinksFromOthersToMe_ForBinding)
				{
					using (link.ForceLoopValidation())
					{
						ProcessHeaderLinkCycleValidationHelper.CheckLoop(link,
						loopedWorkflowsHandler: (loopedWorkflows, relationshipDescription) =>
							Parent.AddRowError(Res.GetString("B8789CAA-E534-4840-AF5F-7E7B83FDFE31",
@"A looped {0} presents on the diagram. The following workflows are involved in the loop:
{1}", relationshipDescription, loopedWorkflows)),
						hierarchyRelationshipBetweenDependenciesHandler: () => Parent.AddRowError(Res.GetString("99ED6503-230A-460E-850E-291965135293", "There is a dependency link between workflows that are also involved in a Parent-Child relationship.")));
					}
				}
			}
		}

		#endregion

		#region Schedule Violations

		public void ValidateScheduleDateViolations()
		{
			if (Parent.IsDiagram && Parent.Owner == null && Parent.IsDiagramSurfaceFixed)
			{
				var lastShapeIsAnnotationError = Res.GetString("14a70355-ee96-4b67-b43d-ebd3af298f64", "The rightmost shape in this diagram is an annotation. This is not permitted when entering both a Scheduled Start and Scheduled Finish time. Please move the annotation so the rightmost shape is not an annotation.");
				var shouldHaveLastShapeBeingAnnotationError = Parent.Network.Entities.Any()
					&& Parent.Network.Entities.Cast<ShapeNetworkEntity>().Where(shape => !shape.IsNonScheduled)
					.CollectMaxBy(e => e.X + e.Width)
					.All(s => !s.CanHaveSchedule);

				Parent.ToggleRowError(shouldHaveLastShapeBeingAnnotationError, lastShapeIsAnnotationError, lastShapeIsAnnotationError);

				var diagramWidth = Parent.Width;
				var diagramScheduledFinishTime = Parent.ScheduledFinishTimeLocal;
				var childEntitiesWithSchedules = Parent.Children.Where(e => e.CanHaveSchedule);

				var shapesOutsideFinishTime = childEntitiesWithSchedules
					.Where(entity => !DoesEntityFitWithinProposedSchedule(entity, diagramScheduledFinishTime, diagramWidth))
					.ToList();

				var shouldHaveScheduledFinishDateRowError = shapesOutsideFinishTime.Any();

				var finishDateViolationMessageCore = Res.GetString("A17C08AC-157E-4556-BA48-9CEACFB33DEB", "There are shapes scheduled to finish after the Diagram Scheduled Finish Date:");
				var finishDateViolationMessage = finishDateViolationMessageCore + GetDateViolationMessage(shapesOutsideFinishTime);
				Parent.ToggleRowError(shouldHaveScheduledFinishDateRowError, finishDateViolationMessageCore, finishDateViolationMessage);

				var diagramScheduledStartTime = Parent.ScheduledStartTimeLocal;
				var shapesOutsideStartTime = childEntitiesWithSchedules
					.Where(x => x.ScheduledStartTimeLocal < diagramScheduledStartTime)
					.ToList();

				var shouldHaveScheduledStartDateRowError = shapesOutsideStartTime.Any();

				var startDateViolationMessageCore = Res.GetString("82C2CA87-2668-4EE3-B10A-55D614B6109F", @"There are shapes scheduled to start before the Diagram Scheduled Start Date:");
				var startDateViolationMessage = startDateViolationMessageCore + GetDateViolationMessage(shapesOutsideStartTime);
				Parent.ToggleRowError(shouldHaveScheduledStartDateRowError, startDateViolationMessageCore, startDateViolationMessage);
			}
		}

		static bool DoesEntityFitWithinProposedSchedule(ShapeNetworkEntity entity, ZDateTime proposedDiagramScheduledFinishTime, double diagramWidth)
		{
			var entityFinishTime = entity.ScheduledFinishTimeLocal;

			if (!entityFinishTime.IsValid)
			{
				return entity.X + entity.Width <= diagramWidth;
			}

			return entityFinishTime <= proposedDiagramScheduledFinishTime;
		}

		void ValidateLinkedDiagramConflictsFound()
		{
			if (Parent.IsRoot && Parent.Network.DiagramContainsSchedulingConflicts)
			{
				Parent.AddRowError(LinkedDiagramConflictsValidationError);
			}
		}

		#endregion

		#region Leveling Rules

		public void ValidateLevelingRuleViolations()
		{
			if (Parent.IsScaled)
			{
				foreach (var levelingRule in Parent.AppliedLevelingRuleViolations)
				{
					switch (levelingRule.BNR_Type)
					{
						case LevelingRuleTypeList.Codes.MaximumConcurrentEntities:
							levelingRule.AddRowWarning(Res.GetString("ff74c74c-c358-4395-b4d8-94b320fdede0", @"There are more shapes in this time slot than are allowed by the Leveling Rule named [{0}].", levelingRule.BNR_Name));
							break;

						case LevelingRuleTypeList.Codes.MinimumEntityStartGapSize:
							levelingRule.AddRowWarning(Res.GetString("1fc402d9-754a-41e6-8c40-89b2a66f2830", @"The distance between the start of shapes is too small, as specified in the Leveling Rule named [{0}].", levelingRule.BNR_Name));
							break;

						case LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities:
							levelingRule.AddRowWarning(Res.GetString("2446edcf-2ea1-4a6e-8c2d-592f0acc3af7", @"The distance between the end of one or more shapes and the start of this shape is too small, as specified in the Leveling Rule named [{0}].", levelingRule.BNR_Name));
							break;
					}
				}
			}
		}

		#endregion

		#region Res Strings

		public static string LinkedDiagramConflictsValidationError => Res.GetString("3a437829-80cf-4ffb-9e02-b77440fbc11c", "Could not save the diagram due to scheduling conflicts with linked diagrams. Please correct those conflicts before saving the form.");

		static string GetDateViolationMessage(List<ShapeNetworkEntity> shapesOutsideDateRange)
		{
			return Res.GetString("30460B9D-459A-438B-A589-7CFBD71DCE00", @"{1}{0}.{1}
This leads to scheduling violations and needs to be fixed.", new ZStringBuilder(shapesOutsideDateRange.Select(sh => "- " + sh.Name)).ToStringWithNewLineBetweenAppends(), System.Environment.NewLine);
		}

		static string BackInTimeHiddenDependencyMessage
		{
			get { return Res.GetString("fa13b4ba-f1ca-4f38-a33f-1cd0d1a64126", "There is a hidden dependency involving this shape that would cause the post-requisite to be scheduled to begin before its pre-requisite is scheduled to complete."); }
		}

		static string AgreedDeliveryDateViolationMessage
		{
			get { return Res.GetString("28568ff8-04bd-46f1-b4a5-6833fe3046c3", "This shape is scheduled to finish after the Agreed Delivery Date of its job. This would require completion with no buffer consumption."); }
		}

		static string AgreedDeliveryDateOnWorkflowPlusBufferViolationMessage
		{
			get { return Res.GetString("e524f47e-d8c9-49ce-8b80-6c07be552d34", "This shape's scheduled finish plus reasonable buffer consumption is after the Agreed Delivery Date of its workflow. This could lead to scheduling conflict."); }
		}

		static string AgreedDeliveryDateOnJobPlusBufferViolationMessage
		{
			get { return Res.GetString("10064972-3864-4408-8237-44bf554d40f8", "This shape's scheduled finish plus reasonable buffer consumption is after the Agreed Delivery Date of its job. This could lead to scheduling conflict."); }
		}

		static string ShapeHasCircularDependencies
		{
			get { return Res.GetString("066fdaa0-1db1-4853-95f6-d97b364cd08a", "This shape has a circular dependency"); }
		}

		static string DiagramHasNonScheduledShapes => Res.GetString("82b92065-85ae-4483-9c4d-dae6f70f7f8f", "Unable to hide Non-Scheduled section, as there are non-scheduled items on the diagram");

		#endregion
	}
}
