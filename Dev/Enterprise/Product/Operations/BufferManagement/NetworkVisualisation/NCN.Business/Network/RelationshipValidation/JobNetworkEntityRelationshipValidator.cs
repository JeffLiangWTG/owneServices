using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Implementation;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using QuikGraph;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	static class JobNetworkEntityRelationshipValidator
	{
		#region Relationship Validation

		internal static RelationshipValidationResult ValidateCreateDependencyRelationship(ShapeNetworkEntity source, ShapeNetworkEntity destination, IEnumerable<BMNCNShape> entities, IEnumerable<BMNCNAttachment> attachments)
		{
			if (source.IsChildOfShapeUpHierarchy(destination.Shape) || destination.IsChildOfShapeUpHierarchy(source.Shape))
			{
				return RelationshipValidationResult.Failure(Res.GetString("8bb602fb-9361-4469-87ab-ceb1293ac409", "Cannot create an arrow between a parent and its child."));
			}
			else if (NewLinkCreatesCycle(source.Shape, destination.Shape, entities, attachments))
			{
				return RelationshipValidationResult.Failure(Res.GetString("6fdd5bf6-8849-4afc-b02c-8791c9422076", "Creating this arrow would cause a circular dependency on this diagram."));
			}
			else
			{
				return RelationshipValidationResult.Success;
			}
		}

		#endregion

		#region Link to Related Entity

		internal static RelationshipValidationResult GetLinkFailureMessage(BusinessObject entityToLink, IShapeNetworkEntity shape, IShapeNetworkEntity diagramEntity, IEnumerable<IShapeNetworkEntity> entities)
		{
			switch (entityToLink)
			{
				case ProcessHeader processHeaderToLink:
					return GetLinkFailureMessage(processHeaderToLink, shape, diagramEntity, entities);

				case BMNCNShape shapeToLink:
					return GetLinkFailureMessage(shapeToLink, shape, diagramEntity, entities);

				default:
					throw new ArgumentException("Unsupported linked entity type: " + entityToLink.GetType(), nameof(entityToLink));
			}
		}

		static RelationshipValidationResult GetLinkFailureMessage(BMNCNShape shapeToLink, IShapeNetworkEntity shape, IShapeNetworkEntity diagramEntity, IEnumerable<IShapeNetworkEntity> entities)
		{
			if (shapeToLink.BNS_ShapeType == ShapeTypeList.Codes.Annotation)
			{
				return RelationshipValidationResult.Failure(Res.GetString("c15d35c7-2050-4879-822e-4f0150164c01", "Cannot link a shape to an annotation."));
			}

			if (shape.Shape.BNS_ShapeType == ShapeTypeList.Codes.Shape && shapeToLink.BNS_ShapeType != ShapeTypeList.Codes.Diagram)
			{
				return RelationshipValidationResult.Failure(Res.GetString("882d8cad-8022-4212-86b4-8dedc63c8b08", "Only a diagram can be linked to a shape on another diagram."));
			}

			if (shapeToLink.PK == shape.PK)
			{
				return RelationshipValidationResult.Failure(Res.GetString("39aaf4e6-9a63-48d6-b2b2-880c04e3da04", "Cannot link a shape to itself."));
			}

			if (shapeToLink.RootShapePK == shape.Shape.RootShapePK)
			{
				return RelationshipValidationResult.Failure(Res.GetString("18a55dfd-9a9d-49b3-a983-55af571014e8", "Cannot link a shape to this diagram."));
			}

			if (IsEntityAlreadyLinkedOnDiagram(shapeToLink.PK, diagramEntity, entities))
			{
				return GetEntityAlreadyLinkedToShapeOnThisDiagramFailure();
			}

			return RelationshipValidationResult.Success;
		}

		static RelationshipValidationResult GetLinkFailureMessage(ProcessHeader newProcessHeader, IShapeNetworkEntity shape, IShapeNetworkEntity diagramEntity, IEnumerable<IShapeNetworkEntity> entities)
		{
			if (newProcessHeader == null)
			{
				return RelationshipValidationResult.Failure(Res.GetString("94994987-8eb5-4742-96b0-a54a0e5d38be", "No entity selected."));
			}
			else if (shape.Descendants().Any(x => x.ProcessHeader != null && x.ProcessHeader.GetChildWorkflowsDownTheHierarchy().Contains(newProcessHeader)))
			{
				return RelationshipValidationResult.Failure(Res.GetString("AAFC4376-5F57-485F-8690-0134C5CD4CBD", "Cannot link this Business Entity to the shape because the parent of this Business Entity is already linked to the child shape. This would cause a circular dependency between entities."));
			}
			else if (shape.Ancestors(new ShapeNetworkEntityDescendantsStrategy()).OfType<IShapeNetworkEntity>().Any(x =>
					x.ProcessHeader != null && x.ProcessHeader.GetParentsUpTheHierarchy().Select(h => h.ProcessHeader).Contains(newProcessHeader)))
			{
				return RelationshipValidationResult.Failure(Res.GetString("DB0E105A-83E2-4DB8-9555-99C43A5F7DA9", "Cannot link this Business Entity to the shape because the child of this Business Entity is already linked to the parent shape. This would cause a circular dependency between entities."));
			}
			else
			{
				return GetLinkFailureMessageForOwner(newProcessHeader, shape.Owner, diagramEntity, entities);
			}
		}

		internal static RelationshipValidationResult GetLinkFailureMessageForOwner(BusinessObject entityToLink, IShapeNetworkEntity owner, IShapeNetworkEntity diagramEntity, IEnumerable<IShapeNetworkEntity> entities)
		{
			switch (entityToLink)
			{
				case ProcessHeader processHeaderToLink:
					return GetLinkFailureMessageForOwner(processHeaderToLink, owner, diagramEntity, entities);

				case BMNCNShape shapeToLink:
					return GetLinkFailureMessageForOwner(shapeToLink, owner, diagramEntity, entities);

				default:
					throw new ArgumentException("Unsupported linked entity type: " + entityToLink.GetType(), nameof(entityToLink));
			}
		}

		static RelationshipValidationResult GetLinkFailureMessageForOwner(BMNCNShape shapeToLink, IShapeNetworkEntity owner, IShapeNetworkEntity diagramEntity, IEnumerable<IShapeNetworkEntity> entities)
		{
			// No differences in validation rules depending on the hierarchy within a diagram where the shape will be linked.

			return GetLinkFailureMessage(shapeToLink, owner, diagramEntity, entities);
		}

		static RelationshipValidationResult GetLinkFailureMessageForOwner(ProcessHeader newProcessHeader, IShapeNetworkEntity owner, IShapeNetworkEntity diagramEntity, IEnumerable<IShapeNetworkEntity> entities)
		{
			var relevantDescendantPks = Lazy.Create(() => new HashSet<ZGuid>(newProcessHeader.Descendants().Select(ph => ph.PK).Append(newProcessHeader.PK)));

			if (IsEntityAlreadyLinkedOnDiagram(newProcessHeader.PK, diagramEntity, entities))
			{
				return GetEntityAlreadyLinkedToShapeOnThisDiagramFailure();
			}
			else if (owner != null && owner.HiddenEntities.Contains(newProcessHeader))
			{
				return RelationshipValidationResult.Success;
			}
			else if (IsHiddenEntityOnDiagram(relevantDescendantPks, diagramEntity, entities))
			{
				return RelationshipValidationResult.Failure(Res.GetString("f9b339d2-112d-4866-ba2b-378ba69c62b9", "Cannot link to an entity that is already a hidden entity on the diagram."));
			}
			else
			{
				return RelationshipValidationResult.Success;
			}
		}

		static RelationshipValidationResult GetEntityAlreadyLinkedToShapeOnThisDiagramFailure()
		{
			return RelationshipValidationResult.Failure(Res.GetString("57031104-97f9-4c2c-a604-d368568be687", "Cannot link this Business Entity to the shape because another shape with this Business Entity already exists on the diagram."));
		}

		#endregion

		#region Importing Shapes

		internal static RelationshipValidationResult ValidateImport(BMNCNShape shape, IShapeNetworkEntity diagramEntity, IEnumerable<IShapeNetworkEntity> entities, ShapeNetworkEntity parentShape, IEntityPositionStrategy positionStrategy)
		{
			if (shape == null)
			{
				return RelationshipValidationResult.Failure(Res.GetString("a9b9c45a-adbd-4fbb-ac9a-c4c57dd4f03d", "Tried to import shape, but shape could not be found."));
			}
			else if (shape.PK == diagramEntity.PK)
			{
				return RelationshipValidationResult.Failure(Res.GetString("e08b668b-7b20-4ef1-a871-0f81cc594ae5", "A diagram cannot be imported into itself."));
			}
			else if (IsScaledShapeOnNonScaledDiagram(shape, diagramEntity))
			{
				return RelationshipValidationResult.Failure(Res.GetString("810d4047-f026-417b-9907-28a5802cad92", "Scaled diagrams cannot be imported into non-scaled diagrams. To import a scaled diagram, this diagram must be scaled. You can create a scaled copy of this diagram using the Switch to Scaled Mode action."));
			}
			else if (IsShapeAlreadyOnDiagram(shape, diagramEntity.Shape))
			{
				return RelationshipValidationResult.Failure(Res.GetString("36fca692-8c6e-45a4-ba22-b5d4e912a94b", "This shape is already present on the diagram."));
			}
			else if (DoesRelatedEntityAlreadyExistOnDiagram(shape, entities))
			{
				return RelationshipValidationResult.Failure(Res.GetString("6a9838af-599b-4df2-80d7-7cfc011b6fd3", "The selected diagram or its descendants already exist as linked entities."));
			}
			else if (IsHiddenEntityOnDiagram(GetHiddenEntityPKs(shape), diagramEntity, entities))
			{
				return RelationshipValidationResult.Failure(Res.GetString("b2d5a9fa-826c-4912-99e2-7fe860ccb499", "Cannot import an entity that already exists as a hidden entity."));
			}
			else if (IsUnableToFitWithinPinnedShape(shape, parentShape, positionStrategy))
			{
				return RelationshipValidationResult.Failure(Res.GetString("f1a7fde4-c0a5-42ed-9a9b-e1bbd9ead207", "Could not perform action because the imported shape wouldn't fit in the parent's bounds."));
			}
			else
			{
				return RelationshipValidationResult.Success;
			}
		}

		#endregion

		#region Utils

		static Lazy<HashSet<ZGuid>> GetHiddenEntityPKs(BMNCNShape shape)
		{
			return Lazy.Create(() =>
			{
				var result = new HashSet<ZGuid>();

				result.Add(shape.BNS_RelatedEntityID);

				if (shape.ProcessHeader != null)
				{
					foreach (var descendent in shape.ProcessHeader.Descendants(new ProcessHeaderDescendantsStrategy()))
					{
						result.Add(descendent.PK);
					}
				}

				return result;
			});
		}

		static bool NewLinkCreatesCycle(BMNCNShape source, BMNCNShape destination, IEnumerable<BMNCNShape> entities, IEnumerable<BMNCNAttachment> attachments)
		{
			var entityHash = new HashSet<ILinkEntity>(entities);
			var descendantsStrategy = new BMNCNShapeDescendantsStrategy();
			var scheduleGraph = descendantsStrategy.CreateRelationshipGraph(source, entityHash, attachments);

			descendantsStrategy.DoToApplicableLinks(source, new[] { new VirtualLink(source, destination) }, (l, pre, post) => scheduleGraph.AddVerticesAndEdge(new SEdge<ILinkEntity>(pre, post)));

			var cyclicVertexes = QuickGraphUtils.GetCyclicVertexes(scheduleGraph);

			return cyclicVertexes.Any();
		}

		static bool IsShapeAlreadyOnDiagram(BMNCNShape shape, BMNCNShape diagram)
		{
			return shape.BNS_BNS_RootShape == diagram.PK;
		}

		static bool DoesRelatedEntityAlreadyExistOnDiagram(BMNCNShape shape, IEnumerable<IShapeNetworkEntity> entities)
		{
			var descendantProcessHeaders = new HashSet<ZGuid>(shape.Descendants(new BMNCNShapeDescendantsStrategy()).Append(shape).Select(s => s.BNS_RelatedEntityID).Where(guid => guid.IsValid));
			var entityProcessHeaders = new HashSet<ZGuid>(entities.Select(s => s.RelatedEntityPK).Where(guid => guid.IsValid));
			return descendantProcessHeaders.Overlaps(entityProcessHeaders);
		}

		static bool IsHiddenEntityOnDiagram(Lazy<HashSet<ZGuid>> relevantDescendantPks, IShapeNetworkEntity diagramEntity, IEnumerable<IShapeNetworkEntity> entities)
		{
			return entities.Append(diagramEntity).SelectMany(shape => shape.HiddenEntities).Cast<ProcessHeader>().Any(ph => relevantDescendantPks.Value.Contains(ph.PK));
		}

		static bool IsUnableToFitWithinPinnedShape(BMNCNShape shape, ShapeNetworkEntity parentShape, IEntityPositionStrategy postionStrategy)
		{
			var importedDescendants = DescendantsWithSetOwnership(shape).ToArray();
			var importedDescendantMaxWidth = importedDescendants.MaxOrDefault(s => s.Width + s.X);
			var importedDescendantMaxHeight = importedDescendants.MaxOrDefault(s => s.Height + s.Y);
			var descendantsStrategy = new ShapeNetworkEntityDescendantsStrategy();
			var pinnedAncestors = GetPinnedAncestorSizes(parentShape, descendantsStrategy, postionStrategy).ToArray();
			if (pinnedAncestors.Length > 0)
			{
				return pinnedAncestors.Any(ancestor => importedDescendantMaxWidth > ancestor.Item1) || pinnedAncestors.Any(ancestor => importedDescendantMaxHeight > ancestor.Item2);
			}
			else
			{
				return false;
			}
		}

		static bool IsScaledShapeOnNonScaledDiagram(BMNCNShape shape, IShapeNetworkEntity diagramEntity)
		{
			return shape.IsScaled && !diagramEntity.IsScaled;
		}

		static IEnumerable<ShapeNetworkEntity> DescendantsWithSetOwnership(BMNCNShape shape)
		{
			return JobNetwork.CreateTemporaryNetwork(shape).Entities.ShapeEntities;
		}

		static IEnumerable<Tuple<double, double>> GetPinnedAncestorSizes(ShapeNetworkEntity shape, ILinkDescendantsStrategy descendantsStrategy, IEntityPositionStrategy postionStrategy, int depth = 0)
		{
			if (shape.IsPinned)
			{
				yield return Tuple.Create(shape.Width - depth * (postionStrategy.LeftMargin + postionStrategy.RightMargin), shape.Height - depth * (postionStrategy.TopMargin + postionStrategy.BottomMargin));
			}
			else
			{
				foreach (var ancestor in shape.Parents(descendantsStrategy).SelectMany(parent => GetPinnedAncestorSizes((ShapeNetworkEntity)parent, descendantsStrategy, postionStrategy, depth + 1)))
				{
					yield return ancestor;
				}
			}
		}

		static bool IsEntityAlreadyLinkedOnDiagram(ZGuid relevantPK, IShapeNetworkEntity diagramEntity, IEnumerable<IShapeNetworkEntity> entities)
		{
			return entities.Append(diagramEntity).Any(shape => shape.RelatedEntityPK == relevantPK);
		}

		#endregion
	}
}
