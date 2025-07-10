using System;
using System.Reflection;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngineCore.DocWrappers
{
	[AttributeUsage(AttributeTargets.Class)]
	public class WrapperTypeNameAttribute : Attribute
	{
		public WrapperTypeNameAttribute(string wrapperTypeName)
		{
			WrapperTypeName = wrapperTypeName;
		}
		readonly string WrapperTypeName;

		public static string GetName(Type wrapperType)
		{
			string nameFromAttribute = GetNameFromAttributeOnly(wrapperType);
			if (!string.IsNullOrEmpty(nameFromAttribute))
			{
				return nameFromAttribute;
			}
			if (typeof(DocumentWrapper).IsAssignableFrom(wrapperType))
			{
				return wrapperType.Name.Replace("Wrapper", "");
			}
			if (typeof(DocumentWrapperCollection).IsAssignableFrom(wrapperType))
			{
				string result = wrapperType.Name.Replace("WrapperCollection", "");
				MethodInfo indexerInfo = wrapperType.GetMethod("get_Item", new Type[] { typeof(int) });
				if (indexerInfo != null)
				{
					string nameFromAttributeOnTypeFromIndexer = GetNameFromAttributeOnly(indexerInfo.ReturnType);
					if (!string.IsNullOrEmpty(nameFromAttributeOnTypeFromIndexer))
					{
						result = nameFromAttributeOnTypeFromIndexer;
					}
				}
				return result + (NoResString)" Collection";
			}
			return wrapperType.Name;
		}

		static string GetNameFromAttributeOnly(Type typeToCheck)
		{
			object[] wrapperTypeNameAttributes = typeToCheck.GetCustomAttributes(typeof(WrapperTypeNameAttribute), true);
			if (wrapperTypeNameAttributes.Length > 0)
			{
				WrapperTypeNameAttribute wrapperTypeNameAttribute = (WrapperTypeNameAttribute)wrapperTypeNameAttributes[0];
				return wrapperTypeNameAttribute.WrapperTypeName;
			}
			return null;
		}
	}
}
