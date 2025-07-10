using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class FilterRulesDiagnosisUserControl : ZUserControl
	{
		public FilterRulesDiagnosisUserControl()
		{
			InitializeComponent();
		}

		#region ZUserControl Overrides

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null)
			{
				toComponentGrid = (ZGrid)FindForm().Controls.Find("TransferFailureGrid", searchAllChildren: true)[0];
				toComponentGrid.ListManager.CurrentChanged += ToComponentGrid_CurrentChanged;

				BindFilterStripsControl();
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					if (components != null)
					{
						components.Dispose();
					}

					if (toComponentGrid != null && toComponentGrid.ListManager != null)
					{
						toComponentGrid.ListManager.CurrentChanged -= ToComponentGrid_CurrentChanged;
					}
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			// This is happening here because of designer + DPI scaling issues. Can be removed once Anchor.Left | Anchor.Right works as it should.

			ControlDpiScalingHelper.SetWidth(ref FilterRulesGroupBox, Width - Padding.Left - Padding.Right - FilterRulesGroupBox.Left, false);
			ControlDpiScalingHelper.SetWidth(ref FilterStripsGroupBox, Width - Padding.Left - Padding.Right - FilterStripsGroupBox.Left, false);
			ControlDpiScalingHelper.SetHeight(ref FilterStripsGroupBox, Height - Padding.Top - Padding.Bottom - FilterStripsGroupBox.Top, false);
		}

		#endregion

		#region Implementation

		void ToComponentGrid_CurrentChanged(object sender, EventArgs e)
		{
			BindFilterStripsControl();
		}

		void BindFilterStripsControl()
		{
			var selectedItem = (WorkflowTransferDiagnosis)toComponentGrid.ListManager.GetCurrent();

			if (selectedItem != null)
			{
				FilterStripsControl.SetDataBinding(selectedItem.Link?.FilterRule, string.Empty);
			}

			FilterStripsControl.SetReadOnly(true);
		}

		ZGrid toComponentGrid;

		#endregion
	}
}
