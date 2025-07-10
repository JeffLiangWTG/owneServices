namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class IncidentGroupStatusConfigurationControl
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
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.IncidentGroupStatusConfigurationsZGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.IncidentGroupTypeZGrid = new Enterprise.ZArchitecture.ZGrid();
			this.kSplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.IncidentGroupStatusConfigurationsZGrid)).BeginInit();
			this.IncidentGroupStatusConfigurationsZGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.IncidentGroupTypeZGrid)).BeginInit();
			this.IncidentGroupTypeZGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).BeginInit();
			this.kSplitContainer1.Panel1.SuspendLayout();
			this.kSplitContainer1.Panel2.SuspendLayout();
			this.kSplitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.IncidentGroupTypeCollection);
			// 
			// zLabel2
			// 
			this.zLabel2.CaptionResourceString = ZClientEDI.Res.GetData("d8e13134-d07a-4ee9-8090-d39d454893da", "Group Stage Setup");
			this.zLabel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(870, 28, true);
			this.zLabel2.TabIndex = 0;
			// 
			// IncidentGroupStatusConfigurationsZGrid
			// 
			this.IncidentGroupStatusConfigurationsZGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.IncidentGroupStatusConfigurationsZGrid, "IncidentGroupStatusConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupType)(null)).IncidentGroupStatusConfigurations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupType)(null)).IncidentGroupStatusConfigurations)).SyncRoot)).Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupType)(null)).IncidentGroupStatusConfigurations)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupType)(null)).IncidentGroupStatusConfigurations)).SyncRoot)).DescriptionOnGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupType)(null)).IncidentGroupStatusConfigurations)).SyncRoot)).ControlIncidents)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupType)(null)).IncidentGroupStatusConfigurations)).SyncRoot)).ApprovalGate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupType)(null)).IncidentGroupStatusConfigurations)).SyncRoot)).CascadeCriticality)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupType)(null)).IncidentGroupStatusConfigurations)).SyncRoot)).CascadeProductDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupType)(null)).IncidentGroupStatusConfigurations)).SyncRoot)).GroupCompleted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupType)(null)).IncidentGroupStatusConfigurations)).SyncRoot)).IncidentCompleted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupType)(null)).IncidentGroupStatusConfigurations)).SyncRoot)).TriggerOn)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupType)(null)).IncidentGroupStatusConfigurations)).SyncRoot)).Enabled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupType)(null)).IncidentGroupStatusConfigurations)).SyncRoot)).IsReversible)));
			this.IncidentGroupStatusConfigurationsZGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Sequence";
			zCalcEditColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "DescriptionOnGroup";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "ControlIncidents";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "ApprovalGate";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.ColumnName = "CascadeCriticality";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.IsVisible = false;
			zCheckBoxColumnStyleInfo4.ColumnName = "CascadeProductDetails";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo5.ColumnName = "GroupCompleted";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo6.ColumnName = "IncidentCompleted";
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "TriggerOn";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo7.ColumnName = "Enabled";
			zCheckBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo8.ColumnName = "IsReversible";
			zCheckBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.IncidentGroupStatusConfigurationsZGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.IncidentGroupStatusConfigurationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.IncidentGroupStatusConfigurationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.IncidentGroupStatusConfigurationsZGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.IncidentGroupStatusConfigurationsZGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.IncidentGroupStatusConfigurationsZGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.IncidentGroupStatusConfigurationsZGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.IncidentGroupStatusConfigurationsZGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.IncidentGroupStatusConfigurationsZGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.IncidentGroupStatusConfigurationsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.IncidentGroupStatusConfigurationsZGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.IncidentGroupStatusConfigurationsZGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo8);
			this.IncidentGroupStatusConfigurationsZGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IncidentGroupStatusConfigurationsZGrid.GridId = "733ce2f2-1ea6-49fb-ac64-b38961ee5d28";
			this.IncidentGroupStatusConfigurationsZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.IncidentGroupStatusConfigurationsZGrid.LayoutKey = "IncidentGroupStatusConfigurationsZGrid";
			this.IncidentGroupStatusConfigurationsZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 28, true);
			this.IncidentGroupStatusConfigurationsZGrid.Name = "IncidentGroupStatusConfigurationsZGrid";
			this.IncidentGroupStatusConfigurationsZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(870, 297, true);
			this.IncidentGroupStatusConfigurationsZGrid.TabIndex = 1;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = ZClientEDI.Res.GetData("c4d4a74e-67b1-4c5c-845d-33c41c2aff27", "Incident Group Type");
			this.zLabel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(870, 28, true);
			this.zLabel1.TabIndex = 0;
			// 
			// IncidentGroupTypeZGrid
			// 
			this.IncidentGroupTypeZGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.IncidentGroupTypeZGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupType)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupType)(null)).GroupType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupType)(null)).Description)));
			this.IncidentGroupTypeZGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "GroupType";
			zTextBoxColumnStyleInfo5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "Description";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.IncidentGroupTypeZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.IncidentGroupTypeZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.IncidentGroupTypeZGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IncidentGroupTypeZGrid.GridId = "4ba77e2f-17b5-446e-99f6-437868b35102";
			this.IncidentGroupTypeZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.IncidentGroupTypeZGrid.LayoutKey = "IncidentGroupTypeZGrid";
			this.IncidentGroupTypeZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 28, true);
			this.IncidentGroupTypeZGrid.Name = "IncidentGroupTypeZGrid";
			this.IncidentGroupTypeZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(870, 158, true);
			this.IncidentGroupTypeZGrid.TabIndex = 1;
			this.IncidentGroupTypeZGrid.SelectedRowsChangedInMouseDown += IncidentGroupTypeZGrid_SelectIndexChanged;
			// 
			// kSplitContainer1
			// 
			this.kSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kSplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.kSplitContainer1.Name = "kSplitContainer1";
			this.kSplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// kSplitContainer1.Panel1
			// 
			this.kSplitContainer1.Panel1.Controls.Add(this.IncidentGroupTypeZGrid);
			this.kSplitContainer1.Panel1.Controls.Add(this.zLabel1);
			// 
			// kSplitContainer1.Panel2
			// 
			this.kSplitContainer1.Panel2.Controls.Add(this.IncidentGroupStatusConfigurationsZGrid);
			this.kSplitContainer1.Panel2.Controls.Add(this.zLabel2);
			this.kSplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(870, 519, true);
			this.kSplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(186);
			this.kSplitContainer1.SplitterWidth = 8;
			this.kSplitContainer1.TabIndex = 0;
			// 
			// IncidentGroupStatusConfigurationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.kSplitContainer1);
			this.Name = "IncidentGroupStatusConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(870, 519, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.IncidentGroupStatusConfigurationsZGrid)).EndInit();
			this.IncidentGroupStatusConfigurationsZGrid.ResumeLayout(false);
			this.IncidentGroupStatusConfigurationsZGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.IncidentGroupTypeZGrid)).EndInit();
			this.IncidentGroupTypeZGrid.ResumeLayout(false);
			this.IncidentGroupTypeZGrid.PerformLayout();
			this.kSplitContainer1.Panel1.ResumeLayout(false);
			this.kSplitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).EndInit();
			this.kSplitContainer1.ResumeLayout(false);
			this.kSplitContainer1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.ZGrid IncidentGroupStatusConfigurationsZGrid;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZGrid IncidentGroupTypeZGrid;
		private CargoWise.Windows.UI.KSplitContainer kSplitContainer1;
	}
}
