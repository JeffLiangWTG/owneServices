using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	internal static class ListMemberVerification
	{
		public static PropertyDescriptor VerifyLookupList(Control control, string additionalDataMember, string controlListMember, INotifications notifications)
		{
			var dataSource = KBindingSource.GetBindingSource(control).DataSource;
			var controlDataMember = KBindingSource.GetBindingSource(control).GetFullBindingMember(control);
			var dataMember = new KBindingMemberInfo(controlDataMember, additionalDataMember).BindingMember;
			var bindingManager = control.BindingContext[dataSource, controlDataMember];
			return VerifyLookupList(control, bindingManager, new KBindingMemberInfo(dataMember).BindingField, controlListMember, notifications);
		}

		public static PropertyDescriptor VerifyLookupList(Control control, BindingManagerBase bindingManager, string dataPropertyName, string controlListMember, INotifications notifications)
		{
			// cv 14/7/08 - temporary until brett/david get back to me about what are the right lists
			if (dataPropertyName == "JU_MessageStatus" ||
				(dataPropertyName.StartsWith("Container+JC_") && control.FindForm() != null && control.FindForm().GetType().Name == "CartageForm"))
			{
				return null;
			}
			PropertyDescriptor listMember = null;
			var dataProperty = bindingManager.GetItemProperties()[dataPropertyName];
			if (dataProperty != null && dataProperty.GetAttributeFromMostSpecificComponentType(typeof(BusinessObjectTestExclude)) == null)
			{
				if (!string.IsNullOrEmpty(controlListMember))
				{
					listMember = string.IsNullOrEmpty(controlListMember) ? null : bindingManager.GetItemProperties()[controlListMember];
				}
				else
				{
					listMember = ZMetaData.GetMetaDataProperty(dataProperty.ComponentType, dataProperty, MetaDataTypes.ListDataSource);
					if (listMember == null)
					{
						var listMemberOnAttributes = MetadataAccessor.GetListMember("", dataProperty, dataPropertyName);
						listMember = bindingManager.GetItemProperties()[listMemberOnAttributes];
					}
				}
				if (listMember == null)
				{
					if (!string.IsNullOrEmpty(controlListMember))
					{
						notifications.AddError(ControlDescription.GetControlPath(control) + " - " + dataPropertyName + ": Could not find list specified by BindToList=" + controlListMember);
					}
					else
					{
						notifications.AddError(ControlDescription.GetControlPath(control) + " - " + dataPropertyName + ": The [List] attribute must be applied to the property with a valid list property name");
					}
				}
			}
			return listMember;
		}
	}
}
