using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	partial class EventsVisibilityOverrideRegistryControl : RegistryZUserControl
	{
		ZGroupBox ParentGroupBox;
		CargoWise.Windows.UI.KSplitter GridSplitter;
		ZGroupBox ChildGroupBox;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.ParentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ParentGrid = new Enterprise.ZArchitecture.ZGrid();
			this.GridSplitter = new CargoWise.Windows.UI.KSplitter();
			this.ChildGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ChildGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ParentGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ParentGrid)).BeginInit();
			this.ParentGrid.SuspendLayout();
			this.ChildGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChildGrid)).BeginInit();
			this.ChildGrid.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.EventVisibilityOverrideCollection);
			//
			// ParentGroupBox
			//
			this.ParentGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("EventVisibilityOverrideRegistryControl|324d46bd-e215-4053-86ca-124c09858369", "Workflow Types");
			this.ParentGroupBox.Controls.Add(this.ParentGrid);
			this.ParentGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ParentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ParentGroupBox.Name = "ParentGroupBox";
			this.ParentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 176, true);
			this.ParentGroupBox.TabIndex = 1;
			this.ParentGroupBox.TabStop = false;
			//
			// ParentGrid
			//
			this.ParentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ParentGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.EventVisibilityOverride)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.EventVisibilityOverride)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.EventVisibilityOverride)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.EventVisibilityOverride)(null)).IncludeRelatedEvents)));
			this.ParentGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("741cdafd-5048-42d6-90df-cd708be70dfa", "Code");
			zDropEditColumnStyleInfo1.ColumnName = "Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4e1659f2-4233-4a95-9c9e-c63be1f79840", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("39fbfa3c-d133-4b74-b3ed-372a42be6642", "Include Related Events");
			zCheckBoxColumnStyleInfo5.ColumnName = "IncludeRelatedEvents";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ParentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ParentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ParentGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.ParentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ParentGrid.GridId = "89db08b3-5143-4cfe-9e00-e0d758b0ed74";
			this.ParentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ParentGrid.LayoutKey = "zGrid1";
			this.ParentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ParentGrid.Name = "ParentGrid";
			this.ParentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 160, true);
			this.ParentGrid.TabIndex = 0;
			//
			// GridSplitter
			//
			this.GridSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.GridSplitter.DoNotSaveSplitterLayout = false;
			this.GridSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 176, true);
			this.GridSplitter.Name = "GridSplitter";
			this.GridSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 3, true);
			this.GridSplitter.TabIndex = 3;
			this.GridSplitter.TabStop = false;
			//
			// ChildGroupBox
			//
			this.ChildGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("EventsVisibilityOverrideRegistryControl|324d46bd-e215-4053-86ca-124c09858369", "Event Overrides");
			this.ChildGroupBox.Controls.Add(this.ChildGrid);
			this.ChildGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChildGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 179, true);
			this.ChildGroupBox.Name = "ChildGroupBox";
			this.ChildGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 177, true);
			this.ChildGroupBox.TabIndex = 4;
			this.ChildGroupBox.TabStop = false;
			//
			// ChildGrid
			//
			this.ChildGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChildGrid, "EventVisibilityOverrideSettings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.EventVisibilityOverride)(null)).EventVisibilityOverrideSettings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.EventVisibilityOverrideSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.EventVisibilityOverride)(null)).EventVisibilityOverrideSettings)).SyncRoot)).EventCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.EventVisibilityOverrideSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.EventVisibilityOverride)(null)).EventVisibilityOverrideSettings)).SyncRoot)).IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.EventVisibilityOverrideSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.EventVisibilityOverride)(null)).EventVisibilityOverrideSettings)).SyncRoot)).IsSystem)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.EventVisibilityOverrideSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.EventVisibilityOverride)(null)).EventVisibilityOverrideSettings)).SyncRoot)).EventDescriptionOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.EventVisibilityOverrideSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.EventVisibilityOverride)(null)).EventVisibilityOverrideSettings)).SyncRoot)).EventDetail)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.EventVisibilityOverrideSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.EventVisibilityOverride)(null)).EventVisibilityOverrideSettings)).SyncRoot)).QuickView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.EventVisibilityOverrideSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.EventVisibilityOverride)(null)).EventVisibilityOverrideSettings)).SyncRoot)).DuplicateEventsHandlingMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.EventVisibilityOverrideSetting)(((System.Collections.IList)(((Enterprise.Registry.Business.EventVisibilityOverride)(null)).EventVisibilityOverrideSettings)).SyncRoot)).DuplicateEventsHandlingMethodList)));
			this.ChildGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("EC36A5A5-E8A9-4B97-8E0A-7C22FFBA1400", "Event Code");
			zDropEditColumnStyleInfo2.ColumnName = "EventCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("66f36b88-fae6-4b27-9e87-dfe8f4a256e6", "System");
			zCheckBoxColumnStyleInfo6.ColumnName = "IsSystem";
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ef8eb937-c978-4a2d-9075-003ad41302ea", "Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("d9b91feb-f2f2-44a2-8b0d-dcab451c389c", "Event Description Override");
			zTextBoxColumnStyleInfo2.ColumnName = "EventDescriptionOverride";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ceb56225-e464-4ed8-88c3-829ec133d3d6", "Event Detail");
			zCheckBoxColumnStyleInfo2.ColumnName = "EventDetail";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("f537ccca-99ff-4847-ba1d-30f2a416d740", "Quick View");
			zCheckBoxColumnStyleInfo3.ColumnName = "QuickView";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.BindToList = "DuplicateEventsHandlingMethodList";
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("9b4314c2-3e0c-4773-ad4a-a60b2e6b4828", "Duplicate Event Handling Method");
			zDropEditColumnStyleInfo3.ColumnName = "DuplicateEventsHandlingMethod";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b4a1e519-250f-48f1-aaf6-5f4bc3e933ed", "Include Estimates");
			zCheckBoxColumnStyleInfo4.ColumnName = "IncludeEstimates";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ChildGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ChildGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.ChildGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ChildGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ChildGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ChildGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.ChildGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ChildGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.ChildGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChildGrid.GridId = "1e923bb9-0742-426e-a71c-b6b1f5cb13d0";
			this.ChildGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChildGrid.LayoutKey = "zGrid1";
			this.ChildGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ChildGrid.Name = "ChildGrid";
			this.ChildGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(523, 161, true);
			this.ChildGrid.TabIndex = 1;
			//
			// EventsVisibilityOverrideRegistryControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ChildGroupBox);
			this.Controls.Add(this.GridSplitter);
			this.Controls.Add(this.ParentGroupBox);
			this.Name = "EventsVisibilityOverrideRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 356, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ParentGroupBox.ResumeLayout(false);
			this.ParentGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ParentGrid)).EndInit();
			this.ParentGrid.ResumeLayout(false);
			this.ParentGrid.PerformLayout();
			this.ChildGroupBox.ResumeLayout(false);
			this.ChildGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChildGrid)).EndInit();
			this.ChildGrid.ResumeLayout(false);
			this.ChildGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
