
namespace Enterprise.Customs.IE.GUI
{
	partial class MiscOptionsLayoutUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.PaymentSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PaymentSeparatorUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IE.Business.Declaration.JobDeclaration);
			// 
			// PaymentSeparatorUserControl
			// 
			this.PaymentSeparatorUserControl.AllowDrop = true;
			this.PaymentSeparatorUserControl.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("B79821E2-B3DC-49CA-A511-6CB0959322C0", "Payment");
			this.PaymentSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PaymentSeparatorUserControl.Name = "PaymentSeparatorUserControl";
			this.PaymentSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
			this.PaymentSeparatorUserControl.TabIndex = 23;
			// 
			// MiscOptionsLayoutUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PaymentSeparatorUserControl);
			this.Name = "MiscOptionsLayoutUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(775, 537, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PaymentSeparatorUserControl.ResumeLayout(true);
			this.PaymentSeparatorUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.SeparatorUserControl PaymentSeparatorUserControl;
	}
}
