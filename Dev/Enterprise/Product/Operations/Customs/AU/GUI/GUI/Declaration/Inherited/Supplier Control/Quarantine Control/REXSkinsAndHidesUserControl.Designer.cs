namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class REXSkinsAndHidesUserControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(REXSkinsAndHidesUserControl));
			this.LoadingEstablishmentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AQISLoadingEstablishmentLocationDocAddress = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.QH_LoadingDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LoadingEstablishmentIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LoadingEstablishmentGroupBox.SuspendLayout();
			this.AQISLoadingEstablishmentLocationDocAddress.SuspendLayout();
			this.QH_LoadingDateDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader);
			// 
			// LoadingEstablishmentGroupBox
			// 
			this.LoadingEstablishmentGroupBox.Controls.Add(this.AQISLoadingEstablishmentLocationDocAddress);
			this.LoadingEstablishmentGroupBox.Controls.Add(this.LoadingEstablishmentIDTextBox);
			this.LoadingEstablishmentGroupBox.Controls.Add(this.QH_LoadingDateDateEdit);
			this.LoadingEstablishmentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 7, true);
			this.LoadingEstablishmentGroupBox.Name = "LoadingEstablishmentGroupBox";
			this.LoadingEstablishmentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 75, true);
			this.LoadingEstablishmentGroupBox.TabIndex = 0;
			this.LoadingEstablishmentGroupBox.TabStop = false;
			this.LoadingEstablishmentGroupBox.Text = "Loading Establishment";
			// 
			// AQISLoadingEstablishmentLocationDocAddress
			// 
			this.AQISLoadingEstablishmentLocationDocAddress.AddressValidationProcessCmdKey = null;
			this.AQISLoadingEstablishmentLocationDocAddress.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AQISLoadingEstablishmentLocationDocAddress, "AQISLoadingEstablishmentLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).AQISLoadingEstablishmentLocation)));
			this.AQISLoadingEstablishmentLocationDocAddress.BindToOrganisations = "Lookups.AQISLoadingEstablishmentLocationOrganisations";
			this.AQISLoadingEstablishmentLocationDocAddress.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("7e50383a-7834-4899-842b-9f3a74048b2b", "Location");
			this.AQISLoadingEstablishmentLocationDocAddress.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.AQISLoadingEstablishmentLocationDocAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 17, true);
			this.AQISLoadingEstablishmentLocationDocAddress.Name = "AQISLoadingEstablishmentLocationDocAddress";
			this.AQISLoadingEstablishmentLocationDocAddress.ReadOnly = false;
			this.AQISLoadingEstablishmentLocationDocAddress.SingleLineNoGroupBoxPanelWidth = 296;
			this.AQISLoadingEstablishmentLocationDocAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.AQISLoadingEstablishmentLocationDocAddress.TabIndex = 1;
			this.AQISLoadingEstablishmentLocationDocAddress.ValidationJustForced = false;
			// 
			// LoadingEstablishmentIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.LoadingEstablishmentIDTextBox, "AQISLoadingEstablishmentLocation.EXDOCEstablishmentNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).AQISLoadingEstablishmentLocation.EXDOCEstablishmentNumber)));
			this.LoadingEstablishmentIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 17, true);
			this.LoadingEstablishmentIDTextBox.Name = "LoadingEstablishmentIDTextBox";
			this.LoadingEstablishmentIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 17, true);
			this.LoadingEstablishmentIDTextBox.TabIndex = 2;
			// 
			// QH_LoadingDateDateEdit
			// 
			this.QH_LoadingDateDateEdit.AllowDrop = true;
			this.QH_LoadingDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.QH_LoadingDateDateEdit, "QuarantineExDocHeader+QH_LoadingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_LoadingDate)));
			this.QH_LoadingDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 41, true);
			this.QH_LoadingDateDateEdit.Name = "QH_LoadingDateDateEdit";
			this.QH_LoadingDateDateEdit.TabIndex = 3;
			// 
			// REXSkinsAndHidesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LoadingEstablishmentGroupBox);
			this.Name = "REXSkinsAndHidesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 277, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LoadingEstablishmentGroupBox.ResumeLayout(false);
			this.LoadingEstablishmentGroupBox.PerformLayout();
			this.AQISLoadingEstablishmentLocationDocAddress.ResumeLayout(true);
			this.AQISLoadingEstablishmentLocationDocAddress.PerformLayout();
			this.QH_LoadingDateDateEdit.ResumeLayout(true);
			this.QH_LoadingDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox LoadingEstablishmentGroupBox;
		private ZArchitecture.GUI.ZDateEdit QH_LoadingDateDateEdit;
		private ZArchitecture.ZTextBox LoadingEstablishmentIDTextBox;
		private MasterFiles.GUI.ZDocAddressControl AQISLoadingEstablishmentLocationDocAddress;
	}
}
