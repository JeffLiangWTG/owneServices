using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Implementation;
using CargoWise.Shared;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Network.NetworkEntity;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class JobNetwork : IJobNetwork, INotifyPropertyChanged
	{
		public static JobNetwork CreateTemporaryNetwork(BMNCNShape diagram)
		{
			return Create(diagram, controller: null, temporaryNetwork: true);
		}

		public static JobNetwork CreateTemporaryNetwork(BMNCNShape diagram, IBMNetworkEntityController controller)
		{
			return Create(diagram, controller, temporaryNetwork: true);
		}

		public static JobNetwork Create(BMNCNShape diagram, IBMNetworkEntityController controller, INetworkRefresher refresher = null, IJobNetworkValidator validator = null, bool temporaryNetwork = false)
		{
			return new JobNetwork(new BMNetworkViewModel(diagram), controller, refresher ?? new NonRefreshingNetworkRefresher(), validator, temporaryNetwork);
		}

		#region For Test
#if DEBUG
		public static JobNetwork Create_ForTest(BMNetworkViewModel networkViewModel, IBMNetworkEntityController controller, INetworkRefresher refresher = null, IJobNetworkValidator validator = null)
		{
			return new JobNetwork(networkViewModel, controller, refresher ?? new NonRefreshingNetworkRefresher(), validator);
		}
#endif
		#endregion

		public JobNetwork(BMNetworkViewModel viewModel, IBMNetworkEntityController controller, INetworkRefresher refresher, IJobNetworkValidator validator = null, bool temporaryNetwork = false)
		{
			this.viewModel = viewModel;
			RootShapeProvider.SetRoot(viewModel.DiagramShape);
			Controller = controller;
			Refresher = refresher;
			this.validator = validator ?? new JobNetworkValidator();
			this.temporaryNetwork = temporaryNetwork;

			if (DiagramEntity.ProcessHeader != null)
			{
				Name = DiagramEntity.ProcessHeader.Description;
			}
			else
			{
				Name = DiagramShape.BNS_Name;
			}

			if (!temporaryNetwork)
			{
				refresher.Refreshed += Refresher_Refreshed;
				DiagramShape.BNS_RelatedEntityIDInfo.ValueChanged += JobLink_ValueChanged;
			}

			JobNetworkStrategyProvider.GetInitialisationStrategy(viewModel).InitialiseDiagram();
		}

		readonly BMNetworkViewModel viewModel;
		readonly bool temporaryNetwork;

		#region IEntityPositionStrategy

		public IEntityPositionStrategy EntityPositionStrategy
		{
			get { return JobNetworkStrategyProvider.GetPositionStrategy(viewModel); }
		}

		#endregion

		#region Create Entity

		public ShapeNetworkEntity AddNewShape(BMNCNShape shape)
		{
			var entity = Entities.GetInstance(shape);

			CopyRelevantDiagramPropertiesToNewShape(shape);
			DiagramShape.RegisterEditableChildObject(shape);

			return entity;
		}

		void CopyRelevantDiagramPropertiesToNewShape(BMNCNShape newShape)
		{
			if (!newShape.IsInDatabase)
			{
				foreach (var property in PropertiesToCopyFromDiagramToNewShapes)
				{
					var targetPropertyInfo = newShape.ZPropertyInfoHash[property.Name];
					var sourceValue = property.Value;
					var targetValue = targetPropertyInfo.Value;

					if ((sourceValue == null ^ targetValue == null) || (sourceValue == null && targetValue == null) || !sourceValue.Equals(targetValue))
					{
						targetPropertyInfo.Value = sourceValue;
					}
				}
			}
		}

		ZPropertyInfo[] PropertiesToCopyFromDiagramToNewShapes
		{
			get
			{
				if (propertiesToCopyFromDiagramToNewShapes == null)
				{
					propertiesToCopyFromDiagramToNewShapes = (
						from ZPropertyInfo zProp in DiagramShape.ZPropertyInfoHash
						where zProp.PropertyDescriptor.Attributes.OfType<CopiedFromDiagramShapeOnCreationAttribute>().Any()
						select zProp
						).ToArray();
				}

				return propertiesToCopyFromDiagramToNewShapes;
			}
		}

		ZPropertyInfo[] propertiesToCopyFromDiagramToNewShapes;

		#endregion

		#region Delete Entity

		public bool DeleteEntity(IProposedNetworkEntity proposedEntity)
		{
			if (proposedEntity == DiagramShape)
			{
				ErrorReporter.ReportOnce("Should not try to remove the diagram node.");
				return false;
			}

			var result = DeleteEntityCore(proposedEntity);
			var entity = proposedEntity as INetworkEntity;
			if (result && entity != null)
			{
				Refresher.Refresh(RefreshType.EntityRemoved, entity);
			}
			return result;
		}

		bool DeleteEntityCore(IProposedNetworkEntity proposedEntity)
		{
			var entity = Entities.GetInstance(proposedEntity);
			var shape = entity.Shape;

			if (shape.IsDeleted)
			{
				return true;
			}

			if (entity.EntityState.HasFlag(EntityState.Approved))
			{
				var message = Res.GetString("7931611a-5607-46d3-b6fe-1541550bf920", "This shape cannot be deleted as it has been approved by [{0}].", shape.ApprovedBy.GS_FullName);
				Controller.UserInteractionImplementor.ShowMessage(message);

				return false;
			}

			var processHeader = shape.ProcessHeader;
			var isOnMultipleDiagrams = JobNetworkStrategyProvider.GetUpdateEntitiesStrategy(this).IsRelatedEntityPresentOnMultipleDiagrams(shape);

			if (isOnMultipleDiagrams)
			{
				var message = Res.GetString("93641c36-8e57-413f-b188-05e46c245950", "This entity is present on other diagrams and cannot be deleted. Would you like to hide the entity instead?");
				var caption = Res.GetString("8971201f-c542-4bf3-b331-52d2c519967b", "Hide entity instead");
				var hideEntityInstead = Controller.UserInteractionImplementor.HasUserConfirmed(message, caption);

				if (hideEntityInstead)
				{
					HideEntity(entity);
					return true;
				}
				else
				{
					return false;
				}
			}

			var shouldDeleteShape = !(shape.ShapeType == ShapeTypeList.Codes.Shape && processHeader != null) || WorkflowManagementViewModel.TryDelete(DeleteWorkflowDialogWrapper, processHeader);

			if (shouldDeleteShape)
			{
				if (!shape.IsDeleted)
				{
					shape.Delete();
				}
				return true;
			}

			return false;
		}

		public IMultiActionButtonDialogWrapper<DeleteWorkflowOption> DeleteWorkflowDialogWrapper { get; set; }

		#endregion

		#region Hide / Show Entity

		public IEnumerable<IProposedNetworkEntity> HideEntity(IProposedNetworkEntity proposedEntity)
		{
			if (proposedEntity == DiagramShape)
			{
				ErrorReporter.ReportOnce("Should not try to hide the diagram node.");
				return Enumerable.Empty<IProposedNetworkEntity>();
			}

			var result = HideEntityCore(proposedEntity);

			if (result.Any())
			{
				Refresher.Refresh(RefreshType.EntityRemoved, result.ToArray());
			}

			return result;
		}

		IEnumerable<INetworkEntity> HideEntityCore(IProposedNetworkEntity proposedEntity)
		{
			var shapesToHide = new HashSet<INetworkEntity>();
			var entity = Entities.GetInstance(proposedEntity);
			var shape = entity.Shape;

			if (shape != null && !shape.IsDeleted)
			{
				var shapeEntity = Entities.GetInstance(shape);
				if (shapeEntity.EntityState.HasFlag(EntityState.Approved))
				{
					var message = Res.GetString("2bd0421c-d183-480c-8dd4-aec8b2938572", "This shape cannot be hidden as it has been approved by [{0}].", shape.ApprovedBy.GS_FullName);
					Controller.UserInteractionImplementor.ShowMessage(message);

					return Array.Empty<INetworkEntity>();
				}

				var isLinkedToRealEntity = shape.IsLinkedToRealEntity;
				var userSelection = isLinkedToRealEntity ? GetUnLinkVariant(shape) : UnlinkEntityVariant.None;

				if (userSelection != UnlinkEntityVariant.Cancel)
				{
					var entitiesToRefresh = GetShapesRelatedByBackingEntityDependencies(shape).ToList();

					shapesToHide.UnionWith(shape.GetDescendants());

					var processHeader = shape.ProcessHeader;
					if (processHeader != null && !processHeader.IsDeleted)
					{
						if (!processHeader.FH_ParentId.IsValid)
						{
							processHeader.Delete();
						}
						else
						{
							foreach (var parentHeader in GetParentShapes(processHeader))
							{
								var parent = Entities.GetInstance(parentHeader);
								parent.HiddenEntities.Add(processHeader);
								entitiesToRefresh.Add(parent);
							}
						}
					}

					shapesToHide.Add(shape);

					foreach (var dependency in shapeEntity.DependencyAttachments)
					{
						var buffer = dependency.GetBuffer();
						if (buffer != null && !shapesToHide.Contains(buffer))
						{
							shapesToHide.Add(buffer);
						}
					}

					bool isUnlinkSuccessful = true;
					if (isLinkedToRealEntity)
					{
						isUnlinkSuccessful = UnlinkEntity(shape, allowRefresh: true, userSelection: userSelection);
					}

					if (isUnlinkSuccessful)
					{
						shape.Delete();
					}
					else
					{
						shapesToHide.Clear();
					}

					foreach (var entityToRefresh in entitiesToRefresh.Distinct())
					{
						entityToRefresh.HiddenRelationships.Reload();
					}
				}
			}

			return Entities.GetInstances(shapesToHide);
		}

		public IEnumerable<INetworkEntity> ShowEntity(IProposedNetworkEntity entity, INetworkEntity parentEntity)
		{
			var shownShapes = new List<BMNCNShape>();
			var processHeader = entity as ProcessHeader;
			var parentShape = parentEntity.AsShape();

			if (processHeader != null && parentEntity != null)
			{
				var shape = processHeader.Factory.New<BMNCNShape>();
				shape.BNS_ShapeType = ShapeTypeList.Codes.Shape;
				shape.BNS_RelatedEntityID = processHeader.PK;
				shape.BNS_Name = processHeader.FH_CompletionStatement.SubstringSafe(0, BMNCNShapeSchema.BNS_Name.MaxLength);

				shape.MakeChildOf(parentShape);

				foreach (var parent in Entities.GetInstances((IEnumerable<INetworkEntity>)GetParentShapes(processHeader)))
				{
					if (parent.HiddenEntities.Contains(entity))
					{
						parent.HiddenEntities.Remove(entity);
					}
				}

				ImportIntoDiagram(shape, parentShape, shownShapes);

				RefreshShapeAndAncestorsHiddenRelationShips(shape);

				ShowRelationshipsOfShownEntity(entity, shape);

				return Entities.GetInstances((IEnumerable<INetworkEntity>)shownShapes);
			}

			return null;
		}

		void ShowRelationshipsOfShownEntity(IProposedNetworkEntity entity, BMNCNShape shape, bool refreshDiagram = true)
		{
			Argument.NotNull(entity, nameof(entity));
			Argument.NotNull(shape, nameof(shape));

			if (!(entity is ProcessHeader))
			{
				return;
			}

#if NETFRAMEWORK
			var shownEntities = Entities.ShapeEntities
				.Union([DiagramEntity])
				.Where(e => e?.RelatedEntityPK != ZGuid.Empty)
				.DistinctBy(e => e.RelatedEntityPK)
				.ToDictionary(e => e.RelatedEntityPK);
#else
			var shownEntities = Enumerable.DistinctBy(
				Entities.ShapeEntities.Union([DiagramEntity]).Where(e => e?.RelatedEntityPK != ZGuid.Empty),
				e => e.RelatedEntityPK)
				.ToDictionary(e => e.RelatedEntityPK);
#endif

			var dependencyLinks = entity.PreRequisiteLinks.Concat(entity.PostRequisiteLinks);
			var shapeEntity = Entities.GetInstance(shape);

			foreach (var link in dependencyLinks)
			{
				var isEntityLinkSrc = entity == link.From;
				var otherLinkEntityPK = isEntityLinkSrc ? ((ProcessHeader)link.To).PK : ((ProcessHeader)link.From).PK;

				if (shownEntities.TryGetValue(otherLinkEntityPK, out ShapeNetworkEntity shownOtherEntity))
				{
					if (isEntityLinkSrc)
					{
						ShowRelationship(shapeEntity, shownOtherEntity, !refreshDiagram);
					}
					else
					{
						ShowRelationship(shownOtherEntity, shapeEntity, !refreshDiagram);
					}

					RefreshShapeAndAncestorsHiddenRelationShips(shapeEntity.AsShape());
					RefreshShapeAndAncestorsHiddenRelationShips(shownOtherEntity.AsShape());
				}
			}

			if (refreshDiagram)
			{
				Refresher.Refresh(RefreshType.RedrawDiagram);
			}
		}

		void RefreshShapeAndAncestorsHiddenRelationShips(BMNCNShape shape)
		{
			Argument.NotNull(shape, nameof(shape));

			Entities.GetInstance(shape).Owner?.HiddenRelationships.Reload();

			foreach (var shapeToRefresh in GetShapesRelatedByBackingEntityDependencies(shape))
			{
				shapeToRefresh.HiddenRelationships.Reload();
			}
		}

		IEnumerable<BMNCNShape> GetParentShapes(ProcessHeader processHeader)
		{
			var parents = new HashSet<ProcessHeader>(processHeader.ParentLinks.Select(w => w.HeaderTo).Append(processHeader.JobHeader));
			return Shapes.Append(DiagramShape).Where(s => parents.Contains(s.ProcessHeader));
		}

		IEnumerable<ShapeNetworkEntity> GetShapesRelatedByBackingEntityDependencies(BMNCNShape shape)
		{
			var processHeader = shape.ProcessHeader;
			if (processHeader != null)
			{
				foreach (var link in processHeader.Links.Where(l => l.FP_LinkType == ProcessHeaderLinkTypeList.Codes.Dependency))
				{
					var relevantKey = link.FP_FH_HeaderFrom == processHeader.PK ? link.FP_FH_HeaderTo : link.FP_FH_HeaderFrom;
					var targetShape = Entities.ShapeEntities.FirstOrDefault(s => s.RelatedEntityPK == relevantKey);
					var ownerShape = targetShape != null ? targetShape.Owner : null;

					if (ownerShape != null)
					{
						yield return ownerShape;
					}
				}
			}
		}

#endregion

		#region Import

		public IEnumerable<INetworkEntity> PickAndImportEntities(IProposedNetworkEntity parentEntity)
		{
			var shape = (BMNCNShape)Controller.PickEntity(ModuleIDs.NetworkDiagram);

			if (shape != null)
			{
				return Entities.GetInstances(ImportEntities(parentEntity, shape));
			}
			else
			{
				return Enumerable.Empty<INetworkEntity>();
			}
		}

#if DEBUG
		public
#endif
		IEnumerable<INetworkEntity> ImportEntities(IProposedNetworkEntity proposedParent, BMNCNShape shape)
		{
			shape = viewModel.DiagramShape.Factory.Load<BMNCNShape>(shape.PK);
			var parentEntity = Entities.GetInstance(proposedParent) ?? DiagramEntity;

			var result = JobNetworkEntityRelationshipValidator.ValidateImport(shape, DiagramEntity, Entities.ShapeEntities, parentEntity, EntityPositionStrategy);
			if (!result.IsValid)
			{
				Controller.UserInteractionImplementor.ShowMessage(result.FailureReason);
			}
			else
			{
				var shapesImported = new List<BMNCNShape>();

				using (SuspendRefreshingOnEntityCountChanged())
				{
					var cloneNetwork = JobNetwork.CreateTemporaryNetwork(shape);
					shape = cloneNetwork.DiagramEntity.CloneShapeAndAllDescendantsIntoNewParent(parentEntity.Shape, unPinPinnedShapes: true);

					if (shape.BNS_ShapeType != ShapeTypeList.Codes.Shape)
					{
						shape.BNS_ShapeType = ShapeTypeList.Codes.Shape;
					}

					viewModel.DiagramShape.RegisterEditableChildObject(shape);

					ShapeNetworkEntity entity;
					using (ActiveBusinessObjectCollection.DelayListChangedEvents(DiagramEntity.Factory))
					{
						entity = Entities.GetInstance(shape);

						ImportIntoDiagram(shape, parentEntity.Shape, shapesImported);

						if (shape.IsLinkedToRealEntity && parentEntity.Shape.IsLinkedToRealEntity)
						{
							var childProcessHeader = shape.ProcessHeader;
							var parentProcessHeader = parentEntity.ProcessHeader;

							childProcessHeader.GetOrCreateLinkToParent(parentProcessHeader);
						}
					}

					FitChildren(entity);
				}
				Refresher.Refresh(RefreshType.RedrawDiagram);

				return Entities.GetInstances((IEnumerable<INetworkEntity>)shapesImported);
			}
			return Enumerable.Empty<INetworkEntity>();
		}

		void FitChildren(ShapeNetworkEntity shape)
		{
			var minWidth = 0d;
			var minHeight = 0d;

			foreach (var child in shape.Children)
			{
				var childWidth = child.Width;
				if (childWidth == 0d)
				{
					childWidth = EntityPositionStrategy.DefaultWidth;
				}

				var childHeight = child.Height;
				if (childHeight == 0d)
				{
					childHeight = EntityPositionStrategy.DefaultHeight;
				}

				minWidth = Math.Max(minWidth, EntityPositionStrategy.ConvertXToPixels(child.X - shape.X + childWidth));
				minHeight = Math.Max(minHeight, EntityPositionStrategy.ConvertYToPixels(child.Y - shape.Y + childHeight));
			}

			shape.Width = minWidth;
			shape.Height = minHeight;
		}

		#endregion

		#region Entities And Attachments

		#region Collections

		public NetworkEntityCollection Entities
		{
			get
			{
				if (observableEntities == null)
				{
					InitialiseShapesAndAttachments();
				}
				return observableEntities;
			}
		}
		NetworkEntityCollection observableEntities;

		public BMNCNShapeCollection Shapes
		{
			get
			{
				if (shapes == null)
				{
					InitialiseShapesAndAttachments();
				}
				return shapes;
			}
		}
		BMNCNShapeCollection shapes;

		#region Initialisation

		void InitialiseShapesAndAttachments()
		{
			var factory = viewModel.Factory;

			shapes = BMNCNShapeCollection.ForDescendantShapes(DiagramShape);

			if (!temporaryNetwork)
			{
				shapes.CountChanged += Entities_CountChanged;
			}

			observableEntities = new NetworkEntityCollection(shapes, this);

			LoadRelatedEntities(factory, shapes);
			JobNetworkStrategyProvider.GetEntityCountChangedStrategy(this).HandleCountChanged(this, shapes);
		}

		void LoadRelatedEntities(BusinessObjectFactory factory, IEnumerable<BMNCNShape> entities)
		{
			var allShapes = entities.Append(DiagramShape);
			var validGuids = allShapes.Select(e => e.BNS_RelatedEntityID).Where(g => g.IsValid).ToArray();

			if (validGuids.Any())
			{
				var headers = factory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, validGuids));

				ProcessHeaderLink.LoadLinksIntoFactoryOptimisingForUnsavedProcessHeaders(factory, headers);

#if NETFRAMEWORK
				var workflows = headers.DistinctBy(w => w.FH_ParentId);
#else
				var workflows = Enumerable.DistinctBy(headers, w => w.FH_ParentId);
#endif
				foreach (var workflow in workflows)
				{
					factory.AddFetchHint(ProcessTasksSchema.Instance, new ZQuery(ProcessTasksSchema.P9_ParentID, workflow.FH_ParentId));
					workflow.AddDeepFetchHintForParentType(factory);
				}
			}

			factory.Load<BMNCNAttachment>(ExtensionMethods.GetDescendantAttachmentQuery(allShapes)); // Pre-fetch all attachments in one hit. All other calls to Factory.Load<BMNCNAttachment> should use the in-memory cache.
			factory.Load<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_BNS_ParentShape, allShapes.Select(s => s.PK))); // Pre-fetch all shapes that are children of the shapes on this network. They will already have been loaded based on the root shape, but this makes it possible to use the ChildShapes collection without extra db hits.
		}

		void Entities_CountChanged(object sender, EventArgs e)
		{
			if (!DiagramShape.IsDeleted)
			{
				JobNetworkStrategyProvider.GetEntityCountChangedStrategy(this).HandleCountChanged(this, shapes);
				observableEntities.Update();
				if (!refreshOnEntityCountChangedSuspender.IsSuspended)
				{
					Refresher.Refresh(RefreshType.EntitiesReloaded);
				}
			}
			else
			{
				OnDiagramDeleted();
				Refresher.Refresh(RefreshType.RefreshButton);
			}
		}

		readonly ActionSuspender refreshOnEntityCountChangedSuspender = new ActionSuspender();

		public IDisposable SuspendRefreshingOnEntityCountChanged()
		{
			return refreshOnEntityCountChangedSuspender.Suspend();
		}

		void OnDiagramDeleted()
		{
			Refresher.Refreshed -= Refresher_Refreshed;
			DiagramShape.BNS_RelatedEntityIDInfo.ValueChanged -= JobLink_ValueChanged;
		}

		internal void SetupEntityForDiagram(ShapeNetworkEntity entity, ShapeNetworkEntity owner)
		{
			owner.RegisterEditableChildObject(entity);

			if (!entity.Shape.IsInDatabase)
			{
				CopyRelevantDiagramPropertiesToNewShape(entity.Shape);
			}

			viewModel.DiagramShape.RegisterEditableChildObject(entity.Shape);

			if (!viewModel.DiagramShape.IsDefaultDiagram)
			{
				var processHeader = entity.ProcessHeader;
				if (processHeader != null)
				{
					entity.Shape.RegisterEditableChildObject(processHeader);
				}
			}
		}

