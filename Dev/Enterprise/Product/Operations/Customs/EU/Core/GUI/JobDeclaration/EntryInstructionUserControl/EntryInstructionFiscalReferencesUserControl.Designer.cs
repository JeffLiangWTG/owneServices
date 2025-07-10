using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.GUI
{
	public partial class EntryInstructionFiscalReferencesUserControl
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
			if (disposing)
			{
				components?.Dispose();
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
			this.FiscalReferencesUserControl = new Enterprise.Customs.EU.GUI.PlugIn.FiscalReferencesUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FiscalReferencesUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// FiscalReferencesUserControl
			// 
			this.FiscalReferencesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FiscalReferencesUserControl, "CustomsEntryInstructions.FiscalReferences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.Declaration.ICusFiscalReferenceCollection<CusFiscalReference>)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).FiscalReferences)));
			this.FiscalReferencesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FiscalReferencesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FiscalReferencesUserControl.Name = "FiscalReferencesUserControl";
			this.FiscalReferencesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(630, 158, true);
			this.FiscalReferencesUserControl.TabIndex = 1;
			// 
			// EntryInstructionFiscalReferencesUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FiscalReferencesUserControl);
			this.Name = "EntryInstructionFiscalReferencesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(630, 158, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FiscalReferencesUserControl.ResumeLayout(true);
			this.FiscalReferencesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.Customs.EU.GUI.PlugIn.FiscalReferencesUserControl FiscalReferencesUserControl;
	}
}
