using System;
using System.ComponentModel;

using CargoWise.Common.Collections;
using CargoWise.Common.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Applied to the type for a control and used at design time to determine the default BindingMember.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class DefaultDataSourceBindingMemberAttribute : Attribute
	{
		public DefaultDataSourceBindingMemberAttribute(string defaultBindingMember)
		{
			this.defaultBindingMember = defaultBindingMember;
		}

		public static string GetDefaultBindingMember(Type controlType)
		{
			var result = defaultBindingMemberCache[controlType];
			if (result == null)
			{
				var attr = (DefaultDataSourceBindingMemberAttribute)TypeDescriptor.GetAttributes(controlType)[typeof(DefaultDataSourceBindingMemberAttribute)];
				result = attr == null ? "" : attr.DefaultBindingMember ?? "";
				defaultBindingMemberCache.Add(controlType, result);
			}
			return string.IsNullOrEmpty(result) ? null : result;
		}
		[SuppressThreadStaticFieldMessage]
		static readonly LRUCache<Type, string> defaultBindingMemberCache = new LRUCache<Type, string>();

		public string DefaultBindingMember
		{
			get { return defaultBindingMember; }
		}
		readonly string defaultBindingMember;
	}
}