#endregion

#endregion

		#region Operations

		void ImportIntoDiagram(BMNCNShape shape, BMNCNShape owner, IList<BMNCNShape> shapesAdded = null)
		{
			var ownerEntity = Entities.GetInstance(owner);
			var shapeEntity = Entities.GetInstance(shape);
			SetupEntityForDiagram(shapeEntity, ownerEntity);

			if (shapesAdded != null)
			{
				shapesAdded.Add(shape);
			}

			foreach (var childShape in shape.ChildShapes)
			{
				ImportIntoDiagram(childShape, shape, shapesAdded);
			}
		}

		#endregion

		#region Properties

		public ShapeNetworkEntity DiagramEntity => Entities.GetInstance(DiagramShape);

		public BMNCNShape DiagramShape => viewModel.DiagramShape;

		#endregion

#endregion

		#region Hidden Entities

		void JobLink_ValueChanged(object sender, EventArgs e)
		{
			Entities.GetInstance(viewModel.DiagramShape).HiddenEntities.Reload();
			RebuildHiddenRelationships();
		}

		void RebuildHiddenRelationships()
		{
			Entities.GetInstance(viewModel.DiagramShape).HiddenRelationships.Reload();
		}

		#endregion

		#region Relationships

		public IEntityRelationship CreateRelationship(IProposedNetworkEntity sourceEntity, IProposedNetworkEntity destEntity)
		{
			return CreateRelationship(sourceEntity, destEntity, refreshDiagram: true);
		}

		public IEntityRelationship CreateRelationship(IProposedNetworkEntity sourceEntity, IProposedNetworkEntity destEntity, bool refreshDiagram = true)
		{
			if (sourceEntity != null && destEntity != null)
			{
				var fromEntity = Entities.GetInstance(sourceEntity);
				var toEntity = Entities.GetInstance(destEntity);
				return CreateRelationship(sourceEntity, destEntity, GetOwnerForRelationship(fromEntity, toEntity).AsShape(), useAttachementValidation: true, refreshDiagram);
			}
			else
			{
				return null;
			}
		}

		public IEntityRelationship CreateRelationship(IProposedNetworkEntity sourceEntity, IProposedNetworkEntity destEntity, BMNCNShape ownerShape, bool useAttachementValidation = true, bool refreshDiagram = true)
		{
			if (sourceEntity != null && destEntity != null)
			{
				var fromEntity = Entities.GetInstance(sourceEntity);
				var toEntity = Entities.GetInstance(destEntity);
				var fromShape = fromEntity.Shape;
				var toShape = toEntity.Shape;

				if (useAttachementValidation)
				{
					var attachments = DiagramShape.Factory.Load<BMNCNAttachment>(ExtensionMethods.GetDescendantAttachmentQuery(Shapes.Append(DiagramShape)));
					var result = JobNetworkEntityRelationshipValidator.ValidateCreateDependencyRelationship(fromEntity, toEntity, Shapes.Append(DiagramShape), attachments);

					if (!result.IsValid)
					{
						var caption = Res.GetString("f0f7daf3-02b4-47af-bef3-1ab880fa13b6", "Cannot create arrow");
						Controller.UserInteractionImplementor.ShowMessage(result.FailureReason, caption);

						return null;
					}
				}

				var workflowFrom = fromShape.ProcessHeader;
				var workflowTo = toShape.ProcessHeader;
				ProcessHeaderLink link = null;

				if (workflowFrom != null && workflowTo != null)
				{
					link = workflowFrom.GetOrCreateDependencyLink(workflowTo);

					if (link.HasErrors)
					{
						var caption = Res.GetString("44512A01-7984-454B-A6AB-6321BE69069D", "Cannot create arrow");
						var errorMessage = new ZStringBuilder(link.NotificationsIncludingChildren.GetErrors().GetUniqueMessageList()).ToStringWithNewLineBetweenAppends();

						link.CancelChanges();

						Controller.UserInteractionImplementor.ShowMessage(errorMessage, caption);
						return null;
					}
				}

				var attachment = GetExistingAttachment(ownerShape, fromShape, toShape);

				if (attachment == null)
				{
					attachment = fromShape.Factory.New<BMNCNAttachment>();
					using (ActiveBusinessObjectCollection.DelayListChangedEvents(attachment.Factory))
					{
						attachment.BNA_BNS_Owner = ownerShape.PK;
						attachment.BNA_BNS_FromShape = fromShape.PK;
						attachment.BNA_BNS_ToShape = toShape.PK;
						attachment.BNA_Type = AttachmentTypeList.Codes.Dependency;
					}
				}
				else if (attachment.IsResourceDependency)
				{
					var caption = Res.GetString("c5d58e36-530c-4ce2-8b85-962855e8af1e", "Promote Resource Arrow");
					var message = Res.GetString("c20957e6-99a8-42a6-b80c-6932b3a16371", "A resource dependency already exists between [{0}] and [{1}]. Would you like to change this into a {2}?", fromShape.Name, toShape.Name, AttachmentTypeList.Descriptions.Dependency);
					var shouldPromote = Controller.UserInteractionImplementor.HasUserConfirmed(message, caption);

					if (shouldPromote)
					{
						attachment.BNA_Type = AttachmentTypeList.Codes.Dependency;
						attachment.BNA_IsDecouple = ZBool.False;
					}
					else
					{
						return null;
					}
				}
				else if (attachment.BNA_Type == AttachmentTypeList.Codes.Dependency)
				{
					if (attachment.BNA_IsDecouple)
					{
						attachment.BNA_IsDecouple = ZBool.False;
					}
				}
				else
				{
					ErrorReporter.ReportOnce("f5733e52-0b62-4ed9-a48c-f3c80ab8e1f9", string.Format(CultureInfo.InvariantCulture, "Create relationship was called and an invalid relationship exists between {0} and {1}", fromShape.Name, toShape.Name));

					return null;
				}

				if (link != null)
				{
					attachment.BNA_FP_ProcessHeaderLink = link.PK;
					link.RegisterEditableChildObject(attachment);
				}

				viewModel.DiagramShape.RegisterEditableChildObject(attachment);
				attachment.Validation.ValidateAll();

				if (refreshDiagram)
				{
					this.Refresh(RefreshType.RelationshipAdded, fromShape, toShape);
				}

				var networkAttachment = Entities.GetInstance(attachment);
				networkAttachment.Validation.ValidateAll();
				return networkAttachment;
			}

			return null;
		}

		public bool DeleteRelationship(IEntityRelationship relationship)
		{
			var attachment = relationship as BusinessObject;

			INetworkEntity from;
			INetworkEntity to;
			if (attachment != null && attachment.IsDeleted)
			{
				to = from = null;
			}
			else
			{
				from = relationship.From as INetworkEntity;
				to = relationship.To as INetworkEntity;
			}

			var result = DeleteRelationshipCore(relationship);

			if (result && from != null && to != null)
			{
				this.Refresh(RefreshType.RelationshipRemoved, from, to);
			}

			return result;
		}

		bool DeleteRelationshipCore(IEntityRelationship relationship)
		{
			var shouldDelete = true;
			var attachment = relationship.AsAttachment();
			var startingEntityCount = Entities.Count;

			if (attachment != null && !attachment.IsDeleted && relationship.From != null && relationship.To != null)
			{
				if (attachment.BNA_IsDecouple && attachment.IsApproved)
				{
					var message = Res.GetString("9983b2fb-0fe1-4f3e-b3f6-700611271682", "This arrow has already been decoupled and cannot be removed.");
					Controller.UserInteractionImplementor.ShowMessage(message);

					return false;
				}

				var fromShape = attachment.FromShape;
				var toShape = attachment.ToShape;

				if (fromShape.IsBufferShape || toShape.IsBufferShape)
				{
					HideRelationship(relationship);
					return false;
				}

				var approvedArrowOnAnotherDiagram = !attachment.IsApproved && attachment.ProcessHeaderLink != null ? attachment.ProcessHeaderLink.ApprovedArrow : null;

				if (attachment.IsApproved || approvedArrowOnAnotherDiagram != null)
				{
					string message = string.Empty;
					string caption = string.Empty;

					if (attachment.IsApproved && attachment.BNA_Type == AttachmentTypeList.Codes.ResourceDependency)
					{
						message = Res.GetString("a2ba7121-9a5b-4f81-ae02-797711ec60d6", "This is an approved arrow and cannot be deleted.");
						caption = Res.GetString("1cf87912-254e-4274-a358-747164654100", "Unable to delete approved arrow");
						Controller.UserInteractionImplementor.ShowMessage(message, caption);
						return false;
					}
					else
					{
						message = attachment.IsApproved
							? Res.GetString("e16a704c-dd3a-4306-b55a-b66eea2f302e", "This is an approved arrow and cannot be deleted. Would you like to decouple the arrow instead?")
							: Res.GetString("92b98c30-612a-4208-8b6d-9a1b39c721ee", "This arrow represents a dependency which has been approved on a project plan. Deleting the arrow on this diagram will cause the approved arrow to be decoupled.");

						message += System.Environment.NewLine + System.Environment.NewLine + Res.GetString("4e39ba48-b4bd-4419-9d81-93b9fa967796", "Decoupling causes the dependency link to be removed but the arrow to be preserved on the project plan.");
						caption = Res.GetString("735ba76d-3c22-4a6c-9d10-fe0f1c51f517", "Decouple arrow instead");

						var shouldDecouple = Controller.UserInteractionImplementor.HasUserConfirmed(message, caption);
						if (shouldDecouple)
						{
							if (approvedArrowOnAnotherDiagram != null)
							{
								approvedArrowOnAnotherDiagram.Decouple();
								return true;
							}
							else
							{
								attachment.Decouple();
							}
						}

						return false;
					}
				}

				shouldDelete = !JobNetworkStrategyProvider.GetUpdateEntitiesStrategy(this).IsPresentOnMultipleDiagrams(attachment);
				if (!shouldDelete)
				{
					var message = Res.GetString("b10d55fe-5314-4f81-ae4b-c5a64416fdb6", "This arrow is present on other diagrams and cannot be deleted. Would you like to hide the arrow instead?");
					var caption = Res.GetString("0391556f-f08e-4b46-b7f9-62345aa17e25", "Hide arrow instead");
					var hideInstead = Controller.UserInteractionImplementor.HasUserConfirmed(message, caption);

					if (hideInstead)
					{
						HideRelationship(relationship);
						return true;
					}
					else
					{
						return false;
					}
				}
			}

			if (attachment == null || (attachment != null && !attachment.IsDeleted))
			{
				shouldDelete = SeekUserConfirmationForDeletingRelationship(relationship, attachment);
			}

			if (shouldDelete)
			{
				DeleteRelationshipOrAttachment(relationship, attachment);
			}

			if (startingEntityCount != Entities.Count)
			{
				this.Refresh(RefreshType.RedrawDiagram);
			}

			return shouldDelete;
		}

		bool SeekUserConfirmationForDeletingRelationship(IEntityRelationship relationship, BMNCNAttachment attachment)
		{
			var message = Res.GetString("c7148320-1339-4568-885b-c43f353ded9c", "Delete link between shapes '{0}' and '{1}'?", relationship.From.Name, relationship.To.Name);
			var caption = Res.GetString("fdcdf38b-281b-491a-a1d8-a7562a963fa3", "Confirm Delete");

			if (attachment != null && !attachment.BNA_FP_ProcessHeaderLink.IsEmpty)
			{
				message += " " + Res.GetString("87664d4e-ff2c-477a-88d6-ad0a27c4108d", "This will also remove the pre-requisite relationship between the linked workflows.");
			}

			return Controller.UserInteractionImplementor.HasUserConfirmed(message, caption);
		}

		void DeleteRelationshipOrAttachment(IEntityRelationship relationship, BMNCNAttachment attachment)
		{
			if (attachment != null && !attachment.IsDeleted)
			{
				if (attachment.ProcessHeaderLink != null)
				{
					attachment.ProcessHeaderLink.Delete();
				}
				if (!attachment.IsDeleted)
				{
					attachment.Delete();
				}
			}
			else
			{
				var link = relationship as ProcessHeaderLink;
				if (link != null)
				{
					link.Delete();
				}
			}
		}

		public bool HideRelationship(IEntityRelationship relationship)
		{
			var networkAttachment = Entities.GetInstance(relationship);
			if (networkAttachment != null && !networkAttachment.Attachment.IsDeleted)
			{
				var attachment = networkAttachment.Attachment;
				if (attachment.IsApproved)
				{
					var message = Res.GetString("0c3729ce-4b66-4081-a748-96963610c703", "This arrow cannot be hidden as has been approved by [{0}].", attachment.ApprovedBy.GS_FullName);
					Controller.UserInteractionImplementor.ShowMessage(message);

					return false;
				}

				var startingEntityCount = Entities.Count;

				var fromShape = networkAttachment.From;
				var toShape = networkAttachment.To;

				var fromShapeOwner = fromShape.Owner;
				var toShapeOwner = toShape.Owner;

				var continueWithHide = true;

				if (fromShape.IsBufferShape || toShape.IsBufferShape)
				{
					var message = Res.GetString("765bc114-300b-4039-b6f8-d76090fee979", "This arrow cannot be removed independently from the connected buffer. Would you like to remove the buffer?");
					var caption = Res.GetString("a692c643-1cde-4539-9566-f6e8503ac04a", "Hide buffer instead");

					if (!Controller.UserInteractionImplementor.HasUserConfirmed(message, caption))
					{
						continueWithHide = false;
					}
				}

				if (continueWithHide)
				{
					attachment.Delete();

					if (fromShapeOwner != null)
					{
						fromShapeOwner.HiddenRelationships.Reload();
					}
					if (toShapeOwner != null)
					{
						toShapeOwner.HiddenRelationships.Reload();
					}

					if (startingEntityCount != Entities.Count)
					{
						this.Refresh(RefreshType.RedrawDiagram);
					}
				}

				return true;
			}

			return false;
		}

		public IEntityRelationship ShowRelationship(IProposedNetworkEntity sourceEntity, IProposedNetworkEntity destEntity)
		{
			return ShowRelationship(sourceEntity, destEntity, refreshDiagram: true);
		}

		public IEntityRelationship ShowRelationship(IProposedNetworkEntity sourceEntity, IProposedNetworkEntity destEntity, bool refreshDiagram = true)
		{
			var attachment = CreateRelationship(sourceEntity, destEntity, refreshDiagram).AsAttachment();

			var link = attachment != null ? attachment.ProcessHeaderLink : null;
			var fromEntity = Entities.GetInstance(sourceEntity);
			var toEntity = Entities.GetInstance(destEntity);

			if (link != null)
			{
				var fromParent = (fromEntity != null ? fromEntity.Owner : null) ?? DiagramEntity;
				var toParent = (toEntity != null ? toEntity.Owner : null) ?? DiagramEntity;

				if (fromParent.HiddenRelationships.Contains(link))
				{
					fromParent.HiddenRelationships.Remove(link);
				}
				if (toParent.HiddenRelationships.Contains(link))
				{
					toParent.HiddenRelationships.Remove(link);
				}
			}

			return attachment != null ? Entities.GetInstance(attachment) : null;
		}

		public IEntityRelationship GetRelationship(IProposedNetworkEntity sourceEntity, IProposedNetworkEntity destEntity)
		{
			var fromEntity = Entities.GetInstance(sourceEntity);
			var toEntity = Entities.GetInstance(destEntity);
			var fromShape = fromEntity.Shape;
			var toShape = toEntity.Shape;

			var ownerEntity = GetOwnerForRelationship(fromEntity, toEntity);

			var attachment = GetExistingAttachment(ownerEntity.Shape, fromShape, toShape);

			NetworkAttachment networkAttachment = null;

			if (attachment != null)
			{
				networkAttachment = Entities.GetInstance(attachment);
			}

			return networkAttachment;
		}

		#region Implementation

		ShapeNetworkEntity GetOwnerForRelationship(ShapeNetworkEntity sourceShape, ShapeNetworkEntity destShape)
		{
			return GetOwnerForRelationship(sourceShape, destShape, DiagramEntity);
		}

		/// <summary>
		/// Find the best owner shape for an attachment by finding their closest shared ancestor.
		/// </summary>
		internal static ShapeNetworkEntity GetOwnerForRelationship(ShapeNetworkEntity sourceShape, ShapeNetworkEntity destShape, ShapeNetworkEntity rootShape)
		{
			if (sourceShape.Owner == null || destShape.Owner == null)
			{
				return rootShape;
			}
			else if (sourceShape.Owner == destShape.Owner)
			{
				return sourceShape.Owner;
			}
			else
			{
				var sourceShapeOwners = sourceShape.GetOwnersUpHierarchy().ToArray();
				var destShapeOwners = destShape.GetOwnersUpHierarchy().ToArray();

				return sourceShapeOwners.First(s => destShapeOwners.Contains(s));
			}
		}

		BMNCNAttachment GetExistingAttachment(BMNCNShape owner, BMNCNShape fromShape, BMNCNShape toShape)
		{
			var query = new ZQuery(BMNCNAttachmentSchema.BNA_BNS_Owner, owner.PK);
			query.AddToFilter(BMNCNAttachmentSchema.BNA_BNS_FromShape, fromShape.PK);
			query.AddToFilter(BMNCNAttachmentSchema.BNA_BNS_ToShape, toShape.PK);

			return owner.Factory.LoadTop1<BMNCNAttachment>(query);
		}

		#endregion

		#endregion

		#region Properties

		INetworkEntityController INetwork.Controller => Controller;
		public IBMNetworkEntityController Controller { get; private set; }

		public string Name { get; set; }

		public string EntityTypeDescription
		{
			get { return Res.GetString("c9a8560a-7b28-4e50-b2f8-afd9cad34ed9", "Shape"); }
		}

		public NetworkActions SupportedActions
		{
			get { return JobNetworkStrategyProvider.GetSupportedActionsStrategy(this).GetSupportedActions(); }
		}

		#endregion

		#region Refresh

		public INetworkRefresher Refresher { get; private set; }

		void Refresher_Refreshed(object sender, RefreshArgs args)
		{
			switch (args.RefreshType)
			{
				case RefreshType.EntityAdded:
				case RefreshType.EntitySize:
				case RefreshType.RelationshipAdded:
				case RefreshType.RelationshipRemoved:
				case RefreshType.EntityRemoved:
					schedulesNeedRecalculation = true;
					break;
				case RefreshType.EntityEdited:
				case RefreshType.ResourceDependencyAdded:
				case RefreshType.RedrawDiagram:
					schedulesNeedRecalculation = true;
					scaleDescriptor = null;
					break;
				case RefreshType.RefreshButton:
					schedulesNeedRecalculation = true;
					RefreshSchedules();
					scaleDescriptor = null;
					break;
				case RefreshType.Saving:
					OnBeforeSave();
					break;
				case RefreshType.Affinities:
				case RefreshType.AffinitiesRefreshRequired:
				case RefreshType.EntityCoordinates:
				case RefreshType.EntitiesMovedToDiagramSection:
				case RefreshType.EntitiesReloaded:
				case RefreshType.EntityPinnedOrUnpinned:
				case RefreshType.EntityApprovedOrUnapproved:
				case RefreshType.EntityStatusChanged:
				case RefreshType.ConnectionAdded:
				case RefreshType.Close:
				case RefreshType.TextColorUpdated:
				case RefreshType.ShapeInspectorVisibilityChanged:
				case RefreshType.ToogleFreezeChannelHeadersAndTimeLabels:
				case RefreshType.ChannelView:
					// These do not require any state changes.
					break;
				case RefreshType.Saved:
					scaleDescriptor = null;
					break;
				case RefreshType.Scale:
					scaleDescriptor = null;
					break;
				default:
					throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "The refresh type [{0}] is unhandled", args.RefreshType));
			}
		}

		public void FullRefresh()
		{
			JobNetworkStrategyProvider.GetUpdateEntitiesStrategy(this).OnFullRefresh();
			Refresher.Refresh(RefreshType.RedrawDiagram);
		}

		public void RefreshSchedules(bool forceReCalculation = false)
		{
			// Don't re-calculate schedules for non-root shapes - scaled diagram schedules must only be edited in full context of the root diagram and not when a nested shape is opened in its own form.
			if (DiagramShape.IsScaled && !DiagramShape.HasParent && (forceReCalculation || SchedulesNeedRecalculation()))
			{
				var requiredDurationForRootDiagram = Entities.GetInstance(DiagramShape).CalculateRequiredDurationToAccommodateScheduleAndChildShapes();

				if (requiredDurationForRootDiagram != null)
				{
					DiagramShape.ExplicitDurationMinutes = (int)requiredDurationForRootDiagram.Value.TotalMinutes;
				}

				new AsyncShapeFloatCalculator().RunTransform(this, new SynchronousActionExecutionStrategy());
				schedulesNeedRecalculation = false;
			}
		}

		internal bool SchedulesNeedRecalculation()
		{
			return schedulesNeedRecalculation || ScheduleHasChanges(DiagramShape) || Shapes.Any(ScheduleHasChanges);
		}

		static bool ScheduleHasChanges(BMNCNShape shape)
		{
			return shape?.ScheduleBizo?.HasChanges ?? false;
		}

		bool schedulesNeedRecalculation;

		internal ShapeNetworkEntity GetRightmostEntity()
		{
			return Entities.Cast<ShapeNetworkEntity>().Where(e => e.CanHaveSchedule).MaxBySafe(e => e.X + e.Width);
		}

		public bool DiagramContainsSchedulingConflicts { get; internal set; }

		#endregion

		#region On Save

		internal void OnBeforeSave()
		{
			JobNetworkStrategyProvider.GetSaveStrategy(this).OnBeforeSave();
		}

		#endregion

		#region Entity Actions

		public void ShowNetworkDiagramsModule(INetworkEntity entity)
		{
			Controller.ShowNetworkDiagramsModule(entity);
		}

		public void LinkEntity(IProposedNetworkEntity entity, ModuleIdentifier moduleIDForLinkedEntityType)
		{
			var entityToLink = Controller.PickEntity(moduleIDForLinkedEntityType, moduleIDForLinkedEntityType == ModuleIDs.NetworkDiagram);

			if (entityToLink != null)
			{
				var entityToLinkInCorrectFactory = DiagramShape.Factory.Load(entityToLink.TablePrefix, entityToLink.PK);

				if (LinkEntity(entity.AsShape(), entityToLinkInCorrectFactory))
				{
					this.Refresh(RefreshType.RedrawDiagram);
				}
			}
		}

		public bool LinkEntity(ShapeNetworkEntity entity, BusinessObject entityToLink)
		{
			return LinkEntity(entity.Shape, entityToLink);
		}

		bool IJobNetwork.LinkEntity(BMNCNShape shape, BusinessObject entityToLink) => LinkEntity(shape, entityToLink);

		public bool LinkEntity(BMNCNShape shape, BusinessObject entityToLink, bool? shouldSynchroniseSchedule = null, bool autoShowRelationships = true)
		{
			if (entityToLink != null)
			{
				var result = JobNetworkEntityRelationshipValidator.GetLinkFailureMessage(entityToLink, Entities.GetInstance(shape), DiagramEntity, Entities.ShapeEntities);

				if (result.IsValid)
				{
					if (entityToLink is BMNCNShape shapeToLink && shapeToLink.IsScaled && shape.IsScaled)
					{
						if (shouldSynchroniseSchedule.HasValue)
						{
							shape.ShouldSynchroniseScheduleWithLinkedEntity = shouldSynchroniseSchedule.Value;
						}
						else if (!PromptUserToSpecifyShouldSynchroniseScheduleValue(shape))
						{
							return false;
						}

						if (shape.ShouldSynchroniseScheduleWithLinkedEntity && !ConfirmLinkingIfOtherShapesCanAffectEntitySchedule(shapeToLink))
						{
							return false;
						}
					}

					var showChildren = false;
					var shapeToChildHeaders = new Dictionary<BMNCNShape, List<ProcessHeader>>
					{
						{ shape, new List<ProcessHeader>() }
					};

					if (!shape.IsDiagram && entityToLink is ProcessHeader processHeader)
					{
						var headersToCheck = processHeader.ChildHeaders.Prepend(processHeader)
																		.Where(header => header.PrerequisiteLinks.Any() || header.PostrequisiteLinks.Any())
																		.ToHashSet();

						foreach (var existingShape in Shapes.Where(s => s.IsLinkedToRealEntity))
						{
							var (headersOfEntityToBeLinked, childHeadersOfExistingEntity) = GetChildHeadersForNewlyLinkedEntityAndExistingShapeToDisplay(headersToCheck, processHeader, existingShape);

							shapeToChildHeaders[shape].AddRange(headersOfEntityToBeLinked);

							if (childHeadersOfExistingEntity.Count > 0)
							{
								shapeToChildHeaders.Add(existingShape, childHeadersOfExistingEntity.ToList());
							}
						}

						shapeToChildHeaders[shape] = shapeToChildHeaders[shape]
														.Distinct()
														.ToList();
					}

					if (shape.IsDiagram || shapeToChildHeaders.Count > 1 || shapeToChildHeaders[shape].Count > 0)
					{
						switch (ShouldProceedLinkWithShowChildrenOrCancelLink())
						{
							case ZDialogResult.Yes:
								showChildren = true;
								break;
							case ZDialogResult.Cancel:
								return false;
							case ZDialogResult.No:
							default:
								break;
						}
					}

					JobNetworkStrategyProvider.GetLinkingStrategy(shape).LinkEntity(this, shape, entityToLink);

					if (showChildren)
					{
						if (shape.IsDiagram)
						{
							ShowChildEntities(shape);
						}
						else
						{
							foreach (var shapeHeaderCollectionPair in shapeToChildHeaders)
							{
								foreach (var entity in shapeHeaderCollectionPair.Value)
								{
									ShowEntity(entity, shapeHeaderCollectionPair.Key);
								}
							}
						}
					}

					if (autoShowRelationships)
					{
						ShowRelationshipsOfShownEntity((IProposedNetworkEntity)entityToLink, shape, refreshDiagram: false);
					}

					this.Refresh(RefreshType.EntityEdited);

					return true;
				}
				else if (!result.FailureReason.IsEmpty)
				{
					Controller.UserInteractionImplementor.ShowMessage(result.FailureReason);
				}
			}

			return false;
		}

		(IReadOnlyCollection<ProcessHeader>, IReadOnlyCollection<ProcessHeader>) GetChildHeadersForNewlyLinkedEntityAndExistingShapeToDisplay(IEnumerable<ProcessHeader> headersToCheck, ProcessHeader targetHeader, BMNCNShape shapeToCheck)
		{
			IEnumerable<ProcessHeader> GetDependencies(ProcessHeader h) => h.Prerequisites().Union(h.Postrequisites());

			var headerOfExistingShape = shapeToCheck.ProcessHeader;

			var headerToDependencies = headerOfExistingShape.ChildHeaders
				.Append(headerOfExistingShape)
				.ToDictionary(h => h, h => GetDependencies(h)
				.Intersect(headersToCheck));

			var headersOfTargetEntityThatMayNeedToBeDisplayed = headerToDependencies.Values
				.SelectMany(x => x)
				.Distinct()
				.ToList();

			if (headersOfTargetEntityThatMayNeedToBeDisplayed.Count == 0)
			{
				return (new List<ProcessHeader>(), new List<ProcessHeader>());
			}

			var headersOfTargetEntityToDisplay = headersOfTargetEntityThatMayNeedToBeDisplayed
				.Except(targetHeader)
				.ToList();

			var childHeadersOfExistingEntityToDisplay = headerToDependencies.Keys
				.Except(headerOfExistingShape)
				.Except(shapeToCheck.ChildShapes.Select(s => s.ProcessHeader))
				.Where(h => headerToDependencies[h].Any())
				.ToList();

			return (headersOfTargetEntityToDisplay, childHeadersOfExistingEntityToDisplay);
		}

		void ShowChildEntities(BMNCNShape parentShape)
		{
			BusinessObject linkedEntity;
			if (parentShape.LinkedEntity is BMNCNRootDiagramShape)
			{
				linkedEntity = ((BMNCNShape)parentShape.LinkedEntity).LinkedEntity;
			}
			else
			{
				linkedEntity = parentShape.LinkedEntity;
			}

			if (linkedEntity == null)
			{
				return;
			}

			var parentEntity = (ProcessHeader)linkedEntity;

			if (parentEntity.ChildHeaders.IsNullOrEmpty())
			{
				return;
			}

			foreach (var entity in parentEntity.ChildHeaders)
			{
				ShowEntity(entity, parentShape);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "This code is only ever called from the view model layer.")]
