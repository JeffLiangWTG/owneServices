namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class SecurityFilterControl
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
			this.SecurityFindBox = new Enterprise.MasterFiles.GUI.SecurityFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.RuntimeOptions.SecurityFilterField);
			// 
			// SecurityFindBox
			// 
			this.SecurityFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SecurityFindBox, "FilterContainer.LookupKey");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Modules.CheckpointLookupKey)(((Enterprise.DocumentEngine.RuntimeOptions.SecurityFilterField)(null)).FilterContainer.LookupKey)));
			this.SecurityFindBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("SecurityFilterControl|638a7c90-a493-455d-9ab0-3be265554439", "Security Right");
			this.SecurityFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 0, true);
			this.SecurityFindBox.Name = "SecurityFindBox";
			this.SecurityFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.SecurityFindBox.TabIndex = 1;
			// 
			// SecurityFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SecurityFindBox);
			this.Name = "SecurityFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 25, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		Enterprise.MasterFiles.GUI.SecurityFindBox SecurityFindBox;
    }
}
