using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// An IExtenderProvider component that provides functionality for
	/// automatically hiding/showing controls based on:
	///		1. The control name
	///		2. The "Mode" of the operational job
	///		3. A collection of settings - usually provided from the system registry
	///
	/// You must add this component to your form or user control to get this functionality.
	/// </summary>
	[DesignTimeVisible(false)]
	[ToolboxItem(false)]
	[ProvideProperty("IsVisibilityConfigured", typeof(Control))]
	public class ControlVisibilityConfigurationProvider : Component, IExtenderProvider
	{
		#region GetIsVisibilityConfigured / SetIsVisibilityConfigured

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(false)]
		public bool GetIsVisibilityConfigured(Control containerControl)
		{
			return configurations.ContainsKey(containerControl);
		}

		public void SetIsVisibilityConfigured(Control containerControl, bool value)
		{
			var configuration = GetConfiguration(containerControl);
			if (value)
			{
				if (configuration == null)
				{
					configuration = new ControlVisibilityConfiguration(containerControl);
					containerControl.Disposed += new EventHandler(ContainerControl_Disposed);
					configurations[containerControl] = configuration;
				}
			}
			else
			{
				if (configuration != null)
				{
					configuration.Dispose();
					containerControl.Disposed -= new EventHandler(ContainerControl_Disposed);
					configurations.Remove(containerControl);
				}
			}
		}

		void ContainerControl_Disposed(object sender, EventArgs e)
		{
			SetIsVisibilityConfigured((Control)sender, false);
		}

		public void ClearAllVisibilityConfigurations()
		{
			foreach (var configuration in configurations)
			{
				configuration.Key.Disposed -= ContainerControl_Disposed;
				configuration.Value.Dispose();
			}
			configurations.Clear();
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			foreach (var configuration in new List<ControlVisibilityConfiguration>(configurations.Values))
			{
				configuration.Dispose();
			}
		}

		#endregion

		#region IExtenderProvider Members

		bool IExtenderProvider.CanExtend(object extendee)
		{
			return extendee is Panel || extendee is GroupBox || (extendee is ContainerControl && IsRootComponent(extendee));
		}

		bool IsRootComponent(object obj)
		{
			var component = obj as IComponent;
			var designerHost = component.Site == null ? null : (IDesignerHost)component.Site.GetService(typeof(IDesignerHost));
			return designerHost != null && designerHost.RootComponent == obj;
		}

		#endregion

		#region Implementation

#if DEBUG
		internal
#endif
 Dictionary<Control, ControlVisibilityConfiguration> configurations = new Dictionary<Control, ControlVisibilityConfiguration>();

		ControlVisibilityConfiguration GetConfiguration(Control containerControl)
		{
			ControlVisibilityConfiguration result = null;
			configurations.TryGetValue(containerControl, out result);
			return result;
		}

		#endregion
	}
}
