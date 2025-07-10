using Enterprise.BufferManagement.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.PAVE.MENT.GUI
{
	public partial class MENTAgedScoreQueryForm : ZTemplateForm
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
		protected new void InitializeComponent()
		{
			this.ScheduleTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.scheduleSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.recurrenceControl = new Enterprise.BufferManagement.GUI.ScheduleTaskRecurrenceControl();
			this.purgeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PurgeDataLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PurgeDaysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DataPointSetCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.dataCollectionControl1 = new Enterprise.PAVE.MENT.GUI.DataCollectionControl();
			this.VisualisationConfigurationTabPage = new Enterprise.PAVE.MENT.GUI.MENTNavigationTabPage();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ScheduleTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.scheduleSplitContainer)).BeginInit();
			this.scheduleSplitContainer.Panel1.SuspendLayout();
			this.scheduleSplitContainer.Panel2.SuspendLayout();
			this.scheduleSplitContainer.SuspendLayout();
			this.recurrenceControl.SuspendLayout();
			this.purgeGroupBox.SuspendLayout();
			this.dataCollectionControl1.SuspendLayout();
			this.VisualisationConfigurationTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.ScheduleTabPage);
			this.MainTabControl.Controls.Add(this.VisualisationConfigurationTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 833, true);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.VisualisationConfigurationTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ScheduleTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.dataCollectionControl1);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 814, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(839, 760, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 814, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 833, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery);
			// 
			// ScheduleTabPage
			// 
			this.ScheduleTabPage.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("e6935060-0ebc-411c-aedf-f849e1c9b2dd", "Schedule");
			this.ScheduleTabPage.Controls.Add(this.scheduleSplitContainer);
			this.ScheduleTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ScheduleTabPage.Name = "ScheduleTabPage";
			this.ScheduleTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ScheduleTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 814, true);
			this.ScheduleTabPage.TabIndex = 2;
			this.ScheduleTabPage.UseVisualStyleBackColor = true;
			// 
			// scheduleSplitContainer
			// 
			this.scheduleSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.scheduleSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.scheduleSplitContainer.Name = "scheduleSplitContainer";
			this.scheduleSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// scheduleSplitContainer.Panel1
			// 
			this.scheduleSplitContainer.Panel1.Controls.Add(this.recurrenceControl);
			// 
			// scheduleSplitContainer.Panel2
			// 
			this.scheduleSplitContainer.Panel2.Controls.Add(this.purgeGroupBox);
			this.scheduleSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(833, 754, true);
			this.scheduleSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(351);
			this.scheduleSplitContainer.TabIndex = 5;
			// 
			// recurrenceControl
			// 
			this.recurrenceControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.recurrenceControl, "QuerySchedule");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Scheduler.Business.StmScheduleTask)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).QuerySchedule)));
			this.recurrenceControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.recurrenceControl.IsCollectionActiveCheckboxVisible = true;
			this.recurrenceControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.recurrenceControl.Name = "recurrenceControl";
			this.recurrenceControl.ReadOnly = false;
			this.recurrenceControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(833, 351, true);
			this.recurrenceControl.TabIndex = 0;
			// 
			// purgeGroupBox
			// 
			this.purgeGroupBox.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("e0ce812a-48f0-4644-b489-7ed3ee97d187", "Purge Data");
			this.purgeGroupBox.Controls.Add(this.zLabel2);
			this.purgeGroupBox.Controls.Add(this.zLabel1);
			this.purgeGroupBox.Controls.Add(this.PurgeDataLabel);
			this.purgeGroupBox.Controls.Add(this.PurgeDaysCalcEdit);
			this.purgeGroupBox.Controls.Add(this.DataPointSetCalcEdit);
			this.purgeGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.purgeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.purgeGroupBox.Name = "purgeGroupBox";
			this.purgeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(833, 399, true);
			this.purgeGroupBox.TabIndex = 3;
			this.purgeGroupBox.TabStop = false;
			// 
			// PurgeDataLabel
			// 
			this.PurgeDataLabel.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("d8e0d71b-b3d0-4ef1-8dfb-cba570e1282c", "Set the below values to 0 to disable purging.");
			this.PurgeDataLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PurgeDataLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 19, true);
			this.PurgeDataLabel.Name = "PurgeDataLabel";
			this.PurgeDataLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 18, true);
			this.PurgeDataLabel.TabIndex = 0;
			// 
			// PurgeDaysCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PurgeDaysCalcEdit, "MAQ_PurgeDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).MAQ_PurgeDays)));
			this.PurgeDaysCalcEdit.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("112709b7-a849-40c5-b283-b9fb48b31923", "Purge data older than");
			this.PurgeDaysCalcEdit.DecimalPlaces = 2;
			this.PurgeDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 45, true);
			this.PurgeDaysCalcEdit.Name = "PurgeDaysCalcEdit";
			this.PurgeDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 18, true);
			this.PurgeDaysCalcEdit.TabIndex = 1;
			this.PurgeDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DataPointSetCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DataPointSetCalcEdit, "MAQ_PurgeAllButLatestQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).MAQ_PurgeAllButLatestQuantity)));
			this.DataPointSetCalcEdit.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("ad734533-1eb6-4d62-bedc-06b83e5ccec1", "Keep only the newest");
			this.DataPointSetCalcEdit.DecimalPlaces = 2;
			this.DataPointSetCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 67, true);
			this.DataPointSetCalcEdit.Name = "DataPointSetCalcEdit";
			this.DataPointSetCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 18, true);
			this.DataPointSetCalcEdit.TabIndex = 2;
			this.DataPointSetCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// dataCollectionControl1
			// 
			this.dataCollectionControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dataCollectionControl1, ".");
			this.dataCollectionControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataCollectionControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.dataCollectionControl1.Name = "dataCollectionControl1";
			this.dataCollectionControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 814, true);
			this.dataCollectionControl1.TabIndex = 0;
			// 
			// VisualisationConfigurationTabPage
			// 
			this.VisualisationConfigurationTabPage.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("05003b1b-dc9d-492d-b999-8a041d57faaf", "Visualization");
			this.VisualisationConfigurationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.VisualisationConfigurationTabPage.Name = "VisualisationConfigurationTabPage";
			this.VisualisationConfigurationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.VisualisationConfigurationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(804, 806, true);
			this.VisualisationConfigurationTabPage.TabIndex = 3;
			this.VisualisationConfigurationTabPage.UseVisualStyleBackColor = true;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("5f61752e-271e-41ee-a044-9a9e4efb3b43", "day(s)");
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 45, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 18, true);
			this.zLabel1.TabIndex = 3;
			// 
			// zLabel2
			// 
			this.zLabel2.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("8d550eb4-a420-4f92-afe0-4515f1f02cfe", "data point set(s)");
			this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 67, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 18, true);
			this.zLabel2.TabIndex = 4;
			// 
			// MENTAgedScoreQueryForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("471c83fa-0197-44c3-9d6a-45fd859641a0", "MENT Aged Score Query");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 860, true);
			this.DataSourceType = typeof(Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 860, true);
			this.Name = "MENTAgedScoreQueryForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ScheduleTabPage.ResumeLayout(false);
			this.ScheduleTabPage.PerformLayout();
			this.scheduleSplitContainer.Panel1.ResumeLayout(false);
			this.scheduleSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.scheduleSplitContainer)).EndInit();
			this.scheduleSplitContainer.ResumeLayout(false);
			this.scheduleSplitContainer.PerformLayout();
			this.recurrenceControl.ResumeLayout(true);
			this.recurrenceControl.PerformLayout();
			this.purgeGroupBox.ResumeLayout(false);
			this.purgeGroupBox.PerformLayout();
			this.dataCollectionControl1.ResumeLayout(true);
			this.dataCollectionControl1.PerformLayout();
			this.VisualisationConfigurationTabPage.ResumeLayout(false);
			this.VisualisationConfigurationTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZTabPage ScheduleTabPage;
		private ScheduleTaskRecurrenceControl recurrenceControl;
		private DataCollectionControl dataCollectionControl1;
		private MENTNavigationTabPage VisualisationConfigurationTabPage;
		private CargoWise.Windows.UI.KSplitContainer scheduleSplitContainer;
		private ZArchitecture.GUI.ZGroupBox purgeGroupBox;
		private ZArchitecture.ZLabel PurgeDataLabel;
		private ZArchitecture.ZCalcEdit PurgeDaysCalcEdit;
		private ZArchitecture.ZCalcEdit DataPointSetCalcEdit;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.ZLabel zLabel1;
	}
}