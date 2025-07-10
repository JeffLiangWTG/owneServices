namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5ContainersAndSealsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.SealTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ContainerTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ContainersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AdditionalSealsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.AdditionalSealsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdditionalSealsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ContainersAndSealsSpliiter = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SealTabControl.SuspendLayout();
			this.ContainerTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).BeginInit();
			this.ContainersGrid.SuspendLayout();
			this.AdditionalSealsTabControl.SuspendLayout();
			this.AdditionalSealsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalSealsGrid)).BeginInit();
			this.AdditionalSealsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersAndSealsSpliiter)).BeginInit();
			this.ContainersAndSealsSpliiter.Panel1.SuspendLayout();
			this.ContainersAndSealsSpliiter.Panel2.SuspendLayout();
			this.ContainersAndSealsSpliiter.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// SealTabControl
			// 
			this.SealTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.SealTabControl.Controls.Add(this.ContainerTabPage);
			this.SealTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SealTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SealTabControl.Name = "SealTabControl";
			this.SealTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 100, true);
			this.SealTabControl.TabIndex = 2;
			// 
			// ContainerTabPage
			// 
			this.ContainerTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("35A8FF22-7DD1-474B-8C78-F61CE5A00479", "Containers/Equipment");
			this.ContainerTabPage.Controls.Add(this.ContainersGrid);
			this.ContainerTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContainerTabPage.Name = "ContainerTabPage";
			this.ContainerTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ContainerTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 73, true);
			this.ContainerTabPage.TabIndex = 0;
			this.ContainerTabPage.UseVisualStyleBackColor = true;
			// 
			// ContainersGrid
			// 
			this.ContainersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContainersGrid, "DepartureHeaderContainers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).DepartureHeaderContainers)));
			this.ContainersGrid.CaptionVisible = false;
			this.ContainersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersGrid.GridId = "1eea3a76-d153-47d0-81d8-a88a6d6b1e48";
			this.ContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainersGrid.LayoutKey = "zGrid1";
			this.ContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ContainersGrid.Name = "ContainersGrid";
			this.ContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 67, true);
			this.ContainersGrid.TabIndex = 3;
			// 
			// AdditionalSealsTabControl
			// 
			this.AdditionalSealsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.AdditionalSealsTabControl.Controls.Add(this.AdditionalSealsTabPage);
			this.AdditionalSealsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalSealsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalSealsTabControl.Name = "AdditionalSealsTabControl";
			this.AdditionalSealsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 101, true);
			this.AdditionalSealsTabControl.TabIndex = 3;
			// 
			// AdditionalSealsTabPage
			// 
			this.AdditionalSealsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("F42DCB09-13E7-4464-BF20-2ECEA1EE39DB", "Additional Seals");
			this.AdditionalSealsTabPage.Controls.Add(this.AdditionalSealsGrid);
			this.AdditionalSealsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdditionalSealsTabPage.Name = "AdditionalSealsTabPage";
			this.AdditionalSealsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AdditionalSealsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 74, true);
			this.AdditionalSealsTabPage.TabIndex = 0;
			this.AdditionalSealsTabPage.UseVisualStyleBackColor = true;
			// 
			// AdditionalSealsGrid
			// 
			this.AdditionalSealsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalSealsGrid, "DepartureHeaderContainers.AdditionalSeals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureHeaderContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).DepartureHeaderContainers)).SyncRoot)).AdditionalSeals)));
			this.AdditionalSealsGrid.CaptionVisible = false;
			this.AdditionalSealsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalSealsGrid.GridId = "517c767e-9686-4d31-a688-f35063fc115e";
			this.AdditionalSealsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalSealsGrid.LayoutKey = "AdditionalSealsGrid";
			this.AdditionalSealsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AdditionalSealsGrid.Name = "AdditionalSealsGrid";
			this.AdditionalSealsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 68, true);
			this.AdditionalSealsGrid.TabIndex = 1;
			// 
			// ContainersAndSealsSpliiter
			// 
			this.ContainersAndSealsSpliiter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersAndSealsSpliiter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainersAndSealsSpliiter.Name = "ContainersAndSealsSpliiter";
			this.ContainersAndSealsSpliiter.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// ContainersAndSealsSpliiter.Panel1
			// 
			this.ContainersAndSealsSpliiter.Panel1.Controls.Add(this.SealTabControl);
			this.ContainersAndSealsSpliiter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 205, true);
			this.ContainersAndSealsSpliiter.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			// 
			// ContainersAndSealsSpliiter.Panel2
			// 
			this.ContainersAndSealsSpliiter.Panel2.Controls.Add(this.AdditionalSealsTabControl);
			this.ContainersAndSealsSpliiter.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			this.ContainersAndSealsSpliiter.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			this.ContainersAndSealsSpliiter.TabIndex = 4;
			// 
			// Phase5ContainersAndSealsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ContainersAndSealsSpliiter);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 205, true);
			this.Name = "Phase5ContainersAndSealsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 205, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SealTabControl.ResumeLayout(false);
			this.SealTabControl.PerformLayout();
			this.ContainerTabPage.ResumeLayout(false);
			this.ContainerTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).EndInit();
			this.ContainersGrid.ResumeLayout(false);
			this.ContainersGrid.PerformLayout();
			this.AdditionalSealsTabControl.ResumeLayout(false);
			this.AdditionalSealsTabControl.PerformLayout();
			this.AdditionalSealsTabPage.ResumeLayout(false);
			this.AdditionalSealsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalSealsGrid)).EndInit();
			this.AdditionalSealsGrid.ResumeLayout(false);
			this.AdditionalSealsGrid.PerformLayout();
			this.ContainersAndSealsSpliiter.Panel1.ResumeLayout(false);
			this.ContainersAndSealsSpliiter.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ContainersAndSealsSpliiter)).EndInit();
			this.ContainersAndSealsSpliiter.ResumeLayout(false);
			this.ContainersAndSealsSpliiter.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZTabControl SealTabControl;
		internal ZArchitecture.GUI.ZTabPage ContainerTabPage;
		internal ZArchitecture.ZGrid ContainersGrid;
		internal ZArchitecture.GUI.ZTabControl AdditionalSealsTabControl;
		internal ZArchitecture.GUI.ZTabPage AdditionalSealsTabPage;
		internal ZArchitecture.ZGrid AdditionalSealsGrid;
		internal CargoWise.Windows.UI.KSplitContainer ContainersAndSealsSpliiter;
	}
}
