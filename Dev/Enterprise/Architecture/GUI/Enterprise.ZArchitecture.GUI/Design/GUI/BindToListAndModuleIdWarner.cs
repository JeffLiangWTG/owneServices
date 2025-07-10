using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public static class BindToListAndModuleIdWarner
	{
		public static void ShowOnChangedBindToListWarning(IComponent control)
		{
			if (ShouldShowPropertyGridChangePropertyWarning(control))
			{
				SuppressMessageBoxUntilApplicationIdle();
				ShowWarning(BindToListMessage, BindToListCaption, BindToListWikiAddress);
			}
		}

		public static void ShowOnChangedModuleIDWarning(IComponent control)
		{
			if (ShouldShowPropertyGridChangePropertyWarning(control))
			{
				SuppressMessageBoxUntilApplicationIdle();
				ShowWarning(ModuleIDMessage, ModuleIDCaption, ModuleIDWikiAddress);
			}
		}

		static void ShowWarning(string message, string caption, string wikiAddress)
		{
			using (var notification = new ZMessageBox(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning))
			{
				var result = notification.ShowDialog();
				if (result == DialogResult.OK)
				{
					WebUrlLauncher.Launch(wikiAddress);
				}
			}
		}

		static bool ShouldShowPropertyGridChangePropertyWarning(IComponent component)
		{
			var designerHost = component.Site == null ? null : (IDesignerHost)component.Site.GetService(typeof(IDesignerHost));
			var control = component as Control;
			return
				designerHost != null &&
				!designerHost.Loading &&
				component.IsDesignMode() &&
				(control == null || control.Created) &&
				!suppressedUntilApplicationIdle;
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer design constant")]
		const string BindToListMessage =
			"The BindToList property should be used sparingly. Consider using the [List] attribute on the property this control is bound to.\r\n" +
			"Press Ok to navigate to the wiki page for more information.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer design constant")]
		const string BindToListCaption = "BindToList assignment";
		const string BindToListWikiAddress = "https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/ZArchitectureMetaData.aspx";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer design constant")]
		const string ModuleIDMessage = "You need to specify the [ModuleID] attribute on the business object collection class.\r\nPress Ok to navigate to wiki page for more information.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer design constant")]
		const string ModuleIDCaption = "ModuleID assignment";
		const string ModuleIDWikiAddress = "https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/ModuleIDs%20for%20controls.aspx";

		[ThreadStatic]
		static bool suppressedUntilApplicationIdle;

		static void SuppressMessageBoxUntilApplicationIdle()
		{
			suppressedUntilApplicationIdle = true;
			Application.Idle -= new EventHandler(ApplicationIdle_UnsuppressMessageBox);
			Application.Idle += new EventHandler(ApplicationIdle_UnsuppressMessageBox);
		}

		static void ApplicationIdle_UnsuppressMessageBox(object sender, EventArgs e)
		{
			suppressedUntilApplicationIdle = false;
			Application.Idle -= new EventHandler(ApplicationIdle_UnsuppressMessageBox);
		}

		#endregion
	}
}