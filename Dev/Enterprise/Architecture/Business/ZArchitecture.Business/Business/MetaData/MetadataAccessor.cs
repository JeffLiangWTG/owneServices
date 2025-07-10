using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.ComponentModel
{
	public static class MetadataAccessor
	{
		public static string GetListMember(string defaultValue, PropertyDescriptor descriptor, string fullPath)
		{
			Argument.NotNull(descriptor, "descriptor");

			if (!string.IsNullOrEmpty(defaultValue))
			{
				return defaultValue;
			}

			var attribute = descriptor.Attributes[typeof(ListAttribute)] as ListAttribute;
			return GetListMember(attribute, fullPath);
		}

		public static string GetListMember(ListAttribute attribute, string fullPath)
		{
			string result = null;
			if (attribute != null)
			{
				KBindingMemberInfo listMember = new KBindingMemberInfo(new KBindingMemberInfo(fullPath).BindingPath, attribute.ListDataSourceMember.Replace(".", "+"));
				result = listMember.BindingMember.Replace(ZLookups.LookupsBindingMember + ".", ZLookups.LookupsBindingMember + "+");
			}
			return result;
		}
	}
}
