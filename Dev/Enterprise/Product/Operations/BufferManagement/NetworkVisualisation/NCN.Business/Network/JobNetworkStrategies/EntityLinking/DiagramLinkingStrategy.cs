using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	class DiagramLinkingStrategy
	{
		#region LinkEntity

		internal void LinkEntity(JobNetwork network, BMNCNShape shape, BusinessObject entityToLink)
		{
			var canLink = shape.ProcessHeader == null || network.UnlinkEntity(shape, allowRefresh: false);

			if (canLink)
			{
				shape.BNS_RelatedEntityID = entityToLink.PK;

				if (entityToLink is ProcessHeader processHeaderToLink)
				{
					CreateBackingProcessHeaderLinksForDirectAttachments(shape, processHeaderToLink);

					foreach (var childShape in shape.ChildShapes)
					{
						EnsureParentChildProcessHeaderLinkExists(childShape);
					}

					if (shape.ParentShape != null)
					{
						EnsureParentChildProcessHeaderLinkExists(shape, shape.ParentShape);
					}

					CreateBackingProcessHeaderLinksForIndirectAttachments(shape, processHeaderToLink);
				}
			}
		}

		static void CreateBackingProcessHeaderLinksForDirectAttachments(BMNCNShape shape, ProcessHeader newProcessHeader)
		{
			foreach (var attachment in shape.AllAttachments)
			{
				if (attachment.BNA_Type == AttachmentTypeList.Codes.Dependency)
				{
					if (attachment.FromShape == shape)
					{
						EnsureDependencyLinkCreation(newProcessHeader, attachment.ToShape, attachment, DependencyDirection.PostRequisite);
					}
					else if (attachment.ToShape == shape)
					{
						EnsureDependencyLinkCreation(newProcessHeader, attachment.FromShape, attachment, DependencyDirection.PreRequisite);
					}
				}
			}
		}

		static void CreateBackingProcessHeaderLinksForIndirectAttachments(BMNCNShape shape, ProcessHeader newProcessHeader)
		{
			var strategy = new BMNCNShapeDescendantsStrategy();
			var relevantDescendents = GetShapesToCreateParentChildLinks(newProcessHeader, shape, strategy).ToArray();

			foreach (var descendent in relevantDescendents)
			{
				EnsureParentChildProcessHeaderLinkExists(descendent, shape);
			}
		}

		static IEnumerable<BMNCNShape> GetShapesToCreateParentChildLinks(ProcessHeader parentWorkflow, BMNCNShape shape, ILinkDescendantsStrategy<BMNCNShape> strategy)
		{
			foreach (var child in shape.Children(strategy))
			{
				if (child.IsLinkedToRealEntity)
				{
					if (!parentWorkflow.IsParentOf(child.ProcessHeader))
					{
						yield return child;
					}
				}
				else
				{
					foreach (var grandchild in GetShapesToCreateParentChildLinks(parentWorkflow, child, strategy))
					{
						yield return grandchild;
					}
				}
			}
		}

		static void EnsureParentChildProcessHeaderLinkExists(BMNCNShape childShape, BMNCNShape ancestorShape = null)
		{
			if (childShape.IsLinkedToRealEntity)
			{
				if (ancestorShape == null)
				{
					ancestorShape = childShape.ParentShape;
				}

				if (ancestorShape != null)
				{
					var ancestorShapeHierarchy = ancestorShape.WrapWithEnumerable().Concat(ancestorShape.SelectUntilNull(s => s.ParentShape));
					var ancestorProcessHeader = ancestorShapeHierarchy.Select(s => s.ProcessHeader).FirstOrDefault(w => w != null);

					if (ancestorProcessHeader != null)
					{
						if (IsAlreadyLinked(childShape, ancestorProcessHeader, out var link))
						{
							// Newly linked ProcessHeader may be part of the same job represented by the ancestor shape's job-level workflow.
							// These are linked via FH_FH_ParentHeader rather than via PCH ProcessHeaderLinks.
						}
						else
						{
							link = ancestorProcessHeader.Factory.New<ProcessHeaderLink>();

							using (ActiveBusinessObjectCollection.DelayListChangedEvents(ancestorProcessHeader.Factory))
							{
								link.FP_FH_HeaderFrom = childShape.BNS_RelatedEntityID;
								link.FP_FH_HeaderTo = ancestorProcessHeader.PK;
								link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;
							}

							link.ProcessNewLink();
						}
					}
				}
			}
		}

		static bool IsAlreadyLinked(BMNCNShape shape, ProcessHeader ancestorProcessHeader, out ProcessHeaderLink existingLink)
		{
			var ancestorJobHeader = ancestorProcessHeader as ProcessJobHeader;

			if (ancestorJobHeader != null && ancestorJobHeader.ProcessHeaders.Any(w => w.PK == shape.BNS_RelatedEntityID))
			{
				existingLink = null;
				return true;
			}
			else
			{
				existingLink = ancestorProcessHeader.ChildLinks.FirstOrDefault(l => l.FP_FH_HeaderFrom == shape.BNS_RelatedEntityID);
				return existingLink != null;
			}
		}

		static void EnsureDependencyLinkCreation(ProcessHeader newProcessHeader, BMNCNShape shape, BMNCNAttachment attachment, DependencyDirection direction)
		{
			if (shape != null && shape.ProcessHeader != null)
			{
				ProcessHeaderLink headerLink = null;
				if (direction == DependencyDirection.PostRequisite)
				{
					headerLink = newProcessHeader.GetOrCreateDependencyLink(shape.ProcessHeader);
				}
				else if (direction == DependencyDirection.PreRequisite)
				{
					var childrenFromAllLevels = newProcessHeader.GetChildWorkflowsDownTheHierarchy();
					if (!childrenFromAllLevels.Contains(shape.ProcessHeader))
					{
						headerLink = shape.ProcessHeader.GetOrCreateDependencyLink(newProcessHeader);
					}
				}

				if (headerLink != null)
				{
					attachment.BNA_FP_ProcessHeaderLink = headerLink.PK;
				}
			}
		}

		#endregion

		#region UnlinkEntity

		internal bool UnlinkEntity(JobNetwork network, BMNCNShape shape, UnlinkEntityVariant variant)
		{
			var oldHeader = shape.ProcessHeader;
			var unLinkAction = GetUnLinkAction(variant);
			if (oldHeader != null)
			{
				bool shouldCancelUnlinkOperation = false;
				ShapeHierarchyLinkingStrategy.ProposeRemovalOfHierarchicalParentChildLinks(shape, network, out shouldCancelUnlinkOperation);

				if (shouldCancelUnlinkOperation)
				{
					return false;
				}

				UnlinkRelatedEntityCore(shape);

				foreach (var attachment in shape.AllAttachments)
				{
					var link = attachment.ProcessHeaderLink;

					if (link != null)
					{
						attachment.BNA_FP_ProcessHeaderLink = ZGuid.Empty;
						unLinkAction(link);
					}
				}
			}
			else if (shape.RelatedShape != null)
			{
				UnlinkRelatedEntityCore(shape);
			}
			else
			{
				throw new InvalidOperationException("Trying to unlink, but there is no known RelatedEntity type.");
			}

			return true;
		}

		static void UnlinkRelatedEntityCore(BMNCNShape shape)
		{
			shape.BNS_RelatedEntityID = ZGuid.Empty;
		}

		Action<ProcessHeaderLink> GetUnLinkAction(UnlinkEntityVariant variant)
		{
			if (variant == UnlinkEntityVariant.DeleteHeaderLinks)
			{
				return e => e.Delete();
			}
			else
			{
				return e => { };
			}
		}

		#endregion
	}
}
