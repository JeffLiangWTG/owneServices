using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.Design;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IDynamicControlCreationUserControl
	{
		void ForceCreateHostedControl();
	}

	/// <summary>
	/// Dynamically creates inner controls only when this control becomes visible.
	/// Used for performance enhancements.
	/// </summary>
	[SuppressFormDesignerAnalysis]
	public partial class ZDynamicControlCreationUserControl : ZUserControl, IDynamicControlCreationUserControl
	{
		public ZDynamicControlCreationUserControl()
		{
			InitializeComponent();
		}

		[Editor(DesignerTypes.TypeValueIntellisenseEditor, typeof(UITypeEditor))]
		[TypeConverter(typeof(TypeTypeConverter))]
		[TypeValueIntellisenseEditorSubtypeFilter(typeof(Control))]
		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(null)]
		public Type UserControlType
		{
			get { return userControlType; }
			set
			{
				if (UserControlType != value)
				{
					Control[] oldControls = null;

					if (Controls.Count > 0)
					{
						oldControls = Controls.Cast<Control>().ToArray();
						foreach (var control in oldControls)
						{
							Controls.Remove(control);
						}
					}

					userControlType = value;
					CreateHostedControlIfVisible();

					if (oldControls != null)
					{
						oldControls.ForEach(c => c.Tag = ToBeDisposed);
						Controls.AddRange(oldControls);
						BeginInvoke(
							new MethodInvoker(() =>
							{
								foreach (var control in oldControls)
								{
									control.Dispose();
								}
							}));
					}
				}
			}
		}
		Type userControlType;
		const string ToBeDisposed = "ToBeDisposed";

		[Category(ZGUIConstants.DesignerCategory)]
		[DefaultValue(".")]
		public string BindingMember
		{
			get { return bindingMember; }
			set
			{
				if (bindingMember != value)
				{
					if (HostedControl != null)
					{
						BindingSource.SetBindingMember(HostedControl, bindingMember);
					}

					bindingMember = value;
				}
			}
		}
		string bindingMember = ".";

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public Control HostedControl
		{
			get { return HostedControlNotBeingDiposed; }
			private set
			{
				if (HostedControlNotBeingDiposed == null)
				{
					if (value.Dock != DockStyle.Fill)
					{
						value.Dock = DockStyle.Fill;
					}
					if (!value.Visible)
					{
						value.Visible = true;
					}
					Controls.Add(value);
				}
			}
		}

		Control HostedControlNotBeingDiposed
		{
			get
			{
				foreach (Control control in Controls)
				{
					var tag = control.Tag as string;
					if (string.IsNullOrEmpty(tag) || tag != ToBeDisposed)
					{
						return control;
					}
				}
				return null;
			}
		}

		public event EventHandler HostedControlCreated;

		protected virtual void OnHostedControlCreated(EventArgs e)
		{
			using (PerformanceStatisticsCollector.StartMonitoring("ZDynamicControlCreationUserControl.OnHostedControlCreated", Text))
			{
				HostedControlCreated?.Invoke(this, e);
			}
		}

		#region Implementation

		void IDynamicControlCreationUserControl.ForceCreateHostedControl()
		{
			CreateHostedControl();
		}

		void CreateHostedControlIfVisible()
		{
			if (Visible && Created)
			{
				CreateHostedControl();
			}
		}

		void CreateHostedControl()
		{
			if (UserControlType != null && (HostedControl == null))
			{
				var control = (Control)Activator.CreateInstance(UserControlType);
				if (control is IDataBoundControl &&
					string.IsNullOrEmpty(BindingSource.GetBindingMember(control)))
				{
					BindingSource.SetBindingMember(control, bindingMember);
				}

				HostedControl = control;

				OnHostedControlCreated(EventArgs.Empty);
			}
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			CreateHostedControlIfVisible();
		}

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			CreateHostedControlIfVisible();
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			CreateHostedControlIfVisible();
		}

		#endregion
	}
}
