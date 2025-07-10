using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	class AncestorShapeHierarchyLinkingStrategy : ShapeHierarchyLinkingStrategy
	{
		protected override IEnumerable<BMNCNShape> GetHierarchyShapes(BMNCNShape shape, JobNetwork network)
		{
			return shape.GetAncestorShapes(network).Where(s => s.IsLinkedToRealEntity && s.ProcessHeader.IsParentOf(shape.ProcessHeader));
		}

		protected override string BaseRemovalMessage => Res.GetString("761ddd4a-7d9a-4ae6-965e-78dad1ffdbdf", "The workflow being un-linked is a child of the below linked ancestor shapes' workflows. Would you like to disconnect these relationships?");

		protected override IEnumerable<ProcessHeaderLink> GetLinkCollectionBetweenShapeBeingUnlinkedAndHierarchyShape(BMNCNShape hierarchyShape)
		{
			return hierarchyShape.ProcessHeader.LinksFromOthersToMe;
		}

		protected override ZGuid GetForeignKeyValueIdentifyingShapeBeingUnlinked(ProcessHeaderLink parentChildLink)
		{
			return parentChildLink.FP_FH_HeaderFrom;
		}
	}

	class ChildShapeHierarchyLinkingStrategy : ShapeHierarchyLinkingStrategy
	{
		protected override IEnumerable<BMNCNShape> GetHierarchyShapes(BMNCNShape shape, JobNetwork network)
		{
			var strategy = new BMNCNShapeDescendantsStrategy();
			var descendents = shape.Descendants(strategy);

			return descendents.Where(s => s.IsLinkedToRealEntity && s.ProcessHeader.IsChildOf(shape.ProcessHeader));
		}

		protected override string BaseRemovalMessage => Res.GetString("f5617de0-9fd1-4cb6-90c8-7b46edbc5128", "The workflow being un-linked is a parent of the below linked descendant shapes' workflows. Would you like to disconnect these relationships?");

		protected override IEnumerable<ProcessHeaderLink> GetLinkCollectionBetweenShapeBeingUnlinkedAndHierarchyShape(BMNCNShape hierarchyShape)
		{
			return hierarchyShape.ProcessHeader.LinksFromMeToOthers;
		}

		protected override ZGuid GetForeignKeyValueIdentifyingShapeBeingUnlinked(ProcessHeaderLink parentChildLink)
		{
			return parentChildLink.FP_FH_HeaderTo;
		}
	}

	abstract class ShapeHierarchyLinkingStrategy
	{
		internal static void ProposeRemovalOfHierarchicalParentChildLinks(BMNCNShape shapeBeingUnlinked, JobNetwork network, out bool shouldCancelUnlinkOperation)
		{
			new AncestorShapeHierarchyLinkingStrategy().ProposeRemovalOfHierarchicalParentChildLinksCore(shapeBeingUnlinked, network, out shouldCancelUnlinkOperation);

			if (!shouldCancelUnlinkOperation)
			{
				new ChildShapeHierarchyLinkingStrategy().ProposeRemovalOfHierarchicalParentChildLinksCore(shapeBeingUnlinked, network, out shouldCancelUnlinkOperation);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "This is interaction business logic called from a viewmodel.")]
		void ProposeRemovalOfHierarchicalParentChildLinksCore(BMNCNShape shapeBeingUnlinked, JobNetwork network, out bool shouldCancelUnlinkOperation)
		{
			var relevantLinkedShapes = GetHierarchyShapes(shapeBeingUnlinked, network).ToArray();

			if (relevantLinkedShapes.Any())
			{
				var message = new StringBuilder(BaseRemovalMessage);
				message.AppendLine();

				foreach (var relevantShape in relevantLinkedShapes.OrderBy(r => r.BNS_Name))
				{
					message.AppendLine();
					message.AppendFormat(CultureInfo.InvariantCulture, "[{0}]: [{1}]", relevantShape.BNS_Name, relevantShape.ProcessHeader.Description);
				}

				var caption = Res.GetString("c92bbcda-06b0-498b-a674-dcda0e642aa6", "Remove parent-child relationships");
				var response = Globals.Message.Show(message.ToString(), caption, ZMessageBoxButtons.YesNoCancel, ZDialogResult.Yes); // This is interaction business logic called from a viewmodel.

				switch (response)
				{
					case ZDialogResult.Yes:
						DisconnectLinks(relevantLinkedShapes, shapeBeingUnlinked);
						break;

					case ZDialogResult.Cancel:
						shouldCancelUnlinkOperation = true;
						return;
				}
			}

			shouldCancelUnlinkOperation = false;
		}

		void DisconnectLinks(BMNCNShape[] relevantLinkedShapes, BMNCNShape shapeBeingUnlinked)
		{
			foreach (var relevantShape in relevantLinkedShapes)
			{
				var links = GetLinkCollectionBetweenShapeBeingUnlinkedAndHierarchyShape(relevantShape).ToArray();

				foreach (var link in links)
				{
					if (link.FP_LinkType == ProcessHeaderLinkTypeList.Codes.ParentChild && GetForeignKeyValueIdentifyingShapeBeingUnlinked(link) == shapeBeingUnlinked.BNS_RelatedEntityID)
					{
						link.Delete(ProcessHeaderLink.DeleteOption.DisconnectRelatedAttachments);
					}
				}
			}
		}

		protected abstract IEnumerable<BMNCNShape> GetHierarchyShapes(BMNCNShape shape, JobNetwork network);
		protected abstract string BaseRemovalMessage { get; }
		protected abstract IEnumerable<ProcessHeaderLink> GetLinkCollectionBetweenShapeBeingUnlinkedAndHierarchyShape(BMNCNShape hierarchyShape);
		protected abstract ZGuid GetForeignKeyValueIdentifyingShapeBeingUnlinked(ProcessHeaderLink parentChildLink);
	}
}
