namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	partial class BoardSectionDiagramConfigurationControl
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
			this.DiagramFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ConfigGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConfigGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.NetworkVisualisation.Business.DiagramBoardSectionConfiguration);
			// 
			// DiagramFindBox
			// 
			this.DiagramFindBox.AllowDrop = true;
			this.DiagramFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DiagramFindBox, "DiagramPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.NetworkVisualisation.Business.DiagramBoardSectionConfiguration)(null)).DiagramPK)));
			this.DiagramFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 19, true);
			this.DiagramFindBox.Name = "DiagramFindBox";
			this.DiagramFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(309, 21, true);
			this.DiagramFindBox.TabIndex = 0;
			// 
			// ConfigGroupBox
			// 
			this.ConfigGroupBox.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("54658526-3689-488a-8c4a-23e9a54f56c0", "Configuration");
			this.ConfigGroupBox.Controls.Add(this.DiagramFindBox);
			this.ConfigGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfigGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConfigGroupBox.Name = "ConfigGroupBox";
			this.ConfigGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 121, true);
			this.ConfigGroupBox.TabIndex = 1;
			this.ConfigGroupBox.TabStop = false;
			// 
			// BoardSectionDiagramConfigurationControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ConfigGroupBox);
			this.Name = "BoardSectionDiagramConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 121, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConfigGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGuidFindBox DiagramFindBox;
		private ZArchitecture.GUI.ZGroupBox ConfigGroupBox;
	}
}
