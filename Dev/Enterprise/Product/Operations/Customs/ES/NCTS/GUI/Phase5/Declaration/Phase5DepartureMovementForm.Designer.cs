namespace Enterprise.Customs.ES.NCTS.GUI
{
	partial class Phase5DepartureMovementForm
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
			if (nctsHeader != null)
			{
				UnhookStatusChangeEvents();
			}

			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.AnnexTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.NCTS.Business.NctsHeader);
			// 
			// AnnexTabPage
			// 
			this.AnnexTabPage.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("FF1F9731-AFAF-4E7E-A7AB-18632A615A3C", "Annexes");
			this.AnnexTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AnnexTabPage.Name = "AnnexTabPage";
			this.AnnexTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AnnexTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 456, true);
			this.AnnexTabPage.TabIndex = 0;
			this.AnnexTabPage.UseVisualStyleBackColor = true;
			this.AnnexTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.AnnexTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.ES.NCTS.Business.NctsCusStorageDocPivotCollection)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).EDocPivotCollection)));
			// 
			// Phase5DepartureMovementForm
			// 
			this.DataSourceType = typeof(Enterprise.Customs.ES.NCTS.Business.NctsHeader);
			this.Name = "Phase5DepartureMovementForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

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
			this.AnnexesTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 450, true);
			this.AnnexesTabUserControl.TabIndex = 0;
			this.AnnexesTabUserControl.CaptionRenderingEnabled = true;
			this.AnnexTabPage.PerformLayout();
			this.AnnexesTabUserControl.ResumeLayout(true);
			this.AnnexesTabUserControl.PerformLayout();
			this.AnnexTabPage.ResumeLayout(true);

		}

		#endregion

		internal ZArchitecture.GUI.ZTabPage AnnexTabPage;
		internal ES.GUI.AnnexesTabUserControl AnnexesTabUserControl;
	}
}
