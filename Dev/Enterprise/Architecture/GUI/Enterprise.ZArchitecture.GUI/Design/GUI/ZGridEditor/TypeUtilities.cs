using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Design
{
	public static class TypeUtilities
	{
		public static bool IsSubclassOfBusinessObject(Type currentType)
		{
			return IsType1SubclassOfType2(currentType, typeof(BusinessObject));
		}

		public static bool IsSubclassOfBusinessObjectCollection(Type currentType)
		{
			return DoesTypeImplementInterface(currentType, typeof(IBusinessObjectCollection));
		}

		public static bool IsCodeDescriptionPairList(Type currentType)
		{
			if (currentType.Name == nameof(CodeDescriptionPairList))
			{
				return true;
			}
			else
			{
				return IsType1SubclassOfType2(currentType, typeof(CodeDescriptionPairList));
			}
		}

		public static bool DoesTypeImplementInterface(Type type1, Type @interface)
		{
			var result = false;

			foreach (var implementedInterface in type1.GetInterfaces())
			{
				if (@interface.FullName == implementedInterface.FullName)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		public static bool IsType1SubclassOfType2(Type type1, Type type2)
		{
			var result = false;
			while (type1.BaseType != null)
			{
				if (type1.BaseType.Name == type2.Name)
				{
					result = true;
					break;
				}
				else
				{
					type1 = type1.BaseType;
				}
			}

			return result;
		}
	}
}
