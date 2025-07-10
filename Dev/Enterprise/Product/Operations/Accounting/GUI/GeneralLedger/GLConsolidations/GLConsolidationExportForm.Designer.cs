using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLConsolidations;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLConsolidations
{
	public partial class GLConsolidationExportForm
	{
		#region Windows Form Designer generated code

		ZButton ExportButton;
		ZButton CloseButton;
		private ZPeriodEdit FromPeriodEdit;
		private ZPeriodEdit ToPeriodEdit;
		readonly System.ComponentModel.Container components = null;

		new void InitializeComponent()
		{
			this.ExportButton = new ZButton();
			this.CloseButton = new ZButton();
			this.FromPeriodEdit = new ZPeriodEdit();
			this.ToPeriodEdit = new ZPeriodEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 99, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 26, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 4;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(265);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(265);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ConsolidationBatchExportAdapter);
			// 
			// ExportButton
			// 
			this.ExportButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("bee20ec9-b8e9-47bc-b56e-c6cabc76ab7d", "Export");
			this.ExportButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ExportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 70, true);
			this.ExportButton.Name = "ExportButton";
			this.ExportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ExportButton.TabIndex = 2;
			this.ExportButton.UseVisualStyleBackColor = true;
			this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("292138dd-4303-4d63-86b7-4fa137ef7317", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 70, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// FromPeriodEdit
			// 
			this.FromPeriodEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.FromPeriodEdit, "FromPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZInt)(((ConsolidationBatchExportAdapter)(null)).FromPeriod)));
			this.FromPeriodEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("21219f17-8d0b-4c02-b39a-e68563da8f46", "From Period");
			this.FromPeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 12, true);
			this.FromPeriodEdit.Name = "FromPeriodEdit";
			this.FromPeriodEdit.TabIndex = 0;
			// 
			// ToPeriodEdit
			// 
			this.ToPeriodEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ToPeriodEdit, "ToPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZInt)(((ConsolidationBatchExportAdapter)(null)).ToPeriod)));
			this.ToPeriodEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0cb76e99-a9b6-43f6-b102-dccc9509eb8e", "To Period");
			this.ToPeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 38, true);
			this.ToPeriodEdit.Name = "ToPeriodEdit";
			this.ToPeriodEdit.TabIndex = 1;
			// 
			// GLConsolidationExportForm
			// 
			this.AcceptButton = this.ExportButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3d654e46-a84a-4992-aff5-efc5cedc944c", "Consolidations Export");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 125, true);
			this.Controls.Add(this.ToPeriodEdit);
			this.Controls.Add(this.FromPeriodEdit);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.ExportButton);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(ConsolidationBatchExportAdapter);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.ConsolidationBatchE" +
	"xportAdapter";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "GLConsolidationExportForm";
			this.Controls.SetChildIndex(this.ExportButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.FromPeriodEdit, 0);
			this.Controls.SetChildIndex(this.ToPeriodEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
