using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Services.OperationalActions.GUI
{
	[SuppressFormsLocalizedTest]
	[SuppressBindingMemberBashingTest(IncludingChildren = false)]
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	internal sealed partial class RunnerFieldControlHost : ZUserControl
	{
		public RunnerFieldControlHost()
		{
			this.AutoScroll = true;
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			OperationalActionRunner runner = (OperationalActionRunner)CurrentDataItem;

			if (runner != null)
			{
				Populate(runner.Fields);
			}
			else
			{
				RemoveAllControls();
			}
		}

		void Populate(RunnerFieldCollection fields)
		{
			RemoveAllControls();

			if (fields.Count == 0)
			{
				ZLabel cover = new ZLabel();
				cover.Text = Res.GetString("RunnerFieldControlHost|NoFields", "This action has no fields defined.");
				cover.TextAlign = ContentAlignment.MiddleCenter;
				cover.Dock = DockStyle.Fill;
				Controls.Add(cover);
			}
			else
			{
				int top = 0;

				foreach (RunnerField field in fields)
				{
					Control[] controls = ControlProvider.GetControls(field);
					KBindingSource bindingSource = new KBindingSource(this, typeof(RunnerField));

					int offSet = top;

					foreach (Control control in controls)
					{
						bindingSource.SetBindingMember(control, control.GetBindingMember());
						LabelCaptionRenderProvider.SetLabelCaptionVisible(control, false);

						ControlDpiScalingHelper.SetTop(control, control.Top + offSet, false);
						Controls.Add(control);
						top = Math.Max(top, control.Bottom);
					}

					top += ControlDpiScalingHelper.ScaleToCurrentDpiY(ControlProvider.BottomGap);

					bindingSource.SetDataBinding(field, "");
				}
			}
		}

		void RemoveAllControls()
		{
			if (Controls.Count > 0)
			{
				Control[] controls = new Control[Controls.Count];
				Controls.CopyTo(controls, 0);

				foreach (Control control in controls)
				{
					control.Dispose();
				}

				Controls.Clear();
			}
		}
	}
}

#region Test
#if DEBUG

#region Debug Members

namespace Enterprise.Services.OperationalActions.GUI
{
	using System.ComponentModel;

	partial class RunnerFieldControlHost
	{
		public override ISite Site
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Site; }
			set
			{
				base.Site = value;

				if (value != null && value.DesignMode)
				{
					Label label = new Label();
					label.Text = "Runner Field Host";
					label.TextAlign = ContentAlignment.MiddleCenter;
					label.Dock = DockStyle.Fill;
					Controls.Add(label);
				}
			}
		}
	}
}

#endregion

#endif
#endregion
