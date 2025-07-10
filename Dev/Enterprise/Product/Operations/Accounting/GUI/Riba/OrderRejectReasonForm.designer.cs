namespace Enterprise.Accounting.GUI.Riba
{
	partial class OrderRejectReasonForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.OKReasonButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelReasonButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RejectReasonCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ReasonLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 111, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 0, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.Base.Transaction.TransactionReasonHolder);
			// 
			// OKReasonButton
			// 
			this.OKReasonButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OKReasonButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OrderRejectReasonForm|638a3fe3-cd9f-4b54-a56a-33172994aabe", "OK", "&OK", "");
			this.OKReasonButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 83, true);
			this.OKReasonButton.Name = "OKReasonButton";
			this.OKReasonButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.OKReasonButton.TabIndex = 4;
			this.OKReasonButton.Click += new System.EventHandler(this.OKReasonButton_Click);
			// 
			// CancelReasonButton
			// 
			this.CancelReasonButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelReasonButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OrderRejectReasonForm|4256b02a-9e2a-4a0a-aba4-802bd648d732", "Cancel", "Cancel", "");
			this.CancelReasonButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 83, true);
			this.CancelReasonButton.Name = "CancelReasonButton";
			this.CancelReasonButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.CancelReasonButton.TabIndex = 5;
			this.CancelReasonButton.Click += new System.EventHandler(this.CancelReasonButton_Click);
			// 
			// ReasonTextBox
			// 
			this.ReasonTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReasonTextBox, "Reason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.Base.Transaction.TransactionReasonHolder)(null)).Reason)));
			this.ReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 58, true);
			this.ReasonTextBox.Name = "ReasonTextBox";
			this.ReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(413, 20, true);
			this.ReasonTextBox.TabIndex = 3;
			// 
			// RejectReasonCodeDropEdit
			// 
			this.RejectReasonCodeDropEdit.AllowDrop = true;
			this.RejectReasonCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RejectReasonCodeDropEdit, "Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.Base.Transaction.TransactionReasonHolder)(null)).Code)));
			this.RejectReasonCodeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OrderRejectReasonForm|0dc15545-b582-4cb6-8d79-e4ff2df86fda", "Code", "Code", "");
			this.RejectReasonCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 34, true);
			this.RejectReasonCodeDropEdit.Name = "RejectReasonCodeDropEdit";
			this.RejectReasonCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(413, 20, true);
			this.RejectReasonCodeDropEdit.TabIndex = 2;
			// 
			// ReasonLabel
			// 
			this.ReasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 4, true);
			this.ReasonLabel.Name = "ReasonLabel";
			this.ReasonLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(445, 23, true);
			this.ReasonLabel.TabIndex = 1;
			// 
			// OrderRejectReasonForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 111, true);
			this.ControlBox = false;
			this.Controls.Add(this.ReasonLabel);
			this.Controls.Add(this.RejectReasonCodeDropEdit);
			this.Controls.Add(this.ReasonTextBox);
			this.Controls.Add(this.OKReasonButton);
			this.Controls.Add(this.CancelReasonButton);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.Base.Transaction.TransactionReasonHolder);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 150, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 150, true);
			this.Name = "OrderRejectReasonForm";
			this.Text = "OrderRejectReasonForm";
			this.Controls.SetChildIndex(this.CancelReasonButton, 0);
			this.Controls.SetChildIndex(this.OKReasonButton, 0);
			this.Controls.SetChildIndex(this.ReasonTextBox, 0);
			this.Controls.SetChildIndex(this.RejectReasonCodeDropEdit, 0);
			this.Controls.SetChildIndex(this.ReasonLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton OKReasonButton;
		private Enterprise.ZArchitecture.GUI.ZButton CancelReasonButton;
		private Enterprise.ZArchitecture.ZTextBox ReasonTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit RejectReasonCodeDropEdit;
		private ZArchitecture.ZLabel ReasonLabel;
	}
}
