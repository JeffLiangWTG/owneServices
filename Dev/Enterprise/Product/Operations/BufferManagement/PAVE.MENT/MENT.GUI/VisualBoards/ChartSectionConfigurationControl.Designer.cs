namespace Enterprise.PAVE.MENT.GUI
{
	partial class ChartSectionConfigurationControl
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
			this.graphVisualisationConfigurationControl1 = new Enterprise.PAVE.MENT.GUI.GraphVisualisationConfigurationControl();
			this.extractionZGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.overrideVisualisationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.graphVisualisationConfigurationControl1.SuspendLayout();
			this.extractionZGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.PAVE.MENT.Business.ChartSectionConfiguration);
			// 
			// graphVisualisationConfigurationControl1
			// 
			this.graphVisualisationConfigurationControl1.AllowDrop = true;
			this.graphVisualisationConfigurationControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.graphVisualisationConfigurationControl1, "RelatedVisualisation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(((Enterprise.PAVE.MENT.Business.ChartSectionConfiguration)(null)).RelatedVisualisation)));
			this.graphVisualisationConfigurationControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 73, true);
			this.graphVisualisationConfigurationControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 363, true);
			this.graphVisualisationConfigurationControl1.Name = "graphVisualisationConfigurationControl1";
			this.graphVisualisationConfigurationControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 485, true);
			this.graphVisualisationConfigurationControl1.TabIndex = 0;
			// 
			// extractionZGuidFindBox
			// 
			this.extractionZGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.extractionZGuidFindBox, "ExtractionPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.PAVE.MENT.Business.ChartSectionConfiguration)(null)).ExtractionPK)));
			this.extractionZGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 22, true);
			this.extractionZGuidFindBox.Name = "extractionZGuidFindBox";
			this.extractionZGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 17, true);
			this.extractionZGuidFindBox.TabIndex = 1;
			// 
			// overrideVisualisationCheckBox
			// 
			this.overrideVisualisationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.overrideVisualisationCheckBox, "OverrideDefaultVisualisation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.PAVE.MENT.Business.ChartSectionConfiguration)(null)).OverrideDefaultVisualisation)));
			this.overrideVisualisationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.overrideVisualisationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.overrideVisualisationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 48, true);
			this.overrideVisualisationCheckBox.Name = "overrideVisualisationCheckBox";
			this.overrideVisualisationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.overrideVisualisationCheckBox.TabIndex = 2;
			this.overrideVisualisationCheckBox.UseVisualStyleBackColor = true;
			this.overrideVisualisationCheckBox.CheckedChanged += new System.EventHandler(this.OverrideVisualisationCheckBox_CheckedChanged);
			// 
			// ChartSectionConfigurationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.overrideVisualisationCheckBox);
			this.Controls.Add(this.extractionZGuidFindBox);
			this.Controls.Add(this.graphVisualisationConfigurationControl1);
			this.Name = "ChartSectionConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(591, 567, true);
			this.Load += new System.EventHandler(this.ChartSectionConfigurationControl_Load);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.graphVisualisationConfigurationControl1.ResumeLayout(true);
			this.graphVisualisationConfigurationControl1.PerformLayout();
			this.extractionZGuidFindBox.ResumeLayout(true);
			this.extractionZGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private GraphVisualisationConfigurationControl graphVisualisationConfigurationControl1;
		private ZArchitecture.GUI.ZGuidFindBox extractionZGuidFindBox;
		private ZArchitecture.GUI.ZCheckBox overrideVisualisationCheckBox;
	}
}
