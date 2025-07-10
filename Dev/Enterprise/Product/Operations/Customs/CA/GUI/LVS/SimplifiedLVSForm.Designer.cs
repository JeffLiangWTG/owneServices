namespace Enterprise.Customs.CA.GUI
{
	partial class SimplifiedLVSForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.simplifiedLVSUserControl = new Enterprise.Customs.CA.GUI.SimplifiedLVSUserControl();
			this.LVSSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.LastJobButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CalculateDutyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GaveUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ContinueAndSaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.simplifiedLVSUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LVSSplitContainer)).BeginInit();
			this.LVSSplitContainer.Panel1.SuspendLayout();
			this.LVSSplitContainer.Panel2.SuspendLayout();
			this.LVSSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 901, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1321, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.SimplifiedLVS);
			// 
			// simplifiedLVSUserControl
			// 
			this.simplifiedLVSUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.simplifiedLVSUserControl, ".");
			this.simplifiedLVSUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.simplifiedLVSUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.simplifiedLVSUserControl.Name = "simplifiedLVSUserControl";
			this.simplifiedLVSUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1321, 868, true);
			this.simplifiedLVSUserControl.TabIndex = 0;
			// 
			// LVSSplitContainer
			// 
			this.LVSSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LVSSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.LVSSplitContainer.IsSplitterFixed = true;
			this.LVSSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LVSSplitContainer.Name = "LVSSplitContainer";
			this.LVSSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// LVSSplitContainer.Panel1
			// 
			this.LVSSplitContainer.Panel1.Controls.Add(this.simplifiedLVSUserControl);
			// 
			// LVSSplitContainer.Panel2
			// 
			this.LVSSplitContainer.Panel2.Controls.Add(this.LastJobButton);
			this.LVSSplitContainer.Panel2.Controls.Add(this.CalculateDutyButton);
			this.LVSSplitContainer.Panel2.Controls.Add(this.GaveUpButton);
			this.LVSSplitContainer.Panel2.Controls.Add(this.ContinueAndSaveButton);
			this.LVSSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1321, 901, true);
			this.LVSSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(868);
			this.LVSSplitContainer.TabIndex = 1;
			// 
			// LastJobButton
			// 
			this.LastJobButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LastJobButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a09b82ac-1f44-41dc-87d7-ee2adaf05b69", "&Last Job");
			this.LastJobButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(994, 3, true);
			this.LastJobButton.Name = "LastJobButton";
			this.LastJobButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.LastJobButton.TabIndex = 0;
			this.LastJobButton.UseVisualStyleBackColor = true;
			this.LastJobButton.Click += new System.EventHandler(this.LastJobButton_Click);
			// 
			// CalculateDutyButton
			// 
			this.CalculateDutyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CalculateDutyButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e3b43e21-e396-41cc-bdd6-458f90602d2e", "Calculate &Duty");
			this.CalculateDutyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(994, 3, true);
			this.CalculateDutyButton.Name = "CalculateDutyButton";
			this.CalculateDutyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.CalculateDutyButton.TabIndex = 0;
			this.CalculateDutyButton.UseVisualStyleBackColor = true;
			this.CalculateDutyButton.Click += new System.EventHandler(this.CalculateDutyButton_Click);
			// 
			// GaveUpButton
			// 
			this.GaveUpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.GaveUpButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("eed68071-f918-404a-a36a-4f892a5c2fb7", "&Close");
			this.GaveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1214, 3, true);
			this.GaveUpButton.Name = "GaveUpButton";
			this.GaveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.GaveUpButton.TabIndex = 3;
			this.GaveUpButton.UseVisualStyleBackColor = true;
			this.GaveUpButton.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// ContinueAndSaveButton
			// 
			this.ContinueAndSaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ContinueAndSaveButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c0706f84-187f-4558-b05e-6861eed0e70b", "Co&ntinue");
			this.ContinueAndSaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1104, 3, true);
			this.ContinueAndSaveButton.Name = "ContinueAndSaveButton";
			this.ContinueAndSaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.ContinueAndSaveButton.TabIndex = 1;
			this.ContinueAndSaveButton.UseVisualStyleBackColor = true;
			this.ContinueAndSaveButton.Click += new System.EventHandler(this.ContinueAndSaveButton_Click);
			// 
			// SimplifiedLVSForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9ee61d1d-8f03-4e0a-8efa-3eeb4d66c354", "Simplified Low Value Shipment");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1321, 925, true);
			this.Controls.Add(this.LVSSplitContainer);
			this.DataSourceType = typeof(Enterprise.Customs.CA.Business.SimplifiedLVS);
			this.Name = "SimplifiedLVSForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.LVSSplitContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.simplifiedLVSUserControl.ResumeLayout(true);
			this.simplifiedLVSUserControl.PerformLayout();
			this.LVSSplitContainer.Panel1.ResumeLayout(false);
			this.LVSSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.LVSSplitContainer)).EndInit();
			this.LVSSplitContainer.ResumeLayout(false);
			this.LVSSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal SimplifiedLVSUserControl simplifiedLVSUserControl;
		private CargoWise.Windows.UI.KSplitContainer LVSSplitContainer;
		internal ZArchitecture.GUI.ZButton GaveUpButton;
		internal ZArchitecture.GUI.ZButton ContinueAndSaveButton;
		internal ZArchitecture.GUI.ZButton CalculateDutyButton;
		internal ZArchitecture.GUI.ZButton LastJobButton;
	}
}