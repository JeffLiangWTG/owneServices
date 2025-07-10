namespace Enterprise.Customs.ES.GUI;
partial class MessageUserControl
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
		this.AnnexTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
		this.AnnexesTabUserControl = new Enterprise.Customs.ES.GUI.AnnexesTabUserControl();
		this.NewEntryDetailsTabPage.SuspendLayout();
		this.EntryDetailsUserControl.SuspendLayout();
		this.EntriesGroupBox.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.EntriesBoundGrid)).BeginInit();
		this.EntriesBoundGrid.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.MainHorizontalSplitContainer)).BeginInit();
		this.MainHorizontalSplitContainer.Panel1.SuspendLayout();
		this.MainHorizontalSplitContainer.Panel2.SuspendLayout();
		this.MainHorizontalSplitContainer.SuspendLayout();
		this.EntryLinesMessagesTabControl.SuspendLayout();
		this.EntryLinesTabPage.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.EntryLineGrid)).BeginInit();
		this.EntryLineGrid.SuspendLayout();
		this.ExtendedInfoGroupBox.SuspendLayout();
		this.MessageTabPage.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.TopVerticalSplitContainer)).BeginInit();
		this.TopVerticalSplitContainer.Panel1.SuspendLayout();
		this.TopVerticalSplitContainer.Panel2.SuspendLayout();
		this.TopVerticalSplitContainer.SuspendLayout();
		this.BaseMessageUserControl.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.AnnexTabPage.SuspendLayout();
		this.AnnexesTabUserControl.SuspendLayout();
		this.SuspendLayout();
		// 
		// NewEntryDetailsTabPage
		// 
		this.NewEntryDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 456, true);
		// 
		// EntryDetailsUserControl
		// 
		this.EntryDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 456, true);
		// 
		// EntriesBoundGrid
		// 
		this.EntriesBoundGrid.AfterBind += new System.EventHandler(this.EntriesBoundGrid_AfterBind);
		// 
		// MainHorizontalSplitContainer
		// 
		// 
		// EntryLinesMessagesTabControl
		// 
		this.EntryLinesMessagesTabControl.Controls.Add(this.AnnexTabPage);
		this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.NewEntryDetailsTabPage, 0);
		this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.AnnexTabPage, 0);
		this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.EntryLinesTabPage, 0);
		this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.MessageTabPage, 0);
		// 
		// EntryLineGrid
		// 
		this.EntryLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 333, true);
		// 
		// ExtendedInfoGroupBox
		// 
		this.ExtendedInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 336, true);
		// 
		// TopVerticalSplitContainer
		// 
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.JobDeclaration);
		// 
		// AnnexTabPage
		// 
		this.AnnexTabPage.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("6EEA5D6B-F693-4F36-801D-7C41E391FC33", "Annexes");
		this.AnnexTabPage.Controls.Add(this.AnnexesTabUserControl);
		this.AnnexTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
		this.AnnexTabPage.Name = "AnnexTabPage";
		this.AnnexTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
		this.AnnexTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 456, true);
		this.AnnexTabPage.TabIndex = 2;
		// 
		// AnnexesTabUserControl
		// 
		this.AnnexesTabUserControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.AnnexesTabUserControl, "CustomsEntryHeaders.EDocPivotCollection");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.ES.Business.Declaration.CusStorageDocPivotCollection)(((Enterprise.Customs.ES.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).EDocPivotCollection)));
		this.AnnexesTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.AnnexesTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
		this.AnnexesTabUserControl.Name = "AnnexesTabUserControl";
		this.AnnexesTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 450, true);
		this.AnnexesTabUserControl.TabIndex = 0;
		// 
		// MessageUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.Name = "MessageUserControl";
		this.NewEntryDetailsTabPage.ResumeLayout(false);
		this.NewEntryDetailsTabPage.PerformLayout();
		this.EntryDetailsUserControl.ResumeLayout(true);
		this.EntryDetailsUserControl.PerformLayout();
		this.EntriesGroupBox.ResumeLayout(false);
		this.EntriesGroupBox.PerformLayout();
		((System.ComponentModel.ISupportInitialize)(this.EntriesBoundGrid)).EndInit();
		this.EntriesBoundGrid.ResumeLayout(false);
		this.EntriesBoundGrid.PerformLayout();
		this.MainHorizontalSplitContainer.Panel1.ResumeLayout(false);
		this.MainHorizontalSplitContainer.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)(this.MainHorizontalSplitContainer)).EndInit();
		this.MainHorizontalSplitContainer.ResumeLayout(false);
		this.MainHorizontalSplitContainer.PerformLayout();
		this.EntryLinesMessagesTabControl.ResumeLayout(false);
		this.EntryLinesMessagesTabControl.PerformLayout();
		this.EntryLinesTabPage.ResumeLayout(false);
		this.EntryLinesTabPage.PerformLayout();
		((System.ComponentModel.ISupportInitialize)(this.EntryLineGrid)).EndInit();
		this.EntryLineGrid.ResumeLayout(false);
		this.EntryLineGrid.PerformLayout();
		this.ExtendedInfoGroupBox.ResumeLayout(false);
		this.ExtendedInfoGroupBox.PerformLayout();
		this.MessageTabPage.ResumeLayout(false);
		this.MessageTabPage.PerformLayout();
		this.TopVerticalSplitContainer.Panel1.ResumeLayout(false);
		this.TopVerticalSplitContainer.Panel2.ResumeLayout(false);
		this.TopVerticalSplitContainer.Panel2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)(this.TopVerticalSplitContainer)).EndInit();
		this.TopVerticalSplitContainer.ResumeLayout(false);
		this.TopVerticalSplitContainer.PerformLayout();
		this.BaseMessageUserControl.ResumeLayout(true);
		this.BaseMessageUserControl.PerformLayout();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.AnnexTabPage.ResumeLayout(false);
		this.AnnexTabPage.PerformLayout();
		this.AnnexesTabUserControl.ResumeLayout(true);
		this.AnnexesTabUserControl.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion


	public ZArchitecture.GUI.ZTabPage AnnexTabPage;
	public AnnexesTabUserControl AnnexesTabUserControl;
}
