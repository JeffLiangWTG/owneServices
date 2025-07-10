using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class ApproveDiagramAction : ApproveDiagramActionBase
	{
		public ApproveDiagramAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region JobNetworkAction

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			var entity = Network.Entities.GetInstance(shape);
			return BMNetworkActionAccessibilityHelper.CheckShapeDoesNotBelongToDefaultDiagram(shape)
				.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckDiagramIsScaled(Network.DiagramEntity, shape)
					.Union(BMNetworkActionAccessibilityHelper.CheckEntityIsRoot(entity))
					.Union(BMNetworkActionAccessibilityHelper.CheckShapeIsNotApproved(shape)));
		}

		protected override INetworkActionAccessibility PerformPreExecutionChecksForShape(BMNCNShape shape)
		{
			var notifications = new List<ConfirmationNotification>();
			var shouldRefreshSchedules = true;

			if (!shape.IsScaled)
			{
				notifications.Add(new ConfirmationNotification(NotificationTypes.Error, Res.GetString("78460df4-5bd4-46bb-8fc4-d278a61b56b8", "A non-scaled diagram cannot be approved.")));
			}

			if (shape.ScheduledStartTimeUtc.IsEmpty && shape.ScheduledFinishTimeUtc.IsEmpty)
			{
				notifications.Add(new ConfirmationNotification(NotificationTypes.Error, Res.GetString("3edb41e6-ca93-4bae-b156-f2d2c83b7e81", "The diagram requires either a Scheduled Start Time or a Scheduled Finish Time. These can be set on the Properties form accessed from the designer surface.")));
			}

			if (shape.ScheduleBizo.BNC_GB_Branch.IsEmpty)
			{
				notifications.Add(new ConfirmationNotification(NotificationTypes.Error, Res.GetString("35DBF39A-68B2-4333-814E-172812FD4ED9", "The diagram requires a valid branch. This can be set on the Properties form accessed from the designer surface under 'Branch' .")));
				shouldRefreshSchedules = false;
			}

			if (shape.ScheduleBizo.BNC_GE_Department.IsEmpty)
			{
				notifications.Add(new ConfirmationNotification(NotificationTypes.Error, Res.GetString("5810B15F-C815-4EAA-8CC3-C08A835AE375", "The diagram requires a valid department. This can be set on the Properties form accessed from the designer surface under 'Department'.")));
				shouldRefreshSchedules = false;
			}

			if (shouldRefreshSchedules)
			{
				Network.RefreshSchedules(forceReCalculation: true); // It's possible conflicting edits across user sessions have introduced a circular dependency. Let's be doubly sure before approving.
			}

			var checkBufferConnections = shape.IsBuffered;
			var checksToMake = checkBufferConnections ? Network.Entities.Count * 3 : Network.Entities.Count * 2;

			using (var progressReporter = UserInteractionImplementor.ProgressReporterProvider.CreateProgressReporter(Res.GetString("7b21d25a-2809-42ee-806d-9e607c241ec7", "Validating diagram for approval"), checksToMake))
			{
				if (checkBufferConnections)
				{
					CheckForEntitiesWithNoConnectionToABuffer(progressReporter, Network, notifications);
				}

				CheckForNotifications(progressReporter, Network, notifications);

				if (progressReporter.IsCancelled)
				{
					return NetworkActionAccessibility.GetCancelledByUserWithoutNotification(shape);
				}
			}

			if (notifications.Count > 0)
			{
				var confirmed = UserInteractionImplementor.HasUserConfirmed(
					Res.GetString("8bf7cb26-3b66-4b44-b20a-98f25801a0b5", "Approve Diagram"),
					Res.GetString("c2a1c4a6-2f8f-4b91-a5e1-2b718ce4be19", "Please review the following notifications before this diagram can be approved."),
					notifications.ToArray()
					);

				return confirmed ? NetworkActionAccessibility.Allowed : NetworkActionAccessibility.GetCancelledByUserWithoutNotification(shape);
			}

			return NetworkActionAccessibility.Allowed;
		}

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			if (shape.PK != Network.DiagramShape.PK)
			{
				throw new ArgumentException("shape must be root-level", nameof(shape));
			}

			if (CheckHasNoApprovedShapesInDiagram() && CheckHasNoApprovedShapesOnOtherDiagrams(shape) && CheckHasNoApprovedShapesForNonVisibleChildren())
			{
				var resourceCode = GlbStaff.CurrentUser.GS_Code;
				var logReference = string.Format(CultureInfo.InvariantCulture, (NoResString)"Diagram approved by [{0}]", GlbStaff.CurrentUser.GS_FullName); // Log reference should not be translated

				var entity = Network.Entities.GetInstance(shape);
				entity.ToggleApproval(resourceCode);

				foreach (var item in GetItemsRequiringApproval(entity, expectedApprovalState: !shape.IsApproved))
				{
					item.ToggleApproval(resourceCode);
				}

				Network.Refresher.Refresh(RefreshType.EntityApprovedOrUnapproved, shape);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				shape.Logs.AddNew(AutoEvents.EditedARecord, logReference);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("aeea0045-0f98-43d5-be08-c1e0bac6451b", "Approve Diagram");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ResString.GetMultilingualString("16af4a69-de06-48d2-855e-25356fc164d9", "Marks this diagram as approved, causing its shapes to be released according to CCPM or NCN scheduling rules");
		}

		protected override string IconName => (NoResString)"Approve"; // resource name

		#endregion

		#region Notification Checks

		static void CheckForEntitiesWithNoConnectionToABuffer(IProgressReporter progressReporter, IJobNetwork network, List<ConfirmationNotification> notifications)
		{
			var shapesNotConnectedToBuffers = new LinkedList<BMNCNShape>();

			//TODO: This check can be O(n). Hint: Work backwards from Buffer Shapes...
			foreach (var entity in network.Entities.ShapeEntities.Where(s => s != network.DiagramEntity))
			{
				if (entity.CanHaveSchedule && !entity.IsBufferShape && !entity.AnyDownstreamPostRequisiteMatches(e => e.IsBufferShape, new ShapeNetworkEntityDescendantsStrategy()))
				{
					shapesNotConnectedToBuffers.AddLast(entity.Shape);
				}

				progressReporter.ReportOneItemProcessed();
			}

			if (shapesNotConnectedToBuffers.Count > 0)
			{
				var message = new StringBuilder(Res.GetString("cb57de8d-260f-4865-975b-3e1c3eea635a", "There are shapes which have no connection to a buffer. This will cause the entities linked to those shapes to not use the CCPM schedule when considering them for release to a buffer. Consider connecting these shapes to a downstream buffer."));
				message.Append(" ");

				if (shapesNotConnectedToBuffers.Count > 1)
				{
					message.Append(Res.GetString("320fc3ce-3ee8-481b-8e43-db4282dcd57e", "Shapes"));
				}
				else
				{
					message.Append(Res.GetString("7322406f-5669-42f1-96e0-ef1a9dcba740", "Shape"));
				}

				message.Append(": ");
				message.Append(string.Join(", ", shapesNotConnectedToBuffers.Select(s => string.Format(CultureInfo.InvariantCulture, "[{0}]", s.Name))));

				notifications.Add(new ConfirmationNotification(NotificationTypes.Warning, message.ToString()));
			}
		}

		static void CheckForNotifications(IProgressReporter progressReporter, IJobNetwork network, List<ConfirmationNotification> notifications)
		{
			ConfirmationNotification error = null;
			ConfirmationNotification warning = null;
			ConfirmationNotification messageError = null;

			foreach (var entity in network.Entities.ShapeEntities)
			{
				if (progressReporter.IsCancelled)
				{
					return;
				}

				entity.Shape.Validation.ValidateAll();
				entity.Validation.ValidateAll();
				entity.Validation.ValidateBackInTimeArrows();
				entity.ProcessHeader?.Validation.ValidateLoops();

				progressReporter.ReportOneItemProcessed();
			}

			foreach (var entity in network.Entities.ShapeEntities)
			{
				if (progressReporter.IsCancelled)
				{
					return;
				}

				var validatableEntities = new BusinessObject[] { entity, entity.Shape }.Concat(entity.DependencyAttachments);

				foreach (var bizo in validatableEntities)
				{
					var notificationCollector = new HumanReadableNotificationCollector(bizo, includeChildren: true, includeNotificationTypeInMessage: false, descriptionToInclude: ZNotificationCollector.PropertyDescriptionType.HumanReadableName);

					MaybePopulateNotification(ref error, NotificationTypes.Error, notificationCollector, Lazy.Create(() => Res.GetString("c25cf88b-8b7a-46df-b50d-152bbfbd8204", "Shapes on this diagram have errors. See error icons on the relevant shapes.")));
					MaybePopulateNotification(ref warning, NotificationTypes.Warning, notificationCollector, Lazy.Create(() => Res.GetString("26685a69-44f8-4dc9-817b-66ec9d260427", "Shapes on this diagram have warnings. See warning icons on the relevant shapes.")));
					MaybePopulateNotification(ref messageError, NotificationTypes.MessageError, notificationCollector, Lazy.Create(() => Res.GetString("20799b96-3686-4ccf-a7cc-e22855ff08fa", "Shapes on this diagram have message errors. See message icons on the relevant shapes.")));
				}

				progressReporter.ReportOneItemProcessed();
			}

			if (error != null)
			{
				notifications.Add(error);
			}

			if (warning != null)
			{
				notifications.Add(warning);
			}

			if (messageError != null)
			{
				notifications.Add(messageError);
			}
		}

		static void MaybePopulateNotification(ref ConfirmationNotification notification, NotificationTypes notificationType, ZNotificationCollector notificationCollector, Lazy<string> messagePrefix)
		{
			if (notification == null)
			{
				var notifications = GetNotifications(notificationType, notificationCollector);

				if (notifications.Any())
				{
					var notificationText = notifications.GetUniqueMessageList();
					var message = messagePrefix.Value + System.Environment.NewLine + string.Join(System.Environment.NewLine, notificationText);

					notification = new ConfirmationNotification(notificationType, message);
				}
			}
		}

		static IEnumerable<INotification> GetNotifications(NotificationTypes notificationType, ZNotificationCollector notificationCollector)
		{
			switch (notificationType)
			{
				case NotificationTypes.Error:
					return notificationCollector.GetErrors();
				case NotificationTypes.Warning:
					return notificationCollector.GetWarnings();
				case NotificationTypes.MessageError:
					return notificationCollector.GetMessageErrors();

				default:
					return Enumerable.Empty<INotification>();
			}
		}

		#endregion

		#region Implementation

		bool CheckHasNoApprovedShapesInDiagram()
		{
			var approvedShapes = Network.Shapes.Where(s => s.IsApproved).ToArray();

			return CheckHasNoShapes(approvedShapes, () => Res.GetString("388b8ab4-23f1-48d4-b15e-82fc5f55aa85", "This diagram contains approved shapes and cannot be approved. The following shapes are approved on this diagram:{0}\t{1}",
				/*0*/ System.Environment.NewLine,
				/*1*/ string.Join(System.Environment.NewLine + "\t", approvedShapes.Select(s => s.Name))));
		}

		bool CheckHasNoApprovedShapesOnOtherDiagrams(BMNCNShape shape)
		{
			var processHeaderPKs = Network.Shapes.Select(s => s.BNS_RelatedEntityID).ToArray();
			var approvedShapes = GetApprovedShapes(processHeaderPKs, shape.Factory);
			var shapesOnThisDiagram = Network.Shapes.Where(s => approvedShapes.Any(approvedShape => s.BNS_RelatedEntityID == approvedShape.BNS_RelatedEntityID)).ToArray();

			return CheckHasNoShapes(shapesOnThisDiagram, () => Res.GetString("0b8d6b40-a8a2-4eea-b287-2510e66c0205", "This diagram contains shapes that are approved on other diagrams, and cannot be approved. The following shapes are approved on other diagrams:{0}\t{1}",
				/*0*/ System.Environment.NewLine,
				/*1*/ string.Join(System.Environment.NewLine + "\t", shapesOnThisDiagram.Select(s => s.Name).OrderBy(n => n))));
		}

		bool CheckHasNoApprovedShapesForNonVisibleChildren()
		{
			var shapesOnThisDiagramWithApprovedNonVisibleChildren = new[]
			{
				Network.DiagramShape
			}.Concat(Network.Shapes).Select(s => GetApprovedShapesForNonVisibleChildren(s)).Where(x => x.Item2.Length > 0).ToArray();

			if (shapesOnThisDiagramWithApprovedNonVisibleChildren.Length > 0)
			{
				var message = Res.GetString("351e6b49-7f91-4b74-982d-8e8b4fda9d40", "This diagram contains shapes with non-visible child entities that are approved on other diagrams, and so cannot be approved. The following shapes have non-visible entities approved on other diagrams:{0}\t{1}",
					System.Environment.NewLine,
					string.Join(System.Environment.NewLine + System.Environment.NewLine + "\t", shapesOnThisDiagramWithApprovedNonVisibleChildren.OrderBy(x => x.Item1.Name).Select(x => GetMessageForNonVisibleApprovedShapes(x.Item1, x.Item2))));

				UserInteractionImplementor.ShowMessage(message);

				return false;
			}

			return true;
		}

		static string GetMessageForNonVisibleApprovedShapes(BMNCNShape shapeOnThisDiagram, BMNCNShape[] approvedChildShapes)
		{
			return shapeOnThisDiagram.Name
				+ System.Environment.NewLine
				+ "\t\t"
				+ string.Join(System.Environment.NewLine + "\t\t", approvedChildShapes.OrderBy(s => s.Name).Select(s => Res.GetString("4fe145b1-dc9b-4766-ad02-92491ee173d2", "Approved shape: [{0}] for entity: [{1}]", s.Name, s.ProcessHeader.Name)));
		}

		static Tuple<BMNCNShape, BMNCNShape[]> GetApprovedShapesForNonVisibleChildren(BMNCNShape source)
		{
			var childProcessHeaders = GetProcessHeadersIncludingChildren(source.ProcessHeader);
			var approvedShapes = GetApprovedShapes(childProcessHeaders.Select(x => x.PK).ToArray(), source.Factory);

			return Tuple.Create(source, approvedShapes);
		}

		static IEnumerable<ProcessHeader> GetProcessHeadersIncludingChildren(ProcessHeader source, HashSet<ProcessHeader> processHeadersFoundSoFar = null)
		{
			if (processHeadersFoundSoFar == null)
			{
				processHeadersFoundSoFar = new HashSet<ProcessHeader>();
			}

			if (source != null && !processHeadersFoundSoFar.Contains(source))
			{
				processHeadersFoundSoFar.Add(source);
				yield return source;

				foreach (var childLink in source.ChildLinks)
				{
					foreach (var childWorkflow in GetProcessHeadersIncludingChildren(childLink.HeaderFrom, processHeadersFoundSoFar))
					{
						yield return childWorkflow;
					}
				}
			}
		}

		static BMNCNShape[] GetApprovedShapes(ZGuid[] processHeaderPKs, BusinessObjectFactory factory)
		{
			var query = new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, processHeaderPKs);
			query.AddToFilter(BMNCNShapeSchema.BNS_GS_NKApprovedBy, SQLComparisonOperator.NotEqual, ZString.Empty);
			query.IncludeBlob(BMNCNShapeSchema.BNS_LayoutData);

			return factory.Load<BMNCNShape>(query);
		}

		bool CheckHasNoShapes(BMNCNShape[] shapes, Func<string> messageGetter)
		{
			if (shapes.Length > 0)
			{
				UserInteractionImplementor.ShowMessage(messageGetter());
				return false;
			}

			return true;
		}

		#endregion
	}
}
