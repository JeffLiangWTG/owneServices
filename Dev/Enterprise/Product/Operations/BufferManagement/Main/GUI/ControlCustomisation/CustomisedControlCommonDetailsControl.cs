using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;
#if DEBUG
using Enterprise.ZArchitecture.GUI.Testing;
#endif

namespace Enterprise.BufferManagement.GUI
{
	public partial class CustomisedControlCommonDetailsControl : ZUserControl, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public CustomisedControlCommonDetailsControl()
		{
			InitializeComponent();
#if DEBUG
			MissingResourceStringChecker.ExcludeFromTest(this.OrientationDropEdit);
			MissingResourceStringChecker.ExcludeFromTest(this.AlignmentDropEdit);
			MissingResourceStringChecker.ExcludeFromTest(this.FontDropEdit);
#endif
		}

		ControlCustomisationBase Customisation => CurrentDataItem as ControlCustomisationBase;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			((ZDropEditInternals)PropertyBackgroundDropEdit).SetControlWidth(ControlDpiScalingHelper.ScaleToCurrentDpiX(150));
			((ZDropEditInternals)PropertyForegroundDropEdit).SetControlWidth(ControlDpiScalingHelper.ScaleToCurrentDpiX(150));
			((ZDropEditInternals)FontDropEdit).SetControlWidth(ControlDpiScalingHelper.ScaleToCurrentDpiX(150));

			PropertyBackgroundDropEdit.AllowOutsideOfParent();
			PropertyForegroundDropEdit.AllowOutsideOfParent();
			FontDropEdit.AllowOutsideOfParent();
		}

		void RestoreDefaultButton_Click(object sender, EventArgs e)
		{
			var currentDataItem = base.CurrentDataItem as BMControlCustomisationLine;

			if (currentDataItem != null)
			{
				currentDataItem.RestoreDefaultLabel();
			}
		}

		#region Drop Edit Visibility

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (Customisation != null)
			{
				Customisation.ControlTypeInfo.ValueChanged -= SetStatusButtonBehaviourControlVisibility;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (Customisation != null)
			{
				SetStatusButtonBehaviourControlVisibility(this, EventArgs.Empty);
				Customisation.ControlTypeInfo.ValueChanged += SetStatusButtonBehaviourControlVisibility;
			}
		}

		void SetStatusButtonBehaviourControlVisibility(object sender, EventArgs e)
		{
			if (Customisation != null)
			{
				var controlType = Customisation.ControlType;
				var allControlsShouldBeVisible = Customisation != null && controlType == StaticControlTypeList.Codes.StatusButtons;

				PlayButtonDropEdit.Visible = allControlsShouldBeVisible || controlType == StaticControlTypeList.Codes.WorkingStatusButton;
				SuspendButtonDropEdit.Visible = allControlsShouldBeVisible || controlType == StaticControlTypeList.Codes.SuspendStatusButton;
				CloseTaskButtonDropEdit.Visible = allControlsShouldBeVisible || controlType == StaticControlTypeList.Codes.CompletedStatusButton;
			}
		}

		#endregion

		#region IAllowTabBackwardBetweenSomeOfMyChildren

		public bool AllowTabBackward(Control control, Control previousControl)
		{
			return control is ZDropEdit || previousControl is ZDropEdit;
		}

		#endregion
	}
}
