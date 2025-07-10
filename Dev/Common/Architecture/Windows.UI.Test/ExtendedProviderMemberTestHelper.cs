using System;
using System.ComponentModel;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ExtendedProviderMemberTestHelper
	{
		public static void AssertExtenderMembersAreProtectedFields(Type controlType)
		{
			foreach (MemberInfo member in controlType.GetMembers(BindingFlags.NonPublic | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (typeof(IExtenderProvider).IsAssignableFrom(GetFieldOrPropertyType(member)) &&
					!IsFieldOrPropertyPrivate(member))
				{
					Assertion.AssertEquals(
						"If you are providing a field of a type that implements IExtenderProvider " +
						"with the view that subclasses may also use the IExtenderProvider at " +
						"design time, the field must be a field (not a property) and must be " +
						"public or protected (" + member.Name + ")",
						true, member is FieldInfo);
				}
			}
		}

		static Type GetFieldOrPropertyType(MemberInfo member)
		{
			FieldInfo field = member as FieldInfo;
			PropertyInfo property = member as PropertyInfo;
			if (field != null)
			{
				return field.FieldType;
			}
			if (property != null)
			{
				return property.PropertyType;
			}
			return null;
		}

		static bool IsFieldOrPropertyPrivate(MemberInfo member)
		{
			FieldInfo field = member as FieldInfo;
			PropertyInfo property = member as PropertyInfo;
			if (field != null)
			{
				return (field.Attributes & FieldAttributes.Private) == FieldAttributes.Private;
			}
			if (property != null)
			{
				return ((property.GetGetMethod(true) ?? property.GetSetMethod(true)).Attributes & MethodAttributes.Private) == MethodAttributes.Private;
			}
			return false;
		}
	}
}
