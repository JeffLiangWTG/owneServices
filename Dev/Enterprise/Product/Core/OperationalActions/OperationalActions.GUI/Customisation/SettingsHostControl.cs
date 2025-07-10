using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Interop;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Services.OperationalActions.GUI
{
	[SuppressFormsLocalizedTest]
	[DefaultDataSourceBindingMember(null)]
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	internal sealed partial class SettingsHostControl : ZUserControl
	{
		public SettingsHostControl()
		{
			BindingSource.DataSourceType = typeof(OperationalActionMethodDescriptor);
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			OperationalActionMethodDescriptor currentDescriptor = CurrentDataItem as OperationalActionMethodDescriptor;

			if (currentDescriptor != null)
			{
				UnhookDescriptor(currentDescriptor);
			}
		}
		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			OperationalActionMethodDescriptor currentDescriptor = CurrentDataItem as OperationalActionMethodDescriptor;

			if (currentDescriptor != null)
			{
				HookDescriptor(currentDescriptor);
				Attach(currentDescriptor.Method, currentDescriptor.Settings);
			}
			else
			{
				Attach(null, null);
			}

			base.OnCurrentDataItemChanged(e);
		}

		void HookDescriptor(OperationalActionMethodDescriptor descriptor)
		{
			descriptor.MethodGroupInfo.ValueChanged += UpdateSettingsHost;
			descriptor.MethodIDInfo.ValueChanged += UpdateSettingsHost;
		}
		void UnhookDescriptor(OperationalActionMethodDescriptor descriptor)
		{
			descriptor.MethodGroupInfo.ValueChanged -= UpdateSettingsHost;
			descriptor.MethodIDInfo.ValueChanged -= UpdateSettingsHost;
		}
		void UpdateSettingsHost(object sender, EventArgs e)
		{
			OperationalActionMethodDescriptor currentDescriptor = CurrentDataItem as OperationalActionMethodDescriptor;
			Attach(currentDescriptor.Method, currentDescriptor.Settings);
		}

		void Attach(OperationalActionMethod method, OperationalActionMethodSettings settings)
		{
			if (method != currentMethod || settings != currentSettings)
			{
				currentMethod = method;
				currentSettings = settings;
				AttachCore(method, settings);
			}
		}
		void AttachCore(OperationalActionMethod method, OperationalActionMethodSettings settings)
		{
			if (method == null)
			{
				CoverText = NoMethod;
			}
			else if (!method.HasSettings || settings == null)
			{
				CoverText = NoSettings;
			}
			else
			{
				Control control = (Control)method.NewSettingsControl();
				control.Dock = DockStyle.Fill;

				ZBindingSource bindingSource = new ZBindingSource();
				bindingSource.SetBindingMember(control, ".");
				bindingSource.SetDataBinding(settings, "");

				((IDefaultBindingSettings)this.BindingSource).ExcludeFromDefaultBinding(control);

				ContentControl = control;
			}
		}

		protected override void InitLayout()
		{
			base.InitLayout();
#if DEBUG
			if (Site != null && Site.DesignMode)
			{
				CoverText = "Settings Host";
			}
			else
#endif
			{
				CoverText = NoMethod;
			}
		}

		string CoverText
		{
			set
			{
				ZLabel coverLabel = ContentControl as ZLabel;

				if (coverLabel == null)
				{
					ZLabel newLabel = new ZLabel();
					newLabel.Dock = DockStyle.Fill;
					newLabel.TextAlign = ContentAlignment.MiddleCenter;
					newLabel.Text = value;

					ContentControl = newLabel;
				}
				else
				{
					coverLabel.Text = value;
				}
			}
		}

		Control ContentControl
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return contentControl; }
			set
			{
				if (contentControl != value)
				{
					try
					{
						#if !WINZOR
						SafeNativeMethods.LockWindowUpdate(Handle);
						#endif

						if (contentControl != null)
						{
							Controls.Remove(contentControl);
							if (contentControl.IsHandleCreated)
							{
								BeginInvoke(new MethodInvoker(contentControl.Dispose));
							}
						}

						contentControl = value;

						if (contentControl != null)
						{
							Controls.Add(contentControl);
						}
					}
					finally
					{
						#if !WINZOR
						SafeNativeMethods.UnlockWindowUpdate(Handle);
						#endif
					}
				}
			}
		}

		string NoMethod
		{
			get { return Res.GetString("SettingsHostControl|NoMethod", "No defined process selected."); }
		}

		string NoSettings
		{
			get { return Res.GetString("SettingsHostControl|NoSettings", "This defined process has no settings."); }
		}

		Control contentControl;
		OperationalActionMethod currentMethod;
		OperationalActionMethodSettings currentSettings;
	}
}
