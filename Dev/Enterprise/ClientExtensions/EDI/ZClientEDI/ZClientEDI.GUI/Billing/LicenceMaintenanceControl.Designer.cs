namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class LicenceMaintenanceControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LicenceMaintenanceControl));
			this.lastMaintenanceBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.nextNewSeatMaintenanceBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.renewalPeriodBox = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.zCalcEdit2 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.lastAmountBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel7 = new Enterprise.ZArchitecture.ZLabel();
			this.lastNewSeatMaintenanceBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.nextMaintenanceBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.notesBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel8 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel9 = new Enterprise.ZArchitecture.ZLabel();
			this.clientReferenceBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zCodeFindBox1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zLabelSurcharge = new Enterprise.ZArchitecture.ZLabel();
			this.zCalcEditSurcharge = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zTextBoxSurchargeDescription = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabelCurrency = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelSurchargeDescription = new Enterprise.ZArchitecture.ZLabel();
			this.zGroupBoxMaintenance = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.pictureBox1 = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel10 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel11 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel12 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel13 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zGroupBoxSurcharge = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.renewalPeriodBox)).BeginInit();
			this.zGroupBoxMaintenance.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.zGroupBoxSurcharge.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader);
			// 
			// lastMaintenanceBox
			// 
			this.BindingSource.SetBindingMember(this.lastMaintenanceBox, "Billing.L0_LastMaintenancePercent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Billing.L0_LastMaintenancePercent)));
			this.lastMaintenanceBox.DecimalPlaces = 2;
			this.lastMaintenanceBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 82, true);
			this.lastMaintenanceBox.Name = "lastMaintenanceBox";
			this.lastMaintenanceBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20, true);
			this.lastMaintenanceBox.TabIndex = 9;
			this.lastMaintenanceBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// nextNewSeatMaintenanceBox
			// 
			this.BindingSource.SetBindingMember(this.nextNewSeatMaintenanceBox, "Billing.L0_NextNewSeatMaintenancePercent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Billing.L0_NextNewSeatMaintenancePercent)));
			this.nextNewSeatMaintenanceBox.DecimalPlaces = 2;
			this.nextNewSeatMaintenanceBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 55, true);
			this.nextNewSeatMaintenanceBox.Name = "nextNewSeatMaintenanceBox";
			this.nextNewSeatMaintenanceBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20, true);
			this.nextNewSeatMaintenanceBox.TabIndex = 7;
			this.nextNewSeatMaintenanceBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel3
			// 
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 16, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.zLabel3.TabIndex = 0;
			this.zLabel3.Text = "Renewal Period";
			this.zLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// renewalPeriodBox
			// 
			this.BindingSource.SetBindingMember(this.renewalPeriodBox, "Billing.L0_RenewalMonths");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Billing.L0_RenewalMonths)));
			this.renewalPeriodBox.BindTo = "Billing.L0_RenewalMonths";
			this.renewalPeriodBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 16, true);
			this.renewalPeriodBox.Name = "renewalPeriodBox";
			this.renewalPeriodBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.renewalPeriodBox.TabIndex = 1;
			// 
			// zLabel4
			// 
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 16, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 20, true);
			this.zLabel4.TabIndex = 0;
			this.zLabel4.Text = "Amount";
			this.zLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zCalcEdit2
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit2, "Billing.L0_FixedMaintenanceAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Billing.L0_FixedMaintenanceAmount)));
			this.zCalcEdit2.DecimalPlaces = 2;
			this.zCalcEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 16, true);
			this.zCalcEdit2.Name = "zCalcEdit2";
			this.zCalcEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.zCalcEdit2.TabIndex = 1;
			this.zCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// lastAmountBox
			// 
			this.BindingSource.SetBindingMember(this.lastAmountBox, "Billing.L0_LastMaintenanceAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Billing.L0_LastMaintenanceAmount)));
			this.lastAmountBox.DecimalPlaces = 2;
			this.lastAmountBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 82, true);
			this.lastAmountBox.Name = "lastAmountBox";
			this.lastAmountBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.lastAmountBox.TabIndex = 11;
			this.lastAmountBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel7
			// 
			this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 30, true);
			this.zLabel7.Name = "zLabel7";
			this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20, true);
			this.zLabel7.TabIndex = 4;
			this.zLabel7.Text = "Amount";
			// 
			// lastNewSeatMaintenanceBox
			// 
			this.BindingSource.SetBindingMember(this.lastNewSeatMaintenanceBox, "Billing.L0_LastNewSeatMaintenancePercent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Billing.L0_LastNewSeatMaintenancePercent)));
			this.lastNewSeatMaintenanceBox.DecimalPlaces = 2;
			this.lastNewSeatMaintenanceBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 82, true);
			this.lastNewSeatMaintenanceBox.Name = "lastNewSeatMaintenanceBox";
			this.lastNewSeatMaintenanceBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20, true);
			this.lastNewSeatMaintenanceBox.TabIndex = 10;
			this.lastNewSeatMaintenanceBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// nextMaintenanceBox
			// 
			this.BindingSource.SetBindingMember(this.nextMaintenanceBox, "Billing.NextAndCurrentMaintenancePercent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Billing.NextAndCurrentMaintenancePercent)));
			this.nextMaintenanceBox.DecimalPlaces = 2;
			this.nextMaintenanceBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 55, true);
			this.nextMaintenanceBox.Name = "nextMaintenanceBox";
			this.nextMaintenanceBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20, true);
			this.nextMaintenanceBox.TabIndex = 6;
			this.nextMaintenanceBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// notesBox
			// 
			this.notesBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.notesBox, "Billing.L0_Comment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Billing.L0_Comment)));
			this.notesBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.notesBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 46, true);
			this.notesBox.Name = "notesBox";
			this.notesBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 20, true);
			this.notesBox.TabIndex = 4;
			// 
			// zLabel8
			// 
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 46, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.zLabel8.TabIndex = 3;
			this.zLabel8.Text = "Notes";
			this.zLabel8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel9
			// 
			this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 69, true);
			this.zLabel9.Name = "zLabel9";
			this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 31, true);
			this.zLabel9.TabIndex = 5;
			this.zLabel9.Text = "Client Reference (shown on invoice)";
			this.zLabel9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// clientReferenceBox
			// 
			this.clientReferenceBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.clientReferenceBox, "Billing.L0_ClientRef");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Billing.L0_ClientRef)));
			this.clientReferenceBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.clientReferenceBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 75, true);
			this.clientReferenceBox.Name = "clientReferenceBox";
			this.clientReferenceBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 20, true);
			this.clientReferenceBox.TabIndex = 6;
			// 
			// zCodeFindBox1
			// 
			this.zCodeFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCodeFindBox1, "Billing.L0_RX_NKFixedMaintenanceCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Billing.L0_RX_NKFixedMaintenanceCurrency)));
			this.zCodeFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 16, true);
			this.zCodeFindBox1.Name = "zCodeFindBox1";
			this.zCodeFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.zCodeFindBox1.TabIndex = 3;
			// 
			// zLabelSurcharge
			// 
			this.zLabelSurcharge.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 18, true);
			this.zLabelSurcharge.Name = "zLabelSurcharge";
			this.zLabelSurcharge.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(17, 20, true);
			this.zLabelSurcharge.TabIndex = 1;
			this.zLabelSurcharge.Text = "%";
			this.zLabelSurcharge.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zCalcEditSurcharge
			// 
			this.BindingSource.SetBindingMember(this.zCalcEditSurcharge, "Billing.L0_Surcharge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Billing.L0_Surcharge)));
			this.zCalcEditSurcharge.DecimalPlaces = 2;
			this.zCalcEditSurcharge.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 19, true);
			this.zCalcEditSurcharge.Name = "zCalcEditSurcharge";
			this.zCalcEditSurcharge.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20, true);
			this.zCalcEditSurcharge.TabIndex = 0;
			this.zCalcEditSurcharge.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zTextBoxSurchargeDescription
			// 
			this.zTextBoxSurchargeDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zTextBoxSurchargeDescription, "Billing.L0_SurchargeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Billing.L0_SurchargeDescription)));
			this.zTextBoxSurchargeDescription.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBoxSurchargeDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 19, true);
			this.zTextBoxSurchargeDescription.Name = "zTextBoxSurchargeDescription";
			this.zTextBoxSurchargeDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 20, true);
			this.zTextBoxSurchargeDescription.TabIndex = 3;
			// 
			// zLabelCurrency
			// 
			this.zLabelCurrency.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 16, true);
			this.zLabelCurrency.Name = "zLabelCurrency";
			this.zLabelCurrency.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 20, true);
			this.zLabelCurrency.TabIndex = 2;
			this.zLabelCurrency.Text = "Currency";
			this.zLabelCurrency.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabelSurchargeDescription
			// 
			this.zLabelSurchargeDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 18, true);
			this.zLabelSurchargeDescription.Name = "zLabelSurchargeDescription";
			this.zLabelSurchargeDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.zLabelSurchargeDescription.TabIndex = 2;
			this.zLabelSurchargeDescription.Text = "Description";
			this.zLabelSurchargeDescription.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zGroupBoxMaintenance
			// 
			this.zGroupBoxMaintenance.Controls.Add(this.pictureBox1);
			this.zGroupBoxMaintenance.Controls.Add(this.zLabel6);
			this.zGroupBoxMaintenance.Controls.Add(this.zLabel10);
			this.zGroupBoxMaintenance.Controls.Add(this.zLabel11);
			this.zGroupBoxMaintenance.Controls.Add(this.zLabel12);
			this.zGroupBoxMaintenance.Controls.Add(this.zLabel13);
			this.zGroupBoxMaintenance.Controls.Add(this.lastMaintenanceBox);
			this.zGroupBoxMaintenance.Controls.Add(this.lastNewSeatMaintenanceBox);
			this.zGroupBoxMaintenance.Controls.Add(this.zLabel7);
			this.zGroupBoxMaintenance.Controls.Add(this.nextNewSeatMaintenanceBox);
			this.zGroupBoxMaintenance.Controls.Add(this.lastAmountBox);
			this.zGroupBoxMaintenance.Controls.Add(this.nextMaintenanceBox);
			this.zGroupBoxMaintenance.Controls.Add(this.zLabel2);
			this.zGroupBoxMaintenance.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.zGroupBoxMaintenance.Name = "zGroupBoxMaintenance";
			this.zGroupBoxMaintenance.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 110, true);
			this.zGroupBoxMaintenance.TabIndex = 0;
			this.zGroupBoxMaintenance.TabStop = false;
			this.zGroupBoxMaintenance.Text = "Variable";
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 14, true);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 90, true);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 26;
			this.pictureBox1.TabStop = false;
			// 
			// zLabel6
			// 
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 10, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 20, true);
			this.zLabel6.TabIndex = 0;
			this.zLabel6.Text = "Annual %";
			this.zLabel6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// zLabel10
			// 
			this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 57, true);
			this.zLabel10.Name = "zLabel10";
			this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.zLabel10.TabIndex = 5;
			this.zLabel10.Text = "Next";
			this.zLabel10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel11
			// 
			this.zLabel11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 80, true);
			this.zLabel11.Name = "zLabel11";
			this.zLabel11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.zLabel11.TabIndex = 8;
			this.zLabel11.Text = "Last";
			this.zLabel11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel12
			// 
			this.zLabel12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 30, true);
			this.zLabel12.Name = "zLabel12";
			this.zLabel12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20, true);
			this.zLabel12.TabIndex = 3;
			this.zLabel12.Text = "New Seats";
			// 
			// zLabel13
			// 
			this.zLabel13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 30, true);
			this.zLabel13.Name = "zLabel13";
			this.zLabel13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20, true);
			this.zLabel13.TabIndex = 2;
			this.zLabel13.Text = "Old Seats";
			// 
			// zLabel2
			// 
			this.zLabel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 19, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 3, true);
			this.zLabel2.TabIndex = 1;
			// 
			// zGroupBoxSurcharge
			// 
			this.zGroupBoxSurcharge.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBoxSurcharge.Controls.Add(this.zTextBoxSurchargeDescription);
			this.zGroupBoxSurcharge.Controls.Add(this.zLabelSurcharge);
			this.zGroupBoxSurcharge.Controls.Add(this.zLabelSurchargeDescription);
			this.zGroupBoxSurcharge.Controls.Add(this.zCalcEditSurcharge);
			this.zGroupBoxSurcharge.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 171, true);
			this.zGroupBoxSurcharge.Name = "zGroupBoxSurcharge";
			this.zGroupBoxSurcharge.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 45, true);
			this.zGroupBoxSurcharge.TabIndex = 2;
			this.zGroupBoxSurcharge.TabStop = false;
			this.zGroupBoxSurcharge.Text = "Surcharge(+)/Discount(-)";
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.zCalcEdit2);
			this.zGroupBox1.Controls.Add(this.zLabel4);
			this.zGroupBox1.Controls.Add(this.zCodeFindBox1);
			this.zGroupBox1.Controls.Add(this.zLabelCurrency);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 120, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 45, true);
			this.zGroupBox1.TabIndex = 1;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.Text = "Fixed";
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 16, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 20, true);
			this.zLabel1.TabIndex = 2;
			this.zLabel1.Text = "Months";
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox2.Controls.Add(this.zLabel3);
			this.zGroupBox2.Controls.Add(this.zLabel1);
			this.zGroupBox2.Controls.Add(this.renewalPeriodBox);
			this.zGroupBox2.Controls.Add(this.notesBox);
			this.zGroupBox2.Controls.Add(this.zLabel8);
			this.zGroupBox2.Controls.Add(this.clientReferenceBox);
			this.zGroupBox2.Controls.Add(this.zLabel9);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 222, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 116, true);
			this.zGroupBox2.TabIndex = 3;
			this.zGroupBox2.TabStop = false;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "Billing.L0_TagNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Billing.L0_TagNote)));
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 345, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.zTextBox1.TabIndex = 7;
			// 
			// zLabel5
			// 
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 345, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 20, true);
			this.zLabel5.TabIndex = 7;
			this.zLabel5.Text = "Tag/Note";
			this.zLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// LicenceMaintenanceControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.zLabel5);
			this.Controls.Add(this.zTextBox1);
			this.Controls.Add(this.zGroupBox2);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.zGroupBoxSurcharge);
			this.Controls.Add(this.zGroupBoxMaintenance);
			this.Name = "LicenceMaintenanceControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 480, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.renewalPeriodBox)).EndInit();
			this.zGroupBoxMaintenance.ResumeLayout(false);
			this.zGroupBoxMaintenance.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.zGroupBoxSurcharge.ResumeLayout(false);
			this.zGroupBoxSurcharge.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZCalcEdit lastMaintenanceBox;
		private ZArchitecture.ZCalcEdit nextNewSeatMaintenanceBox;
		private ZArchitecture.ZLabel zLabel3;
		private ZArchitecture.GUI.ZNumericUpDown renewalPeriodBox;
		private ZArchitecture.ZLabel zLabel4;
		private ZArchitecture.ZCalcEdit zCalcEdit2;
		private ZArchitecture.ZCalcEdit lastNewSeatMaintenanceBox;
		private ZArchitecture.ZCalcEdit nextMaintenanceBox;
		private ZArchitecture.ZCalcEdit lastAmountBox;
		private ZArchitecture.ZLabel zLabel7;
		private ZArchitecture.ZTextBox notesBox;
		private ZArchitecture.ZLabel zLabel8;
		private ZArchitecture.ZLabel zLabel9;
		private ZArchitecture.ZTextBox clientReferenceBox;
		private ZArchitecture.GUI.ZCodeFindBox zCodeFindBox1;
		private ZArchitecture.ZLabel zLabelSurcharge;
		private ZArchitecture.ZCalcEdit zCalcEditSurcharge;
		private ZArchitecture.ZTextBox zTextBoxSurchargeDescription;
		private ZArchitecture.ZLabel zLabelCurrency;
		private ZArchitecture.ZLabel zLabelSurchargeDescription;
		private ZArchitecture.GUI.ZGroupBox zGroupBoxMaintenance;
		private ZArchitecture.GUI.ZGroupBox zGroupBoxSurcharge;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.ZLabel zLabel6;
		private ZArchitecture.ZLabel zLabel10;
		private ZArchitecture.ZLabel zLabel11;
		private ZArchitecture.ZLabel zLabel12;
		private ZArchitecture.ZLabel zLabel13;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.GUI.ZGroupBox zGroupBox2;
		private Enterprise.ZArchitecture.GUI.ZPictureBox pictureBox1;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.ZTextBox zTextBox1;
		private ZArchitecture.ZLabel zLabel5;
	}
}
