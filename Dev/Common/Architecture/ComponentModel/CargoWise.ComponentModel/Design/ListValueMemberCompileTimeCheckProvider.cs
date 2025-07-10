using System;
using System.ComponentModel;
using CargoWise.Common.Collections;

namespace CargoWise.ComponentModel.Design
{
	/// <summary>
	/// A IBindingMemberForCompileTimeCheckProvider implementation to ensure the ListValueMember meta-data type
	/// is specified with the correct value on the bound ListDataSource. Typically used for ListControls
	/// (DComboBox and DListBox).
	/// </summary>
	public class ListValueMemberCompileTimeCheckProvider : IBindingMemberForCompileTimeCheckProvider
	{
		public CompileTimeCheckBindingMemberCollection GetBindingMembersForCompileTimeCheck(Type dataSourceType, string dataMember)
		{
			var result = new CompileTimeCheckBindingMemberCollection();
			var properties = CompileTimeCheckBindingMember.Empty.NavigateProperties(dataSourceType, dataMember);
			if (properties != null && properties.Length > 0)
			{
				var rightmostBoundProperty = properties[properties.Length - 1];

				var valueMembers = MetaDataValueMemberLocator.GetInstance(rightmostBoundProperty, MetaDataTypes.ListValueMember).MetaDataValues;
				var valueMember = (valueMembers.Length == 1 && valueMembers[0] != null) ? valueMembers[0].ToString() : null;
				var locator = MetaDataValueMemberLocator.GetInstance(rightmostBoundProperty, MetaDataTypes.ListDataSource);

				if (valueMember != null)
				{
					// compile time check the value members of the list when the ListDataSource is specified by a member
					foreach (var listMember in locator.MetaDataMembers)
					{
						var listMemberProperty = TypeDescriptor.GetProperties(rightmostBoundProperty.ComponentType)[listMember];
						if (listMemberProperty != null && listMemberProperty.PropertyType != null)
						{
							var listElementType = ListUtil.GetListElementType(listMemberProperty.PropertyType);
							if (listElementType != null)
							{
								var valueMemberProperty = TypeDescriptor.GetProperties(listElementType)[valueMember];
								if (valueMemberProperty != null)
								{
									var compileTimeCheck = new CompileTimeCheckBindingMember(dataSourceType, valueMemberProperty.PropertyType, dataMember);
									result.Add(compileTimeCheck);
								}
							}
						}
					}
				}

				// compile time check the value members of the list when the ListDataSource is constant
				foreach (object listValue in locator.MetaDataValues)
				{
					var valueMemberType = listValue == null ? null : listValue.GetType();
					var listElementType = valueMemberType == null ? null : ListUtil.GetListElementType(listValue.GetType());
					if (listValue != null && valueMember != null)
					{
						var valueMemberProperty = TypeDescriptor.GetProperties(listElementType)[valueMember];
						if (valueMemberProperty != null)
						{
							valueMemberType = valueMemberProperty.PropertyType;
						}
					}
					var compileTimeCheck = new CompileTimeCheckBindingMember(dataSourceType, valueMemberType, null);
					result.Add(compileTimeCheck);
				}
			}
			return result;
		}
	}
}
