namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	partial class ShapeEntityLevelingRulesUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo colorDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.RulesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChannelsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RulesHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ChannelLinksGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChannelLinksGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ChannelLinksHintLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RulesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChannelsGrid)).BeginInit();
			this.ChannelsGrid.SuspendLayout();
			this.ChannelLinksGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChannelLinksGrid)).BeginInit();
			this.ChannelLinksGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNRootDiagramShape);
			// 
			// RulesGroupBox
			// 
			this.RulesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.RulesGroupBox.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("111e09b9-d021-415e-bb6a-bfc53e6f9d12", "Diagram Leveling Rules");
			this.RulesGroupBox.Controls.Add(this.ChannelsGrid);
			this.RulesGroupBox.Controls.Add(this.RulesHintLabel);
			this.RulesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RulesGroupBox.Name = "RulesGroupBox";
			this.RulesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 271, true);
			this.RulesGroupBox.TabIndex = 6;
			this.RulesGroupBox.TabStop = false;
			// 
			// ChannelsGrid
			// 
			this.ChannelsGrid.AllowNavigation = false;
			this.ChannelsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ChannelsGrid, "LevelingRules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNRootDiagramShape)(null)).LevelingRules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNLevelingRule)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNRootDiagramShape)(null)).LevelingRules)).SyncRoot)).BNR_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNLevelingRule)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNRootDiagramShape)(null)).LevelingRules)).SyncRoot)).BNR_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNLevelingRule)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNRootDiagramShape)(null)).LevelingRules)).SyncRoot)).RuleEffect)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNLevelingRule)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNRootDiagramShape)(null)).LevelingRules)).SyncRoot)).BNR_RuleValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNLevelingRule)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNRootDiagramShape)(null)).LevelingRules)).SyncRoot)).ColorName)));
			this.ChannelsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "BNR_Name";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo1.ColumnName = "BNR_Type";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			colorDropEditColumnStyleInfo.ColumnName = "ColorName";
			colorDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "RuleEffect";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(498);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "BNR_RuleValue";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			this.ChannelsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChannelsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ChannelsGrid.ColumnStyles.Add(colorDropEditColumnStyleInfo);
			this.ChannelsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ChannelsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ChannelsGrid.GridId = "18df05ac-4174-4f70-9136-32a21f372a83";
			this.ChannelsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChannelsGrid.LayoutKey = "TagRulesGrid";
			this.ChannelsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 51, true);
			this.ChannelsGrid.Name = "ChannelsGrid";
			this.ChannelsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(627, 214, true);
			this.ChannelsGrid.TabIndex = 2;
			// 
			// RulesHintLabel
			// 
			this.RulesHintLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.RulesHintLabel.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("cbf867be-90b0-4f7b-823b-c6d56f2df6ca", "Define the rules which govern how work will be scheduled within this diagram. Leveling rules control how many entities within the diagram can be scheduled in parallel, or the size of gaps between entities.");
			this.RulesHintLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RulesHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.RulesHintLabel.Name = "RulesHintLabel";
			this.RulesHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(630, 32, true);
			this.RulesHintLabel.TabIndex = 1;
			// 
			// ChannelLinksGroupBox
			// 
			this.ChannelLinksGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ChannelLinksGroupBox.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("feb49546-f068-4f5b-bba9-2a558a499792", "Applicable Channels");
			this.ChannelLinksGroupBox.Controls.Add(this.ChannelLinksGrid);
			this.ChannelLinksGroupBox.Controls.Add(this.ChannelLinksHintLabel);
			this.ChannelLinksGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 280, true);
			this.ChannelLinksGroupBox.Name = "ChannelLinksGroupBox";
			this.ChannelLinksGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 245, true);
			this.ChannelLinksGroupBox.TabIndex = 7;
			this.ChannelLinksGroupBox.TabStop = false;
			// 
			// ChannelLinksGrid
			// 
			this.ChannelLinksGrid.AllowNavigation = false;
			this.ChannelLinksGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ChannelLinksGrid, "LevelingRules.ChannelLinks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNLevelingRule)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNRootDiagramShape)(null)).LevelingRules)).SyncRoot)).ChannelLinks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNLevelingRuleChannelLink)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNLevelingRule)(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNRootDiagramShape)(null)).LevelingRules)).SyncRoot)).ChannelLinks)).SyncRoot)).BNK_BNL_Channel)));
			this.ChannelLinksGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.ColumnName = "BNK_BNL_Channel";
			zGuidDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.ChannelLinksGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.ChannelLinksGrid.GridId = "18df05ac-4174-4f70-9136-32a21f372a83";
			this.ChannelLinksGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChannelLinksGrid.LayoutKey = "TagRulesGrid";
			this.ChannelLinksGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 51, true);
			this.ChannelLinksGrid.Name = "ChannelLinksGrid";
			this.ChannelLinksGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 188, true);
			this.ChannelLinksGrid.TabIndex = 2;
			// 
			// ChannelLinksHintLabel
			// 
			this.ChannelLinksHintLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ChannelLinksHintLabel.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("59ab293e-8a78-4738-9968-75e9c2c1158f", "Specify the channels to which the selected Leveling Rule applies. If no channels are specified, the rule applies to all entities within the diagram.");
			this.ChannelLinksHintLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ChannelLinksHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.ChannelLinksHintLabel.Name = "ChannelLinksHintLabel";
			this.ChannelLinksHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(627, 32, true);
			this.ChannelLinksHintLabel.TabIndex = 1;
			// 
			// ShapeEntityLevelingRulesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ChannelLinksGroupBox);
			this.Controls.Add(this.RulesGroupBox);
			this.Name = "ShapeEntityLevelingRulesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 528, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RulesGroupBox.ResumeLayout(false);
			this.RulesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChannelsGrid)).EndInit();
			this.ChannelsGrid.ResumeLayout(false);
			this.ChannelsGrid.PerformLayout();
			this.ChannelLinksGroupBox.ResumeLayout(false);
			this.ChannelLinksGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChannelLinksGrid)).EndInit();
			this.ChannelLinksGrid.ResumeLayout(false);
			this.ChannelLinksGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox RulesGroupBox;
		private ZArchitecture.ZLabel RulesHintLabel;
		private ZArchitecture.ZGrid ChannelsGrid;
		private ZArchitecture.GUI.ZGroupBox ChannelLinksGroupBox;
		private ZArchitecture.ZGrid ChannelLinksGrid;
		private ZArchitecture.ZLabel ChannelLinksHintLabel;
	}
}
