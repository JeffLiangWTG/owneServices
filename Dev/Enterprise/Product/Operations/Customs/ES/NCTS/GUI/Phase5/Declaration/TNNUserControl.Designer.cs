using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	partial class TNNUserControl
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
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.DeclarationDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TransportAndPackagingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HouseConsignmentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AnnexTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.NCTS.Business.NctsHeader);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.DeclarationDetailsTabPage);
			this.MainTabControl.Controls.Add(this.TransportAndPackagingTabPage);
			this.MainTabControl.Controls.Add(this.HouseConsignmentsTabPage);
			this.MainTabControl.Controls.Add(this.AnnexTabPage);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1366, 789, true);
			this.MainTabControl.TabIndex = 2;
			// 
			// DeclarationDetailsTabPage
			// 
			this.DeclarationDetailsTabPage.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("FCFECE73-B2BB-490B-9146-284E28152317", "Details");
			this.DeclarationDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DeclarationDetailsTabPage.Name = "DeclarationDetailsTabPage";
			this.DeclarationDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DeclarationDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 492, true);
			this.DeclarationDetailsTabPage.TabIndex = 1;
			this.DeclarationDetailsTabPage.UseVisualStyleBackColor = true;
			this.DeclarationDetailsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.DeclarationDetailsTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)))));
			// 
			// TransportAndPackagingTabPage
			// 
			this.TransportAndPackagingTabPage.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("A2D6FEA4-196F-42E4-83C4-226486910892", "Transport && Containers");
			this.TransportAndPackagingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TransportAndPackagingTabPage.Name = "TransportAndPackagingTabPage";
			this.TransportAndPackagingTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TransportAndPackagingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 492, true);
			this.TransportAndPackagingTabPage.TabIndex = 2;
			this.TransportAndPackagingTabPage.UseVisualStyleBackColor = true;
			this.TransportAndPackagingTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.TransportAndPackagingTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)))));
			// 
			// HouseConsignmentsTabPage
			// 
			this.HouseConsignmentsTabPage.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("32B4F8D8-C7A9-4894-A033-AE06D9D7D525", "House Consignments");
			this.HouseConsignmentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HouseConsignmentsTabPage.Name = "HouseConsignmentsTabPage";
			this.HouseConsignmentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.HouseConsignmentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 492, true);
			this.HouseConsignmentsTabPage.TabIndex = 3;
			this.HouseConsignmentsTabPage.UseVisualStyleBackColor = true;
			this.HouseConsignmentsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.HouseConsignmentsTabPage_InitializeTab));
			// 
			// AnnexTabPage
			// 
			this.AnnexTabPage.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("41F9C982-EAA1-435E-B403-6C7F2F8EBAB9", "Annexes");
			this.AnnexTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AnnexTabPage.Name = "AnnexTabPage";
			this.AnnexTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AnnexTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1358, 762, true);
			this.AnnexTabPage.TabIndex = 4;
			this.AnnexTabPage.UseVisualStyleBackColor = true;
			this.AnnexTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.AnnexTabPage_InitializeTab));
			// 
			// TNNUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainTabControl);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1366, 725, true);
			this.Name = "TNNUserControl";
			this.ShouldSerializeTabPageMethods = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1366, 789, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		void DeclarationDetailsTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.DeclarationDetailsTabUserControl = new Phase5DeclarationDetailsTabUserControl();
			this.DeclarationDetailsTabPage.SuspendLayout();
			this.DeclarationDetailsTabUserControl.SuspendLayout();
			this.DeclarationDetailsTabPage.Controls.Add(this.DeclarationDetailsTabUserControl);
			// 
			// DeclarationDetailsTabUserControl
			// 
			this.DeclarationDetailsTabUserControl.AllowDrop = true;
			this.DeclarationDetailsTabUserControl.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DeclarationDetailsTabUserControl, ".");
			this.DeclarationDetailsTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeclarationDetailsTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DeclarationDetailsTabUserControl.Name = "DeclarationDetailsTabUserControl";
			this.DeclarationDetailsTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 486, true);
			this.DeclarationDetailsTabUserControl.TabIndex = 0;
			this.DeclarationDetailsTabPage.PerformLayout();
			this.DeclarationDetailsTabUserControl.ResumeLayout(true);
			this.DeclarationDetailsTabUserControl.PerformLayout();
			this.DeclarationDetailsTabPage.ResumeLayout(true);

		}

		void TransportAndPackagingTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.TransportAndPackagingTabUserControl = new Enterprise.Customs.EU.NCTS.GUI.Phase5TransportAndPackagingTabUserControl();
			this.TransportAndPackagingTabPage.SuspendLayout();
			this.TransportAndPackagingTabUserControl.SuspendLayout();
			this.TransportAndPackagingTabPage.Controls.Add(this.TransportAndPackagingTabUserControl);
			// 
			// TransportAndPackagingTabUserControl
			// 
			this.TransportAndPackagingTabUserControl.AllowDrop = true;
			this.TransportAndPackagingTabUserControl.AutoSize = true;
			this.TransportAndPackagingTabUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.TransportAndPackagingTabUserControl, ".");
			this.TransportAndPackagingTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportAndPackagingTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TransportAndPackagingTabUserControl.Name = "TransportAndPackagingTabUserControl";
			this.TransportAndPackagingTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 486, true);
			this.TransportAndPackagingTabUserControl.TabIndex = 0;
			this.TransportAndPackagingTabPage.PerformLayout();
			this.TransportAndPackagingTabUserControl.ResumeLayout(true);
			this.TransportAndPackagingTabUserControl.PerformLayout();
			this.TransportAndPackagingTabPage.ResumeLayout(true);

		}

		void HouseConsignmentsTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.HouseConsignmentsTabUserControl = new HouseConsignmentsTabUserControl();
			this.HouseConsignmentsTabPage.SuspendLayout();
			this.HouseConsignmentsTabUserControl.SuspendLayout();
			this.HouseConsignmentsTabPage.Controls.Add(this.HouseConsignmentsTabUserControl);
			// 
			// HouseConsignmentsTabUserControl
			// 
			this.HouseConsignmentsTabUserControl.AllowDrop = true;
			this.HouseConsignmentsTabUserControl.AutoSize = true;
			this.HouseConsignmentsTabUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.HouseConsignmentsTabUserControl, "Bills");
			this.HouseConsignmentsTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseConsignmentsTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HouseConsignmentsTabUserControl.Name = "HouseConsignmentsTabUserControl";
			this.HouseConsignmentsTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 486, true);
			this.HouseConsignmentsTabUserControl.TabIndex = 0;
			this.HouseConsignmentsTabPage.PerformLayout();
			this.HouseConsignmentsTabUserControl.ResumeLayout(true);
			this.HouseConsignmentsTabUserControl.PerformLayout();
			this.HouseConsignmentsTabPage.ResumeLayout(true);

		}

		void AnnexTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.AnnexesTabUserControl = new Enterprise.Customs.ES.GUI.AnnexesTabUserControl();
			this.AnnexTabPage.SuspendLayout();
			this.AnnexesTabUserControl.SuspendLayout();
			this.AnnexTabPage.Controls.Add(this.AnnexesTabUserControl);
			// 
			// AnnexesTabUserControl
			// 
			this.AnnexesTabUserControl.AllowDrop = true;
			this.AnnexesTabUserControl.AutoSize = true;
			this.AnnexesTabUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.AnnexesTabUserControl, "EDocPivotCollection");
			this.AnnexesTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AnnexesTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AnnexesTabUserControl.Name = "AnnexesTabUserControl";
			this.AnnexesTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1352, 756, true);
			this.AnnexesTabUserControl.TabIndex = 0;
			this.AnnexTabPage.PerformLayout();
			this.AnnexesTabUserControl.ResumeLayout(true);
			this.AnnexesTabUserControl.PerformLayout();
			this.AnnexTabPage.ResumeLayout(true);

		}

		#endregion

		ZTemplateTabControl MainTabControl;
		public Enterprise.ZArchitecture.GUI.ZTabPage DeclarationDetailsTabPage;
		public Enterprise.ZArchitecture.GUI.ZTabPage TransportAndPackagingTabPage;
		public Enterprise.ZArchitecture.GUI.ZTabPage HouseConsignmentsTabPage;
		internal ZArchitecture.GUI.ZTabPage AnnexTabPage;
		internal Phase5DeclarationDetailsTabUserControl DeclarationDetailsTabUserControl;
		internal Phase5TransportAndPackagingTabUserControl TransportAndPackagingTabUserControl;
		internal HouseConsignmentsTabUserControl HouseConsignmentsTabUserControl;
		internal ES.GUI.AnnexesTabUserControl AnnexesTabUserControl;
	}
}
