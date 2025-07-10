using System;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace CargoWise.ComponentModel.Design
{
	/// <summary>
	/// When applying CargoWise.ComponentModel.Design.BindingMemberEditor to a property
	/// that specifies a bind to member, this attribute must be applied to the 
	/// property to specify the data source type of the bind to member.<br/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
	public sealed class BindingMemberEditorAttribute : Attribute
	{
		public BindingMemberEditorAttribute(string dataSourceTypeMember, string bindingMemberTypeFilterMember)
			: this(dataSourceTypeMember, bindingMemberTypeFilterMember, true)
		{
		}

		public BindingMemberEditorAttribute(string dataSourceTypeMember, string bindingMemberTypeFilterMember, bool allowListProperties)
		{
			DataSourceTypeMember = dataSourceTypeMember;
			BindingMemberTypeFilterMember = bindingMemberTypeFilterMember;
			AllowListProperties = allowListProperties;
		}

		public BindingMemberEditorAttribute(string dataSourceTypeMember, Type bindingMemberTypeFilter)
			: this(dataSourceTypeMember, bindingMemberTypeFilter, true)
		{
		}

		public BindingMemberEditorAttribute(string dataSourceTypeMember, Type bindingMemberTypeFilter, bool allowListProperties)
		{
			DataSourceTypeMember = dataSourceTypeMember;
			BindingMemberTypeFilter = bindingMemberTypeFilter;
			AllowListProperties = allowListProperties;
		}

		public bool AllowListProperties { get; private set; }

		public Type GetDataSourceType(object component, PropertyDescriptor bindingMemberProperty)
		{
			Type result = null;
			if (!string.IsNullOrEmpty(DataSourceTypeMember))
			{
				result = GetTypeFromMember(component, bindingMemberProperty, DataSourceTypeMember);
			}
			return result;
		}

		public Type GetBindingMemberTypeFilter(object component, PropertyDescriptor bindingMemberProperty)
		{
			var result = BindingMemberTypeFilter;
			if (result == null && !string.IsNullOrEmpty(BindingMemberTypeFilterMember))
			{
				result = GetTypeFromMember(component, bindingMemberProperty, BindingMemberTypeFilterMember);
			}
			return result;
		}

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			var rhs = obj as BindingMemberEditorAttribute;
			return
				rhs != null &&
				DataSourceTypeMember == rhs.DataSourceTypeMember &&
				AllowListProperties == rhs.AllowListProperties &&
				BindingMemberTypeFilter == rhs.BindingMemberTypeFilter &&
				BindingMemberTypeFilterMember == rhs.BindingMemberTypeFilterMember;
		}

		public override int GetHashCode()
		{
			return DataSourceTypeMember != null ? DataSourceTypeMember.GetHashCode() : -1;
		}

		#endregion

		#region Implementation

		internal readonly string DataSourceTypeMember;
		internal readonly Type BindingMemberTypeFilter;
		internal readonly string BindingMemberTypeFilterMember;

		Type GetTypeFromMember(object component, PropertyDescriptor bindingMemberProperty, string member)
		{
			component = GetNonDesignerActionListComponent(component);

			var property = TypeDescriptor.GetProperties(component)[member];
			if (property != null && component != null)
			{
				var properties = KPropertyDescriptorCollection.FromType(component.GetType()).AllPropertiesSafe();
				property = properties != null ? properties[member] : null;
			}
			var result = property == null ? null : (Type)property.GetValue(component);
			if (result == null)
			{
				var provider = bindingMemberProperty.GetExtenderProvider();
				if (provider == null)
				{
					property = null;
				}
				else
				{
					var properties = PropertyDescriptorCollectionWithWrappingProperties.FromType(provider.GetType()).AllPropertiesSafe();
					property = properties != null ? properties[member] : null;
				}
				result = property == null ? null : (Type)property.GetValue(provider);
			}
			return result;
		}

		static object GetNonDesignerActionListComponent(object component)
		{
			var result = component;
			var actionList = component as DesignerActionList;
			if (actionList != null)
			{
				result = actionList.Component;
			}
			return result;
		}

		#endregion
	}
}
