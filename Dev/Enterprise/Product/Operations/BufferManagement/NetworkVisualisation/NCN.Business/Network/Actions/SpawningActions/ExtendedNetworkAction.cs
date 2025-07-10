using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class ExtendedNetworkAction : SpawningIndependentNetworkAction
	{
		static ZString[] SupportedDiagramTypes => new ZString[] { ShapeTypeList.Codes.Diagram, ShapeTypeList.Codes.DefaultDiagram };
		static ZString[] SupportedShapeTypes => new ZString[] { ShapeTypeList.Codes.Shape, ShapeTypeList.Codes.DefaultWorkflow };

		public ExtendedNetworkAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected override void ExecuteForShape(BMNCNShape targetShape)
		{
			BMNCNShape parentShape = null;

			if (SupportedDiagramTypes.Contains(targetShape.BNS_ShapeType))
			{
				parentShape = targetShape;
			}
			else if (SupportedShapeTypes.Contains(targetShape.BNS_ShapeType))
			{
				parentShape = targetShape.ParentShape;
			}
			else
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Can only view extended network for Shape or Diagram"));
			}

			if (!ShouldProceedWithNoBuffers())
			{
				return;
			}

			var extendedNetwork = CreateExtendedNetwork(parentShape);

			if (extendedNetwork == null)
			{
				var caption = Res.GetString("5f3c3e5d-ccf1-435d-919f-ac10c46d91f8", "Error Creating Extended Network");
				var message = Res.GetString("9163af25-97db-4195-9eef-edea9d86c9a9", "Cannot view Extended Network as cyclic hierarchy exists");
				Controller.UserInteractionImplementor.ShowError(message, caption);
			}
			else
			{
				extendedNetwork.DiagramShape.ShouldValidateLoopsOnOpen = true;
				//Show the diagram without saving it first.
				Controller.ViewDiagram(extendedNetwork.DiagramShape);
			}
		}

		#region Implementation

		bool ShouldProceedWithNoBuffers()
		{
			if (Network.Shapes.Any(s => s.IsBufferShape))
			{
				var caption = Res.GetString("30ECA438-0608-41C3-9681-92E92B3F8355", "Buffers cannot be recreated on extended diagrams");
				var message = Res.GetString("50E95269-3F6B-45C7-AC60-CFA832429C0C", "The original diagram contains buffers which will not be created on the extended diagram. Do you want to proceed?");
				return Controller.UserInteractionImplementor.HasUserAnsweredYes(message, caption);
			}
			return true;
		}

		JobNetwork CreateExtendedNetwork(BMNCNShape parentShape)
		{
			var previousParentEntity = Network.Entities.GetInstance(parentShape);

			var factoryForSpawnedNetwork = CreateNewFactoryForSpawnedNetwork();
			var cloneDiagram = factoryForSpawnedNetwork.New<BMNCNShape>();
			cloneDiagram.BNS_ShapeType = parentShape.BNS_ShapeType;
			cloneDiagram.BNS_Name = GetNameForExtendedNetwork(parentShape);

			var clonedRootShape = previousParentEntity.CloneShapeAndAllDescendantsIntoNewParent(cloneDiagram, unPinPinnedShapes: true, factoryForSpawnedNetwork);
			clonedRootShape.BNS_ShapeType = ShapeTypeList.Codes.Shape;
			cloneDiagram.BNS_ShapeType = ShapeTypeList.Codes.Diagram;

			var cloneNetwork = CreateSpawnedTemporaryNetwork(cloneDiagram);
			AdjustChildShapesSizeAndPositon(clonedRootShape);

			var initialShapes = GetChildShapesLinkedToProcessHeadersDownTheHierarchy(clonedRootShape);

			var initialLinks = cloneDiagram.AllAttachments;
			var shapePKMap = new Dictionary<ProcessHeader, BMNCNShape>();
			var existingPKs = new HashSet<ZGuid>();

			foreach (var shape in initialShapes)
			{
				existingPKs.Add(shape.ProcessHeader.PK);
				shapePKMap[shape.ProcessHeader] = shape;
			}

			existingPKs.UnionWith(initialLinks.Select(l => l.ProcessHeaderLink.PK));

			foreach (var shape in initialShapes.OrderBy(s => s.ProcessHeader.FH_CompletionStatement))
			{
				if (!AddLinkedShapesToDiagram(shape, cloneDiagram, cloneNetwork, existingPKs, shapePKMap))
				{
					cloneDiagram.Delete();
					return null;
				}
			}

			return cloneNetwork;
		}

		void AdjustChildShapesSizeAndPositon(BMNCNShape clonedRootShape)
		{
			if (!clonedRootShape.ChildShapes.Any())
			{
				return;
			}

			var childShapesWithOrigin = clonedRootShape.ChildShapes
				.Join(NetworkViewModel.Network.Entities, shape => shape.ClonedFromPK, originEntity => originEntity.EntityPK,
				(shape, originEntity) => new { shape, originEntity });

			foreach (var shapeAndOrigin in childShapesWithOrigin)
			{
				shapeAndOrigin.shape.Left = shapeAndOrigin.originEntity.X;
				shapeAndOrigin.shape.Width = shapeAndOrigin.originEntity.Width;

				if (shapeAndOrigin.shape.Width == 0)
				{
					shapeAndOrigin.shape.Width = Network.EntityPositionStrategy.DefaultWidth;
				}

				shapeAndOrigin.shape.Top = shapeAndOrigin.originEntity.Y;
				shapeAndOrigin.shape.Height = shapeAndOrigin.originEntity.Height;

				if (shapeAndOrigin.shape.Height == 0)
				{
					shapeAndOrigin.shape.Height = Network.EntityPositionStrategy.DefaultHeight;
				}
			}

			clonedRootShape.Width = clonedRootShape.ChildShapes.Max(c => c.Left + c.Width);
			clonedRootShape.Height = clonedRootShape.ChildShapes.Max(c => c.Top + c.Height);
		}

		BMNCNShape[] GetChildShapesLinkedToProcessHeadersDownTheHierarchy(BMNCNShape rootShape)
		{
			return GetChildShapesDownTheHierarchyCore(rootShape, new HashSet<BMNCNShape>() { rootShape }).Where(s => s.BNS_ShapeType == ShapeTypeList.Codes.Shape && s.ProcessHeader != null).ToArray();
		}

		HashSet<BMNCNShape> GetChildShapesDownTheHierarchyCore(BMNCNShape currentShape, HashSet<BMNCNShape> foundShapes)
		{
			var shapesAtThisLevel = currentShape.ChildShapes;
			foundShapes.UnionWith(shapesAtThisLevel);

			foreach (var shape in shapesAtThisLevel)
			{
				GetChildShapesDownTheHierarchyCore(shape, foundShapes);
			}

			return foundShapes;
		}

		bool AddLinkedShapesToDiagram(BMNCNShape currentShape, BMNCNShape rootShape, JobNetwork cloneNetwork, HashSet<ZGuid> existingPKs, Dictionary<ProcessHeader, BMNCNShape> shapeHeaderMap)
		{
			var processHeader = currentShape.ProcessHeader;

			if (processHeader == null)
			{
				return true; //Shape is not linked to a WF, no further action required
			}

			if (processHeader.GetHierarchicCycles().Length > 1)
			{
				return false; //We have some circular hierarchy, for now, short-circuit network creation
			}

			if (!(processHeader is ProcessJobHeader) && !(currentShape.ParentShape.IsLinkedToRealEntity && currentShape.ParentShape.ProcessHeader.JobHeader.Equals(processHeader.JobHeader)))
			{
				if (!shapeHeaderMap.TryGetValue(processHeader.JobHeader, out var headerShape))
				{
					//If the parent isn't linked to an entity, we can place the new job header inside it, otherwise just place it on the root diagram.
					var parentShapeForHeaderShape = !currentShape.ParentShape.IsLinkedToRealEntity ? currentShape.ParentShape : rootShape;
					headerShape = ShowOrCreateShape(processHeader.JobHeader, parentShapeForHeaderShape, cloneNetwork, shapeHeaderMap);
				}
				currentShape.MakeChildOf(headerShape);
			}

			var linksToFollow = processHeader.Links.Where(l => l.FP_LinkType == ProcessHeaderLinkTypeList.Codes.Dependency || l.FP_LinkType == ProcessHeaderLinkTypeList.Codes.ParentChild);
			foreach (var link in linksToFollow)
			{
				if (!existingPKs.Contains(link.PK))
				{
					var foundHeader = link.HeaderTo.Equals(processHeader) ? link.HeaderFrom : link.HeaderTo;

					if (link.FP_LinkType == ProcessHeaderLinkTypeList.Codes.Dependency)
					{
						if (!CreateNewShapeWithDependencyLink(foundHeader, currentShape, link, rootShape, cloneNetwork, existingPKs, shapeHeaderMap))
						{
							return false;
						}
					}
					else if (link.FP_LinkType == ProcessHeaderLinkTypeList.Codes.ParentChild)
					{
						if (!CreateNewShapeWithParentChildLink(foundHeader, currentShape, link, rootShape, cloneNetwork, existingPKs, shapeHeaderMap))
						{
							return false; //Attempted to add a circular link
						}
					}
				}
			}

			return true;
		}

		bool CreateNewShapeWithDependencyLink(ProcessHeader newHeader, BMNCNShape currentShape, ProcessHeaderLink link, BMNCNShape rootShape, JobNetwork cloneNetwork, HashSet<ZGuid> existingPKs, Dictionary<ProcessHeader, BMNCNShape> shapeHeaderMap)
		{
			var newShape = AddNewShapeWithinCorrectParent(newHeader, currentShape, rootShape, cloneNetwork, existingPKs, shapeHeaderMap);

			var newShapeIsPrereq = link.HeaderFrom.Equals(newHeader);
			var prereqShape = newShapeIsPrereq ? newShape : currentShape;
			var postreqShape = newShapeIsPrereq ? currentShape : newShape;

			if (!CreateNewAttachment(prereqShape, postreqShape, rootShape, cloneNetwork))
			{
				return false;
			}

			existingPKs.Add(link.PK);

			return AddLinkedShapesToDiagram(newShape, rootShape, cloneNetwork, existingPKs, shapeHeaderMap);
		}

		bool CreateNewShapeWithParentChildLink(ProcessHeader newHeader, BMNCNShape currentShape, ProcessHeaderLink link, BMNCNShape rootShape, JobNetwork cloneNetwork, HashSet<ZGuid> existingPKs, Dictionary<ProcessHeader, BMNCNShape> shapeHeaderMap)
		{
			var newShape = AddNewShapeWithinCorrectParent(newHeader, currentShape, rootShape, cloneNetwork, existingPKs, shapeHeaderMap);

			var newShapeIsChild = link.HeaderFrom.Equals(newHeader);
			var childShape = newShapeIsChild ? newShape : currentShape;
			var parentShape = newShapeIsChild ? currentShape : newShape;

			childShape.MakeChildOf(parentShape);

			existingPKs.Add(link.PK);

			return AddLinkedShapesToDiagram(newShape, rootShape, cloneNetwork, existingPKs, shapeHeaderMap);
		}

		BMNCNShape AddNewShapeWithinCorrectParent(ProcessHeader newHeader, BMNCNShape currentShape, BMNCNShape rootShape, JobNetwork cloneNetwork, HashSet<ZGuid> existingPKs, Dictionary<ProcessHeader, BMNCNShape> shapeHeaderMap)
		{
			if (!shapeHeaderMap.TryGetValue(newHeader, out var newShape))
			{
				var parentOfNewHeader = TryFindParentHeader(newHeader);

				if (shapeHeaderMap.TryGetValue(parentOfNewHeader, out var foundParentShape))
				{
					newShape = ShowOrCreateShape(newHeader, foundParentShape, cloneNetwork, shapeHeaderMap);
				}
				else if (parentOfNewHeader is ProcessJobHeader)
				{
					//Usually we will place the new shape linked to a job header on the diagram's root
					//Unless the shape's parent is not linked to something (eg. the original diagram was not linked), then it makes more sense to place the job header shape at the same level
					var parentShape = currentShape.ParentShape.IsLinkedToRealEntity ? rootShape : currentShape.ParentShape;

					if (newHeader is ProcessJobHeader)
					{
						newShape = ShowOrCreateShape(newHeader, parentShape, cloneNetwork, shapeHeaderMap);
					}
					else
					{
						var shapeForNewJobHeader = ShowOrCreateShape(parentOfNewHeader, parentShape, cloneNetwork, shapeHeaderMap);
						existingPKs.Add(parentOfNewHeader.PK);
						newShape = ShowOrCreateShape(newHeader, shapeForNewJobHeader, cloneNetwork, shapeHeaderMap);
					}
				}
				else
				{
					newShape = ShowOrCreateShape(newHeader, rootShape, cloneNetwork, shapeHeaderMap);
				}
			}

			existingPKs.Add(newHeader.PK);
			return newShape;
		}

		ProcessHeader TryFindParentHeader(ProcessHeader child)
		{
			var potentialParents = child.LinksFromMeToOthers.Where(l => l.FP_LinkType == ProcessHeaderLinkTypeList.Codes.ParentChild);
			//The best shape to nest a workflow in is it's (hopefully single) parent. However, if there are no PCH relations, we will nest it inside the jobheader.
			return potentialParents.Any() ? potentialParents.Select(l => l.HeaderTo).First() : child.JobHeader;
		}

		#region Create Diagram Elements

		bool CreateNewAttachment(BMNCNShape toShape, BMNCNShape fromShape, BMNCNShape rootShape, JobNetwork cloneNetwork)
		{
			return cloneNetwork.CreateRelationship(toShape, fromShape, rootShape, useAttachementValidation: false).AsAttachment() != null;
		}

		BMNCNShape ShowOrCreateShape(ProcessHeader newShapeHeader, BMNCNShape parentShape, JobNetwork cloneNetwork, Dictionary<ProcessHeader, BMNCNShape> shapeHeaderMap)
		{
			var previouslyHiddenEntities = cloneNetwork.ShowEntity(newShapeHeader, parentShape);

			if (previouslyHiddenEntities != null)
			{
				var shownShape = previouslyHiddenEntities.First().AsShape();
				shapeHeaderMap[newShapeHeader] = shownShape;
				return shownShape;
			}
			else
			{
				var result = new CreateShapeAction(NetworkViewModel).ExecuteForEntityWithoutAccessCheck(parentShape);
				var newShape = ((IProposedNetworkEntity)result).AsShape();
				cloneNetwork.LinkEntity(newShape, newShapeHeader);
				newShape.Name = newShapeHeader.Name;
				shapeHeaderMap[newShapeHeader] = newShape;
				return newShape;
			}
		}

		#endregion

		#endregion

		#region Network Name

		static string ExtendedNetworkSuffix { get { return Res.GetString("960c5824-6302-488a-af9d-16ab364a54ac", "(Extended Network)"); } }

		public static string GetNameForExtendedNetwork(BMNCNShape parentShape)
		{
			return parentShape.BNS_Name + " " + ExtendedNetworkSuffix;
		}

		#endregion

		#region Overrides

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("18732702-530f-4965-be8f-7c756122cf3c", "Show Extended Network");

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ResString.GetMultilingualString("b7cb1f92-190f-4e53-ac8a-147981d28e70", "Open a new Network Diagram automatically built with all workflows connected to this diagram or any of its nested shapes via pre-requisite relationships or parent/child relationships.");
		}

		protected override string IconName => "ExtendedDiagram";

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			return new NetworkActionAccessibility(
				SupportedDiagramTypes.Contains(shape.BNS_ShapeType) || SupportedShapeTypes.Contains(shape.BNS_ShapeType),
				shape,
				() => Res.GetString("E83ECDD0-AFBC-4FC2-8741-4C20201E0D39", "Only supported for shapes and diagrams."));
		}

		protected override bool RequiresUserConfirmation => true;

		protected override string UserConfirmationMessage => Res.GetString("4B2DA77E-ECDD-48EF-BBD8-947BA88DF4FC", "Are you sure you want to create an extended diagram for the given shape?");

		protected override bool RequiresValidateBeforeExecute => true;

		#endregion
	}
}
