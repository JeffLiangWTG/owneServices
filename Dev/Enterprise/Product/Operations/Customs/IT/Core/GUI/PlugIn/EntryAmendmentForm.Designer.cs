namespace Enterprise.Customs.IT.GUI.PlugIn
{
	partial class EntryAmendmentForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.movementReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.totalEntryLinesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.abortButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 189, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.AutoEntryAmendmentHandler);
			// 
			// movementReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.movementReferenceNumberTextBox, "MovementReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.EntryAmendmentHandler)(null)).MovementReferenceNumber)));
			this.movementReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 28, true);
			this.movementReferenceNumberTextBox.Name = "movementReferenceNumberTextBox";
			this.movementReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 20, true);
			this.movementReferenceNumberTextBox.TabIndex = 1;
			this.movementReferenceNumberTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// totalEntryLinesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.totalEntryLinesCalcEdit, "TotalEntryLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.IT.Business.Declaration.EntryAmendmentHandler)(null)).TotalEntryLines)));
			this.totalEntryLinesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 58, true);
			this.totalEntryLinesCalcEdit.Name = "totalEntryLinesCalcEdit";
			this.totalEntryLinesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 20, true);
			this.totalEntryLinesCalcEdit.TabIndex = 2;
			this.totalEntryLinesCalcEdit.MaxValue = 99999;
			this.totalEntryLinesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("BBF0789E-A048-4D51-BAA5-6DD45DA18B96", "OK");
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 245, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 23, true);
			this.okButton.TabIndex = 4;
			this.okButton.ToolTipCaption = null;
			this.okButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// abortButton
			// 
			this.abortButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.abortButton.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("FA00500D-7895-47DA-AF8F-C10D2D748D42", "Cancel");
			this.abortButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.abortButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 245, true);
			this.abortButton.Name = "abortButton";
			this.abortButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 23, true);
			this.abortButton.TabIndex = 5;
			this.abortButton.ToolTipCaption = null;
			this.abortButton.Click += new System.EventHandler(this.AbortButton_Click);
			// 
			// EntryAmendmentForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.abortButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 300, true);
			this.Controls.Add(this.abortButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.movementReferenceNumberTextBox);
			this.Controls.Add(this.totalEntryLinesCalcEdit);
			this.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.AutoEntryAmendmentHandler);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "EntryAmendmentForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.movementReferenceNumberTextBox, 0);
			this.Controls.SetChildIndex(this.totalEntryLinesCalcEdit, 0);
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.abortButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox movementReferenceNumberTextBox;
		internal ZArchitecture.ZCalcEdit totalEntryLinesCalcEdit;
		internal ZArchitecture.GUI.ZButton okButton;
		internal ZArchitecture.GUI.ZButton abortButton;
	}
}
