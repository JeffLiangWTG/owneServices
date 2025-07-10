using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

partial class EntryInstructionDetailsUserControl
{
	#region Component Designer generated code

	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
		this.EntryInstructionTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
		this.EntryInstructionDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
		this.EntryInstructionOtherPartiesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
		this.EntryInstructionSWControlsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
		this.SupportingDocumentTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
		this.EntryInstructionContainerTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
		this.DetailsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
		this.OtherPartiesUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
		this.SWControlsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
		this.ContainerUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
		this.EntryInstructionTopPanelUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
		this.SupportingDocumentUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
		this.SplitContainer.Panel1.SuspendLayout();
		this.SplitContainer.Panel2.SuspendLayout();
		this.SplitContainer.SuspendLayout();
		this.EntryInstructionTabControl.SuspendLayout();
		this.EntryInstructionDetailsTabPage.SuspendLayout();
		this.EntryInstructionOtherPartiesTabPage.SuspendLayout();
		this.EntryInstructionSWControlsTabPage.SuspendLayout();
		this.SupportingDocumentTabPage.SuspendLayout();
		this.EntryInstructionContainerTabPage.SuspendLayout();
		this.EntryInstructionTopPanelUserControl.SuspendLayout();
		this.SuspendLayout();
		//
		// BindingSource
		//
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.JobDeclaration);
		//
		// SplitContainer
		//
		this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
		this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.SplitContainer.Name = "SplitContainer";
		this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
		//
		// SplitContainer.Panel1
		//
		this.SplitContainer.Panel1.Controls.Add(this.EntryInstructionTopPanelUserControl);
		this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 385, true);
		this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
		//
		// SplitContainer.Panel2
		//
		this.SplitContainer.Panel2.AutoScroll = true;
		this.SplitContainer.Panel2.Controls.Add(this.EntryInstructionTabControl);
		this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
		this.SplitContainer.TabIndex = 3;
		// 
		// EntryInstructionTopPanelUserControl
		// 
		this.EntryInstructionTopPanelUserControl.AllowDrop = true;
		this.EntryInstructionTopPanelUserControl.AutoSize = true;
		this.EntryInstructionTopPanelUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.BindingSource.SetBindingMember(this.EntryInstructionTopPanelUserControl, ".");
		this.EntryInstructionTopPanelUserControl.Name = "EntryInstructionTopPanelUserControl";
		this.EntryInstructionTopPanelUserControl.TabIndex = 0;
		//
		// EntryInstructionTabControl
		//
		this.EntryInstructionTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
		this.EntryInstructionTabControl.Controls.Add(this.EntryInstructionDetailsTabPage);
		this.EntryInstructionTabControl.Controls.Add(this.EntryInstructionOtherPartiesTabPage);
		this.EntryInstructionTabControl.Controls.Add(this.SupportingDocumentTabPage);
		this.EntryInstructionTabControl.Controls.Add(this.EntryInstructionContainerTabPage);
		this.EntryInstructionTabControl.Controls.Add(this.EntryInstructionSWControlsTabPage);
		this.EntryInstructionTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.EntryInstructionTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.EntryInstructionTabControl.Name = "EntryInstructionTabControl";
		this.EntryInstructionTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 301, true);
		this.EntryInstructionTabControl.TabIndex = 0;
		//
		// EntryInstructionDetailsTabPage
		//
		this.EntryInstructionDetailsTabPage.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("C071591F-2071-40BA-8087-C57A1EE82BDF", "Details");
		this.EntryInstructionDetailsTabPage.Controls.Add(this.DetailsUserControl);
		this.EntryInstructionDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
		this.EntryInstructionDetailsTabPage.Name = "EntryInstructionDetailsTabPage";
		this.EntryInstructionDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
		this.EntryInstructionDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1272, 274, true);
		this.EntryInstructionDetailsTabPage.TabIndex = 0;
		this.EntryInstructionDetailsTabPage.UseVisualStyleBackColor = true;
		//
		// EntryInstructionOtherPartiesTabPage
		//
		this.EntryInstructionOtherPartiesTabPage.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("C071591F-2071-40BA-8087-C57A1EE82BDF", "Other Parties");
		this.EntryInstructionOtherPartiesTabPage.Controls.Add(this.OtherPartiesUserControl);
		this.EntryInstructionOtherPartiesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
		this.EntryInstructionOtherPartiesTabPage.Name = "EntryInstructionOtherPartiesTabPage";
		this.EntryInstructionOtherPartiesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
		this.EntryInstructionOtherPartiesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1272, 274, true);
		this.EntryInstructionOtherPartiesTabPage.TabIndex = 0;
		this.EntryInstructionOtherPartiesTabPage.UseVisualStyleBackColor = true;
		//
		// SupportingDocumentTabPage
		//
		this.SupportingDocumentTabPage.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("0c04ba9e-75c4-4a09-83c8-d9f6ca78957d", "Supporting Documents");
		this.SupportingDocumentTabPage.Controls.Add(this.SupportingDocumentUserControl);
		this.SupportingDocumentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
		this.SupportingDocumentTabPage.Name = "SupportingDocumentTabPage";
		this.SupportingDocumentTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
		this.SupportingDocumentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1272, 274, true);
		this.SupportingDocumentTabPage.TabIndex = 0;
		this.SupportingDocumentTabPage.UseVisualStyleBackColor = true;
		//
		// EntryInstructionContainerTabPage
		//
		this.EntryInstructionContainerTabPage.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("DD6FBD1E-2902-45D9-A462-D08BF4A04078", "Containers");
		this.EntryInstructionContainerTabPage.Controls.Add(this.ContainerUserControl);
		this.EntryInstructionContainerTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
		this.EntryInstructionContainerTabPage.Name = "EntryInstructionContainerTabPage";
		this.EntryInstructionContainerTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
		this.EntryInstructionContainerTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1272, 274, true);
		this.EntryInstructionContainerTabPage.TabIndex = 0;
		this.EntryInstructionContainerTabPage.UseVisualStyleBackColor = true;
		//
		// EntryInstructionSWControlsTabPage
		//
		this.EntryInstructionSWControlsTabPage.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("3A480F38-B7AB-4ECA-A446-5D4AB2FF7A06", "SW Controls");
		this.EntryInstructionSWControlsTabPage.Controls.Add(this.SWControlsUserControl);
		this.EntryInstructionSWControlsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
		this.EntryInstructionSWControlsTabPage.Name = "EntryInstructionSWControlsTabPage";
		this.EntryInstructionSWControlsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
		this.EntryInstructionSWControlsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1272, 274, true);
		this.EntryInstructionSWControlsTabPage.TabIndex = 0;
		this.EntryInstructionSWControlsTabPage.UseVisualStyleBackColor = true;
		//
		// DetailsUserControl
		//
		this.DetailsUserControl.AllowDrop = true;
		this.DetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.DetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
		this.DetailsUserControl.Name = "DetailsUserControl";
		this.DetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1266, 268, true);
		this.DetailsUserControl.TabIndex = 0;
		//
		// OtherPartiesUserControl
		//
		this.BindingSource.SetBindingMember(this.OtherPartiesUserControl, "CustomsEntryInstructions");
		this.OtherPartiesUserControl.AllowDrop = true;
		this.OtherPartiesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.OtherPartiesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
		this.OtherPartiesUserControl.Name = "OtherPartiesUserControl";
		this.OtherPartiesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1266, 268, true);
		this.OtherPartiesUserControl.TabIndex = 0;
		//
		// SupportingDocumentUserControl
		//
		this.SupportingDocumentUserControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.SupportingDocumentUserControl, "CustomsEntryInstructions.SupportingDocuments");
		this.SupportingDocumentUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.SupportingDocumentUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
		this.SupportingDocumentUserControl.Name = "SupportingDocumentUserControl";
		this.SupportingDocumentUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1266, 268, true);
		this.SupportingDocumentUserControl.TabIndex = 0;
		//
		// ContainerUserControl
		//
		this.BindingSource.SetBindingMember(this.ContainerUserControl, nameof(JobDeclaration.CustomsEntryInstructions));
		this.ContainerUserControl.AllowDrop = true;
		this.ContainerUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.ContainerUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
		this.ContainerUserControl.Name = "ContainerUserControl";
		this.ContainerUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1266, 268, true);
		this.ContainerUserControl.TabIndex = 0;
		//
		// SWControlsUserControl
		//
		this.BindingSource.SetBindingMember(this.SWControlsUserControl, "CustomsEntryInstructions.SWControls");
		this.SWControlsUserControl.AllowDrop = true;
		this.SWControlsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.SWControlsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
		this.SWControlsUserControl.Name = "SWControlsUserControl";
		this.SWControlsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1266, 268, true);
		this.SWControlsUserControl.TabIndex = 0;
		//
		// EntryInstructionDetailsUserControl
		//
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.SplitContainer);
		this.Name = "EntryInstructionDetailsUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 385, true);
		this.Controls.SetChildIndex(this.SplitContainer, 0);
		this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.SplitContainer.Panel1.ResumeLayout(false);
		this.SplitContainer.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
		this.SplitContainer.ResumeLayout(false);
		this.SplitContainer.PerformLayout();
		this.EntryInstructionTabControl.ResumeLayout(false);
		this.EntryInstructionTabControl.PerformLayout();
		this.EntryInstructionDetailsTabPage.ResumeLayout(false);
		this.EntryInstructionDetailsTabPage.PerformLayout();
		this.EntryInstructionOtherPartiesTabPage.ResumeLayout(false);
		this.EntryInstructionOtherPartiesTabPage.PerformLayout();
		this.EntryInstructionContainerTabPage.ResumeLayout(false);
		this.EntryInstructionContainerTabPage.PerformLayout();
		this.EntryInstructionSWControlsTabPage.ResumeLayout(false);
		this.EntryInstructionSWControlsTabPage.PerformLayout();
		this.EntryInstructionTopPanelUserControl.ResumeLayout(false);
		this.EntryInstructionTopPanelUserControl.PerformLayout();
		this.SupportingDocumentTabPage.ResumeLayout(false);
		this.SupportingDocumentTabPage.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	protected CargoWise.Windows.UI.KSplitContainer SplitContainer;
	internal ZArchitecture.GUI.ZTabControl EntryInstructionTabControl;
	protected ZArchitecture.GUI.ZTabPage EntryInstructionDetailsTabPage;
	protected ZArchitecture.GUI.ZTabPage EntryInstructionOtherPartiesTabPage;
	protected ZArchitecture.GUI.ZTabPage SupportingDocumentTabPage;
	internal ZArchitecture.GUI.ZTabPage EntryInstructionContainerTabPage;
	internal ZArchitecture.GUI.ZTabPage EntryInstructionSWControlsTabPage;
	protected ZArchitecture.GUI.ZDynamicControlCreationUserControl DetailsUserControl;
	protected ZArchitecture.GUI.ZDynamicControlCreationUserControl OtherPartiesUserControl;
	protected ZArchitecture.GUI.ZDynamicControlCreationUserControl ContainerUserControl;
	protected ZArchitecture.GUI.ZDynamicControlCreationUserControl SWControlsUserControl;
	protected ZArchitecture.GUI.ZDynamicControlCreationUserControl EntryInstructionTopPanelUserControl;
	protected ZArchitecture.GUI.ZDynamicControlCreationUserControl SupportingDocumentUserControl;
}
