using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLConsolidations;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLConsolidations
{
	public partial class GLEliminationJournalCreationForm
	{
		#region Windows Form Designer generated code

		ZButton CreateButton;
		ZButton CloseButton;
		private ZPeriodEdit FromPeriodEdit;
		private ZPeriodEdit ToPeriodEdit;
		readonly System.ComponentModel.Container components = null;

		new void InitializeComponent()
		{
			this.CreateButton = new ZButton();
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
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 102, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 26, true);
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
			// CreateButton
			// 
			this.CreateButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("057c366c-ce74-4099-882d-2610791cd269", "Create");
			this.CreateButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CreateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 73, true);
			this.CreateButton.Name = "CreateButton";
			this.CreateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CreateButton.TabIndex = 2;
			this.CreateButton.UseVisualStyleBackColor = true;
			this.CreateButton.Click += new System.EventHandler(this.CreateButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("292138dd-4303-4d63-86b7-4fa137ef7317", "Cancel");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 73, true);
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
			this.FromPeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 12, true);
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
			this.ToPeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 38, true);
			this.ToPeriodEdit.Name = "ToPeriodEdit";
			this.ToPeriodEdit.TabIndex = 1;
			// 
			// GLEliminationJournalCreationForm
			// 
			this.AcceptButton = this.CreateButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2b409bda-daea-4a9c-90da-7fc361c66437", "Elimination Journals Create");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 128, true);
			this.Controls.Add(this.ToPeriodEdit);
			this.Controls.Add(this.FromPeriodEdit);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.CreateButton);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(ConsolidationBatchExportAdapter);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.ConsolidationBatchE" +
	"xportAdapter";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "GLEliminationJournalCreationForm";
			this.Controls.SetChildIndex(this.CreateButton, 0);
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
