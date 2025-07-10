namespace Enterprise.Registry.GUI.eHub
{
	partial class BillingTransactionsTransformationSettingsControl
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
		void InitializeComponent()
		{
			this.currentFixIndexCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.currentFixStateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.eHub.BillingTransactionsTransformationSettings);
			// 
			// currentFixIndexCalcEdit
			// 
			this.currentFixIndexCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.currentFixIndexCalcEdit, "CurrentFixIndex");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType) (((Enterprise.Registry.Business.eHub.BillingTransactionsTransformationSettings) (null)).CurrentFixIndex)));
			this.currentFixIndexCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("8ebba551-d601-4ef2-8be7-518a611c8c88", "Current fix index");
			this.currentFixIndexCalcEdit.DecimalPlaces = 0;
			this.currentFixIndexCalcEdit.Decimals = 0;
			this.currentFixIndexCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 3, true);
			this.currentFixIndexCalcEdit.Name = "currentFixIndexCalcEdit";
			this.currentFixIndexCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.currentFixIndexCalcEdit.TabIndex = 0;
			this.currentFixIndexCalcEdit.Text = "0";
			this.currentFixIndexCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// currentFixStateCalcEdit
			// 
			this.currentFixStateCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.currentFixStateCalcEdit, "CurrentFixState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType) (((Enterprise.Registry.Business.eHub.BillingTransactionsTransformationSettings) (null)).CurrentFixState)));
			this.currentFixStateCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("39b6da5a-7f74-4660-9e00-cfe85741a135", "Current fix state");
			this.currentFixStateCalcEdit.DecimalPlaces = 0;
			this.currentFixStateCalcEdit.Decimals = 0;
			this.currentFixStateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 29, true);
			this.currentFixStateCalcEdit.Name = "currentFixStateCalcEdit";
			this.currentFixStateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.currentFixStateCalcEdit.TabIndex = 0;
			this.currentFixStateCalcEdit.Text = "0";
			this.currentFixStateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BillingTransactionsTransformationSettingsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.currentFixIndexCalcEdit);
			this.Controls.Add(this.currentFixStateCalcEdit);
			this.Name = "BillingTransactionsTransformationSettingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(516, 385, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZCalcEdit currentFixIndexCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit currentFixStateCalcEdit;
	}
}
