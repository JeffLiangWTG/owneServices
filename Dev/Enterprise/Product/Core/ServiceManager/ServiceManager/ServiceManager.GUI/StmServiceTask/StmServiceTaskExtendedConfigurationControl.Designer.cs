namespace Enterprise.ServiceManager.GUI
{
	partial class StmServiceTaskExtendedConfigurationControl
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
			this.groupBoxSecondaryProcesses = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.dropEditSecondaryProcessesMaxCount = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.warningLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.warningLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.groupBoxSecondaryProcesses.SuspendLayout();
			this.dropEditSecondaryProcessesMaxCount.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ServiceManager.Business.StmServiceTask);
			// 
			// groupBoxSecondaryProcesses
			// 
			this.groupBoxSecondaryProcesses.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("9bf6055d-77bd-4da0-8c38-8e9ac950a298", "Secondary Processes");
			this.groupBoxSecondaryProcesses.Controls.Add(this.dropEditSecondaryProcessesMaxCount);
			this.groupBoxSecondaryProcesses.Controls.Add(this.warningLabel);
			this.groupBoxSecondaryProcesses.Controls.Add(this.warningLink);
			this.groupBoxSecondaryProcesses.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.groupBoxSecondaryProcesses.Name = "groupBoxSecondaryProcesses";
			this.groupBoxSecondaryProcesses.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(617, 200, true);
			this.groupBoxSecondaryProcesses.TabIndex = 0;
			this.groupBoxSecondaryProcesses.TabStop = false;
			// 
			// errorLinkLabel
			// 
			this.BindingSource.SetBindingMember(this.warningLink, "ExtendedConfigProcessesMaxCountWarningLink");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).ExtendedConfigProcessesMaxCountWarningLink)));
			this.warningLink.IsFontBold = false;
			this.warningLink.Name = "errorLinkLabel";
			this.warningLink.AutoSize = true;
			this.warningLink.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.warningLink.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(617, 200, true);
			this.warningLink.TabIndex = 100;
			this.warningLink.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.warningLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.errorLinkLabel_LinkClicked);
			// 
			// errorDisplayLabel
			// 
			this.BindingSource.SetBindingMember(this.warningLabel, "ExtendedConfigProcessesMaxCountWarningMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).ExtendedConfigProcessesMaxCountWarningMessage)));
			this.warningLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.warningLabel.Name = "errorDisplayLabel";
			this.warningLabel.AutoSize = true;
			this.warningLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.warningLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(617, 200, true);
			this.warningLabel.TabIndex = 100;
			this.warningLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// dropEditSecondaryProcessesMaxCount
			// 
			this.dropEditSecondaryProcessesMaxCount.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dropEditSecondaryProcessesMaxCount, "SecondaryProcessesMaxCountDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.StmServiceTask)(null)).SecondaryProcessesMaxCountDescription)));
			this.dropEditSecondaryProcessesMaxCount.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("edfd079a-75cf-4ebb-9ca0-32817ffd204c", "Max Count", "Maximum Count", "Maximum count of secondary processes running on same server.");
			this.dropEditSecondaryProcessesMaxCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 19, true);
			this.dropEditSecondaryProcessesMaxCount.Name = "dropEditSecondaryProcessesMaxCount";
			this.dropEditSecondaryProcessesMaxCount.ShowDescriptionBox = false;
			this.dropEditSecondaryProcessesMaxCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 15, true);
			this.dropEditSecondaryProcessesMaxCount.SetControlWidth(300);
			this.dropEditSecondaryProcessesMaxCount.TabIndex = 1;
			this.dropEditSecondaryProcessesMaxCount.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			// 
			// ExtendedConfigControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.groupBoxSecondaryProcesses);
			this.Name = "ExtendedConfigControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(630, 344, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.groupBoxSecondaryProcesses.ResumeLayout(false);
			this.groupBoxSecondaryProcesses.PerformLayout();
			this.dropEditSecondaryProcessesMaxCount.ResumeLayout(true);
			this.dropEditSecondaryProcessesMaxCount.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox groupBoxSecondaryProcesses;
		private ZArchitecture.GUI.ZDropEdit dropEditSecondaryProcessesMaxCount;
		private ZArchitecture.ZLabel warningLabel;
		private ZArchitecture.GUI.ZLinkLabel warningLink;
	}
}
