namespace Enterprise.BufferManagement.GUI
{
	partial class AcceptabilityBandsTabPageControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.BandsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NotAvailableLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BandsHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BandsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.Grid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMComponentSectionConfiguration);
			// 
			// BandsGroupBox
			// 
			this.BandsGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("4bf75ad1-1f99-4113-818c-7a157226d96a", "Acceptability Bands");
			this.BandsGroupBox.Controls.Add(this.NotAvailableLabel);
			this.BandsGroupBox.Controls.Add(this.BandsHintLabel);
			this.BandsGroupBox.Controls.Add(this.Grid);
			this.BandsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BandsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BandsGroupBox.Name = "BandsGroupBox";
			this.BandsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 259, true);
			this.BandsGroupBox.TabIndex = 0;
			this.BandsGroupBox.TabStop = false;
			// 
			// NotAvailableLabel
			// 
			this.NotAvailableLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.NotAvailableLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.NotAvailableLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 113, true);
			this.NotAvailableLabel.Name = "NotAvailableLabel";
			this.NotAvailableLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 33, true);
			this.NotAvailableLabel.TabIndex = 3;
			this.NotAvailableLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.NotAvailableLabel.Visible = false;
			// 
			// BandsHintLabel
			// 
			this.BandsHintLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BandsHintLabel.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("b1a6a59e-5a26-45ca-a9f5-c3c4a4a39d91", "Add items below to show Acceptability Bands as tiles or include them in the heading at the top of this board section.");
			this.BandsHintLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.BandsHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.BandsHintLabel.Name = "BandsHintLabel";
			this.BandsHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 33, true);
			this.BandsHintLabel.TabIndex = 2;
			this.BandsHintLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// Grid
			// 
			this.Grid.AllowNavigation = false;
			this.Grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.Grid, "AcceptabilityBands");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).AcceptabilityBands)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BoardSectionAcceptabilityBand)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).AcceptabilityBands)).SyncRoot)).DisplaySequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.BoardSectionAcceptabilityBand)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).AcceptabilityBands)).SyncRoot)).AcceptabilityBandPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BoardSectionAcceptabilityBand)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).AcceptabilityBands)).SyncRoot)).DisplayName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BoardSectionAcceptabilityBand)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).AcceptabilityBands)).SyncRoot)).DisplayUnits)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BoardSectionAcceptabilityBand)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).AcceptabilityBands)).SyncRoot)).FiltersByReleaseGroupOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BoardSectionAcceptabilityBand)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).AcceptabilityBands)).SyncRoot)).FiltersBySectionOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BoardSectionAcceptabilityBand)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).AcceptabilityBands)).SyncRoot)).ShowOn)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BoardSectionAcceptabilityBand)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).AcceptabilityBands)).SyncRoot)).MaximumItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.BoardSectionAcceptabilityBand)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).AcceptabilityBands)).SyncRoot)).AreBoundaryValuesOverridden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BoardSectionAcceptabilityBand)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).AcceptabilityBands)).SyncRoot)).CautionMinEffectiveValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BoardSectionAcceptabilityBand)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).AcceptabilityBands)).SyncRoot)).GoodMinEffectiveValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BoardSectionAcceptabilityBand)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).AcceptabilityBands)).SyncRoot)).ExcellentMinEffectiveValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BoardSectionAcceptabilityBand)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).AcceptabilityBands)).SyncRoot)).ExcellentMaxEffectiveValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BoardSectionAcceptabilityBand)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).AcceptabilityBands)).SyncRoot)).GoodMaxEffectiveValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BoardSectionAcceptabilityBand)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).AcceptabilityBands)).SyncRoot)).CautionMaxEffectiveValue)));
			this.Grid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "DisplaySequence";
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AcceptabilityBandPK";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo1.ColumnName = "DisplayName";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.ColumnName = "DisplayUnits";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "FiltersByReleaseGroupOverride";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo2.ColumnName = "FiltersBySectionOverride";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo3.ColumnName = "ShowOn";
			zDropEditColumnStyleInfo3.IsMandatory = true;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "MaximumItems";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "AreBoundaryValuesOverridden";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "CautionMinEffectiveValue";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "GoodMinEffectiveValue";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "ExcellentMinEffectiveValue";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "ExcellentMaxEffectiveValue";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "GoodMaxEffectiveValue";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "CautionMaxEffectiveValue";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.Grid.GridId = "c214f0ce-fde0-4e2b-8444-f197b7506e20";
			this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.LayoutKey = "zGrid1";
			this.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 52, true);
			this.Grid.Name = "Grid";
			this.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 201, true);
			this.Grid.TabIndex = 1;
			// 
			// AcceptabilityBandsTabPageControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BandsGroupBox);
			this.Name = "AcceptabilityBandsTabPageControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 259, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BandsGroupBox.ResumeLayout(false);
			this.BandsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			this.Grid.ResumeLayout(false);
			this.Grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox BandsGroupBox;
		private ZArchitecture.ZGrid Grid;
		private ZArchitecture.ZLabel BandsHintLabel;
		private ZArchitecture.ZLabel NotAvailableLabel;
	}
}
