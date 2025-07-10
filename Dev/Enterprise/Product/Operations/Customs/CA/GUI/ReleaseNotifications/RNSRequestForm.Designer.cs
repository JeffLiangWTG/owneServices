using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	partial class RNSRequestForm
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
		private new void InitializeComponent()
		{
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RNSRequestGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TransactionNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CargoControlNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SubLocationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DateOfArrivalZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RNSRequestGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 244, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.RNSRequestBO);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(436, 220, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 2;
			this.CancelButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("RNSRequestForm|30b6b045-d281-4de5-b023-6b66b1cb11ee", "Cancel");
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 220, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SendButton.TabIndex = 1;
			this.SendButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("RNSRequestForm|4ffecd5c-6198-46f1-9aa0-e98a7e17626c", "Send");
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// RNSRequestGroupBox
			// 
			this.RNSRequestGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RNSRequestGroupBox.Controls.Add(this.MessageDescriptionTextBox);
			this.RNSRequestGroupBox.Controls.Add(this.OfficeCodeFindBox);
			this.RNSRequestGroupBox.Controls.Add(this.TransactionNumberTextBox);
			this.RNSRequestGroupBox.Controls.Add(this.CargoControlNumberTextBox);
			this.RNSRequestGroupBox.Controls.Add(this.SubLocationCodeFindBox);
			this.RNSRequestGroupBox.Controls.Add(this.DateOfArrivalZDateEdit);
			this.RNSRequestGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.RNSRequestGroupBox.Name = "RNSRequestGroupBox";
			this.RNSRequestGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 213, true);
			this.RNSRequestGroupBox.TabIndex = 4;
			this.RNSRequestGroupBox.TabStop = false;
			this.RNSRequestGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("RNSRequestForm|b0c7bbb9-bcea-48eb-9a87-ea33c22b3322", "RNS Request");
			// 
			// MessageDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageDescriptionTextBox, "MessageDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.CA.Business.RNSRequestBO)(null)).MessageDescription)));
			this.MessageDescriptionTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("RNSRequestForm|e334e4dd-e081-41a6-acbd-ab2d1344731b", "Message Type");
			this.MessageDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 28, true);
			this.MessageDescriptionTextBox.Name = "MessageDescriptionTextBox";
			this.MessageDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.MessageDescriptionTextBox.TabIndex = 10;
			// 
			// OfficeCodeFindBox
			// 
			this.OfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OfficeCodeFindBox, "OfficeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.RNSRequestBO)(null)).OfficeCode)));
			this.OfficeCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("RNSRequestForm|a3342766-d847-4e0c-b5a7-11811dea4936", "CBSA Office");
			this.OfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 160, true);
			this.OfficeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.OfficeCodeFindBox.Name = "OfficeCodeFindBox";
			this.OfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 20, true);
			this.OfficeCodeFindBox.TabIndex = 50;
			// 
			// TransactionNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransactionNumberTextBox, "TransactionNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.RNSRequestBO)(null)).TransactionNumber)));
			this.TransactionNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("RNSRequestForm|0209beda-2273-4ac6-8fd4-a8a196cac6e9", "Transaction #", "Transaction Number", "");
			this.TransactionNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 94, true);
			this.TransactionNumberTextBox.Name = "TransactionNumberTextBox";
			this.TransactionNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.TransactionNumberTextBox.TabIndex = 30;
			// 
			// CargoControlNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CargoControlNumberTextBox, "CargoControlNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.RNSRequestBO)(null)).CargoControlNumber)));
			this.CargoControlNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("RNSRequestForm|21ad0f2d-dd83-4e29-a1bf-f715b07a2259", "CCN", "Cargo Control Number", "");
			this.CargoControlNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 61, true);
			this.CargoControlNumberTextBox.Name = "CargoControlNumberTextBox";
			this.CargoControlNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.CargoControlNumberTextBox.TabIndex = 20;
			// 
			// SubLocationCodeFindBox
			// 
			this.SubLocationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SubLocationCodeFindBox, "SubLocationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.RNSRequestBO)(null)).SubLocationCode)));
			this.SubLocationCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("RNSRequestForm|9CE153D0-410C-407F-8F08-ACD9C58DCB59", "Sub-Location");
			this.SubLocationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 193, true);
			this.SubLocationCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.CA.SubLocation;
			this.SubLocationCodeFindBox.Name = "SubLocationCodeFindBox";
			this.SubLocationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 20, true);
			this.SubLocationCodeFindBox.TabIndex = 51;
			// 
			// DateOfArrivalZDateEdit
			// 
			this.DateOfArrivalZDateEdit.AllowDrop = true;
			this.DateOfArrivalZDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateOfArrivalZDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateOfArrivalZDateEdit, "DateOfArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.RNSRequestBO)(null)).DateOfArrival)));
			this.DateOfArrivalZDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("RNSRequestForm|52112bcb-ba22-4a69-8cf9-98bf1bdc1fcc", "Arrival Date");
			this.DateOfArrivalZDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DateOfArrivalZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 127, true);
			this.DateOfArrivalZDateEdit.Name = "DateOfArrivalZDateEdit";
			this.DateOfArrivalZDateEdit.TabIndex = 40;
			// 
			// RNSRequestForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 268, true);
			this.Controls.Add(this.RNSRequestGroupBox);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.SendButton);
			this.DataSourceType = typeof(Enterprise.Customs.CA.Business.RNSRequestBO);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 307, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 307, true);
			this.Name = "RNSRequestForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.RNSRequestGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RNSRequestGroupBox.ResumeLayout(false);
			this.RNSRequestGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private new ZButton CancelButton;
		public ZButton SendButton;
		private ZGroupBox RNSRequestGroupBox;
		public ZDateEdit DateOfArrivalZDateEdit;
		public ZTextBox CargoControlNumberTextBox;
		public ZTextBox TransactionNumberTextBox;
		public ZCodeFindBox OfficeCodeFindBox;
		public ZTextBox MessageDescriptionTextBox;
		public ZCodeFindBox SubLocationCodeFindBox;
	}
}
