namespace Enterprise.Customs.EU.ExitControl.GUI
{
	partial class ExitControlUserControl
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
			this.components = new System.ComponentModel.Container();
			this.ExitControlTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DetailsTabUserControl = new Enterprise.Customs.EU.ExitControl.GUI.DetailsTabUserControl();
			this.ContainersOrEquipmentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConsignmentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConsignmentsTabUserControl = new Enterprise.Customs.EU.ExitControl.GUI.ConsignmentsTabUserControl();
			this.ReportsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ReportsTabUserControl = new Enterprise.Customs.EU.ExitControl.GUI.ReportsTabUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExitControlTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.DetailsTabUserControl.SuspendLayout();
			this.ConsignmentsTabPage.SuspendLayout();
			this.ConsignmentsTabUserControl.SuspendLayout();
			this.ReportsTabPage.SuspendLayout();
			this.ReportsTabUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.ExitControl.Business.CusExitHeader);
			// 
			// ExitControlTabControl
			// 
			this.ExitControlTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ExitControlTabControl.Controls.Add(this.DetailsTabPage);
			this.ExitControlTabControl.Controls.Add(this.ContainersOrEquipmentsTabPage);
			this.ExitControlTabControl.Controls.Add(this.ConsignmentsTabPage);
			this.ExitControlTabControl.Controls.Add(this.ReportsTabPage);
			this.ExitControlTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExitControlTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExitControlTabControl.Name = "ExitControlTabControl";
			this.ExitControlTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 247, true);
			this.ExitControlTabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("ae2e0641-ea3f-4c54-87ad-b7181fd9f75f", "Details");
			this.DetailsTabPage.Controls.Add(this.DetailsTabUserControl);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(623, 222, true);
			this.DetailsTabPage.TabIndex = 0;
			// 
			// DetailsTabUserControl
			// 
			this.DetailsTabUserControl.AllowDrop = true;
			this.DetailsTabUserControl.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DetailsTabUserControl, ".");
			this.DetailsTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DetailsTabUserControl.Name = "DetailsTabUserControl";
			this.DetailsTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(617, 216, true);
			this.DetailsTabUserControl.TabIndex = 0;
			// 
			// ContainersOrEquipmentsTabPage
			// 
			this.ContainersOrEquipmentsTabPage.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("ab215df9-6099-4344-8b51-14fc76abfa9e", "Containers/Equipments");
			this.ContainersOrEquipmentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.ContainersOrEquipmentsTabPage.Name = "ContainersOrEquipmentsTabPage";
			this.ContainersOrEquipmentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ContainersOrEquipmentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(623, 222, true);
			this.ContainersOrEquipmentsTabPage.TabIndex = 3;
			this.ContainersOrEquipmentsTabPage.UseVisualStyleBackColor = true;
			// 
			// ConsignmentsTabPage
			// 
			this.ConsignmentsTabPage.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("9e014f31-2ec3-413c-8af1-e68ed84e032c", "Declarations/Entries");
			this.ConsignmentsTabPage.Controls.Add(this.ConsignmentsTabUserControl);
			this.ConsignmentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.ConsignmentsTabPage.Name = "ConsignmentsTabPage";
			this.ConsignmentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ConsignmentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(623, 222, true);
			this.ConsignmentsTabPage.TabIndex = 1;
			// 
			// ConsignmentsTabUserControl
			// 
			this.ConsignmentsTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignmentsTabUserControl, "CusExitConsignments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.ExitControlBase.Business.ICusExitConsignmentCollection<Enterprise.Customs.EU.ExitControl.Business.CusExitConsignment>)(((Enterprise.Customs.EU.ExitControl.Business.CusExitHeader)(null)).CusExitConsignments)));
			this.ConsignmentsTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsignmentsTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ConsignmentsTabUserControl.Name = "ConsignmentsTabUserControl";
			this.ConsignmentsTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(617, 216, true);
			this.ConsignmentsTabUserControl.TabIndex = 0;
			// 
			// ReportsTabPage
			// 
			this.ReportsTabPage.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("EB2546EB-2DDC-4B19-B18B-81121480EDAA", "Exit Reports");
			this.ReportsTabPage.Controls.Add(this.ReportsTabUserControl);
			this.ReportsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.ReportsTabPage.Name = "ReportsTabPage";
			this.ReportsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(623, 222, true);
			this.ReportsTabPage.TabIndex = 2;
			// 
			// ReportsTabUserControl
			// 
			this.ReportsTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReportsTabUserControl, "CusExitReports");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.ExitControlBase.Business.ICusExitReportCollection<Enterprise.Customs.EU.ExitControl.Business.CusExitReport>)(((Enterprise.Customs.EU.ExitControl.Business.CusExitHeader)(null)).CusExitReports)));
			this.ReportsTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportsTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportsTabUserControl.Name = "ReportsTabUserControl";
			this.ReportsTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(623, 222, true);
			this.ReportsTabUserControl.TabIndex = 0;
			// 
			// ExitControlUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ExitControlTabControl);
			this.Name = "ExitControlUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 247, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExitControlTabControl.ResumeLayout(false);
			this.ExitControlTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.DetailsTabUserControl.ResumeLayout(true);
			this.DetailsTabUserControl.PerformLayout();
			this.ConsignmentsTabPage.ResumeLayout(false);
			this.ConsignmentsTabPage.PerformLayout();
			this.ConsignmentsTabUserControl.ResumeLayout(true);
			this.ConsignmentsTabUserControl.PerformLayout();
			this.ReportsTabPage.ResumeLayout(false);
			this.ReportsTabPage.PerformLayout();
			this.ReportsTabUserControl.ResumeLayout(true);
			this.ReportsTabUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal DetailsTabUserControl DetailsTabUserControl;
		public ConsignmentsTabUserControl ConsignmentsTabUserControl;
		internal ReportsTabUserControl ReportsTabUserControl;
		public ZArchitecture.GUI.ZTabControl ExitControlTabControl;
		internal ZArchitecture.GUI.ZTabPage DetailsTabPage;
		public ZArchitecture.GUI.ZTabPage ConsignmentsTabPage;
		public ZArchitecture.GUI.ZTabPage ReportsTabPage;
		internal ZArchitecture.GUI.ZTabPage ContainersOrEquipmentsTabPage;
	}
}
