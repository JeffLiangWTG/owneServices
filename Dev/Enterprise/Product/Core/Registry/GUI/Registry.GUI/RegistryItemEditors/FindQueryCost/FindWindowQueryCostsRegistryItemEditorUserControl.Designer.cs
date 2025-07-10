namespace Enterprise.Registry.GUI
{
	partial class FindWindowQueryCostsRegistryItemEditorUserControl
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
			this.AllowedCost = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MaximalCost = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.FindWindowQueryCosts);
			// 
			// AllowedCost
			// 
			this.AllowedCost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AllowedCost, "AllowedCost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.FindWindowQueryCosts)(null)).AllowedCost)));
			this.AllowedCost.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("c1fec4c2-60b6-49a4-bbb1-5a209726e3f4", "Maximum query cost without warning");
			this.AllowedCost.DecimalPlaces = 0;
			this.AllowedCost.Decimals = 0;
			this.AllowedCost.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 19, true);
			this.AllowedCost.Name = "AllowedCost";
			this.AllowedCost.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 17, true);
			this.AllowedCost.TabIndex = 0;
			this.AllowedCost.Text = "0";
			this.AllowedCost.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MaximalCost
			// 
			this.MaximalCost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MaximalCost, "MaximalCost");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.FindWindowQueryCosts)(null)).MaximalCost)));
			this.MaximalCost.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("229d5d52-c687-4126-a7bd-ec3fb42bb158", "Maximum query cost with warning");
			this.MaximalCost.DecimalPlaces = 0;
			this.MaximalCost.Decimals = 0;
			this.MaximalCost.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 45, true);
			this.MaximalCost.Name = "MaximalCost";
			this.MaximalCost.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 17, true);
			this.MaximalCost.TabIndex = 1;
			this.MaximalCost.Text = "0";
			this.MaximalCost.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FindWindowQueryCostsRegistryItemEditorUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.AllowedCost);
			this.Controls.Add(this.MaximalCost);
			this.Name = "FindWindowQueryCostsRegistryItemEditorUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 80, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZCalcEdit AllowedCost;
		private ZArchitecture.ZCalcEdit MaximalCost;
	}
}
