namespace Enterprise.Accounting.Registry.GUI
{
	partial class CASSFileImportDefaultTaxIDControl
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
			this.StandardRatedTaxIDGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ZeroRatedTaxIDGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.CASSFileImportDefaultTaxID);
			// 
			// StandardRatedTaxIDGuidFindBox
			// 
			this.StandardRatedTaxIDGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.StandardRatedTaxIDGuidFindBox, "StandardRatedTaxID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Registry.Business.CASSFileImportDefaultTaxID)(null)).StandardRatedTaxID)));
			this.StandardRatedTaxIDGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CASSFileImportDefaultTaxIDControl|8cac8e0b-b581-492f-8d36-51318a20142b", "Standard Rated Tax ID");
			this.StandardRatedTaxIDGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 3, true);
			this.StandardRatedTaxIDGuidFindBox.Name = "StandardRatedTaxIDGuidFindBox";
			this.StandardRatedTaxIDGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.StandardRatedTaxIDGuidFindBox.TabIndex = 0;
			// 
			// ZeroRatedTaxIDGuidFindBox
			// 
			this.ZeroRatedTaxIDGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ZeroRatedTaxIDGuidFindBox, "ZeroRatedTaxID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Registry.Business.CASSFileImportDefaultTaxID)(null)).ZeroRatedTaxID)));
			this.ZeroRatedTaxIDGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CASSFileImportDefaultTaxIDControl|e051cb2f-a82b-4f55-baa6-511145bced2c", "Zero Rated Tax ID");
			this.ZeroRatedTaxIDGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 29, true);
			this.ZeroRatedTaxIDGuidFindBox.Name = "ZeroRatedTaxIDGuidFindBox";
			this.ZeroRatedTaxIDGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.ZeroRatedTaxIDGuidFindBox.TabIndex = 1;
			// 
			// CASSFileImportDefaultTaxIDControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ZeroRatedTaxIDGuidFindBox);
			this.Controls.Add(this.StandardRatedTaxIDGuidFindBox);
			this.Name = "CASSFileImportDefaultTaxIDControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 52, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGuidFindBox StandardRatedTaxIDGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox ZeroRatedTaxIDGuidFindBox;
	}
}
