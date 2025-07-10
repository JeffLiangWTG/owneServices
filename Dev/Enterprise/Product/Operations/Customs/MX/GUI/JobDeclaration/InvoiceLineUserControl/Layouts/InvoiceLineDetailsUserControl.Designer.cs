namespace Enterprise.Customs.MX.GUI
{
	partial class InvoiceLineDetailsUserControl
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
			this.EntryInstructionGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.ObservationsTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.VehicleDetailsUserControl = new Enterprise.Customs.MX.GUI.VehicleDetailsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EntryInstructionGuidDropEdit.SuspendLayout();
			this.ObservationsTextBox.SuspendLayout();
			this.VehicleDetailsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.MX.Business.JobComInvoiceLine);
			// 
			// EntryInstructionGuidDropEdit
			// 
			this.EntryInstructionGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryInstructionGuidDropEdit, "JI_CEI");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.MX.Business.JobComInvoiceLine)(null)).JI_CEI)));
			this.EntryInstructionGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 463, true);
			this.EntryInstructionGuidDropEdit.Name = "EntryInstructionGuidDropEdit";
			this.EntryInstructionGuidDropEdit.ShowDescriptionBox = false;
			this.EntryInstructionGuidDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.EntryInstructionGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 20, true);
			this.EntryInstructionGuidDropEdit.TabIndex = 1;
			this.EntryInstructionGuidDropEdit.UseFullWidthForCodeBox = true;
			// 
			// ObservationsTextBox
			//
			this.ObservationsTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ObservationsTextBox, "Observations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.MX.Business.JobComInvoiceLine)(null)).Observations)));
			this.ObservationsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.ObservationsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(728, 235, true);
			this.ObservationsTextBox.Name = "ObservationsTextBox";
			this.ObservationsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ObservationsTextBox.TabIndex = 2;
			// 
			// VehicleDetailsUserControl
			// 
			this.VehicleDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VehicleDetailsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.VehicleDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.VehicleDetailsUserControl.Name = "VehicleDetailsUserControl";
			this.VehicleDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 95, true);
			this.VehicleDetailsUserControl.TabIndex = 3;
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EntryInstructionGuidDropEdit);
			this.Controls.Add(this.ObservationsTextBox);
			this.Controls.Add(this.VehicleDetailsUserControl);
			this.Name = "InvoiceLineDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1165, 525, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ObservationsTextBox.ResumeLayout(true);
			this.ObservationsTextBox.PerformLayout();
			this.EntryInstructionGuidDropEdit.ResumeLayout(true);
			this.EntryInstructionGuidDropEdit.PerformLayout();
			this.VehicleDetailsUserControl.ResumeLayout(true);
			this.VehicleDetailsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal Customs.GUI.LongTextControl ObservationsTextBox;
		internal ZArchitecture.GUI.ZGuidDropEdit EntryInstructionGuidDropEdit;
		public VehicleDetailsUserControl VehicleDetailsUserControl;
	}
}
