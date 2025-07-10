using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IDependentCollectionPropertyHelper
	{
		IBusinessObjectCollection GetDependentCollection(BusinessObject rootElement, Type businessObjectBaseType, Type businessObjectRootType);
	}

	class DependentCollectionPropertyHelper : IDependentCollectionPropertyHelper
	{
		public IBusinessObjectCollection GetDependentCollection(BusinessObject rootElement, Type businessObjectBaseType, Type businessObjectRootType)
		{
			if (businessObjectBaseType != null)
			{
				var dependentBusinessObjectAttr = businessObjectBaseType.GetCustomAttributes(typeof(DependentBusinessObjectAttribute), false).Cast<DependentBusinessObjectAttribute>().FirstOrDefault(a => a.MasterType.IsAssignableFrom(businessObjectRootType));
				if (dependentBusinessObjectAttr != null)
				{
					var collectionPropertyInfo = businessObjectRootType.GetProperty(dependentBusinessObjectAttr.DetailRelationshipCollectionProperty);
					if (collectionPropertyInfo != null)
					{
						return collectionPropertyInfo.GetValue(rootElement) as IBusinessObjectCollection;
					}
				}
			}

			return null;
		}
	}
}
