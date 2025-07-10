using CargoWiseOne.ResourceStrings;
namespace Enterprise.ZArchitecture.GUI.ZAudit.PlugIn
{
	partial class ZAuditUserControl
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (this.disableMessage != null)
				{
					this.disableMessage.Dispose();
				}
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZAuditUserControl));
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.dataGridTableStyle1 = new System.Windows.Forms.DataGridTableStyle();
			this.AuditUser = new System.Windows.Forms.DataGridTextBoxColumn();
			this.disableMessage = new Enterprise.ZArchitecture.ZLabel();
			this.splitContainerMain = new CargoWise.Windows.UI.KSplitContainer();
			this.zGroupBoxFilter = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBoxMinAuditDataLocalTime = new Enterprise.ZArchitecture.ZTextBox();
			this.filterSourceEntityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zCodeFindBoxFilterUser = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zTextBoxLastProcessedLocalTime = new Enterprise.ZArchitecture.ZTextBox();
			this.zDateEditTo = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDateEditFrom = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zButtonFind = new Enterprise.ZArchitecture.GUI.ZButton();
			this.splitContainerGrid = new CargoWise.Windows.UI.KSplitContainer();
			this.EventListDisplayGrid = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
			this.EventDetailsDisplayGrid = new AuditEventDetailsDisplayGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
			this.splitContainerMain.Panel1.SuspendLayout();
			this.splitContainerMain.Panel2.SuspendLayout();
			this.splitContainerMain.SuspendLayout();
			this.zGroupBoxFilter.SuspendLayout();
			this.filterSourceEntityDropEdit.SuspendLayout();
			this.zCodeFindBoxFilterUser.SuspendLayout();
			this.zDateEditTo.SuspendLayout();
			this.zDateEditFrom.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerGrid)).BeginInit();
			this.splitContainerGrid.Panel1.SuspendLayout();
			this.splitContainerGrid.Panel2.SuspendLayout();
			this.splitContainerGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EventListDisplayGrid)).BeginInit();
			this.EventListDisplayGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EventDetailsDisplayGrid)).BeginInit();
			this.EventDetailsDisplayGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.AuditDataServices.Business.Audit);
			// 
			// dataGridTableStyle1
			// 
			this.dataGridTableStyle1.DataGrid = null;
			this.dataGridTableStyle1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			// 
			// AuditUser
			// 
			this.AuditUser.Format = "";
			this.AuditUser.FormatInfo = null;
			this.AuditUser.Width = 75;
			// 
			// disableMessage
			// 
			this.disableMessage.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
			this.disableMessage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.disableMessage.Name = "disableMessage";
			this.disableMessage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 15, true);
			this.disableMessage.TabIndex = 0;
			// 
			// splitContainerMain
			// 
			this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainerMain.IsSplitterFixed = true;
			this.splitContainerMain.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainerMain.Name = "splitContainerMain";
			this.splitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainerMain.Panel1
			// 
			this.splitContainerMain.Panel1.Controls.Add(this.zGroupBoxFilter);
			// 
			// splitContainerMain.Panel2
			// 
			this.splitContainerMain.Panel2.Controls.Add(this.splitContainerGrid);
			this.splitContainerMain.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1086, 571, true);
			this.splitContainerMain.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(122);
			this.splitContainerMain.TabIndex = 13;
			// 
			// zGroupBoxFilter
			// 
			this.zGroupBoxFilter.Controls.Add(this.zTextBoxMinAuditDataLocalTime);
			this.zGroupBoxFilter.Controls.Add(this.filterSourceEntityDropEdit);
			this.zGroupBoxFilter.Controls.Add(this.zCodeFindBoxFilterUser);
			this.zGroupBoxFilter.Controls.Add(this.zTextBoxLastProcessedLocalTime);
			this.zGroupBoxFilter.Controls.Add(this.zDateEditTo);
			this.zGroupBoxFilter.Controls.Add(this.zDateEditFrom);
			this.zGroupBoxFilter.Controls.Add(this.zButtonFind);
			this.zGroupBoxFilter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBoxFilter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBoxFilter.Name = "zGroupBoxFilter";
			this.zGroupBoxFilter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1086, 122, true);
			this.zGroupBoxFilter.TabIndex = 0;
			this.zGroupBoxFilter.TabStop = false;
			// 
			// zTextBoxMinAuditDataLocalTime
			// 
			this.BindingSource.SetBindingMember(this.zTextBoxMinAuditDataLocalTime, "MinAuditDataLocalTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.AuditDataServices.Business.Audit)(null)).MinAuditDataLocalTime)));
			this.zTextBoxMinAuditDataLocalTime.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("6038855a-a8ce-409b-9ca2-5b8de3c49c04", "Min Audit Data Local Time");
			this.zTextBoxMinAuditDataLocalTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(636, 40, true);
			this.zTextBoxMinAuditDataLocalTime.Name = "zTextBoxMinAuditDataLocalTime";
			this.zTextBoxMinAuditDataLocalTime.ReadOnly = true;
			this.zTextBoxMinAuditDataLocalTime.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 17, true);
			this.zTextBoxMinAuditDataLocalTime.TabIndex = 12;
			// 
			// filterSourceEntityDropEdit
			// 
			this.filterSourceEntityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.filterSourceEntityDropEdit, "FilterSourceEntity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.AuditDataServices.Business.Audit)(null)).FilterSourceEntity)));
			this.filterSourceEntityDropEdit.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("1bc5de6a-ef88-46ef-a028-ba4082f47f51", "Source");
			this.filterSourceEntityDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.filterSourceEntityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 15, true);
			this.filterSourceEntityDropEdit.Name = "filterSourceEntityDropEdit";
			this.filterSourceEntityDropEdit.ShowDescriptionBox = false;
			this.filterSourceEntityDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.filterSourceEntityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			this.filterSourceEntityDropEdit.TabIndex = 1;
			// 
			// zCodeFindBoxFilterUser
			// 
			this.zCodeFindBoxFilterUser.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCodeFindBoxFilterUser, "FilterUserCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.AuditDataServices.Business.Audit)(null)).FilterUserCode)));
			this.zCodeFindBoxFilterUser.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("f280311b-ca1d-4c40-b05f-3679640251b4", "User");
			this.zCodeFindBoxFilterUser.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 40, true);
			this.zCodeFindBoxFilterUser.Name = "zCodeFindBoxFilterUser";
			this.zCodeFindBoxFilterUser.PreBoundMaxLength = 3;
			this.zCodeFindBoxFilterUser.ShouldResize = true;
			this.zCodeFindBoxFilterUser.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 17, true);
			this.zCodeFindBoxFilterUser.TabIndex = 3;
			// 
			// zTextBoxLastProcessedLocalTime
			// 
			this.BindingSource.SetBindingMember(this.zTextBoxLastProcessedLocalTime, "LastProcessedLocalTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.AuditDataServices.Business.Audit)(null)).LastProcessedLocalTime)));
			this.zTextBoxLastProcessedLocalTime.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ee6605a1-40d7-4ad1-8f96-7229c1e3a48a", "Last Processed Local Time");
			this.zTextBoxLastProcessedLocalTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(636, 65, true);
			this.zTextBoxLastProcessedLocalTime.Name = "zTextBoxLastProcessedLocalTime";
			this.zTextBoxLastProcessedLocalTime.ReadOnly = true;
			this.zTextBoxLastProcessedLocalTime.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 17, true);
			this.zTextBoxLastProcessedLocalTime.TabIndex = 18;
			// 
			// zDateEditTo
			// 
			this.zDateEditTo.AllowDrop = true;
			this.zDateEditTo.AutoCompleteMonthThreshold = 1;
			this.zDateEditTo.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEditTo, "FilterTimeLocalTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.AuditDataServices.Business.Audit)(null)).FilterTimeLocalTo)));
			this.zDateEditTo.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("b773ea19-6464-4a4e-8cf2-0fe961432e86", "To");
			this.zDateEditTo.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEditTo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 65, true);
			this.zDateEditTo.Name = "zDateEditTo";
			this.zDateEditTo.TabIndex = 7;
			// 
			// zDateEditFrom
			// 
			this.zDateEditFrom.AllowDrop = true;
			this.zDateEditFrom.AutoCompleteMonthThreshold = 1;
			this.zDateEditFrom.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEditFrom, "FilterTimeLocalFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.AuditDataServices.Business.Audit)(null)).FilterTimeLocalFrom)));
			this.zDateEditFrom.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("3338b50a-debf-46d4-bde9-23bb31f1e87e", "From");
			this.zDateEditFrom.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.zDateEditFrom.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 65, true);
			this.zDateEditFrom.Name = "zDateEditFrom";
			this.zDateEditFrom.TabIndex = 5;
			// 
			// zButtonFind
			// 
			this.zButtonFind.BackColor = System.Drawing.Color.Transparent;
			this.zButtonFind.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("866b7bb1-7b2a-4aac-a325-516460636421", "Find");
			this.zButtonFind.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold);
			this.zButtonFind.Image = ((System.Drawing.Image)(resources.GetObject("zButtonFind.Image")));
			this.zButtonFind.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.zButtonFind.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 89, true);
			this.zButtonFind.Name = "zButtonFind";
			this.zButtonFind.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 0, true);
			this.zButtonFind.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.zButtonFind.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 22, true);
			this.zButtonFind.TabIndex = 8;
			this.zButtonFind.ToolTipCaption = null;
			this.zButtonFind.UseVisualStyleBackColor = true;
			// 
			// splitContainerGrid
			// 
			this.splitContainerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainerGrid.Name = "splitContainerGrid";
			// 
			// splitContainerGrid.Panel1
			// 
			this.splitContainerGrid.Panel1.Controls.Add(this.EventListDisplayGrid);
			// 
			// splitContainerGrid.Panel2
			// 
			this.splitContainerGrid.Panel2.Controls.Add(this.EventDetailsDisplayGrid);
			this.splitContainerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1086, 447, true);
			this.splitContainerGrid.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(549);
			this.splitContainerGrid.TabIndex = 13;
			// 
			// EventListDisplayGrid
			// 
			this.EventListDisplayGrid.AllowBeginDrag = false;
			this.EventListDisplayGrid.AllowCopyToNewRowMenuItem = false;
			this.EventListDisplayGrid.AllowDragDropWithChanges = false;
			this.EventListDisplayGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EventListDisplayGrid, "AuditEventData");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.AuditDataServices.Business.Audit)(null)).AuditEventData)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.AuditDataServices.Business.AuditEvent)(((System.Collections.IList)(((Enterprise.AuditDataServices.Business.Audit)(null)).AuditEventData)).SyncRoot)).OperationSymbol)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.AuditDataServices.Business.AuditEvent)(((System.Collections.IList)(((Enterprise.AuditDataServices.Business.Audit)(null)).AuditEventData)).SyncRoot)).TimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.AuditDataServices.Business.AuditEvent)(((System.Collections.IList)(((Enterprise.AuditDataServices.Business.Audit)(null)).AuditEventData)).SyncRoot)).TimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.AuditDataServices.Business.AuditEvent)(((System.Collections.IList)(((Enterprise.AuditDataServices.Business.Audit)(null)).AuditEventData)).SyncRoot)).UserName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.AuditDataServices.Business.AuditEvent)(((System.Collections.IList)(((Enterprise.AuditDataServices.Business.Audit)(null)).AuditEventData)).SyncRoot)).SourceName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.AuditDataServices.Business.AuditEvent)(((System.Collections.IList)(((Enterprise.AuditDataServices.Business.Audit)(null)).AuditEventData)).SyncRoot)).RowInfo)));
			this.EventListDisplayGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("513c6c20-843a-4d6a-90d9-5fda5fc1d193", "Op.");
			zTextBoxColumnStyleInfo1.ColumnName = "OperationSymbol";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(12);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("1b4fbbf9-69dc-401d-a58e-892d221fc937", "Time (UTC)");
			zDateEditColumnStyleInfo1.ColumnName = "TimeUtc";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("cfbde48f-4ae5-4e2f-bce3-0991243e0b53", "Time (Local)");
			zDateEditColumnStyleInfo2.ColumnName = "TimeLocal";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("450e081f-9490-448a-8210-5b399c3306ae", "User");
			zTextBoxColumnStyleInfo2.ColumnName = "UserName";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("d177c3c2-2855-4914-9cbe-d2b9f3b9f68e", "Source Entity");
			zTextBoxColumnStyleInfo3.ColumnName = "SourceName";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("4b9301ba-477e-4d53-aed9-98cd768b6a8c", "Entity ID");
			zTextBoxColumnStyleInfo4.ColumnName = "RowInfo";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.EventListDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EventListDisplayGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.EventListDisplayGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.EventListDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EventListDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EventListDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EventListDisplayGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EventListDisplayGrid.GridId = "8ddcc38f-aea7-463f-a0d2-cb15110a877d";
			this.EventListDisplayGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EventListDisplayGrid.IsWholeRowSelectedOnClick = true;
			this.EventListDisplayGrid.LayoutKey = "zDisplayGrid1";
			this.EventListDisplayGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EventListDisplayGrid.Name = "EventListDisplayGrid";
			this.EventListDisplayGrid.ParentRowsVisible = false;
			this.EventListDisplayGrid.ReadOnly = true;
			this.EventListDisplayGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.EventListDisplayGrid.RowHeaderWidth = 75;
			this.EventListDisplayGrid.ShareActiveColorScheme = true;
			this.EventListDisplayGrid.ShouldSetErrorsOnTabPage = false;
			this.EventListDisplayGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(549, 447, true);
			this.EventListDisplayGrid.TabIndex = 0;
			// 
			// EventDetailsDisplayGrid
			// 
			this.EventDetailsDisplayGrid.AllowBeginDrag = false;
			this.EventDetailsDisplayGrid.AllowCopyToNewRowMenuItem = false;
			this.EventDetailsDisplayGrid.AllowDragDropWithChanges = false;
			this.EventDetailsDisplayGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EventDetailsDisplayGrid, "AuditEventData.ChangeCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.AuditDataServices.Business.AuditEvent)(((System.Collections.IList)(((Enterprise.AuditDataServices.Business.Audit)(null)).AuditEventData)).SyncRoot)).ChangeCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.AuditDataServices.Business.AuditChange)(((System.Collections.IList)(((Enterprise.AuditDataServices.Business.AuditEvent)(((System.Collections.IList)(((Enterprise.AuditDataServices.Business.Audit)(null)).AuditEventData)).SyncRoot)).ChangeCollection)).SyncRoot)).ColumnName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.AuditDataServices.Business.AuditChange)(((System.Collections.IList)(((Enterprise.AuditDataServices.Business.AuditEvent)(((System.Collections.IList)(((Enterprise.AuditDataServices.Business.Audit)(null)).AuditEventData)).SyncRoot)).ChangeCollection)).SyncRoot)).ValueBefore)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.AuditDataServices.Business.AuditChange)(((System.Collections.IList)(((Enterprise.AuditDataServices.Business.AuditEvent)(((System.Collections.IList)(((Enterprise.AuditDataServices.Business.Audit)(null)).AuditEventData)).SyncRoot)).ChangeCollection)).SyncRoot)).ValueAfter)));
			this.EventDetailsDisplayGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("cde4a1df-37d3-46be-adfe-891460d55a4f", "Column");
			zTextBoxColumnStyleInfo5.ColumnName = "ColumnName";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("60918904-acd9-46be-8e94-f647387df9e8", "Before");
			zTextBoxColumnStyleInfo6.ColumnName = "ValueBefore";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(175);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("c5fbde91-917e-469a-84bf-32fc380102c4", "After");
			zTextBoxColumnStyleInfo7.ColumnName = "ValueAfter";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(175);
			this.EventDetailsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.EventDetailsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.EventDetailsDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.EventDetailsDisplayGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EventDetailsDisplayGrid.GridId = "8ddcc38f-aea7-463f-a0d2-cb15110a877d";
			this.EventDetailsDisplayGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EventDetailsDisplayGrid.IsWholeRowSelectedOnClick = true;
			this.EventDetailsDisplayGrid.LayoutKey = "zDisplayGrid1";
			this.EventDetailsDisplayGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EventDetailsDisplayGrid.Name = "EventDetailsDisplayGrid";
			this.EventDetailsDisplayGrid.ParentRowsVisible = false;
			this.EventDetailsDisplayGrid.ReadOnly = true;
			this.EventDetailsDisplayGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.EventDetailsDisplayGrid.RowHeaderWidth = 75;
			this.EventDetailsDisplayGrid.ShareActiveColorScheme = true;
			this.EventDetailsDisplayGrid.ShouldSetErrorsOnTabPage = false;
			this.EventDetailsDisplayGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 447, true);
			this.EventDetailsDisplayGrid.TabIndex = 0;
			// 
			// ZAuditUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainerMain);
			this.Name = "ZAuditUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1086, 571, true);
			this.AfterFirstBinding += new System.EventHandler(this.ZAuditUserControl_AfterFirstBinding);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainerMain.Panel1.ResumeLayout(false);
			this.splitContainerMain.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
			this.splitContainerMain.ResumeLayout(false);
			this.splitContainerMain.PerformLayout();
			this.zGroupBoxFilter.ResumeLayout(false);
			this.zGroupBoxFilter.PerformLayout();
			this.filterSourceEntityDropEdit.ResumeLayout(true);
			this.filterSourceEntityDropEdit.PerformLayout();
			this.zCodeFindBoxFilterUser.ResumeLayout(true);
			this.zCodeFindBoxFilterUser.PerformLayout();
			this.zDateEditTo.ResumeLayout(true);
			this.zDateEditTo.PerformLayout();
			this.zDateEditFrom.ResumeLayout(true);
			this.zDateEditFrom.PerformLayout();
			this.splitContainerGrid.Panel1.ResumeLayout(false);
			this.splitContainerGrid.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerGrid)).EndInit();
			this.splitContainerGrid.ResumeLayout(false);
			this.splitContainerGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EventListDisplayGrid)).EndInit();
			this.EventListDisplayGrid.ResumeLayout(false);
			this.EventListDisplayGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EventDetailsDisplayGrid)).EndInit();
			this.EventDetailsDisplayGrid.ResumeLayout(false);
			this.EventDetailsDisplayGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private void InitializeDiabledComponent()
		{
			this.disableMessage = new Enterprise.ZArchitecture.ZLabel();
			this.SuspendLayout();
			// 
			// disableMessage
			// 
			this.disableMessage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.disableMessage.Name = "disableMessage";
			this.disableMessage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 15, true);
			this.disableMessage.TabIndex = 0;
			disableMessage.CaptionResourceString = new ResourceStringData("8DFD502D-58FD-4DBA-84FF-657933F6DF00", "Data Warehouse Server is not set (Registry: System -> BI -> Server -> Data Warehouse Server).");
			disableMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			disableMessage.Dock = System.Windows.Forms.DockStyle.Fill;
			disableMessage.BringToFront();
			// 
			// ZAuditUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.disableMessage);
			this.Name = "ZAuditUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1181, 602, true);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private System.Windows.Forms.DataGridTableStyle dataGridTableStyle1;
		private System.Windows.Forms.DataGridTextBoxColumn AuditUser;
		private ZLabel disableMessage;
		private CargoWise.Windows.UI.KSplitContainer splitContainerMain;
		private ZGroupBox zGroupBoxFilter;
		private ZTextBox zTextBoxLastProcessedLocalTime;
		private ZDateEdit zDateEditTo;
		private ZDateEdit zDateEditFrom;
		private ZButton zButtonFind;
		private CargoWise.Windows.UI.KSplitContainer splitContainerGrid;
		private ZDisplayGrid EventListDisplayGrid;
		private ZDisplayGrid EventDetailsDisplayGrid;
		private ZCodeFindBox zCodeFindBoxFilterUser;
		private ZDropEdit filterSourceEntityDropEdit;
		private ZTextBox zTextBoxMinAuditDataLocalTime;
	}
}
