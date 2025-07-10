namespace Enterprise.PAVE.MENT.GUI
{
	partial class VisualisationConfigurationControl
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
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.extractionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zTabControl1 = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.dataSeriesConfigurationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.seriesConfigurationControl = new Enterprise.PAVE.MENT.GUI.SeriesConfigurationControl();
			this.graphConfigurationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.graphVisualisationConfigurationControl1 = new Enterprise.PAVE.MENT.GUI.GraphVisualisationConfigurationControl();
			this.additionalExtractionsConfigurationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.additionalExtractionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.additionalExtractionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.visualisationSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.extractionsGrid)).BeginInit();
			this.extractionsGrid.SuspendLayout();
			this.zTabControl1.SuspendLayout();
			this.dataSeriesConfigurationTabPage.SuspendLayout();
			this.seriesConfigurationControl.SuspendLayout();
			this.graphConfigurationTabPage.SuspendLayout();
			this.graphVisualisationConfigurationControl1.SuspendLayout();
			this.additionalExtractionsConfigurationTabPage.SuspendLayout();
			this.additionalExtractionGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.additionalExtractionsGrid)).BeginInit();
			this.additionalExtractionsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.visualisationSplitContainer)).BeginInit();
			this.visualisationSplitContainer.Panel1.SuspendLayout();
			this.visualisationSplitContainer.Panel2.SuspendLayout();
			this.visualisationSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("8d945017-9f41-4b16-ad93-69a289cf6106", "Extractions");
			this.zGroupBox1.Controls.Add(this.extractionsGrid);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 100, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// extractionsGrid
			// 
			this.extractionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.extractionsGrid, "Extractions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreExtraction)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)).SyncRoot)).MEX_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreExtraction)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)).SyncRoot)).IsInstantaneous)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreExtraction)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)).SyncRoot)).AggregationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreExtraction)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)).SyncRoot)).CollectionColumn)));
			this.extractionsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "MEX_Name";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCheckBoxColumnStyleInfo1.ColumnName = "IsInstantaneous";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDropEditColumnStyleInfo1.ColumnName = "AggregationType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo2.ColumnName = "CollectionColumn";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.extractionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.extractionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.extractionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.extractionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.extractionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.extractionsGrid.GridId = "2c53f114-2b97-475a-a923-2aad14bf352a";
			this.extractionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.extractionsGrid.LayoutKey = "zGrid1";
			this.extractionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.extractionsGrid.Name = "extractionsGrid";
			this.extractionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(756, 83, true);
			this.extractionsGrid.TabIndex = 0;
			// 
			// zTabControl1
			// 
			this.zTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.zTabControl1.Controls.Add(this.dataSeriesConfigurationTabPage);
			this.zTabControl1.Controls.Add(this.graphConfigurationTabPage);
			this.zTabControl1.Controls.Add(this.additionalExtractionsConfigurationTabPage);
			this.zTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.SelectedIndex = 0;
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 364, true);
			this.zTabControl1.TabIndex = 1;
			// 
			// dataSeriesConfigurationTabPage
			// 
			this.dataSeriesConfigurationTabPage.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("b634bee1-54e5-4fc2-a8b5-33ee53eba342", "Data Series Configuration");
			this.dataSeriesConfigurationTabPage.Controls.Add(this.seriesConfigurationControl);
			this.dataSeriesConfigurationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.dataSeriesConfigurationTabPage.Name = "dataSeriesConfigurationTabPage";
			this.dataSeriesConfigurationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.dataSeriesConfigurationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(755, 478, true);
			this.dataSeriesConfigurationTabPage.TabIndex = 0;
			this.dataSeriesConfigurationTabPage.UseVisualStyleBackColor = true;
			// 
			// seriesConfigurationControl
			// 
			this.seriesConfigurationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.seriesConfigurationControl, ".");
			this.seriesConfigurationControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.seriesConfigurationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.seriesConfigurationControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 160, true);
			this.seriesConfigurationControl.Name = "seriesConfigurationControl";
			this.seriesConfigurationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 160, true);
			this.seriesConfigurationControl.TabIndex = 0;
			// 
			// graphConfigurationTabPage
			// 
			this.graphConfigurationTabPage.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("b69ad8f9-d361-4661-9645-87ce7cfb5160", "Graph Configuration");
			this.graphConfigurationTabPage.Controls.Add(this.graphVisualisationConfigurationControl1);
			this.graphConfigurationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.graphConfigurationTabPage.Name = "graphConfigurationTabPage";
			this.graphConfigurationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.graphConfigurationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(755, 342, true);
			this.graphConfigurationTabPage.TabIndex = 1;
			this.graphConfigurationTabPage.UseVisualStyleBackColor = true;
			// 
			// graphVisualisationConfigurationControl1
			// 
			this.graphVisualisationConfigurationControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.graphVisualisationConfigurationControl1, "Extractions.DefaultVisualisation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.PAVE.MENT.Business.MENTAgedScoreVisualisation)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreExtraction)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)).SyncRoot)).DefaultVisualisation)));
			this.graphVisualisationConfigurationControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.graphVisualisationConfigurationControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.graphVisualisationConfigurationControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 200, true);
			this.graphVisualisationConfigurationControl1.Name = "graphVisualisationConfigurationControl1";
			this.graphVisualisationConfigurationControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(749, 337, true);
			this.graphVisualisationConfigurationControl1.TabIndex = 0;
			this.graphVisualisationConfigurationControl1.AutoScroll = true;
			this.graphVisualisationConfigurationControl1.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(749, 337, true);
			// 
			// additionalExtractionsConfigurationTabPage
			// 
			this.additionalExtractionsConfigurationTabPage.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("4e7fbbf6-213a-4cf6-9665-6c98047725db", "Additional Extractions");
			this.additionalExtractionsConfigurationTabPage.Controls.Add(this.additionalExtractionGroupBox);
			this.additionalExtractionsConfigurationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.additionalExtractionsConfigurationTabPage.Name = "additionalExtractionsConfigurationTabPage";
			this.additionalExtractionsConfigurationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(755, 527, true);
			this.additionalExtractionsConfigurationTabPage.TabIndex = 2;
			// 
			// additionalExtractionGroupBox
			// 
			this.additionalExtractionGroupBox.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("ba808475-8a77-448e-b6f0-40a076fb4f42", "Additional Extractions");
			this.additionalExtractionGroupBox.Controls.Add(this.additionalExtractionsGrid);
			this.additionalExtractionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.additionalExtractionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.additionalExtractionGroupBox.Name = "additionalExtractionGroupBox";
			this.additionalExtractionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(755, 527, true);
			this.additionalExtractionGroupBox.TabIndex = 0;
			this.additionalExtractionGroupBox.TabStop = false;
			// 
			// additionalExtractionsGrid
			// 
			this.additionalExtractionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.additionalExtractionsGrid, "Extractions.AdditionalExtractions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreExtraction)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)).SyncRoot)).AdditionalExtractions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.PAVE.MENT.Business.AdditionalExtractionLink)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreExtraction)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)).SyncRoot)).AdditionalExtractions)).SyncRoot)).QueryPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.PAVE.MENT.Business.AdditionalExtractionLink)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreExtraction)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)).SyncRoot)).AdditionalExtractions)).SyncRoot)).ExtractionPK)));
			this.additionalExtractionsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "QueryPK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zGuidDropEditColumnStyleInfo1.ColumnName = "ExtractionPK";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.additionalExtractionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.additionalExtractionsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.additionalExtractionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.additionalExtractionsGrid.GridId = "568980f8-a8d3-4ff6-9e6b-eebd6d305c28";
			this.additionalExtractionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.additionalExtractionsGrid.LayoutKey = "additionalExtractionsGrid";
			this.additionalExtractionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.additionalExtractionsGrid.Name = "additionalExtractionsGrid";
			this.additionalExtractionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 510, true);
			this.additionalExtractionsGrid.TabIndex = 0;
			// 
			// visualisationSplitContainer
			// 
			this.visualisationSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.visualisationSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.visualisationSplitContainer.Name = "visualisationSplitContainer";
			this.visualisationSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// visualisationSplitContainer.Panel1
			// 
			this.visualisationSplitContainer.Panel1.Controls.Add(this.zGroupBox1);
			// 
			// visualisationSplitContainer.Panel2
			// 
			this.visualisationSplitContainer.Panel2.Controls.Add(this.zTabControl1);
			this.visualisationSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 467, true);
			this.visualisationSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			this.visualisationSplitContainer.TabIndex = 2;
			// 
			// VisualisationConfigurationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.visualisationSplitContainer);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 467, true);
			this.Name = "VisualisationConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 467, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.extractionsGrid)).EndInit();
			this.extractionsGrid.ResumeLayout(false);
			this.extractionsGrid.PerformLayout();
			this.zTabControl1.ResumeLayout(false);
			this.zTabControl1.PerformLayout();
			this.dataSeriesConfigurationTabPage.ResumeLayout(false);
			this.dataSeriesConfigurationTabPage.PerformLayout();
			this.seriesConfigurationControl.ResumeLayout(true);
			this.seriesConfigurationControl.PerformLayout();
			this.graphConfigurationTabPage.ResumeLayout(false);
			this.graphConfigurationTabPage.PerformLayout();
			this.graphVisualisationConfigurationControl1.ResumeLayout(true);
			this.graphVisualisationConfigurationControl1.PerformLayout();
			this.additionalExtractionsConfigurationTabPage.ResumeLayout(false);
			this.additionalExtractionsConfigurationTabPage.PerformLayout();
			this.additionalExtractionGroupBox.ResumeLayout(false);
			this.additionalExtractionGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.additionalExtractionsGrid)).EndInit();
			this.additionalExtractionsGrid.ResumeLayout(false);
			this.additionalExtractionsGrid.PerformLayout();
			this.visualisationSplitContainer.Panel1.ResumeLayout(false);
			this.visualisationSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.visualisationSplitContainer)).EndInit();
			this.visualisationSplitContainer.ResumeLayout(false);
			this.visualisationSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.ZGrid extractionsGrid;
		private ZArchitecture.GUI.ZTabControl zTabControl1;
		private ZArchitecture.GUI.ZTabPage dataSeriesConfigurationTabPage;
		private ZArchitecture.GUI.ZTabPage graphConfigurationTabPage;
		private SeriesConfigurationControl seriesConfigurationControl;
		private GraphVisualisationConfigurationControl graphVisualisationConfigurationControl1;
		private ZArchitecture.GUI.ZTabPage additionalExtractionsConfigurationTabPage;
		private ZArchitecture.GUI.ZGroupBox additionalExtractionGroupBox;
		private ZArchitecture.ZGrid additionalExtractionsGrid;
		private CargoWise.Windows.UI.KSplitContainer visualisationSplitContainer;
	}
}
