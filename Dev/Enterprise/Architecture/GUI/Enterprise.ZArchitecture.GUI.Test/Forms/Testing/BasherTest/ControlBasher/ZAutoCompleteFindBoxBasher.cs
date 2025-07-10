using System.Reflection;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ZAutoCompleteFindBoxBasher : IControlBasher
	{
		public virtual void Bash(Control control, INotifications notifications)
		{
			var findBox = (ZAutoCompleteFindBox)control;
			var popupFindBox = control as ZPopupFindBox;
			if (popupFindBox != null)
			{
				Application.DoEvents();
				if (popupFindBox.PopupButton.Enabled)
				{
					popupFindBox.PopupButton.PerformClick();
					CheckModuleId(popupFindBox, notifications);
					var popupForm = (Form)control.GetType().GetProperty("PopupForm", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(control, null);
					popupForm.Close();
				}
			}

			// Send the "=" key to make a selection in the findbox
			TestKeyStrokeHelper.SendKeyToControl(findBox.CodeBox, Keys.Oemplus, false);
			TestKeyStrokeHelper.SendKeyToControl(control, Keys.Tab);
			control.Focus();
		}

		static void CheckModuleId(ZPopupFindBox popupFindBox, INotifications notifications)
		{
			if (!popupFindBox.ReadOnly &&
				popupFindBox.ModuleID == ModuleIDs.NotAssigned &&
				!SuppressCheckControlModuleIdAttribute.IsApplied(popupFindBox))
			{
				var dataSourceTypeName = ((IDataBoundControl)popupFindBox).DataSource == null ? "<DataSource=null>" : ((IDataBoundControl)popupFindBox).DataSource.GetType().FullName;
				notifications.AddError(ControlDescription.GetControlPath(popupFindBox) + " - a ModuleID was not specified on the lookup collection class or on the control. DataSourceType='" + dataSourceTypeName + "' BindingMember='" + popupFindBox.BindTo + "'");
			}
		}
	}
}
