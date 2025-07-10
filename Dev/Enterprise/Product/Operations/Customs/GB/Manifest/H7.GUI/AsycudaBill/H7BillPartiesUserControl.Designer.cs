using Enterprise.Customs.GB.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.H7.GUI.Bill
{
	public partial class H7BillPartiesUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		readonly System.ComponentModel.IContainer components = null;

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
			this.PostponedVatAccountingCheckBox = new ZCheckBox();
			this.VatNumberTextBox = new ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.VatNumberTextBox.SuspendLayout();
			this.PostponedVatAccountingCheckBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(AsycudaBill);
			// 
			// VatNumberTextBox
			// 
			this.VatNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.VatNumberTextBox, "VATNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.H7.Business.AsycudaBill)(null)).VATNumber)));
			this.VatNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(326, 22, true);
			this.VatNumberTextBox.Name = "VatNumberTextBox";
			this.VatNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.VatNumberTextBox.TabIndex = 6;
			this.VatNumberTextBox.CaptionResourceString = Res.GetData("f2f90db6-f6d4-4170-98d9-02f90816a25b", "VAT Number");
			// 
			// PostponedVatAccountingCheckBox
			// 
			this.PostponedVatAccountingCheckBox.Anchor = (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.PostponedVatAccountingCheckBox, "PostponedVatAccountingCheck");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.H7.Business.AsycudaBill)(null)).PostponedVatAccountingCheck)));
			this.PostponedVatAccountingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 18, true);
			this.PostponedVatAccountingCheckBox.Name = "PostponedVatAccountingCheckBox";
			this.PostponedVatAccountingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 24, true);
			this.PostponedVatAccountingCheckBox.TabIndex = 7;
			this.PostponedVatAccountingCheckBox.UseVisualStyleBackColor = true;
			this.PostponedVatAccountingCheckBox.CaptionResourceString = Res.GetData("1e5f7b18-a2dc-452b-aacf-6b8010a62beb", "Postponed VAT Accounting");
			// 
			// H7BillPartiesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.VatNumberTextBox);
			this.Controls.Add(this.PostponedVatAccountingCheckBox);
			this.Name = "H7BillPartiesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(717, 56, true);
			this.VatNumberTextBox.ResumeLayout(true);
			this.VatNumberTextBox.PerformLayout();
			this.PostponedVatAccountingCheckBox.ResumeLayout(true);
			this.PostponedVatAccountingCheckBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ZTextBox VatNumberTextBox;
		internal ZCheckBox PostponedVatAccountingCheckBox;

		#endregion
	}
}
