using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class ViewUpdateNode : CollectionUpdateNode
	{
		internal ViewUpdateNode(PropertyInfo info)
			: base(info)
		{
		}

		internal bool ElementsHasPropertyInfoForCollection(IBusinessObjectCollection collection)
		{
			var children = GetChildren();

			var view = (IBusinessObjectCollectionView)Info.GetValue(collection, null);

			foreach (var child in children.OfType<CollectionUpdateNode>())
			{
				if (ReflectionHelper.GetPropertyInfo(view.TypeOfElements, child.Info.Name) == null)
				{
					return true;
				}
			}
			return false;
		}
	}
}
