namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class CommitmentSettingControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.amountEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.discountNameDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InfoLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.discountNameDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.CommitmentLicenceSetting);
			// 
			// amountEdit
			// 
			this.BindingSource.SetBindingMember(this.amountEdit, "LicenceUnits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.CommitmentLicenceSetting)(null)).LicenceUnits)));
			this.amountEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("e50e7a27-c6cc-4331-8515-b0c1d1a30b13", "Minimum License Units");
			this.amountEdit.DecimalPlaces = 0;
			this.amountEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.amountEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.amountEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 25, true);
			this.amountEdit.Name = "amountEdit";
			this.amountEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.amountEdit.TabIndex = 0;
			this.amountEdit.Text = "0";
			this.amountEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// discountNameDropEdit
			// 
			this.discountNameDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.discountNameDropEdit, "LS9_Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Billing.Business.CommitmentLicenceSetting)(null)).LS9_Name)));
			this.discountNameDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("f4a1f60f-bf5d-4589-8383-2b81a987b311", "Shared Group Name");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.discountNameDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.discountNameDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 73, true);
			this.discountNameDropEdit.MaxItemsToShowInDropDown = 25;
			this.discountNameDropEdit.Name = "discountNameDropEdit";
			this.discountNameDropEdit.ShouldResizeByMaxLength = true;
			this.discountNameDropEdit.ShowDescriptionBox = false;
			this.discountNameDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.discountNameDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.discountNameDropEdit.TabIndex = 2;
			// 
			// InfoLinkLabel
			// 
			this.InfoLinkLabel.AutoSize = true;
			this.InfoLinkLabel.IsFontBold = false;
			this.InfoLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 96, true);
			this.InfoLinkLabel.Name = "InfoLinkLabel";
			this.InfoLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 13, true);
			this.InfoLinkLabel.TabIndex = 15;
			this.InfoLinkLabel.Text = "What\'s this?";
			this.InfoLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.InfoLinkLabel_LinkClicked);
			// 
			// CommitmentSettingControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.InfoLinkLabel);
			this.Controls.Add(this.discountNameDropEdit);
			this.Controls.Add(this.amountEdit);
			this.Name = "CommitmentSettingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 145, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.discountNameDropEdit.ResumeLayout(true);
			this.discountNameDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZCalcEdit amountEdit;
		private ZArchitecture.GUI.ZDropEdit discountNameDropEdit;
		private ZArchitecture.GUI.ZLinkLabel InfoLinkLabel;
	}
}
