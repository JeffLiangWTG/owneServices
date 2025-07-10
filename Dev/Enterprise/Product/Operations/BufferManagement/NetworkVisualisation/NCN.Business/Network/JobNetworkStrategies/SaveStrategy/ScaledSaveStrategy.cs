using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	class ScaledSaveStrategy : JobNetworkSaveStrategy
	{
		public ScaledSaveStrategy(JobNetwork network)
			: base(network)
		{
		}

		protected override void OnBeforeSaveCore()
		{
			base.OnBeforeSaveCore();

			Network.RefreshSchedules();
			UpdateBufferPenetration();
			PropagateLinkedDiagramSchedules();
		}

		void UpdateBufferPenetration()
		{
			var root = Network.DiagramShape;

			if (root != null && root.HasChanges)
			{
				var context = WorkingTimeContext.Create(root);

				foreach (var bufferShape in Network.Shapes.OfType<BMNCNBufferShape>())
				{
					bufferShape.UpdateBufferPenetration(context);
				}
			}
		}

		void PropagateLinkedDiagramSchedules()
		{
			Network.DiagramContainsSchedulingConflicts = false;

			var diagramShape = Network.DiagramShape as BMNCNRootDiagramShape;

			if (diagramShape != null) // Maybe we've opened a shape within a diagram in its own form.
			{
				var shapesRequiringSchedulePropagation = GetShapeSetsRequiringSchedulePropagation(diagramShape);

				if (shapesRequiringSchedulePropagation.Any())
				{
					var numberOfShapesToPropagate = shapesRequiringSchedulePropagation.Sum(collection => collection.Count);
					var reporterProvider = Network.Controller?.UserInteractionImplementor?.ProgressReporterProvider;
					var scheduleCache = GetShapeScheduleCache(shapesRequiringSchedulePropagation.First.Value);

					using (var progressReporter = reporterProvider?.CreateProgressReporter(Res.GetString("7474c470-f4c7-4843-b9e9-0c635a8dad5b", "Propagating scheduling changes to linked diagrams"), numberOfShapesToPropagate))
					{
						foreach (var collection in shapesRequiringSchedulePropagation)
						{
							foreach (var shape in collection)
							{
								progressReporter?.ReportOneItemProcessed();

								if (!TryPropagateSchedule(shape, scheduleCache))
								{
									Network.DiagramContainsSchedulingConflicts = true;
									return;
								}
							}
						}
					}
				}
			}
		}

		#region Determining Diagrams for Propagation

		/// <summary>
		/// Gets the shapes which are linked to diagrams that need to have their schedules propagated from the linked shape.
		/// </summary>
		/// <returns>A list of collections, where the outer list is the sequence in which schedules should be propagated, so that the required schedule data is available.</returns>
		static LinkedList<ICollection<BMNCNShape>> GetShapeSetsRequiringSchedulePropagation(BMNCNRootDiagramShape diagramShape)
		{
			var result = new LinkedList<ICollection<BMNCNShape>>();

			void AddShapesRequringPropagationRecursively(IEnumerable<BMNCNRootDiagramShape> diagrams, HashSet<BMNCNRootDiagramShape> visitedDiagrams, bool assumeSchedulesWillChange)
			{
				var shapes = diagrams.SelectMany(diagram => GetShapesRequiringPropagation(diagram, assumeSchedulesWillChange)).ToList();
				var shapesRequiringPropagation = new List<BMNCNShape>();
				var linkedDiagramsRequiringPropagation = new List<BMNCNRootDiagramShape>();

				foreach (var shape in shapes.ToArray())
				{
					var linkedDiagram = shape.LinkedDiagram;

					if (linkedDiagram != null)
					{
						if (visitedDiagrams.Add(linkedDiagram))
						{
							shapesRequiringPropagation.Add(shape);
							linkedDiagramsRequiringPropagation.Add(linkedDiagram);
						}
						else if (!AskUserIfTheyWantToProceedDespiteCircularLinkedDiagrams(shape))
						{
							throw new UserCancelledPropagationException();
						}
					}
				}

				if (shapesRequiringPropagation.Any())
				{
					result.AddLast(shapesRequiringPropagation);

					// We assume that since we're propagating schedules at the top-level diagram to nested shapes, there will be changes all the way down. We haven't started that propagation though, so the changes won't be apparent at each level of the hierarchy yet.

					AddShapesRequringPropagationRecursively(linkedDiagramsRequiringPropagation, visitedDiagrams, assumeSchedulesWillChange: true);
				}
			}

			try
			{
				AddShapesRequringPropagationRecursively(new[] { diagramShape }, new HashSet<BMNCNRootDiagramShape>(new[] { diagramShape }), assumeSchedulesWillChange: false);
			}
			catch (UserCancelledPropagationException)
			{
				return new LinkedList<ICollection<BMNCNShape>>();
			}

			return result;
		}

		static IEnumerable<BMNCNShape> GetShapesRequiringPropagation(BMNCNRootDiagramShape diagramShape, bool assumeSchedulesWillChange)
		{
			foreach (var shape in diagramShape.AllNestedShapes)
			{
				if (assumeSchedulesWillChange ? shape.CanPotentiallyPropagateScheduleToLinkedDiagram : shape.ShouldPropagateScheduleToLinkedDiagramNow())
				{
					yield return shape;
				}
			}
		}

		static ShapeScheduleCache GetShapeScheduleCache(ICollection<BMNCNShape> topLevelShapesRequiringPropagation)
		{
			var cache = new ShapeScheduleCache();

			foreach (var shape in topLevelShapesRequiringPropagation)
			{
				cache.AddSchedulesToCache(shape); // We need to add the first hierarchy level into the cache here because each nested layer is added only as we propagate deeper.
			}

			return cache;
		}

		#endregion

		#region Schedule Propagation

		bool TryPropagateSchedule(BMNCNShape shape, ShapeScheduleCache scheduleCache)
		{
			var factoryForPropagatingSchedule = new BusinessObjectFactory { NameForDebugging = FormattableString.Invariant($"{nameof(PropagateLinkedDiagramSchedules)}: {shape.Name}") };
			var linkedDiagram = factoryForPropagatingSchedule.Load<BMNCNRootDiagramShape>(shape.BNS_RelatedEntityID);
			var schedules = scheduleCache.GetScheduledStartAndFinishTimes(shape);

			if (linkedDiagram != null && schedules != null)
			{
				var linkedDiagramNetwork = JobNetwork.CreateTemporaryNetwork(linkedDiagram); // Don't allow the new JobNetwork instance access to a controller, since there is no dedicated UI for it yet. That will happen only if we open the network in its own form.
				var linkedDiagramEntity = linkedDiagramNetwork.DiagramEntity;

				linkedDiagram.ScheduledStartTimeUtc = schedules.Item1;
				linkedDiagramNetwork.RefreshSchedules(); // This needs to happen before setting the end date so that shapes don't get squished into the fixed diagram surface. We want to expose any cases where shapes are scheduled outside the proposed date boundaries.

				linkedDiagram.ScheduledFinishTimeUtc = schedules.Item2;

				linkedDiagramNetwork.RefreshSchedules();
				linkedDiagramEntity.Validation.ValidateScheduleDateViolations();

				if (linkedDiagramEntity.HasErrors)
				{
					if (AskUserIfTheyWantToOpenDiagramToFixSchedulingConflict(shape, linkedDiagram))
					{
						OpenLinkedDiagramInOwnForm(linkedDiagram);
					}

					return false;
				}
				else
				{
					shape.Factory.ChildFactories.Add(factoryForPropagatingSchedule);

					foreach (var childShape in linkedDiagram.AllNestedShapes)
					{
						scheduleCache.AddSchedulesToCache(childShape);
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Stores the schedules on each shape as they are propagated. This is necessary because each diagram is updated in its own factory, so multiple layers of propagation needs to use changes that aren't yet saved to the database.
		/// </summary>
		class ShapeScheduleCache
		{
			readonly Dictionary<ZGuid, Tuple<ZDateTime, ZDateTime>> cache = new Dictionary<ZGuid, Tuple<ZDateTime, ZDateTime>>();

			internal Tuple<ZDateTime, ZDateTime> GetScheduledStartAndFinishTimes(BMNCNShape shape)
			{
				return cache.GetValueSafe(shape.PK);
			}

			internal void AddSchedulesToCache(BMNCNShape shape)
			{
				cache[shape.PK] = Tuple.Create(shape.ScheduledStartTimeUtc, shape.ScheduledFinishTimeUtc);
			}
		}

		#endregion

		#region User Interaction

		void OpenLinkedDiagramInOwnForm(BMNCNRootDiagramShape linkedDiagram)
		{
			var freshFactory = new BusinessObjectFactory { NameForDebugging = linkedDiagram.Factory.NameForDebugging + ", ViewDiagram" };
			var reloadedLinkedDiagram = freshFactory.Load<BMNCNRootDiagramShape>(linkedDiagram.PK);

			Network.Controller.ViewDiagram(reloadedLinkedDiagram); // Don't use the factory in which the diagram was edited because that would open the diagram in an invalid state.
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "This code is only ever called from the view model layer.")]
		static bool AskUserIfTheyWantToProceedDespiteCircularLinkedDiagrams(BMNCNShape shape)
		{
			var caption = Res.GetString("b18e5ce8-e90e-4b89-982b-eab1d11bf907", "Circularly Related Diagrams");
			var message = new StringBuilder(Res.GetString("472a02f2-12a4-4e8d-bca0-672bdc319a12", "A circular relationship between linked diagrams has been found. Would you like to proceed with updating schedules anyway? The process will stop once it reaches a diagram that has already had its schedule updated."));

			message.AppendLine();
			message.AppendLine();

			message.Append(Res.GetString("73a5fda6-006a-49b4-95cc-8a87c75c4b51", "Shape: {0}", shape.Name));

			if (shape.RootShape != null)
			{
				message.AppendLine();
				message.Append(Res.GetString("f00edb21-a955-44fa-bc46-a0514158b788", "Diagram containing shape: {0}", shape.RootShape.Name));
			}

			var dialogResult = Globals.Message.Show(message.ToString(), caption, ZMessageBoxButtons.YesNo, ZDialogResult.Yes); // This code is only ever called from the view model layer.

			return dialogResult == ZDialogResult.Yes;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "This code is only ever called from the view model layer.")]
		static bool AskUserIfTheyWantToOpenDiagramToFixSchedulingConflict(BMNCNShape shape, BMNCNRootDiagramShape linkedDiagram)
		{
			var caption = Res.GetString("2ee7c9cd-efd7-4649-8d03-5fa6757d40ce", "Cannot Propagate Scheduling Changes");
			var message = GetSchedulePropagationFailedMessage(shape, linkedDiagram);

			var dialogResult = Globals.Message.Show(message, caption, ZMessageBoxButtons.YesNo, ZDialogResult.Yes); // This code is only ever called from the view model layer.

			return dialogResult == ZDialogResult.Yes;
		}

		static string GetSchedulePropagationFailedMessage(BMNCNShape shape, BMNCNRootDiagramShape linkedDiagram)
		{
			var message = new StringBuilder(Res.GetString("fdb95779-3e6f-4638-96c7-b228765b355a", "Cannot propagate scheduling changes to linked diagrams because it would introduce conflicts. The diagram with the conflict is named [{0}].", linkedDiagram.Name));

			message.AppendLine();
			message.AppendLine();
			message.Append(Res.GetString("dd2c6bc1-fb9d-4451-aa05-c39136227da5", "The system attempted to update the diagram with the following information:"));

			AppendTimePropertyAndValue(nameof(linkedDiagram.ScheduledStartTimeLocal), shape.ScheduledStartTimeLocal);
			AppendTimePropertyAndValue(nameof(linkedDiagram.ScheduledFinishTimeLocal), shape.ScheduledFinishTimeLocal);

			message.AppendLine();
			message.AppendLine();
			message.Append(Res.GetString("bf9811b2-bfe5-47fd-8618-844f016bf43e", "Would you like to open this diagram to attempt to fix the conflict?"));

			void AppendTimePropertyAndValue(string propertyName, ZDateTime propertyValue)
			{
				message.AppendLine();
				message.Append(BMConstants.BulletPointCharacter);
				message.Append(" ");
				message.Append(DataBoundResourceStrings.GetDataForProperty(linkedDiagram.GetType(), propertyName).Caption);
				message.Append(": ");
				message.Append(propertyValue.ToBestReadableDateTimeString());
			}

			return message.ToString();
		}

		#endregion
	}
}
