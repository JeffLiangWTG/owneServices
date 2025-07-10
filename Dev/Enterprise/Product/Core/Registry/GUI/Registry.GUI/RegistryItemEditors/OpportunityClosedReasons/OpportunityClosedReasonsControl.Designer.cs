namespace Enterprise.Registry.GUI
{
	public partial class OpportunityClosedReasonsControl : RegistryZUserControl
	{
		Enterprise.ZArchitecture.ZLabel MainLabel;
		Enterprise.ZArchitecture.ZLabel SubLabel;
		protected Enterprise.ZArchitecture.ZGrid MainGrid;
		protected Enterprise.ZArchitecture.ZGrid SubGrid;
		protected CargoWise.Windows.UI.KSplitContainer SplitContainer1;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.MainLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MainGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SubLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SubGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).BeginInit();
			this.SplitContainer1.Panel1.SuspendLayout();
			this.SplitContainer1.Panel2.SuspendLayout();
			this.SplitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainGrid)).BeginInit();
			this.MainGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SubGrid)).BeginInit();
			this.SubGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.OpportunityClosedReasonsCollection);
			// 
			// SplitContainer1
			// 
			this.SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer1.Name = "SplitContainer1";
			this.SplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer1.Panel1
			// 
			this.SplitContainer1.Panel1.Controls.Add(this.MainLabel);
			this.SplitContainer1.Panel1.Controls.Add(this.MainGrid);
			// 
			// SplitContainer1.Panel2
			// 
			this.SplitContainer1.Panel2.Controls.Add(this.SubLabel);
			this.SplitContainer1.Panel2.Controls.Add(this.SubGrid);
			this.SplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 384, true);
			this.SplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(198);
			this.SplitContainer1.TabIndex = 1;
			// 
			// MainLabel
			// 
			this.MainLabel.AutoSize = true;
			this.MainLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OpportunityClosedReasons|Reasons", "Reasons");
			this.MainLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.MainLabel.IsFontBold = true;
			this.MainLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.MainLabel.Name = "MainLabel";
			this.MainLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 13, true);
			this.MainLabel.TabIndex = 8;
			// 
			// MainGrid
			// 
			this.MainGrid.AllowNavigation = false;
			this.MainGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MainGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.OpportunityClosedReasons)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.OpportunityClosedReasons)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.OpportunityClosedReasons)(null)).Bool)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.OpportunityClosedReasons)(null)).Code)));
			this.MainGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OpportunityClosedReasons|Description", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OpportunityClosedReasons|Enabled", "Enabled");
			zCheckBoxColumnStyleInfo1.ColumnName = "Bool";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OpportunityClosedReasons|Code", "Code");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "Code";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.MainGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MainGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.MainGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MainGrid.GridId = "7742b326-84c0-4937-ae48-1a293dacfdb0";
			this.MainGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MainGrid.LayoutKey = "MainGrid";
			this.MainGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 15, true);
			this.MainGrid.Name = "MainGrid";
			this.MainGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 181, true);
			this.MainGrid.TabIndex = 5;
			// 
			// SubLabel
			// 
			this.SubLabel.AutoSize = true;
			this.SubLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OpportunityClosedReasons|StatusRules", "Status Rules");
			this.SubLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.SubLabel.IsFontBold = true;
			this.SubLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.SubLabel.Name = "SubLabel";
			this.SubLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 13, true);
			this.SubLabel.TabIndex = 9;
			// 
			// SubGrid
			// 
			this.SubGrid.AllowNavigation = false;
			this.SubGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SubGrid, "StatusRules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.OpportunityClosedReasons)(null)).StatusRules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeSelection)(((System.Collections.IList)(((Enterprise.Registry.Business.OpportunityClosedReasons)(null)).StatusRules)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CodeSelection)(((System.Collections.IList)(((Enterprise.Registry.Business.OpportunityClosedReasons)(null)).StatusRules)).SyncRoot)).Codes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeSelection)(((System.Collections.IList)(((Enterprise.Registry.Business.OpportunityClosedReasons)(null)).StatusRules)).SyncRoot)).Description)));
			this.SubGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Codes";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OpportunityClosedReasons|StatusCode", "Code");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "Code";
			zDropEditColumnStyleInfo1.MaxLengthOverride = 3;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("OpportunityClosedReasons|StatusDescription", "Description");
			zTextBoxColumnStyleInfo3.ColumnName = "Description";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			this.SubGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SubGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SubGrid.GridId = "f5e41b28-1aaf-43fd-9867-ab05a777f9e2";
			this.SubGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SubGrid.LayoutKey = "SubGrid";
			this.SubGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 15, true);
			this.SubGrid.Name = "SubGrid";
			this.SubGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 167, true);
			this.SubGrid.TabIndex = 6;
			// 
			// OpportunityClosedReasonsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer1);
			this.Name = "OpportunityClosedReasonsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 384, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainer1.Panel1.ResumeLayout(false);
			this.SplitContainer1.Panel1.PerformLayout();
			this.SplitContainer1.Panel2.ResumeLayout(false);
			this.SplitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer1)).EndInit();
			this.SplitContainer1.ResumeLayout(false);
			this.SplitContainer1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainGrid)).EndInit();
			this.MainGrid.ResumeLayout(false);
			this.MainGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SubGrid)).EndInit();
			this.SubGrid.ResumeLayout(false);
			this.SubGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
