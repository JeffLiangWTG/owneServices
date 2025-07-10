using System.Collections.Generic;
using CargoWise.PAVE.Common.Interfaces;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BMNCNShapeDescendantsStrategy : ILinkDescendantsStrategy<BMNCNShape>
	{
		IEnumerable<ILinkEntity> ILinkDescendantsStrategy.GetParents(ILinkEntity entity)
		{
			return GetParents((BMNCNShape)entity);
		}

		IEnumerable<ILinkEntity> ILinkDescendantsStrategy.GetChildren(ILinkEntity entity)
		{
			return GetChildren((BMNCNShape)entity);
		}

		IEnumerable<ILink> ILinkDescendantsStrategy.FilterLinksByScope(IEnumerable<ILink> links)
		{
			return links;
		}

		public IEnumerable<BMNCNShape> GetParents(BMNCNShape entity)
		{
			var parent = entity.ParentShape;

			if (parent != null)
			{
				yield return parent;
			}
		}

		public IEnumerable<BMNCNShape> GetChildren(BMNCNShape entity)
		{
			return entity.ChildShapes;
		}
	}
}
