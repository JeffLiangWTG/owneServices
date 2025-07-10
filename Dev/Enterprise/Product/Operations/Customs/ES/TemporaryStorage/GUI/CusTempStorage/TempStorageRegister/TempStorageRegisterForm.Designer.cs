namespace Enterprise.Customs.ES.TemporaryStorage.GUI;

partial class TempStorageRegisterForm
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

	#region Windows Form Designer generated code

	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	protected override void InitializeComponent()
	{
		this.MainTabControl.SuspendLayout();
		this.MainPanel.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.SuspendLayout();
		// 
		// MainTabControl
		// 
		this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1234, 784, true);
		// 
		// MainTabPage
		// 
		this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 757, true);
		this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
		// 
		// NotesTabPage
		// 
		this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 757, true);
		this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
		// 
		// LogsTabPage
		// 
		this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 757, true);
		// 
		// MainPanel
		// 
		this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1234, 784, true);
		// 
		// MainStatusBar
		// 
		this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1234, 24, true);
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader);
		// 
		// TempStorageRegisterForm
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1234, 840, true);
		this.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader);
		this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 725, true);
		this.Name = "TempStorageRegisterForm";
		this.ShouldSerializeTabPageMethods = true;
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

	private void MainTabPage_InitializeTab(object sender, System.EventArgs e)
	{
		// 
		// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
		// 
		this.TempStorageRegisterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
		this.TempStorageRegisterControl = new Enterprise.Customs.ES.TemporaryStorage.GUI.TempStorageRegisterUserControl();
		this.MainTabPage.SuspendLayout();
		this.TempStorageRegisterPanel.SuspendLayout();
		this.TempStorageRegisterControl.SuspendLayout();
		this.MainTabPage.Controls.Add(this.TempStorageRegisterPanel);
		// 
		// TempStorageRegisterPanel
		// 
		this.TempStorageRegisterPanel.Controls.Add(this.TempStorageRegisterControl);
		this.TempStorageRegisterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.TempStorageRegisterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.TempStorageRegisterPanel.Name = "TempStorageRegisterPanel";
		this.TempStorageRegisterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 757, true);
		this.TempStorageRegisterPanel.TabIndex = 1;
		// 
		// TempStorageRegisterControl
		// 
		this.TempStorageRegisterControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.TempStorageRegisterControl, ".");
		this.TempStorageRegisterControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.TempStorageRegisterControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.TempStorageRegisterControl.Name = "TempStorageRegisterControl";
		this.TempStorageRegisterControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 757, true);
		this.TempStorageRegisterControl.TabIndex = 0;
		this.MainTabPage.PerformLayout();
		this.TempStorageRegisterPanel.ResumeLayout(false);
		this.TempStorageRegisterPanel.PerformLayout();
		this.TempStorageRegisterControl.ResumeLayout(true);
		this.TempStorageRegisterControl.PerformLayout();
		this.MainTabPage.ResumeLayout(true);

	}

	private void NotesTabPage_InitializeTab(object sender, System.EventArgs e)
	{
		// 
		// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
		// 
		this.NotesTabPage.SuspendLayout();
		this.NotesTabPage.PerformLayout();
		this.NotesTabPage.ResumeLayout(true);

	}

	#endregion

	private ZArchitecture.GUI.ZPanel TempStorageRegisterPanel;
	private TempStorageRegisterUserControl TempStorageRegisterControl;
}
