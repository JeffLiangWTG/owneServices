namespace Enterprise.Customs.DE.GUI
{
	partial class REXDISREGDeclarationUserControl
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
			this.IdentificationDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.REGLineNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ATBNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.IdentificationDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec);
			// 
			// IdentificationDetailsGroupBox
			// 
			this.IdentificationDetailsGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("dacdaa07-d6f3-4f3f-ba59-9a2b2603158a", "SumA Identification");
			this.IdentificationDetailsGroupBox.Controls.Add(this.REGLineNumberCalcEdit);
			this.IdentificationDetailsGroupBox.Controls.Add(this.ATBNumberTextBox);
			this.IdentificationDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IdentificationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IdentificationDetailsGroupBox.Name = "IdentificationDetailsGroupBox";
			this.IdentificationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(359, 66, true);
			this.IdentificationDetailsGroupBox.TabIndex = 1;
			this.IdentificationDetailsGroupBox.TabStop = false;
			// 
			// REGLineNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.REGLineNumberCalcEdit, "CusTempStorageLines.SumALine.TSL_ReferenceNumberLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageReExportLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).SumALine.TSL_ReferenceNumberLine)));
			this.REGLineNumberCalcEdit.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("f52bb3da-dca3-4409-8809-8a7239903230", "Reference Line No.");
			this.REGLineNumberCalcEdit.DecimalPlaces = 0;
			this.REGLineNumberCalcEdit.Decimals = 0;
			this.REGLineNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 17, true);
			this.REGLineNumberCalcEdit.MaxValue = new decimal(new int[] {
            9999,
            0,
            0,
            0});
			this.REGLineNumberCalcEdit.Name = "REGLineNumberCalcEdit";
			this.REGLineNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.REGLineNumberCalcEdit.TabIndex = 1;
			this.REGLineNumberCalcEdit.Text = "0";
			this.REGLineNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ATBNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ATBNumberTextBox, "CusTempStorageLines.SumALine.ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageReExportLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.REXDISCusTempStorageDec)(null)).CusTempStorageLines)).SyncRoot)).SumALine.ReferenceNumber)));
			this.ATBNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 39, true);
			this.ATBNumberTextBox.Name = "ATBNumberTextBox";
			this.ATBNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ATBNumberTextBox.TabIndex = 2;
			// 
			// REXDISREGDeclarationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IdentificationDetailsGroupBox);
			this.Name = "REXDISREGDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(359, 66, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.IdentificationDetailsGroupBox.ResumeLayout(false);
			this.IdentificationDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox IdentificationDetailsGroupBox;
		private ZArchitecture.ZCalcEdit REGLineNumberCalcEdit;
		private ZArchitecture.ZTextBox ATBNumberTextBox;
	}
}
