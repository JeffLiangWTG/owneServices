
namespace Enterprise.Customs.IT.GUI
{
	partial class DeclarationInvoiceLineRemarksUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.RemarksGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RemarksTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RemarksGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobDeclaration);
			// 
			// RemarksGroupBox
			// 
			this.RemarksGroupBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("05bd647b-1e7f-4875-a700-69b1856bbe13", "[44] Remarks");
			this.RemarksGroupBox.Controls.Add(this.RemarksTextBox);
			this.RemarksGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RemarksGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RemarksGroupBox.Name = "RemarksGroupBox";
			this.RemarksGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 274, true);
			this.RemarksGroupBox.TabIndex = 0;
			this.RemarksGroupBox.TabStop = false;
			// 
			// RemarksTextBox
			// 
			this.RemarksTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RemarksTextBox, "FilteredInvoiceLines.Remarks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Remarks)));
			this.RemarksTextBox.CaptionResourceString = null;
			this.RemarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 16, true);
			this.RemarksTextBox.Multiline = true;
			this.RemarksTextBox.Name = "RemarksTextBox";
			this.RemarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 254, true);
			this.RemarksTextBox.TabIndex = 3;
			// 
			// DeclarationInvoiceLineRemarksUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RemarksGroupBox);
			this.Name = "DeclarationInvoiceLineRemarksUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 274, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RemarksGroupBox.ResumeLayout(false);
			this.RemarksGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		protected ZArchitecture.GUI.ZGroupBox RemarksGroupBox;
		protected ZArchitecture.ZTextBox RemarksTextBox;
	}
}
