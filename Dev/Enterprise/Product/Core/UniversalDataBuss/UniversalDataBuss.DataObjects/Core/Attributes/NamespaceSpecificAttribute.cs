using System;
using CargoWise.Common;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class NamespaceSpecificAttribute : Attribute
	{
		public NamespaceSpecificAttribute(string nameSpace)
		{
			this.nameSpace = Argument.NotNull(nameSpace, "string nameSpace");
		}

		internal readonly string nameSpace;
	}

	public static class NamespaceSpecificAttributeExtensions
	{
		public static bool IsValidInThisNamespace(this Type type, string nameSpace)
		{
			var attribute = type.GetCustomAttribute<NamespaceSpecificAttribute>(false);
			return attribute == null || attribute.nameSpace == nameSpace;
		}
	}
}
