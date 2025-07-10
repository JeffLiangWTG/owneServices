namespace Enterprise.Customs.ES.NCTS.GUI;

partial class Phase5ArrivalMovementForm
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
		if (DataSource != null)
		{
			UnhookStatusChangeEvents();
		}

		if (disposing && (components != null))
		{
			TNNTabPage.Dispose();
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
		this.TNNTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
		this.MainTabControl.SuspendLayout();
		this.MainPanel.SuspendLayout();
		this.SaveButtonUserControl.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.NCTS.Business.NctsHeader);
		// 
		// MainTabControl
		// 
		this.MainTabControl.TabPages.Insert(this.TNNTabPage, 0);
		// 
		// TNNTabPage
		// 
		this.TNNTabPage.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("227C5FE9-1722-4020-8247-D3AD4DDCEA79", "TNN");
		this.TNNTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
		this.TNNTabPage.Name = "TNNTabPage";
		this.TNNTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
		this.TNNTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 456, true);
		this.TNNTabPage.TabIndex = 0;
		this.TNNTabPage.UseVisualStyleBackColor = true;
		this.TNNTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.TNNTabPage_InitializeTab));
		// 
		// Phase5DepartureMovementForm
		// 
		this.Name = "Phase5ArrivalMovementForm";
		this.MainTabControl.ResumeLayout(false);
		this.MainTabControl.PerformLayout();
		this.MainPanel.ResumeLayout(false);
		this.MainPanel.PerformLayout();
		this.SaveButtonUserControl.ResumeLayout(true);
		this.SaveButtonUserControl.PerformLayout();
		((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	void TNNTabPage_InitializeTab(object sender, System.EventArgs e)
	{
		// 
		// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
		// 
		this.TNNTabUserControl = new Enterprise.Customs.ES.NCTS.GUI.TNNUserControl();
		this.TNNTabPage.SuspendLayout();
		this.TNNTabUserControl.SuspendLayout();
		this.TNNTabPage.Controls.Add(this.TNNTabUserControl);
		// 
		// TNNTabUserControl
		// 
		this.TNNTabUserControl.AllowDrop = true;
		this.TNNTabUserControl.AutoSize = true;
		this.TNNTabUserControl.BackColor = System.Drawing.SystemColors.Control;
		this.BindingSource.SetBindingMember(this.TNNTabUserControl, "ArrivalMovementHeader.HeaderTNN");
		this.TNNTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.TNNTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
		this.TNNTabUserControl.Name = "TNNTabUserControl";
		this.TNNTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 450, true);
		this.TNNTabUserControl.TabIndex = 0;
		this.TNNTabUserControl.CaptionRenderingEnabled = true;
		this.TNNTabPage.PerformLayout();
		this.TNNTabUserControl.ResumeLayout(true);
		this.TNNTabUserControl.PerformLayout();
		this.TNNTabPage.ResumeLayout(true);

	}

	#endregion

	internal ZArchitecture.GUI.ZTabPage TNNTabPage;
	internal ES.NCTS.GUI.TNNUserControl TNNTabUserControl;
}
