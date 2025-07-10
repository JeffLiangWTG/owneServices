using System.Collections.Generic;
using CargoWise.PAVE.Common.Interfaces;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class ShapeNetworkEntityDescendantsStrategy : ILinkDescendantsStrategy<ShapeNetworkEntity>
	{
		#region ILinkDescendantsStrategy

		IEnumerable<ILinkEntity> ILinkDescendantsStrategy.GetChildren(ILinkEntity entity)
		{
			return GetChildren((ShapeNetworkEntity)entity);
		}

		IEnumerable<ILinkEntity> ILinkDescendantsStrategy.GetParents(ILinkEntity entity)
		{
			return GetParents((ShapeNetworkEntity)entity);
		}

		IEnumerable<ILink> ILinkDescendantsStrategy.FilterLinksByScope(IEnumerable<ILink> links)
		{
			return links; // No need to filter, because we have already done that. :)
		}

		#endregion

		public IEnumerable<ShapeNetworkEntity> GetChildren(ShapeNetworkEntity entity)
		{
			return entity.Children;
		}

		public IEnumerable<ShapeNetworkEntity> GetParents(ShapeNetworkEntity entity)
		{
			var owner = entity.Owner;
			if (owner != null)
			{
				yield return owner;
			}
		}
	}
}
