using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	class HiddenShapesCollection : HiddenShapeChildCollection<IProposedNetworkEntity>
	{
		internal HiddenShapesCollection(IShapeNetworkEntity ownerShape)
			: base(ownerShape)
		{
		}

		protected override IEnumerable<IProposedNetworkEntity> GetHiddenElements()
		{
			return JobHeader.ProcessHeaders.Union(JobHeader.ChildLinks.Select(l => l.HeaderFrom))
				.Where(w => !w.IsDeleted && !w.IsDeleting && w.IsInDatabase && !OwnerShape.Children.Any(s => s.RelatedEntityPK == w.PK))
				.Cast<IProposedNetworkEntity>()
				.OrderBy(w => w.Name);
		}
	}
}
