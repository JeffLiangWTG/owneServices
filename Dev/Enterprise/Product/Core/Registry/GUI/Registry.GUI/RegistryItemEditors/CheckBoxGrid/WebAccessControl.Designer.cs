namespace Enterprise.Registry.GUI
{
	partial class WebAccessControl
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
			this.CheckBoxGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CheckBoxGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.Web.OrgsRoleAccessCollection);
			// 
			// CheckBoxGrid
			// 
			this.CheckBoxGrid.AllowNavigation = false;
			this.CheckBoxGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CheckBoxGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.Web.OrgsRoleAccess)(null)))));
			this.CheckBoxGrid.CaptionVisible = false;
			this.CheckBoxGrid.GridId = "cf40bd7c-7529-4f6d-8670-14edbb8e27ab";
			this.CheckBoxGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CheckBoxGrid.LayoutKey = "CheckBoxGrid";
			this.CheckBoxGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CheckBoxGrid.Name = "CheckBoxGrid";
			this.CheckBoxGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 350, true);
			this.CheckBoxGrid.TabIndex = 1;
			// 
			// WebAccessControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CheckBoxGrid);
			this.Name = "WebAccessControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 350, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CheckBoxGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid CheckBoxGrid;

	}
}
