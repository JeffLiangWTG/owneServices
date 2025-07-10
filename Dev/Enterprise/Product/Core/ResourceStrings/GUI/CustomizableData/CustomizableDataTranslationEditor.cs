using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ResourceStrings.GUI
{
	public class CustomizableDataTranslationEditor : ICustomizableDataTranslationEditor
	{
		public void EditTranslations(ZTranslatableTextControl control)
		{
			ResourceString initialValue;
			BusinessObject context;
			EditTranslations(GetCustomizableDataCaptionSource(control, out initialValue, out context), initialValue, context);
		}

		public void EditTranslations(CustomizableDataResourceStrings customizable, ResourceString initialValue, object context)
		{
			new CustomizableDataTranslationForm(new CustomizableDataTranslationPage(customizable, initialValue, context)).Show();
		}

		CustomizableDataResourceStrings GetCustomizableDataCaptionSource(ZTranslatableTextControl control, out ResourceString caption, out BusinessObject context)
		{
			CustomizableDataResourceStrings result = null;
			caption = null;
			context = null;
			if (control.IsOnGrid)
			{
				result = GetCustomizableDataCaptionSource(control.GridCurrent as BusinessObject, control.GridMember, out caption);
			}
			else
			{
				var dataBoundControl = control as IDataBoundControl;
				if (dataBoundControl != null && dataBoundControl.DataMember != null && dataBoundControl.DataSource != null)
				{
					var bindingMemberInfo = new KBindingMemberInfo(dataBoundControl.DataMember);
					var bindingManager = control.BindingContext[dataBoundControl.DataSource, bindingMemberInfo.BindingPath];
					if (bindingManager.Count > 0)
					{
						context = bindingManager.GetCurrent() as BusinessObject;
						result = GetCustomizableDataCaptionSource(context, bindingMemberInfo.BindingField, out caption);
					}
				}
			}

			if (result == null)
			{
				throw new InvalidOperationException("Could not determine customizable data caption source for " + control.Name + " - " + ControlDescription.GetControlPathAndLocation(control));
			}
			return result;
		}

		CustomizableDataResourceStrings GetCustomizableDataCaptionSource(BusinessObject current, string bindingField, out ResourceString caption)
		{
			CustomizableDataResourceStrings result = null;
			caption = null;
			ZPropertyInfo propertyInfo;
			if (current != null &&
				(propertyInfo = current[bindingField + "Info"] as ZPropertyInfo) != null &&
				propertyInfo.CustomizableDataResourceStrings != null)
			{
				result = propertyInfo.CustomizableDataResourceStrings;
				caption = current[bindingField + "Multilingual"] as ResourceString;
			}
			return result;
		}
	}
}
