namespace Enterprise.Registry.GUI
{
	partial class PurgeSettingsControl : RegistryZUserControl
	{
		ZArchitecture.GUI.ZGroupBox ApplicationCodesGroupBox;
		protected internal ZArchitecture.ZGrid ApplicationCodesGrid;
		ZArchitecture.GUI.ZGroupBox InterchangesGroupBox;
		ZArchitecture.GUI.ZGroupBox MessageTypesGroupBox;
		ZArchitecture.ZGrid InterchangesGrid;
		ZArchitecture.ZGrid MessageTypesGrid;
		ZArchitecture.GUI.ZGroupBox PurgeMessageBatchSizeGroupBox;
		ZArchitecture.ZCalcEdit PurgeMessageBatchSizeCalcEdit;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.ApplicationCodesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ApplicationCodesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.InterchangesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InterchangesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MessageTypesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageTypesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PurgeMessageBatchSizeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PurgeMessageBatchSizeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ApplicationCodesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApplicationCodesGrid)).BeginInit();
			this.InterchangesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InterchangesGrid)).BeginInit();
			this.MessageTypesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageTypesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.PurgeSettings);
			// 
			// ApplicationCodesGroupBox
			// 
			this.ApplicationCodesGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("41855e48-a13e-4d29-8328-22d0adc502ca", "Application Codes");
			this.ApplicationCodesGroupBox.Controls.Add(this.ApplicationCodesGrid);
			this.ApplicationCodesGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.ApplicationCodesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 3, true);
			this.ApplicationCodesGroupBox.Name = "ApplicationCodesGroupBox";
			this.ApplicationCodesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 105, true);
			this.ApplicationCodesGroupBox.TabIndex = 1;
			this.ApplicationCodesGroupBox.TabStop = false;
			// 
			// ApplicationCodesGrid
			// 
			this.ApplicationCodesGrid.AllowNavigation = false;
			this.ApplicationCodesGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.ApplicationCodesGrid, "ApplicationCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.PurgeSettings)(null)).ApplicationCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ApplicationCodeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.PurgeSettings)(null)).ApplicationCodes)).SyncRoot)).ApplicationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ApplicationCodeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.PurgeSettings)(null)).ApplicationCodes)).SyncRoot)).PurgeTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Registry.Business.ApplicationCodeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.PurgeSettings)(null)).ApplicationCodes)).SyncRoot)).PurgeTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.ApplicationCodeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.PurgeSettings)(null)).ApplicationCodes)).SyncRoot)).PurgeTimeUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.ApplicationCodeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.PurgeSettings)(null)).ApplicationCodes)).SyncRoot)).Selected)));
			this.ApplicationCodesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("48d4dd65-7423-4176-820a-f77495ca164f", "Application Code");
			zTextBoxColumnStyleInfo1.ColumnName = "ApplicationCode";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("47ec60c1-91a3-4cf2-9ae4-df2ac99e35d9", "Purge Type");
			zTextBoxColumnStyleInfo2.ColumnName = "PurgeTypeDescription";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("bcdd4572-0f2a-4914-9814-d82f37c04c25", "Time");
			zCalcEditColumnStyleInfo1.ColumnName = "PurgeTime";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Registry.GUI.Res.GetData("38ecf521-8d98-480e-93db-d91b1b50e986", "Purge Interval");
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.ShowEmptyStringForEmptyValue = true;
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("5d0c375a-7e31-4350-bf34-f72a49eb5a71", "Time Unit");
			zGuidDropEditColumnStyleInfo1.ColumnName = "PurgeTimeUnit";
			zGuidDropEditColumnStyleInfo1.GroupName = Enterprise.Registry.GUI.Res.GetData("38ecf521-8d98-480e-93db-d91b1b50e986", "Purge Interval");
			zGuidDropEditColumnStyleInfo1.IsMandatory = true;
			zGuidDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("c922095e-5d48-4d3c-8ad2-369be055a391", "Selected");
			zCheckBoxColumnStyleInfo1.ColumnName = "Selected";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			this.ApplicationCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ApplicationCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ApplicationCodesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ApplicationCodesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.ApplicationCodesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ApplicationCodesGrid.CopySelectedRowsAllowed = true;
			this.ApplicationCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ApplicationCodesGrid.GridId = "31d0f090-0493-478d-823b-a9fa1c5866ad";
			this.ApplicationCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ApplicationCodesGrid.LayoutKey = "ApplicationCodesGrid";
			this.ApplicationCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ApplicationCodesGrid.Name = "ApplicationCodesGrid";
			this.ApplicationCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 86, true);
			this.ApplicationCodesGrid.TabIndex = 0;
			// 
			// InterchangesGroupBox
			// 
			this.InterchangesGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("c874efbd-5050-4aa8-964f-d33cf1c8b0d2", "Interchanges that have no messages");
			this.InterchangesGroupBox.Controls.Add(this.InterchangesGrid);
			this.InterchangesGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.InterchangesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 114, true);
			this.InterchangesGroupBox.Name = "InterchangesGroupBox";
			this.InterchangesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 55, true);
			this.InterchangesGroupBox.TabIndex = 3;
			this.InterchangesGroupBox.TabStop = false;
			// 
			// InterchangesGrid
			// 
			this.InterchangesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InterchangesGrid, "ApplicationCodes.Interchanges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ApplicationCodeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.PurgeSettings)(null)).ApplicationCodes)).SyncRoot)).Interchanges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.InterchangeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.ApplicationCodeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.PurgeSettings)(null)).ApplicationCodes)).SyncRoot)).Interchanges)).SyncRoot)).PurgeTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.InterchangeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.ApplicationCodeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.PurgeSettings)(null)).ApplicationCodes)).SyncRoot)).Interchanges)).SyncRoot)).PurgeTimeUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.InterchangeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.ApplicationCodeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.PurgeSettings)(null)).ApplicationCodes)).SyncRoot)).Interchanges)).SyncRoot)).Selected)));
			this.InterchangesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("bcdd4572-0f2a-4914-9814-d82f37c04c25", "Time");
			zCalcEditColumnStyleInfo2.ColumnName = "PurgeTime";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Registry.GUI.Res.GetData("38ecf521-8d98-480e-93db-d91b1b50e986", "Purge Interval");
			zCalcEditColumnStyleInfo2.IsMandatory = true;
			zGuidDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("5d0c375a-7e31-4350-bf34-f72a49eb5a71", "Time Unit");
			zGuidDropEditColumnStyleInfo2.ColumnName = "PurgeTimeUnit";
			zGuidDropEditColumnStyleInfo2.GroupName = Enterprise.Registry.GUI.Res.GetData("38ecf521-8d98-480e-93db-d91b1b50e986", "Purge Interval");
			zGuidDropEditColumnStyleInfo2.IsMandatory = true;
			zGuidDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("c0eac360-1e3f-42b9-90fc-2e3688df3ec5", "Selected");
			zCheckBoxColumnStyleInfo2.ColumnName = "Selected";
			zCheckBoxColumnStyleInfo2.IsMandatory = true;
			this.InterchangesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.InterchangesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.InterchangesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.InterchangesGrid.CopySelectedRowsAllowed = true;
			this.InterchangesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InterchangesGrid.GridId = "25bc3f82-7562-4509-9298-62fbcb46c1d4";
			this.InterchangesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InterchangesGrid.LayoutKey = "InterchangesGrid";
			this.InterchangesGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.InterchangesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.InterchangesGrid.Name = "InterchangesGrid";
			this.InterchangesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 39, true);
			this.InterchangesGrid.TabIndex = 0;
			// 
			// MessageTypesGroupBox
			//
			this.MessageTypesGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.MessageTypesGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b075d976-b6f5-48ca-8fe1-369b7cc293b3", "Message Types and Sub Types");
			this.MessageTypesGroupBox.Controls.Add(this.MessageTypesGrid);
			this.MessageTypesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 180, true);
			this.MessageTypesGroupBox.Name = "MessageTypesGroupBox";
			this.MessageTypesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 124, true);
			this.MessageTypesGroupBox.TabIndex = 3;
			this.MessageTypesGroupBox.TabStop = false;
			// 
			// MessageTypesGrid
			// 
			this.MessageTypesGrid.AllowNavigation = false;
			this.MessageTypesGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.MessageTypesGrid, "ApplicationCodes.MessageTypes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ApplicationCodeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.PurgeSettings)(null)).ApplicationCodes)).SyncRoot)).MessageTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.MessageTypeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.ApplicationCodeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.PurgeSettings)(null)).ApplicationCodes)).SyncRoot)).MessageTypes)).SyncRoot)).MessageType_ForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.MessageTypeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.ApplicationCodeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.PurgeSettings)(null)).ApplicationCodes)).SyncRoot)).MessageTypes)).SyncRoot)).MessageTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.MessageTypeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.ApplicationCodeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.PurgeSettings)(null)).ApplicationCodes)).SyncRoot)).MessageTypes)).SyncRoot)).MessageSubType_ForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.MessageTypeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.ApplicationCodeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.PurgeSettings)(null)).ApplicationCodes)).SyncRoot)).MessageTypes)).SyncRoot)).MessageSubTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.MessageTypeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.ApplicationCodeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.PurgeSettings)(null)).ApplicationCodes)).SyncRoot)).MessageTypes)).SyncRoot)).PurgeTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.MessageTypeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.ApplicationCodeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.PurgeSettings)(null)).ApplicationCodes)).SyncRoot)).MessageTypes)).SyncRoot)).PurgeTimeUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.MessageTypeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.ApplicationCodeObj)(((System.Collections.IList)(((Enterprise.Registry.Business.PurgeSettings)(null)).ApplicationCodes)).SyncRoot)).MessageTypes)).SyncRoot)).Selected)));
			this.MessageTypesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("f2365d83-fc1e-4fc9-afb2-b9c308e9424c", "Message Type");
			zTextBoxColumnStyleInfo3.ColumnName = "MessageType_ForBinding";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("2fb6df59-67d2-46c4-8c80-3b6e38ebd96c", "Message Type Description");
			zTextBoxColumnStyleInfo4.ColumnName = "MessageTypeDescription";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("62fe3999-bb5e-42ea-87da-0da488082ed0", "Message Sub Type");
			zTextBoxColumnStyleInfo5.ColumnName = "MessageSubType_ForBinding";
			zTextBoxColumnStyleInfo5.IsMandatory = true;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("5ebf0efb-7c0f-4915-8385-0d75278891f4", "Message Sub Type Description");
			zTextBoxColumnStyleInfo6.ColumnName = "MessageSubTypeDescription";
			zTextBoxColumnStyleInfo6.IsMandatory = true;
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("36f28cfc-7885-4e09-b793-ce14cac4a636", "Time");
			zCalcEditColumnStyleInfo3.ColumnName = "PurgeTime";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Registry.GUI.Res.GetData("c6684548-18dc-4605-9a03-fb33360df836", "Purge Interval");
			zCalcEditColumnStyleInfo3.IsMandatory = true;
			zGuidDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("7d2d6b32-ba4b-41ad-aab0-468b9f1a102d", "Time Unit");
			zGuidDropEditColumnStyleInfo3.ColumnName = "PurgeTimeUnit";
			zGuidDropEditColumnStyleInfo3.GroupName = Enterprise.Registry.GUI.Res.GetData("c6684548-18dc-4605-9a03-fb33360df836", "Purge Interval");
			zGuidDropEditColumnStyleInfo3.IsMandatory = true;
			zGuidDropEditColumnStyleInfo3.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("c922095e-5d48-4d3c-8ad2-369be055a391", "Selected");
			zCheckBoxColumnStyleInfo3.ColumnName = "Selected";
			zCheckBoxColumnStyleInfo3.IsMandatory = true;
			this.MessageTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessageTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessageTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MessageTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.MessageTypesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.MessageTypesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo3);
			this.MessageTypesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.MessageTypesGrid.CopySelectedRowsAllowed = true;
			this.MessageTypesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTypesGrid.GridId = "92c1377f-98d9-4fca-bc9a-a478e675d529";
			this.MessageTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessageTypesGrid.LayoutKey = "MessageTypesGrid";
			this.MessageTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageTypesGrid.Name = "MessageTypesGrid";
			this.MessageTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 95, true);
			this.MessageTypesGrid.TabIndex = 0;
			//
			// PurgeMessageBatchSizeCalcEdit
			//
			this.BindingSource.SetBindingMember(this.PurgeMessageBatchSizeCalcEdit, "BatchSize");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((CargoWise.Types.ZInt)((Enterprise.Registry.Business.PurgeSettings)(null)).BatchSize);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PurgeMessageBatchSizeCalcEdit, false);
			this.PurgeMessageBatchSizeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 15, true);
			this.PurgeMessageBatchSizeCalcEdit.Name = "PurgeMessageBatchSizeCalcEdit";
			this.PurgeMessageBatchSizeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.PurgeMessageBatchSizeCalcEdit.TabIndex = 0;
			this.PurgeMessageBatchSizeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			this.PurgeMessageBatchSizeCalcEdit.DecimalPlaces = 0;
			//
			// PurgeMessageBatchSizeGroupBox
			//
			this.PurgeMessageBatchSizeGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b075d976-b6f5-48ca-8fe1-369b7cc293b4", "Batch Size");
			this.PurgeMessageBatchSizeGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			this.PurgeMessageBatchSizeGroupBox.Controls.Add(this.PurgeMessageBatchSizeCalcEdit);
			this.PurgeMessageBatchSizeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 304, true);
			this.PurgeMessageBatchSizeGroupBox.Name = "PurgeMessageBatchSizeGroupBox";
			this.PurgeMessageBatchSizeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 42, true);
			this.PurgeMessageBatchSizeGroupBox.TabIndex = 3;
			this.PurgeMessageBatchSizeGroupBox.TabStop = false;
			// 
			// PurgeSettingsControl
			//
			this.Controls.Add(this.InterchangesGroupBox);
			this.Controls.Add(this.MessageTypesGroupBox);
			this.Controls.Add(this.ApplicationCodesGroupBox);
			this.Controls.Add(this.PurgeMessageBatchSizeGroupBox);
			this.CaptionRenderingEnabled = true;
			this.Name = "PurgeSettingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(516, 525, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ApplicationCodesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ApplicationCodesGrid)).EndInit();
			this.InterchangesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.InterchangesGrid)).EndInit();
			this.MessageTypesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageTypesGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
