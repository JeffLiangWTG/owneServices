namespace Enterprise.Accounting.Registry.GUI
{
	partial class MexicoNotificationRemainingFolioConfigurationControl
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
            this.FoliosQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.IntervalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.MexicoNotificationRemainingFolioConfiguration);
            // 
            // FoliosQuantityCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.FoliosQuantityCalcEdit, "FoliosQuantity");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Registry.Business.MexicoNotificationRemainingFolioConfiguration)(null)).FoliosQuantity)));
            this.FoliosQuantityCalcEdit.DecimalPlaces = 0;
            this.FoliosQuantityCalcEdit.Decimals = 0;
            this.FoliosQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 10, true);
            this.FoliosQuantityCalcEdit.Name = "FoliosQuantityCalcEdit";
            this.FoliosQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
            this.FoliosQuantityCalcEdit.TabIndex = 2;
            this.FoliosQuantityCalcEdit.Text = "0";
            this.FoliosQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.FoliosQuantityCalcEdit.TrackDisposedAccess = true;
            // 
            // IntervalCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.IntervalCalcEdit, "Interval");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Registry.Business.MexicoNotificationRemainingFolioConfiguration)(null)).Interval)));
            this.IntervalCalcEdit.DecimalPlaces = 0;
            this.IntervalCalcEdit.Decimals = 0;
            this.IntervalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 50, true);
            this.IntervalCalcEdit.Name = "IntervalCalcEdit";
            this.IntervalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
            this.IntervalCalcEdit.TabIndex = 3;
            this.IntervalCalcEdit.Text = "0";
            this.IntervalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.IntervalCalcEdit.TrackDisposedAccess = true;
            // 
            // MexicoNotificationRemainingFolioConfigurationControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.FoliosQuantityCalcEdit);
            this.Controls.Add(this.IntervalCalcEdit);
            this.Name = "MexicoNotificationRemainingFolioConfigurationControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 77, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		ZArchitecture.ZCalcEdit FoliosQuantityCalcEdit;
		ZArchitecture.ZCalcEdit IntervalCalcEdit;

		#endregion
	}
}
