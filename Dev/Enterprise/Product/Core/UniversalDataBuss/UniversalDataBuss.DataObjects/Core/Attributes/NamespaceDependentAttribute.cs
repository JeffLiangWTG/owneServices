using System;
using System.Reflection;
using CargoWise.Common;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
	public sealed class NamespaceDependentAttribute : Attribute
	{
		public NamespaceDependentAttribute(string nameSpace, Type type)
		{
			this.nameSpace = Argument.NotNull(nameSpace, "string nameSpace");
			this.typeForProperty = Argument.NotNull(type, "Type type");
		}

		readonly string nameSpace;
		readonly Type typeForProperty;

		public static bool TryGetType(PropertyInfo property, string nameSpace, out Type typeForProperty)
		{
			var attributes = property.GetCustomAttributes(typeof(NamespaceDependentAttribute), true);
			if (attributes.Length > 0)
			{
				foreach (NamespaceDependentAttribute attribute in attributes)
				{
					if (attribute.nameSpace == nameSpace)
					{
						typeForProperty = attribute.typeForProperty;
						return true;
					}
				}

				typeForProperty = null;
				return true;
			}

			typeForProperty = null;
			return false;
		}
	}
}
