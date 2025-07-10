using System;
using System.Collections;
using System.ComponentModel;
using CargoWise.Common.Collections;

namespace CargoWise.ComponentModel.Testing
{
	public delegate PropertyDescriptorCollection ItemPropertiesGetter(Type type);

	internal static class TypedListHelper
	{
		public static PropertyDescriptorCollection GetItemProperties(Type itemType, PropertyDescriptor[] listAccessors, ItemPropertiesGetter itemPropertiesGetter)
		{
			PropertyDescriptorCollection result = null;
			if (listAccessors == null || listAccessors.Length == 0)
			{
				result = itemPropertiesGetter(itemType);
			}
			else
			{
				Type rightMostPropertyType = listAccessors[listAccessors.Length - 1].PropertyType;
				if (typeof(IList).IsAssignableFrom(rightMostPropertyType))
				{
					Type listElementType = ListUtil.GetListElementType(rightMostPropertyType);
					if (listElementType != null)
					{
						result = itemPropertiesGetter(listElementType);
					}
					else
					{
						result = itemPropertiesGetter(rightMostPropertyType);
					}
				}
				else
				{
					result = itemPropertiesGetter(rightMostPropertyType);
				}
			}
			return result;
		}
	}
}
