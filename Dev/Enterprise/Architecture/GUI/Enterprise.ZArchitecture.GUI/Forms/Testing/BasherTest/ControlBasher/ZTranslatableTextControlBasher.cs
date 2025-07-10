#if DEBUG

using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI.Controls;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZTranslatableTextControlBasher : IControlBasher
	{
		public void Bash(Control control, INotifications notifications)
		{
			((ZTranslatableTextControl)control).Text = "Test";
			Form formCreated = null;
			var formCreatedHandler = new EventHandler(delegate(object sender, EventArgs args) { formCreated = sender as Form; });
			ZForm.FormCreated += formCreatedHandler;
			try
			{
				((ZTranslatableTextControl)control).OnLanguageClick();
				if (formCreated == null || formCreated.GetType().Name != "CustomizableDataTranslationForm")
				{
					notifications.AddError("CustomizableDataTranslationForm was not opened OnLanguageClick for " + control.Name + " - " + ControlDescription.GetControlPathAndLocation(control));
				}
				else
				{
					formCreated.Dispose();
				}
			}
			finally
			{
				ZForm.FormCreated -= formCreatedHandler;
			}
		}
	}
}

#endif