#if DEBUG
		public
#endif
		ZDialogResult ShouldProceedLinkWithShowChildrenOrCancelLink()
		{
			var message = Res.GetString("d7082d99-b3f2-4239-919a-db6c6c0a37db", @"The entity being linked has associated workflow dependencies (pre/post-requisites and/or child entities). Do you want to add all child entities to the diagram that are necessary to display the dependencies in full? If you choose ""No"" the direct pre/post-requisite links (if any) between the displayed entities will be added only.");
			var caption = Res.GetString("ebe2d84e-8a6a-4cd6-9c0d-986502f35201", "Show all child entities");
			var context = new DialogDefaultContext(new ZGuid("8d53fab9-5ddc-46d6-9a8d-2c72e341c4bd"), caption, ZMessageBoxButtons.YesNoCancel, ZMessageBoxIcon.Information, showCheckboxOnly: false, ZDialogResult.No);

			return Globals.Message.ShowOrDefault(context, message);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "This code is only ever called from the view model layer.")]
		static bool PromptUserToSpecifyShouldSynchroniseScheduleValue(BMNCNShape shape)
		{
			var caption = Res.GetString("bc69cd2b-5b0e-4cc5-809b-1cf548021a5a", "Synchronize Diagram Schedule");
			var message = Res.GetString("2c8511ac-6977-4450-884f-fca38c51412f", "Do you want to keep this diagram's schedule in sync with the shape to which you are linking it?");

			var result = Globals.Message.Show(message, caption, ZMessageBoxButtons.YesNoCancel, ZDialogResult.Yes); // This code is only ever called from the view model layer.

			if (result == ZDialogResult.Cancel)
			{
				return false;
			}
			else
			{
				shape.ShouldSynchroniseScheduleWithLinkedEntity = result == ZDialogResult.Yes;
				return true;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "This code is only ever called from the view model layer.")]
		static bool ConfirmLinkingIfOtherShapesCanAffectEntitySchedule(BMNCNShape diagramToLink)
		{
			bool otherShapesCanAffectEntitySchedule = BMNCNShape
				.FindShapesWhichCanPropagateScheduleToLinkedEntity(diagramToLink.Factory, diagramToLink.PK)
				.Any();

			if (otherShapesCanAffectEntitySchedule)
			{
				var dialogCaption = Res.GetString("04ba852e-e897-4d1c-b0db-865dbd076f66", "Confirm linking entities");
				var dialogMessage = Res.GetString("bc317182-8e00-40e2-86f3-acbab1dd8412", "There are shapes on other diagrams that synchronize their schedules with the selected diagram. The linked diagram's Scheduled Start and Scheduled Finish will be overwritten when changes are made to any of those conflicting diagrams.\r\n\r\nDo you wish to proceed?");

				var dialogResult = Globals.Message.Show(dialogMessage, dialogCaption, ZMessageBoxButtons.YesNo, ZDialogResult.Yes); // This code is only ever called from the view model layer.

				return dialogResult == ZDialogResult.Yes;
			}

			return true;
		}

		public void UnlinkEntity(IProposedNetworkEntity entity)
		{
			UnlinkEntity(entity.AsShape(), true);
		}

		internal bool UnlinkEntity(BMNCNShape shape, bool allowRefresh, UnlinkEntityVariant userSelection = UnlinkEntityVariant.None)
		{
			var variant = userSelection == UnlinkEntityVariant.None ? GetUnLinkVariant(shape) : userSelection;

			if (variant != UnlinkEntityVariant.None && variant != UnlinkEntityVariant.Cancel)
			{
				bool isUnlinkSuccessful = JobNetworkStrategyProvider.GetLinkingStrategy(shape).UnlinkEntity(this, shape, variant);

				if (!isUnlinkSuccessful)
				{
					return false;
				}

				Entities.GetInstance(shape).HiddenEntities.Reload();
				Entities.GetInstance(shape).HiddenRelationships.Reload();

				if (allowRefresh)
				{
					this.Refresh(RefreshType.RedrawDiagram);
				}

				return true;
			}
			else
			{
				return false;
			}
		}

		UnlinkEntityVariant GetUnLinkVariant(BMNCNShape shape)
		{
			if (shape.RelatedShape != null)
			{
				return UnlinkEntityVariant.UnLinkRelatedShape;
			}

			var workflowText = shape.ProcessHeader.ProcessHeaderType;
			var strips = GetUnlinkVariants(shape, workflowText).ToArray();

			if (strips.Length > 2)
			{
				return GetUnLinkVariantFromUser(strips, shape, workflowText);
			}
			else
			{
				return UnlinkEntityVariant.PersistHeaderLinks;
			}
		}

		protected virtual UnlinkEntityVariant GetUnLinkVariantFromUser(ButtonStripAction<UnlinkEntityVariant>[] strips, BMNCNShape shape, ZString workflowText)
		{
			var message = Res.GetString("bfc4d47b-254a-44df-9f2c-55c1d8e8cc2d", "You have chosen to Unlink [{0}]. The underlying {1} still has dependencies. What do you want to do with these existing dependencies?", shape.Name, workflowText);
			var caption = Res.GetString("1f3c3939-ca21-4ae9-9ee4-9a75b3d5ed8b", "Choose un-link Action");

			return Controller.UserInteractionImplementor.ShowMultiOptionDialog(message, caption, UnlinkEntityVariant.PersistHeaderLinks, strips);
		}

		static IEnumerable<ButtonStripAction<UnlinkEntityVariant>> GetUnlinkVariants(BMNCNShape entity, string workflowText)
		{
			if (entity.DependencyAttachments.Any(a => a.BNA_FP_ProcessHeaderLink.IsValid))
			{
				yield return new ButtonStripAction<UnlinkEntityVariant>
				{
					Response = UnlinkEntityVariant.DeleteHeaderLinks,
					Text = Res.GetString("8b9dd148-4769-4744-b094-9a28208be25f", "Delete Existing Links on {0}", workflowText),
				};
			}

			yield return new ButtonStripAction<UnlinkEntityVariant>
			{
				Response = UnlinkEntityVariant.PersistHeaderLinks,
				Text = Res.GetString("07a613e4-9a4e-44aa-ac53-3612f1e618e4", "Keep Existing Links on {0}", workflowText),
			};

			yield return new ButtonStripAction<UnlinkEntityVariant>
			{
				Response = UnlinkEntityVariant.Cancel,
				Text = Res.GetString("b99a0580-8118-4a2c-9838-407d2fcde82a", "Cancel"),
			};
		}

		public void OpenLinkedEntity(BMNCNShape shape)
		{
			var linkedEntity = shape.LinkedEntity;
			var controller = shape.GetLinkedEntityControllerID();

			Controller.OpenLinkedEntity(linkedEntity, controller);
		}

		public void EditEntity(IProposedNetworkEntity proposedEntity)
		{
			var entity = Entities.GetInstance(proposedEntity);

			diagramScheduledFinishTimeUtc = DiagramShape.ScheduledFinishTimeUtc;
			diagramScheduledStartTimeUtc = DiagramShape.ScheduledStartTimeUtc;
			var currentExplicitDurationMinutes = DiagramShape.ExplicitDurationMinutes;
			EditEntityAndCheckDiagramDateRange(entity);
			if (CheckShapesFitToDiagramRange(currentExplicitDurationMinutes))
			{
				JobNetworkStrategyProvider.GetUpdateEntitiesStrategy(this).OnEntityEdited();
				this.Refresh(RefreshType.EntityEdited, (INetworkEntity)entity);
			}
		}
		ZDateTime diagramScheduledFinishTimeUtc;
		ZDateTime diagramScheduledStartTimeUtc;

		void EditEntityAndCheckDiagramDateRange(ShapeNetworkEntity entity)
		{
			Controller.EditEntity(entity);

			if (entity.IsDiagram && entity.IsDiagramScaled)
			{
				var requiredDurationForRootDiagram = Entities.GetInstance(DiagramShape).CalculateRequiredDurationToAccommodateScheduleAndChildShapes();
				if (requiredDurationForRootDiagram != null)
				{
					DiagramShape.ExplicitDurationMinutes = (int)requiredDurationForRootDiagram.Value.TotalMinutes;
				}

				var lastShape = DiagramEntity.Children.OrderByDescending(node => node.X).FirstOrDefault();
				if (lastShape != null && (lastShape.X + lastShape.Width > DiagramEntity.Width) && DiagramShape.ScheduledStartTimeUtc == diagramScheduledStartTimeUtc)
				{
					var message = Res.GetString("E2E2ACCC-4C14-4007-9454-DDE9D2B17B6E",
						@"The diagram schedule dates have changed and this diagram contains shapes which are outside the new date range. 
Press OK if you want to move these shapes inside the diagram bounds. Otherwise press Cancel and modify the diagram Schedule Start/Finish Date.");

					var caption = Res.GetString("796BE5FB-10CD-4C6D-86AD-B23A382836AF", "Confirmation Required");

					if (!Controller.UserInteractionImplementor.HasUserConfirmed(message, caption))
					{
						EditEntityAndCheckDiagramDateRange(entity);
					}
				}
			}
		}

		bool CheckShapesFitToDiagramRange(ZInt currentExplicitDurationMinutes)
		{
			if (DiagramEntity.IsDiagramSurfaceFixed && DiagramEntity.Children.Any())
			{
				var maxWidthFromChildShapes = DiagramEntity.Children.Max(x => x.Width);
				if (DiagramEntity.Width <= maxWidthFromChildShapes)
				{
					DiagramShape.ScheduledStartTimeUtc = diagramScheduledStartTimeUtc;
					DiagramShape.ScheduledFinishTimeUtc = diagramScheduledFinishTimeUtc;
					DiagramShape.ExplicitDurationMinutes = currentExplicitDurationMinutes;

					var message = Res.GetString("854F0DAB-7AD0-4814-B4E9-16E2BC8A048B", "The time range of a diagram is too small to fit the biggest shape on this diagram and new schedule dates cannot be committed.");
					Controller.UserInteractionImplementor.ShowError(message, Res.GetString("579ADC06-E933-4CA1-B517-3A8627B56452", "Cannot resize diagram"));
					return false;
				}
			}
			return true;
		}

		public void ModifyAffinities(IDiagramEntity diagramEntity)
		{
			var entity = Entities.GetInstance(diagramEntity);
			Controller.ModifyAffinities(entity);
			ReloadAffinitiesOnEntitiesAndRoot();
		}

		void ReloadAffinitiesOnEntitiesAndRoot()
		{
			((INetworkEntity)DiagramEntity).AppliedAffinities.Reload();
			((INetworkEntity)DiagramEntity).AvailableAffinities.Reload();

			foreach (var entity in Entities)
			{
				entity.AvailableAffinities.Reload();
				entity.AppliedAffinities.Reload();
			}
		}

		#endregion

		#region Scale

		public void SwitchToScaled()
		{
			var diagram = viewModel.DiagramShape;
			SwitchToScaled(diagram);
			SwitchToScaled(diagram, Entities.ShapeEntities.ToArray());
		}

		public static void SwitchToScaled(BMNCNShape diagramShape)
		{
			diagramShape.SwitchToScaled();

			var header = diagramShape.ProcessHeader;

			if (header != null)
			{
				if (diagramShape.ScheduledStartTimeUtc.IsEmpty)
				{
					diagramShape.ScheduledStartTimeUtc = header.FH_DoNotStartBeforeDate;
				}

				if (diagramShape.ScheduledFinishTimeUtc.IsEmpty)
				{
					diagramShape.ScheduledFinishTimeUtc = header.FH_AgreedDeliveryDate;
				}
			}
		}

		void SwitchToScaled(BMNCNShape diagramShape, IEnumerable<ShapeNetworkEntity> descendantEntities)
		{
			var allEntities = descendantEntities.Append(DiagramEntity).ToArray();

			var disposable = new DisposableList(allEntities.Select(e => ((ISingleElementListInternal)e).SuspendListChanged()));
			try
			{
				try
				{
					allEntities.ForEach(e => e.SuspendCalculation());

					foreach (var entity in descendantEntities)
					{
						var shape = entity.Shape;
						shape.IsScaled = diagramShape.IsScaled;
						shape.Scale = diagramShape.Scale;
						shape.ResolutionIncrement = diagramShape.ResolutionIncrement;
						shape.IsReadOnly = ZBool.True;

						var durationInMinutes = GetDefaultDurationInMinutes(shape);

						entity.X = ShapeOffsetToDateConverter.GetMinutesForScaleSize(diagramShape.Scale, (double)shape.Left);
						entity.AdjustLocationToScale(DiagramEntity);

						if (durationInMinutes != shape.ExplicitDurationMinutes)
						{
							entity.SetWidthForDuration(durationInMinutes);
						}
						entity.Width = entity.GetScaleWidth(DiagramEntity);
					}
				}
				finally
				{
					allEntities.ForEach(e => e.ResumeCalculation());
				}

				ShapeFloatCalculator.CalculateFloatForChildren(this);
				PushShapesProvider.PushAllEntities(this, PushDirection.Early);
			}
			finally
			{
				disposable.Dispose();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		int GetDefaultDurationInMinutes(BMNCNShape shape)
		{
			var processHeader = shape.ProcessHeader;
			if (processHeader != null && processHeader.ImplicitDurationHours > 0)
			{
				return (int)(processHeader.ImplicitDurationHours * 60);
			}
			else
			{
				// TODO: Respect the shape widths set by the user a little more.
				var resolutionIncrementMinutes = viewModel.DiagramShape.ResolutionIncrement.GetMinutesFromDateTimeSpan();
				return (int)(3 * resolutionIncrementMinutes);
			}
		}

		public INetworkScaleDescriptor ScaleDescriptor
		{
			get { return scaleDescriptor ?? (scaleDescriptor = DiagramShape.IsScaled ? new NetworkScaleDescriptor(this) : null); }
		}

		INetworkScaleDescriptor scaleDescriptor;

		#endregion

		#region IsReadOnly

		public bool IsReadOnly
		{
			get { return isReadOnly ?? viewModel.DiagramShape.IsReadOnly; }
			set
			{
				isReadOnly = value;
				OnPropertyChanged();
			}
		}

		bool? isReadOnly;

		#endregion

		#region Critical Chain

		public IEnumerable<ShapeNetworkEntity> GetCriticalChain()
		{
			return from entity in Entities.ShapeEntities
				   where entity.IsOnCriticalPath
				   orderby entity.X
				   select entity;
		}

		public TimeSpan GetCriticalChainDuration()
		{
			return TimeSpan.FromMinutes(GetCriticalChain().AsShapes().Sum(s => s.ExplicitDurationMinutes));
		}

		#endregion

		#region Validation

		readonly IJobNetworkValidator validator;

		public bool ValidateAndCheckThereAreNoErrors()
		{
			validator.Validate(this);
			return !DiagramShape.HasErrors && !DiagramEntity.HasErrors;
		}

		public bool ValidateLoopsAndCheckThereAreNoErrors(string progressReporterCaption = null)
		{
			validator.ValidateLoops(this, progressReporterCaption);
			return !DiagramEntity.HasErrors;
		}

		#endregion

		#region Notification

		public IDisposable DelayPropertyChanged()
		{
			var entities = new List<ShapeNetworkEntity>(Entities.Cast<ShapeNetworkEntity>().Concat(new[] { DiagramEntity }));
			var disposables = new DisposableList(entities.Count * 2);

			//OnPropertyChanged on shapes triggers OnPropertyChanged on ShapeNetworkEntities therefore shapes should release delayed property changes before ShapeNetworkEntities in order to avoid double notification
			foreach (var entity in entities)
			{
				disposables.Add(entity.Shape.DelayPropertyChanged());
			}

			foreach (var entity in entities)
			{
				disposables.Add(entity.DelayPropertyChanged());
			}

			return disposables;
		}

		#region INotifyPropertyChanged Members

		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#endregion

		#region For test

#if DEBUG

		public BMNCNShape this[string name]
		{
			get { return Shapes.FirstOrDefault(s => s.Name == name); }
		}

		public event EventHandler<RefreshArgs> Refreshed
		{
			add { Refresher.Refreshed += value; }
			remove { Refresher.Refreshed -= value; }
		}

#endif

		#endregion

		#region Paste

		public bool TryHandlePaste(IEnumerable<INetworkEntity> selectedEntities)
		{
			var count = selectedEntities.Count();
			var selectedShape = count == 0 ? DiagramShape : selectedEntities.AsShapes().FirstOrDefault();

			if (count > 1)
			{
				return false;
			}

			if (selectedShape.BNS_ShapeType != ShapeTypeList.Codes.Diagram &&
				selectedShape.BNS_ShapeType != ShapeTypeList.Codes.Shape)
			{
				return false;
			}

			var results = this.GetJobHeadersFromClipboard(DiagramShape.Factory).ToArray();

			if (results.Length != 1)
			{
				return false;
			}

			var result = results.First();

			if (!result.IsValid)
			{
				return false;
			}

			return LinkEntity(selectedShape, result.EntityToLink);
		}

		#endregion

		#region INetwork Members

		IObservableReloadableCollection<INetworkEntity> INetwork.Entities => Entities;

		IDiagramEntity INetwork.DiagramEntity => DiagramEntity;

		void INetwork.ViewEntity(IProposedNetworkEntity entity) => OpenLinkedEntity(entity.AsShape());

		#region GetCreateEntityActions

		IEnumerable<INetworkAction> INetwork.GetCreateEntityActions(INetworkViewModel networkViewModel)
		{
			var group = 0;

			yield return new CreateWorkflowAction(networkViewModel, group, shouldUpdateOnNetworkEvents: false);
			yield return new CreateShapeAction(networkViewModel, group, shouldUpdateOnNetworkEvents: false);

			group++;
			yield return new CreateShapeFromClipboardAction(networkViewModel, group, shouldUpdateOnNetworkEvents: false);
			yield return new CreateJobActionCollection(networkViewModel, group, shouldUpdateOnNetworkEvents: false);

			group++;
			yield return new CreateDefaultWorkflowAction(networkViewModel, group, shouldUpdateOnNetworkEvents: false); //default diagrams are shown in Workflow Relationship Designer
			yield return new CreateAnnotationAction(networkViewModel, group, shouldUpdateOnNetworkEvents: false);
		}

		#endregion

		#region GetCustomNetworkActions

		IEnumerable<INetworkAction> INetwork.GetCustomNetworkActions(INetworkViewModel networkViewModel)
		{
			var existingGroupWithBaseActions = 20;

			yield return new LinkEntityActions(networkViewModel, existingGroupWithBaseActions, groupIndex: 1);
			yield return new CoreCustomActions(networkViewModel, existingGroupWithBaseActions, groupIndex: 2);
			yield return new OpenAsDiagramAction(networkViewModel, existingGroupWithBaseActions, groupIndex: 25, shouldUpdateOnNetworkEvents: false);
		}

		#endregion

		#endregion

		#region CopyPasteShapes

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "This code is only ever called from the view model layer.")]
		public bool TryCopyShapeStateToClipBoard(IEnumerable<INetworkEntity> shapeStates)
		{
			if (shapeStates.Any(n => n.ShapeType == ShapeTypeList.Codes.Buffer))
			{
				var caption = Res.GetString("2EAB6985-6985-42CF-9003-9FC17D2F7EE8", "Buffers cannot be copied");
				var message = Res.GetString("09EED41F-142A-41F0-A600-1B3A7473FE48", "The selection contains buffers. Each buffer relates to its own diagram and cannot be copied to another diagram. Only the selected shapes and/or annotations will be copied.");
				Globals.Message.ShowWarning(message, caption);
			}

			return ShapeStateContainer.Set(shapeStates, true);
		}

		void CreateShapeFromShapeState(ShapeState shapeState, INetworkViewModel networkViewModel, List<INetworkEntity> insertedShapes, Dictionary<string, string> pkMapping)
		{
			if (pkMapping.ContainsKey(shapeState.PK))
			{
				return;
			}

			//Validation
			ZGuid.TryParse(shapeState.BNS_RelatedEntityID, out var eId);
			var sObject = DiagramShape.Factory.Load(shapeState.BNS_RelatedEntityTableCode, eId);
			if (sObject != null)
			{
				var validationResult = JobNetworkEntityRelationshipValidator.GetLinkFailureMessage(sObject, Entities.GetInstance(DiagramShape), DiagramEntity, Entities.ShapeEntities);

				if (!validationResult.FailureReason.IsEmpty)
				{
					Controller.UserInteractionImplementor.ShowMessage(validationResult.FailureReason);
					return;
				}
			}

			//DFS traversing to build tree from leaves to root
			shapeState.Children.ForEach(child => CreateShapeFromShapeState(child, networkViewModel, insertedShapes, pkMapping));

			//Creating Shapes
			var actionCreateShape = new CreateShapeAction(networkViewModel);

			INetworkActionResult result;

			using (SuspendRefreshingOnEntityCountChanged())
			{
				result = actionCreateShape.Execute();
			}

			if (result != null && result.IsHandledByVisualiser)
			{
				ZGuid.TryParse(shapeState.Style, out var styleZGuid);
				ZGuid.TryParse(shapeState.RelatedEntityID, out var relatedEntityIDGuid);

				var entity = result as INetworkEntity;
				var shape = entity.AsShape();
				shape.JobName = shapeState.JobName;
				shape.ForeColor = shapeState.ForeColor.Name;
				shape.BackColor = shapeState.BackColor.Name;
				shape.Name = shapeState.Name;
				shape.BNS_CompletionStatements = shapeState.CompletionCriteria;
				shape.Height = shapeState.Height;
				shape.Width = shapeState.Width;
				shape.BNS_BNT_Style = styleZGuid;
				shape.BNS_JobType = shapeState.JobType;
				shape.BNS_LayoutData = shapeState.LayoutData;
				shape.BNS_ShapeType = shapeState.ShapeType;
				shape.BNS_Status = shapeState.Status;
				shape.ShapeNotes = shapeState.ShapeNotes;
				ZGuid.TryParse(shapeState.BNS_RelatedEntityID, out var reId);

				_ = LinkEntity(shape, viewModel.Factory.Load(shapeState.RelatedEntityTableCode, reId));

				//Binding Parents and children
				shapeState.Children.ForEach(child =>
				{
					if (pkMapping.TryGetValue(child.PK, out var childPKString))
					{
						var childShape = insertedShapes.FirstOrDefault(x => x.EntityPK.ToString() == childPKString);

						if (childShape != null)
						{
							shape.ChildShapes.Add(childShape.AsShape());
						}
					}
				});

				//Creating Shape
				AddNewShape(shape);

				insertedShapes.Add(shape);
				pkMapping.Add(shapeState.PK, shape.PK.ToString());
			}
		}

		public INetworkActionResult PasteShapeFromClipBoard(INetworkViewModel networkViewModel)
		{
			var (shapes, links) = ShapeStateContainer.Get();
			if (shapes == null || !(shapes is List<ShapeState>) || !shapes.Any())
			{
				return null;
			}

			var insertedShapes = new List<INetworkEntity>();
			var pkMapping = new Dictionary<string, string>();

			//Creating Shapes
			shapes.ForEach(shape => CreateShapeFromShapeState(shape, networkViewModel, insertedShapes, pkMapping));

			//Creating Attachments
			links.ForEach(link =>
			{
				var shape1 = insertedShapes.First(s => s.EntityPK.ToString() == pkMapping[link.From]);
				var shape2 = insertedShapes.First(s => s.EntityPK.ToString() == pkMapping[link.To]);

				var relation = CreateRelationship(shape1, shape2);

				ZGuid.TryParse(link.FPProcessHeaderLink, out var fPProcessHeaderLink);

				relation.AsAttachment().BNA_Type = link.BNAType;
				relation.AsAttachment().BNA_FP_ProcessHeaderLink = fPProcessHeaderLink;
			});

			this.Refresh(RefreshType.RedrawDiagram);

			return null;
		}

		#endregion
	}
}
