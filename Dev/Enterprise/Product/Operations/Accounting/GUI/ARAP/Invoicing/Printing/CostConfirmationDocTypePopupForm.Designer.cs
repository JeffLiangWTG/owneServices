namespace Enterprise.Accounting.GUI
{
	partial class CostConfirmationDocTypePopupForm
	{
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZButton OKButton;
			Enterprise.ZArchitecture.GUI.ZButton CancelButton;
			Enterprise.ZArchitecture.GUI.ZDropEdit DropEdit;
			OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 85, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 24, true);
			this.MainStatusBar.TabIndex = 4;
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.GUI.CostConfirmationDocTypeBizo);
			// 
			// OKButton
			// 
			OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			OKButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostConfirmationDocTypePopupForm|046e6b5c-8dcf-4b29-90c5-8372703d9560", "&OK");
			OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 74, true);
			OKButton.Name = "OKButton";
			OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			OKButton.TabIndex = 2;
			OKButton.UseVisualStyleBackColor = true;
			// 
			// CancelButton
			// 
			CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			CancelButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostConfirmationDocTypePopupForm|ff3e1d7c-b67d-43a8-89c9-e11940d65b8a", "&Cancel");
			CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(179, 74, true);
			CancelButton.Name = "CancelButton";
			CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			CancelButton.TabIndex = 3;
			CancelButton.UseVisualStyleBackColor = true;
			// 
			// DropEdit
			// 
			this.BindingSource.SetBindingMember(DropEdit, "CostConfirmationDocType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.GUI.CostConfirmationDocTypeBizo)(null)).CostConfirmationDocType)));
			DropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostConfirmationDocTypePopupForm|f264e029-c994-4c52-9e13-338fa738dda0", "Choose type of Cost Confirmation Document you want to print");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(DropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 29, true);
			DropEdit.Name = "DropEdit";
			DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 20, true);
			DropEdit.TabIndex = 1;
			// 
			// CostConfirmationDocTypePopupForm
			// 
			this.AcceptButton = OKButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = CancelButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 109, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostConfirmationDocTypePopupForm|5732271b-cdd9-4543-8887-a7cdfb84ec31", "Cost Confirmation Document Type");
			this.Controls.Add(DropEdit);
			this.Controls.Add(OKButton);
			this.Controls.Add(CancelButton);
			this.DataSourceType = typeof(Enterprise.Accounting.GUI.CostConfirmationDocTypeBizo);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "CostConfirmationDocTypePopupForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(CancelButton, 0);
			this.Controls.SetChildIndex(OKButton, 0);
			this.Controls.SetChildIndex(DropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

	}
}
