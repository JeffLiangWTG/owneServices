using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace CargoWise.Windows.UI
{
	public static class DesignTimeUI
	{
		public static void ShowMessage(IComponent component, string message)
		{
			ShowMessage(component == null ? null : component.Site, message);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
		public static void ShowMessage(IServiceProvider serviceProvider, string message)
		{
			if (serviceProvider != null)
			{
				IUIService uiservice = (IUIService)serviceProvider.GetService(typeof(IUIService));
				uiservice.ShowMessage(message);
			}
			else
			{
				MessageBox.Show(message); // This is used at design time
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
		public static void ShowMessageOnceUntilIdle(IServiceProvider serviceProvider, string message)
		{
			if (!MessagesToSuppressUntilIdle.ContainsKey(message))
			{
				ShowMessage(serviceProvider, message);
				MessagesToSuppressUntilIdle.Add(message, message);
			}
			Application.Idle -= new EventHandler(ApplicationIdle_UnsuppressMessage);
			Application.Idle += new EventHandler(ApplicationIdle_UnsuppressMessage);
		}

		static Dictionary<string, object> MessagesToSuppressUntilIdle
		{
			get { return messagesToSuppressUntilIdle ?? (messagesToSuppressUntilIdle = new Dictionary<string, object>()); }
		}
		[ThreadStatic]
		static Dictionary<string, object> messagesToSuppressUntilIdle;

		static void ApplicationIdle_UnsuppressMessage(object sender, EventArgs e)
		{
			Application.Idle -= new EventHandler(ApplicationIdle_UnsuppressMessage);
			MessagesToSuppressUntilIdle.Clear();
		}
	}
}
