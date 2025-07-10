namespace Enterprise.Customs.BR.GUI
{
	partial class DrawbackImportLicenseUserControl
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
			this.DrawbackGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CANumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ItemNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ModalityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DrawbackGroupBox.SuspendLayout();
			this.ModalityDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobComInvoiceLine);
			// 
			// DrawbackGroupBox
			// 
			this.DrawbackGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("db4bc040-3113-45d9-83ce-47cb3f2927fe", "Drawback");
			this.DrawbackGroupBox.Controls.Add(this.CANumberTextBox);
			this.DrawbackGroupBox.Controls.Add(this.ItemNumberCalcEdit);
			this.DrawbackGroupBox.Controls.Add(this.ModalityDropEdit);
			this.DrawbackGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.DrawbackGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DrawbackGroupBox.Name = "DrawbackGroupBox";
			this.DrawbackGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 100, true);
			this.DrawbackGroupBox.TabIndex = 0;
			this.DrawbackGroupBox.TabStop = false;
			// 
			// CANumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CANumberTextBox, "DrawbackCANumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).DrawbackCANumber)));
			this.CANumberTextBox.CaptionResourceString = null;
			this.CANumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 45, true);
			this.CANumberTextBox.Name = "CANumberTextBox";
			this.CANumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.CANumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 20, true);
			this.CANumberTextBox.TabIndex = 2;
			// 
			// ItemNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ItemNumberCalcEdit, "DrawbackItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).DrawbackItemNumber)));
			this.ItemNumberCalcEdit.CaptionResourceString = null;
			this.ItemNumberCalcEdit.DecimalPlaces = 0;
			this.ItemNumberCalcEdit.Decimals = 0;
			this.ItemNumberCalcEdit.MaxLength = 3;
			this.ItemNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 71, true);
			this.ItemNumberCalcEdit.MaxValue = new decimal(new int[] {
            9999,
            0,
            0,
            0});
			this.ItemNumberCalcEdit.Name = "ItemNumberCalcEdit";
			this.ItemNumberCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.ItemNumberCalcEdit.ShowEmptyStringForEmptyValue = true;
			this.ItemNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 20, true);
			this.ItemNumberCalcEdit.TabIndex = 3;
			this.ItemNumberCalcEdit.Text = "0";
			this.ItemNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ModalityDropEdit
			// 
			this.ModalityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ModalityDropEdit, "DrawbackModality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).DrawbackModality)));
			this.ModalityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 20, true);
			this.ModalityDropEdit.Name = "ModalityDropEdit";
			this.ModalityDropEdit.ShouldResizeByMaxLength = true;
			this.ModalityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 20, true);
			this.ModalityDropEdit.TabIndex = 0;
			// 
			// DrawbackImportLicenseUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DrawbackGroupBox);
			this.Name = "DrawbackImportLicenseUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 109, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DrawbackGroupBox.ResumeLayout(false);
			this.DrawbackGroupBox.PerformLayout();
			this.ModalityDropEdit.ResumeLayout(true);
			this.ModalityDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox DrawbackGroupBox;
		private ZArchitecture.GUI.ZDropEdit ModalityDropEdit;
		private ZArchitecture.ZCalcEdit ItemNumberCalcEdit;
		private ZArchitecture.ZTextBox CANumberTextBox;
	}
}
