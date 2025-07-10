namespace Enterprise.Customs.GB.GUI.CDSCashPayments
{
	partial class CDSCashPaymentsUserControl
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
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CDSCashPaymentsUserControl));
            this.PaymentDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.Importer = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
            this.PaymentAmount = new Enterprise.ZArchitecture.ZCalcEdit();
            this.PaymentReference = new Enterprise.ZArchitecture.ZTextBox();
            this.IncomingPayResponseNo = new Enterprise.ZArchitecture.ZTextBox();
            this.BGMReference = new Enterprise.ZArchitecture.ZTextBox();
            this.DeclarationReference = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
            this.ReceiptDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.OpenDeclarationButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.MRN = new Enterprise.ZArchitecture.ZTextBox();
            this.TransactionType = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.PaymentStatus = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.PaymentDate.SuspendLayout();
            this.Importer.SuspendLayout();
            this.ReceiptDate.SuspendLayout();
            this.TransactionType.SuspendLayout();
            this.PaymentStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Business.CusEntryPayInfo);
            // 
            // PaymentDate
            // 
            this.PaymentDate.AllowDrop = true;
            this.PaymentDate.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.PaymentDate, "C9_PaymentDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.CusEntryPayInfo)(null)).C9_PaymentDate)));
            this.PaymentDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(167, 187, true);
            this.PaymentDate.Name = "PaymentDate";
            this.PaymentDate.TabIndex = 3;
            // 
            // Importer
            // 
            this.Importer.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.Importer, "EntryHeader.Declaration.JE_OH_Importer");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.GB.Business.CusEntryPayInfo)(null)).EntryHeader.Declaration.JE_OH_Importer)));
            this.Importer.Captions = new string[0];
            this.Importer.IsCaptionOverridden = false;
            this.Importer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.Importer.Name = "Importer";
            this.Importer.OrgAddressFormatter = null;
            this.Importer.PopupCaption = "";
            this.Importer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
            this.Importer.TabIndex = 1;
            // 
            // PaymentAmount
            // 
            this.BindingSource.SetBindingMember(this.PaymentAmount, "C9_PaymentAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Business.CusEntryPayInfo)(null)).C9_PaymentAmount)));
            this.PaymentAmount.DecimalPlaces = 2;
            this.PaymentAmount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 161, true);
            this.PaymentAmount.Name = "PaymentAmount";
            this.PaymentAmount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.PaymentAmount.TabIndex = 2;
            this.PaymentAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // PaymentReference
            // 
            this.PaymentReference.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.PaymentReference, "C9_PaymentReference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.CusEntryPayInfo)(null)).C9_PaymentReference)));
            this.PaymentReference.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(407, 57, true);
            this.PaymentReference.Name = "PaymentReference";
            this.PaymentReference.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
            this.PaymentReference.TabIndex = 5;
            // 
            // IncomingPayResponseNo
            // 
            this.IncomingPayResponseNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.IncomingPayResponseNo, "C9_IncomingPayResponseNo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.CusEntryPayInfo)(null)).C9_IncomingPayResponseNo)));
            this.IncomingPayResponseNo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(407, 83, true);
            this.IncomingPayResponseNo.Name = "IncomingPayResponseNo";
            this.IncomingPayResponseNo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
            this.IncomingPayResponseNo.TabIndex = 6;
            // 
            // BGMReference
            // 
            this.BGMReference.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.BGMReference, "LRN");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.CusEntryPayInfo)(null)).LRN)));
            this.BGMReference.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 161, true);
            this.BGMReference.Name = "BGMReference";
            this.BGMReference.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 20, true);
            this.BGMReference.TabIndex = 9;
            // 
            // DeclarationReference
            // 
            this.BindingSource.SetBindingMember(this.DeclarationReference, "EntryHeader.Declaration.HumanReadableName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.CusEntryPayInfo)(null)).EntryHeader.Declaration.HumanReadableName)));
            this.DeclarationReference.IsFontBold = false;
            this.DeclarationReference.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 215, true);
            this.DeclarationReference.Name = "DeclarationReference";
            this.DeclarationReference.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 13, true);
            this.DeclarationReference.TabIndex = 4;
            this.DeclarationReference.TabStop = false;
            this.DeclarationReference.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.DeclarationReference.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OpenDeclarationButton_Click);
            // 
            // ReceiptDate
            // 
            this.ReceiptDate.AllowDrop = true;
            this.ReceiptDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ReceiptDate.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.ReceiptDate, "C9_ReceiptDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.CusEntryPayInfo)(null)).C9_ReceiptDate)));
            this.ReceiptDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 187, true);
            this.ReceiptDate.Name = "ReceiptDate";
            this.ReceiptDate.TabIndex = 10;
            // 
            // OpenDeclarationButton
            // 
            this.OpenDeclarationButton.AutoSize = true;
            this.OpenDeclarationButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("f543d2ec-4d57-4937-af4c-e2b85abed325", "Open");
            this.OpenDeclarationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 210, true);
            this.OpenDeclarationButton.Name = "OpenDeclarationButton";
            this.OpenDeclarationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.OpenDeclarationButton.TabIndex = 11;
            this.OpenDeclarationButton.ToolTipCaption = null;
            this.OpenDeclarationButton.UseVisualStyleBackColor = true;
            this.OpenDeclarationButton.Click += new System.EventHandler(this.OpenDeclarationButton_Click);
            // 
            // MRN
            // 
            this.MRN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.MRN, "MRN");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.CusEntryPayInfo)(null)).MRN)));
            this.MRN.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 135, true);
            this.MRN.Name = "MRN";
            this.MRN.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 20, true);
            this.MRN.TabIndex = 8;
            // 
            // TransactionType
            // 
            this.TransactionType.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TransactionType, "C9_TransactionType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.CusEntryPayInfo)(null)).C9_TransactionType)));
            this.TransactionType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(354, 31, true);
            this.TransactionType.Name = "TransactionType";
            this.TransactionType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 20, true);
            this.TransactionType.TabIndex = 4;
            // 
            // PaymentStatus
            // 
            this.PaymentStatus.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PaymentStatus, "C9_PaymentStatus");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.CusEntryPayInfo)(null)).C9_PaymentStatus)));
            this.PaymentStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(354, 109, true);
            this.PaymentStatus.Name = "PaymentStatus";
            this.PaymentStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 20, true);
            this.PaymentStatus.TabIndex = 7;
            // 
            // CDSCashPaymentsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.PaymentStatus);
            this.Controls.Add(this.TransactionType);
            this.Controls.Add(this.MRN);
            this.Controls.Add(this.OpenDeclarationButton);
            this.Controls.Add(this.ReceiptDate);
            this.Controls.Add(this.DeclarationReference);
            this.Controls.Add(this.BGMReference);
            this.Controls.Add(this.IncomingPayResponseNo);
            this.Controls.Add(this.PaymentReference);
            this.Controls.Add(this.PaymentAmount);
            this.Controls.Add(this.Importer);
            this.Controls.Add(this.PaymentDate);
            this.Name = "CDSCashPaymentsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 240, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.PaymentDate.ResumeLayout(true);
            this.PaymentDate.PerformLayout();
            this.Importer.ResumeLayout(true);
            this.Importer.PerformLayout();
            this.ReceiptDate.ResumeLayout(true);
            this.ReceiptDate.PerformLayout();
            this.TransactionType.ResumeLayout(true);
            this.TransactionType.PerformLayout();
            this.PaymentStatus.ResumeLayout(true);
            this.PaymentStatus.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private MasterFiles.GUI.ZOrganisationControl Importer;
		private ZArchitecture.ZCalcEdit PaymentAmount;
		private ZArchitecture.GUI.ZDateEdit PaymentDate;
		private ZArchitecture.ZTextBox PaymentReference;
		private ZArchitecture.ZTextBox IncomingPayResponseNo;
		private ZArchitecture.ZTextBox BGMReference;
		private ZArchitecture.GUI.ZLinkLabel DeclarationReference;
		private ZArchitecture.GUI.ZDateEdit ReceiptDate;
        private ZArchitecture.GUI.ZButton OpenDeclarationButton;
        private ZArchitecture.ZTextBox MRN;
        private ZArchitecture.GUI.ZDropEdit TransactionType;
        private ZArchitecture.GUI.ZDropEdit PaymentStatus;
    }
}
