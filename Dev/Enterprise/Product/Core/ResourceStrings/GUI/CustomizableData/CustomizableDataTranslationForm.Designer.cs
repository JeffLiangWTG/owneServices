namespace Enterprise.ResourceStrings.GUI
{
	partial class CustomizableDataTranslationForm
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
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.translationsOfCurrentValueGrid = new Enterprise.ZArchitecture.ZGrid();
			this.allValuesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.allValuesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zPostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.translationsOfCurrentValueGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.allValuesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 400, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ResourceStrings.Business.CustomizableDataTranslationPage);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.translationsOfCurrentValueGrid);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.allValuesLabel);
			this.splitContainer1.Panel2.Controls.Add(this.allValuesGrid);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 394, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(201);
			this.splitContainer1.TabIndex = 1;
			// 
			// translationsOfCurrentValueGrid
			// 
			this.translationsOfCurrentValueGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.translationsOfCurrentValueGrid, "AllTranslationsOfCurrentValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ResourceStrings.Business.CustomizableDataTranslationPage)(null)).AllTranslationsOfCurrentValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.CustomizableDataTranslationEntry)(((System.Collections.IList)(((Enterprise.ResourceStrings.Business.CustomizableDataTranslationPage)(null)).AllTranslationsOfCurrentValue)).SyncRoot)).Language)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.CustomizableDataTranslationEntry)(((System.Collections.IList)(((Enterprise.ResourceStrings.Business.CustomizableDataTranslationPage)(null)).AllTranslationsOfCurrentValue)).SyncRoot)).LanguageDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.CustomizableDataTranslationEntry)(((System.Collections.IList)(((Enterprise.ResourceStrings.Business.CustomizableDataTranslationPage)(null)).AllTranslationsOfCurrentValue)).SyncRoot)).English)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.CustomizableDataTranslationEntry)(((System.Collections.IList)(((Enterprise.ResourceStrings.Business.CustomizableDataTranslationPage)(null)).AllTranslationsOfCurrentValue)).SyncRoot)).Translation)));
			this.translationsOfCurrentValueGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("9077deaf-5f62-42bb-95c4-f9d7603718d3", "Lang.", "Language Code", "");
			zTextBoxColumnStyleInfo1.ColumnName = "Language";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo2.Caption = "";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("8f43ee32-9c28-447b-aa7a-b06eb7140c8b", "Language");
			zTextBoxColumnStyleInfo2.ColumnName = "LanguageDescription";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("54372c4c-fece-4a6e-944d-1703a01f04e3", "English");
			zTextBoxColumnStyleInfo3.ColumnName = "English";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("646f53ca-a491-4023-aaba-42a42129336d", "Translation");
			zTextBoxColumnStyleInfo4.ColumnName = "Translation";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.translationsOfCurrentValueGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.translationsOfCurrentValueGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.translationsOfCurrentValueGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.translationsOfCurrentValueGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.translationsOfCurrentValueGrid.CopySelectedRowsAllowed = true;
			this.translationsOfCurrentValueGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.translationsOfCurrentValueGrid.GridId = "afb01c8c-8d9e-417a-a495-d0beecd7d45a";
			this.translationsOfCurrentValueGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.translationsOfCurrentValueGrid.LayoutKey = "zGrid1";
			this.translationsOfCurrentValueGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.translationsOfCurrentValueGrid.Name = "translationsOfCurrentValueGrid";
			this.translationsOfCurrentValueGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 201, true);
			this.translationsOfCurrentValueGrid.TabIndex = 0;
			// 
			// allValuesLabel
			// 
			this.allValuesLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.allValuesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.allValuesLabel.Name = "allValuesLabel";
			this.allValuesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 17, true);
			this.allValuesLabel.TabIndex = 2;
			this.allValuesLabel.Text = "All English Translations";
			// 
			// allValuesGrid
			// 
			this.allValuesGrid.AllowNavigation = false;
			this.allValuesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.allValuesGrid, "AllValuesInCurrentLanguage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ResourceStrings.Business.CustomizableDataTranslationPage)(null)).AllValuesInCurrentLanguage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.CustomizableDataTranslationEntry)(((System.Collections.IList)(((Enterprise.ResourceStrings.Business.CustomizableDataTranslationPage)(null)).AllValuesInCurrentLanguage)).SyncRoot)).English)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ResourceStrings.Business.CustomizableDataTranslationEntry)(((System.Collections.IList)(((Enterprise.ResourceStrings.Business.CustomizableDataTranslationPage)(null)).AllValuesInCurrentLanguage)).SyncRoot)).Translation)));
			this.allValuesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("6fc06ccd-a2b7-4172-9d1b-131ab7d97578", "English");
			zTextBoxColumnStyleInfo5.ColumnName = "English";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo6.ColumnName = "Translation";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.allValuesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.allValuesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.allValuesGrid.CopySelectedRowsAllowed = true;
			this.allValuesGrid.GridId = "7cae6a0f-24f8-4bb7-ad6f-f0647e80f9cf";
			this.allValuesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.allValuesGrid.LayoutKey = "allValuesGrid";
			this.allValuesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
			this.allValuesGrid.Name = "allValuesGrid";
			this.allValuesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 169, true);
			this.allValuesGrid.TabIndex = 1;
			// 
			// zPostingButtonsUserControl
			// 
			this.zPostingButtonsUserControl.AllowDrop = true;
			this.zPostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zPostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(426, 400, true);
			this.zPostingButtonsUserControl.Name = "zPostingButtonsUserControl";
			this.zPostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.zPostingButtonsUserControl.TabIndex = 3;
			// 
			// CustomizableDataTranslationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 424, true);
			this.Controls.Add(this.zPostingButtonsUserControl);
			this.Controls.Add(this.splitContainer1);
			this.DataSourceType = typeof(Enterprise.ResourceStrings.Business.CustomizableDataTranslationPage);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 300, true);
			this.Name = "CustomizableDataTranslationForm";
			this.Controls.SetChildIndex(this.splitContainer1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zPostingButtonsUserControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.translationsOfCurrentValueGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.allValuesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		internal ZArchitecture.ZGrid translationsOfCurrentValueGrid;
		internal ZArchitecture.ZGrid allValuesGrid;
		internal Core.Forms.ZPostingButtonsUserControl zPostingButtonsUserControl;
		private ZArchitecture.ZLabel allValuesLabel;
	}
}
