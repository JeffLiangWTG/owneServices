namespace Enterprise.PAVE.MENT.GUI
{
	partial class DataSeriesColumnControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.categoryColumnsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.categoryColumnsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.seriesAndCategorySplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.seriesColumnsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.seriesColumnsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.categoryColumnsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.categoryColumnsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.seriesAndCategorySplitContainer)).BeginInit();
			this.seriesAndCategorySplitContainer.Panel1.SuspendLayout();
			this.seriesAndCategorySplitContainer.Panel2.SuspendLayout();
			this.seriesAndCategorySplitContainer.SuspendLayout();
			this.seriesColumnsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.seriesColumnsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery);
			// 
			// categoryColumnsGroupBox
			// 
			this.categoryColumnsGroupBox.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("cd374079-1e5e-4145-9357-236815f81a8a", "Category Columns");
			this.categoryColumnsGroupBox.Controls.Add(this.categoryColumnsGrid);
			this.categoryColumnsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.categoryColumnsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.categoryColumnsGroupBox.Name = "categoryColumnsGroupBox";
			this.categoryColumnsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 180, true);
			this.categoryColumnsGroupBox.TabIndex = 3;
			this.categoryColumnsGroupBox.TabStop = false;
			// 
			// categoryColumnsGrid
			// 
			this.categoryColumnsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.categoryColumnsGrid, "Extractions.CategoryColumns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreExtraction)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)).SyncRoot)).CategoryColumns)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.PAVE.MENT.Business.SQLColumnSpecification)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreExtraction)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)).SyncRoot)).CategoryColumns)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.PAVE.MENT.Business.SQLColumnSpecification)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreExtraction)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)).SyncRoot)).CategoryColumns)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.PAVE.MENT.Business.SQLColumnSpecification)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreExtraction)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)).SyncRoot)).CategoryColumns)).SyncRoot)).Selected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.PAVE.MENT.Business.SQLColumnSpecification)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreExtraction)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)).SyncRoot)).CategoryColumns)).SyncRoot)).Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.PAVE.MENT.Business.SQLColumnSpecification)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreExtraction)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)).SyncRoot)).CategoryColumns)).SyncRoot)).ColumnFunction.FunctionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.PAVE.MENT.Business.SQLColumnSpecification)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreExtraction)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)).SyncRoot)).CategoryColumns)).SyncRoot)).ColumnFunction.Parameter1)));
			this.categoryColumnsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.ColumnName = "Selected";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Sequence";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo1.ColumnName = "ColumnFunction+FunctionType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "ColumnFunction+Parameter1";
			this.categoryColumnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.categoryColumnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.categoryColumnsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.categoryColumnsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.categoryColumnsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.categoryColumnsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.categoryColumnsGrid.CopySelectedRowsAllowed = true;
			this.categoryColumnsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.categoryColumnsGrid.GridId = "163d1e9c-023a-488c-92ee-30dd90844b95";
			this.categoryColumnsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.categoryColumnsGrid.LayoutKey = "zGrid4";
			this.categoryColumnsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.categoryColumnsGrid.Name = "categoryColumnsGrid";
			this.categoryColumnsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 161, true);
			this.categoryColumnsGrid.TabIndex = 0;
			// 
			// seriesAndCategorySplitContainer
			// 
			this.seriesAndCategorySplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.seriesAndCategorySplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.seriesAndCategorySplitContainer.Name = "seriesAndCategorySplitContainer";
			// 
			// seriesAndCategorySplitContainer.Panel1
			// 
			this.seriesAndCategorySplitContainer.Panel1.Controls.Add(this.seriesColumnsGroupBox);
			// 
			// seriesAndCategorySplitContainer.Panel2
			// 
			this.seriesAndCategorySplitContainer.Panel2.Controls.Add(this.categoryColumnsGroupBox);
			this.seriesAndCategorySplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 180, true);
			this.seriesAndCategorySplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(338);
			this.seriesAndCategorySplitContainer.TabIndex = 5;
			// 
			// seriesColumnsGroupBox
			// 
			this.seriesColumnsGroupBox.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("07a8fcdb-ef6b-4d2d-bdaa-0f1f06bb244d", "Series Columns");
			this.seriesColumnsGroupBox.Controls.Add(this.seriesColumnsGrid);
			this.seriesColumnsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.seriesColumnsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.seriesColumnsGroupBox.Name = "seriesColumnsGroupBox";
			this.seriesColumnsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 180, true);
			this.seriesColumnsGroupBox.TabIndex = 1;
			this.seriesColumnsGroupBox.TabStop = false;
			// 
			// seriesColumnsGrid
			// 
			this.seriesColumnsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.seriesColumnsGrid, "Extractions.SeriesColumns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreExtraction)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)).SyncRoot)).SeriesColumns)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.PAVE.MENT.Business.SQLColumnSpecification)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreExtraction)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)).SyncRoot)).SeriesColumns)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.PAVE.MENT.Business.SQLColumnSpecification)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreExtraction)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)).SyncRoot)).SeriesColumns)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.PAVE.MENT.Business.SQLColumnSpecification)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreExtraction)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)).SyncRoot)).SeriesColumns)).SyncRoot)).Selected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.PAVE.MENT.Business.SQLColumnSpecification)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreExtraction)(((System.Collections.IList)(((Enterprise.PAVE.MENT.Business.MENTAgedScoreQuery)(null)).Extractions)).SyncRoot)).SeriesColumns)).SyncRoot)).Sequence)));
			this.seriesColumnsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.ColumnName = "Code";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.ColumnName = "Description";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zCheckBoxColumnStyleInfo2.ColumnName = "Selected";
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "Sequence";
			this.seriesColumnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.seriesColumnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.seriesColumnsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.seriesColumnsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.seriesColumnsGrid.CopySelectedRowsAllowed = true;
			this.seriesColumnsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.seriesColumnsGrid.GridId = "4e70a1cc-f0a8-43e0-a614-42a359d8dd67";
			this.seriesColumnsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.seriesColumnsGrid.LayoutKey = "zGrid1";
			this.seriesColumnsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.seriesColumnsGrid.Name = "seriesColumnsGrid";
			this.seriesColumnsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 161, true);
			this.seriesColumnsGrid.TabIndex = 0;
			// 
			// DataSeriesColumnControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.seriesAndCategorySplitContainer);
			this.Name = "DataSeriesColumnControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 160, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.categoryColumnsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.categoryColumnsGrid)).EndInit();
			this.seriesAndCategorySplitContainer.Panel1.ResumeLayout(false);
			this.seriesAndCategorySplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.seriesAndCategorySplitContainer)).EndInit();
			this.seriesAndCategorySplitContainer.ResumeLayout(false);
			this.seriesColumnsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.seriesColumnsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox categoryColumnsGroupBox;
		private ZArchitecture.ZGrid categoryColumnsGrid;
		private CargoWise.Windows.UI.KSplitContainer seriesAndCategorySplitContainer;
		private ZArchitecture.GUI.ZGroupBox seriesColumnsGroupBox;
		private ZArchitecture.ZGrid seriesColumnsGrid;
	}
}
