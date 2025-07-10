using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	class HiddenRelationshipsCollection : HiddenShapeChildCollection<IEntityRelationship>
	{
		internal HiddenRelationshipsCollection(IShapeNetworkEntity ownerShape)
			: base(ownerShape)
		{
		}

		protected override IEnumerable<IEntityRelationship> GetHiddenElements()
		{
			var links = new HashSet<ProcessHeaderLink>();
			var innerShapeScope = OwnerShape.Children;

			var rootShape = OwnerShape.Root;
			// Optimisation Opportunity: Make this collection generated in O(n) rather than O(n^2) by sharing internals between multiple instances of this collection.
			var existingLinks = GetExistingLinkPks(innerShapeScope);
			var effectiveWorkflowScope = rootShape.Descendants().Select(s => s.RelatedEntityPK).Distinct().Where(g => g.IsValid).ToHashSet();

			foreach (var shape in innerShapeScope)
			{
				var workflow = shape.ProcessHeader;
				if (workflow != null)
				{
					var dependencyLinks =
						from link in workflow.Links
						where link.FP_LinkType == ProcessHeaderLinkTypeList.Codes.Dependency
						where effectiveWorkflowScope.Contains(link.FP_FH_HeaderFrom)
						where effectiveWorkflowScope.Contains(link.FP_FH_HeaderTo)
						where !existingLinks.Contains(link.PK)
						select link;

					foreach (var link in dependencyLinks)
					{
						if (!links.Contains(link))
						{
							links.Add(link);
						}
					}
				}
			}

			return links.OrderBy(l => l.DisplayText);
		}

		static HashSet<ZGuid> GetExistingLinkPks(IEnumerable<IShapeNetworkEntity> innerShapeScope)
		{
			return innerShapeScope.SelectMany(s => s.DependencyAttachments).Select(a => a.Attachment.BNA_FP_ProcessHeaderLink).Where(g => g.IsValid).ToHashSet();
		}
	}
}
