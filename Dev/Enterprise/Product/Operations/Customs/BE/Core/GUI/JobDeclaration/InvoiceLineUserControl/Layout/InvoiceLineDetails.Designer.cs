using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI
{
	partial class InvoiceLineDetails
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.UCRReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BE.Business.Declaration.JobComInvoiceLine);
			// 
			// UCRReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.UCRReferenceTextBox, "ZG_UCRReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.Declaration.JobComInvoiceLine)(null)).ZG_UCRReference)));
			this.UCRReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 86, true);
			this.UCRReferenceTextBox.Name = "UCRReferenceTextBox";
			this.UCRReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 15, true);
			this.UCRReferenceTextBox.TabIndex = 24;
			// 
			// InvoiceLineDetails
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UCRReferenceTextBox);
			this.Name = "InvoiceLineDetails";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 318, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox UCRReferenceTextBox;

	}
}
