
namespace Enterprise.Client.UPE.GUI
{
	partial class RefundEnquiryForm
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
		protected override void InitializeComponent()
		{
			this.zLabelContact = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelPhoneNumber = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelEnguiryDetails = new Enterprise.ZArchitecture.ZLabel();
			this.zTextEnquiryContact = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextPhoneNumber = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextEnquiryDetails = new Enterprise.ZArchitecture.ZTextBox();
			this.zButtonSave = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zButtonClose = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DropEditRaisedBy = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 263, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 8;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.UPE.Business.ClientRefundWrapper);
			// 
			// zLabelContact
			// 
			this.zLabelContact.AutoSize = true;
			this.zLabelContact.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 20, true);
			this.zLabelContact.Name = "zLabelContact";
			this.zLabelContact.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 13, true);
			this.zLabelContact.TabIndex = 1;
			this.zLabelContact.Text = "Contact:";
			// 
			// zLabelPhoneNumber
			// 
			this.zLabelPhoneNumber.AutoSize = true;
			this.zLabelPhoneNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 46, true);
			this.zLabelPhoneNumber.Name = "zLabelPhoneNumber";
			this.zLabelPhoneNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 13, true);
			this.zLabelPhoneNumber.TabIndex = 3;
			this.zLabelPhoneNumber.Text = "Phone Number:";
			// 
			// zLabelEnguiryDetails
			// 
			this.zLabelEnguiryDetails.AutoSize = true;
			this.zLabelEnguiryDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 72, true);
			this.zLabelEnguiryDetails.Name = "zLabelEnguiryDetails";
			this.zLabelEnguiryDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 13, true);
			this.zLabelEnguiryDetails.TabIndex = 5;
			this.zLabelEnguiryDetails.Text = "Enquiry Details:";
			// 
			// zTextEnquiryContact
			// 
			this.BindingSource.SetBindingMember(this.zTextEnquiryContact, "Contact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Client.UPE.Business.ClientRefundWrapper)(null)).Contact)));
			this.zTextEnquiryContact.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 17, true);
			this.zTextEnquiryContact.Name = "zTextEnquiryContact";
			this.zTextEnquiryContact.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 20, true);
			this.zTextEnquiryContact.TabIndex = 2;
			// 
			// zTextPhoneNumber
			// 
			this.BindingSource.SetBindingMember(this.zTextPhoneNumber, "PhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Client.UPE.Business.ClientRefundWrapper)(null)).PhoneNumber)));
			this.zTextPhoneNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 43, true);
			this.zTextPhoneNumber.Name = "zTextPhoneNumber";
			this.zTextPhoneNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.zTextPhoneNumber.TabIndex = 4;
			// 
			// zTextEnquiryDetails
			// 
			this.BindingSource.SetBindingMember(this.zTextEnquiryDetails, "EnquiryDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Client.UPE.Business.ClientRefundWrapper)(null)).EnquiryDetails)));
			this.zTextEnquiryDetails.HideSelection = false;
			this.zTextEnquiryDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 69, true);
			this.zTextEnquiryDetails.Multiline = true;
			this.zTextEnquiryDetails.Name = "zTextEnquiryDetails";
			this.zTextEnquiryDetails.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.zTextEnquiryDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 105, true);
			this.zTextEnquiryDetails.TabIndex = 6;
			// 
			// zButtonSave
			// 
			this.zButtonSave.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 220, true);
			this.zButtonSave.Name = "zButtonSave";
			this.zButtonSave.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zButtonSave.TabIndex = 9;
			this.zButtonSave.Text = "&Save";
			this.zButtonSave.UseVisualStyleBackColor = true;
			this.zButtonSave.Click += new System.EventHandler(this.zButtonSave_Click);
			// 
			// zButtonClose
			// 
			this.zButtonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.zButtonClose.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 220, true);
			this.zButtonClose.Name = "zButtonClose";
			this.zButtonClose.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zButtonClose.TabIndex = 10;
			this.zButtonClose.Text = "&Close";
			this.zButtonClose.UseVisualStyleBackColor = true;
			this.zButtonClose.Click += new System.EventHandler(this.zButtonClose_Click);
			// 
			// DropEditRaisedBy
			// 
			this.DropEditRaisedBy.BindTo = "EnquiryRaisedBy";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.ClientRefundWrapper)(null)).EnquiryRaisedByInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.ClientRefundWrapper)(null)).EnquiryRaisedBy)));
			this.DropEditRaisedBy.BindToList = "RaisedByList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.ClientRefundWrapper)(null)).RaisedByList)));
			this.DropEditRaisedBy.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 180, true);
			this.DropEditRaisedBy.Name = "DropEditRaisedBy";
			this.DropEditRaisedBy.ShowDescriptionBox = false;
			this.DropEditRaisedBy.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.DropEditRaisedBy.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.DropEditRaisedBy.TabIndex = 9;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 183, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 13, true);
			this.zLabel1.TabIndex = 7;
			this.zLabel1.Text = "Raised By:";
			// 
			// RefundEnquiryForm
			// 
			this.AcceptButton = this.zButtonSave;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.zButtonClose;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 287, true);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.DropEditRaisedBy);
			this.Controls.Add(this.zLabelEnguiryDetails);
			this.Controls.Add(this.zButtonSave);
			this.Controls.Add(this.zButtonClose);
			this.Controls.Add(this.zTextEnquiryDetails);
			this.Controls.Add(this.zTextPhoneNumber);
			this.Controls.Add(this.zLabelPhoneNumber);
			this.Controls.Add(this.zLabelContact);
			this.Controls.Add(this.zTextEnquiryContact);
			this.DataSourceAssemblyName = "ZClientUPE";
			this.DataSourceType = typeof(Enterprise.Client.UPE.Business.ClientRefundWrapper);
			this.DataSourceTypeName = "Enterprise.Client.UPE.Business.ClientRefund";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "RefundEnquiryForm";
			this.Text = "RefundEnquiryForm";
			this.Controls.SetChildIndex(this.zTextEnquiryContact, 0);
			this.Controls.SetChildIndex(this.zLabelContact, 0);
			this.Controls.SetChildIndex(this.zLabelPhoneNumber, 0);
			this.Controls.SetChildIndex(this.zTextPhoneNumber, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zTextEnquiryDetails, 0);
			this.Controls.SetChildIndex(this.zButtonClose, 0);
			this.Controls.SetChildIndex(this.zButtonSave, 0);
			this.Controls.SetChildIndex(this.zLabelEnguiryDetails, 0);
			this.Controls.SetChildIndex(this.DropEditRaisedBy, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel zLabelContact;
		private Enterprise.ZArchitecture.ZLabel zLabelPhoneNumber;
		private Enterprise.ZArchitecture.ZLabel zLabelEnguiryDetails;
		private Enterprise.ZArchitecture.ZTextBox zTextEnquiryContact;
		private Enterprise.ZArchitecture.ZTextBox zTextPhoneNumber;
		private Enterprise.ZArchitecture.ZTextBox zTextEnquiryDetails;
		internal Enterprise.ZArchitecture.GUI.ZButton zButtonSave;
		internal Enterprise.ZArchitecture.GUI.ZButton zButtonClose;
		private Enterprise.ZArchitecture.GUI.ZDropEdit DropEditRaisedBy;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
	}
}