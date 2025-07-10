using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	partial class CusCAeMHHouseUserControl
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo3 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.HouseSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.HouseBillsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.HouseTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DetailsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.LatestNoticeProcessingDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LatestD4NoticeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HouseBillOverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsMasterHouseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CustomsStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DGInstructionsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HandlingInstructionsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReleaseSubLocationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ReleasePortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.VolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.B2BCommentsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MovementTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.WeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.AmendmentReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.HouseBillNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CCNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UCRTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PackLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ItemsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.cusCAeMHItemUserControl = new Enterprise.Customs.CA.GUI.CusCAeMHItemUserControl();
			this.AddressesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.cusCAeMHAddressesUserControl = new Enterprise.Customs.CA.GUI.CusCAeMHAddressesUserControl();
			this.HouseMessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.messagesUserControl = new Enterprise.Customs.CA.GUI.CAMessagesUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HouseSplitContainer)).BeginInit();
			this.HouseSplitContainer.Panel1.SuspendLayout();
			this.HouseSplitContainer.Panel2.SuspendLayout();
			this.HouseSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsGrid)).BeginInit();
			this.HouseBillsGrid.SuspendLayout();
			this.HouseTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DetailsSplitContainer)).BeginInit();
			this.DetailsSplitContainer.Panel1.SuspendLayout();
			this.DetailsSplitContainer.Panel2.SuspendLayout();
			this.DetailsSplitContainer.SuspendLayout();
			this.LatestNoticeProcessingDateDateEdit.SuspendLayout();
			this.CustomsStatusDropEdit.SuspendLayout();
			this.MessageStatusDropEdit.SuspendLayout();
			this.ReleaseSubLocationCodeFindBox.SuspendLayout();
			this.ReleasePortCodeFindBox.SuspendLayout();
			this.VolumeCalcDropEdit.SuspendLayout();
			this.MovementTypeDropEdit.SuspendLayout();
			this.WeightCalcDropEdit.SuspendLayout();
			this.AmendmentReasonDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackLinesGrid)).BeginInit();
			this.PackLinesGrid.SuspendLayout();
			this.ItemsTabPage.SuspendLayout();
			this.cusCAeMHItemUserControl.SuspendLayout();
			this.AddressesTabPage.SuspendLayout();
			this.cusCAeMHAddressesUserControl.SuspendLayout();
			this.HouseMessagesTabPage.SuspendLayout();
			this.messagesUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.CusCAeMHHouseCollection);
			// 
			// HouseSplitContainer
			// 
			this.HouseSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.HouseSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseSplitContainer.Name = "HouseSplitContainer";
			this.HouseSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// HouseSplitContainer.Panel1
			//
			this.HouseSplitContainer.Panel1.Controls.Add(this.HouseBillsGrid);
			// 
			// HouseSplitContainer.Panel2
			// 
			this.HouseSplitContainer.Panel2.AutoScroll = true;
			this.HouseSplitContainer.Panel2.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(908, 370, true);
			this.HouseSplitContainer.Panel2.Controls.Add(this.HouseTabControl);
			this.HouseSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(908, 465, true);
			this.HouseSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(91);
			this.HouseSplitContainer.TabIndex = 0;
			// 
			// HouseBillsGrid
			// 
			this.HouseBillsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.HouseBillsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_MessageReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_HouseCCN)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_HouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_CustomsStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_IsCloseReported)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_OverrideFreightDefaults)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_UCR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_MovementType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_AmendReasonCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_CBSAReleasePort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_CBSAReleaseSubLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_B2BComments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_HandlingInstructions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_DGSpecialInstructions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_IsMasterHouse)));
			this.HouseBillsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("88255bf7-f3c7-4ccc-878c-fad1382a0289", "Msg. Ref.", "Message Reference", "");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "BW_MessageReference";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("05f976f0-f21a-4515-8f7f-89585fda9d84", "CCN");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "BW_HouseCCN";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("cebb5904-d4da-49f0-88fa-81ac95df2b5d", "Bill No.");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "BW_HouseBill";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("885f8783-8ac1-4b66-ac68-9215d58cebbe", "Msg. Sta.", "Message Status", "");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "BW_MessageStatus";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("078d5b6e-3554-4d58-bf3f-5ed88f956710", "Status", "Customs Status", "");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "BW_CustomsStatus";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ee1ffb61-a79b-4cfd-9509-c193becf6aa7", "Is Close Reported?");
			zCheckBoxColumnStyleInfo1.ColumnName = "BW_IsCloseReported";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2e2c17d3-26e4-4178-88be-72866f5e443b", "Override");
			zCheckBoxColumnStyleInfo2.ColumnName = "BW_OverrideFreightDefaults";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f5376fbb-b92c-4dc2-acd3-50cc81f8bbec", "UCR");
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "BW_UCR";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("4fcf96ff-0f9e-4654-985b-e0fce9ecbc42", "Mov. Type");
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "BW_MovementType";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e12c7bda-599a-4530-848e-c35e8e9220ba", "Amend. Reason");
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "BW_AmendReasonCode";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3bcb41f9-2699-4404-baf5-1c592f6ead41", "Weight");
			zCalcEditColumnStyleInfo1.ColumnName = "BW_Weight";
			zCalcEditColumnStyleInfo1.Decimals = 3;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e45561b0-c1d7-4eb7-95d1-d96ab526cbc3", "UQ");
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "BW_WeightUQ";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3e42edb5-da57-455b-a20a-18cbf2495d88", "Volume");
			zCalcEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo2.ColumnName = "BW_Volume";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3e633551-022d-4a9d-a486-363b9dceee46", "Vol. Unit", "Volume Unit", "");
			zDropEditColumnStyleInfo6.ColumnName = "BW_VolumeUQ";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("58f3485a-b3b9-4312-8a29-3583ffa4ac97", "Port of Dest./Exit", "Port of Destination/Exit");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "BW_CBSAReleasePort";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1374a9a9-c6f4-4a7c-9405-7328a013add1", "Dest./Exit Sub-location", "Port of Destination/Exit Sub-location");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "BW_CBSAReleaseSubLocation";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("700bf809-172a-4254-95fc-546b37b9ab77", "B2B Comments");
			zMultiLineTextBoxColumnInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiLineTextBoxColumnInfo1.ColumnName = "BW_B2BComments";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zMultiLineTextBoxColumnInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b96d49af-dcb2-4065-82ed-f19450aabe71", "Handling Instr.", "Handling Instruction", "");
			zMultiLineTextBoxColumnInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiLineTextBoxColumnInfo2.ColumnName = "BW_HandlingInstructions";
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zMultiLineTextBoxColumnInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ca89c8b3-e56d-453e-be97-61fbef226c64", "DG Instr.", "DG Special Instruction", "");
			zMultiLineTextBoxColumnInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiLineTextBoxColumnInfo3.ColumnName = "BW_DGSpecialInstructions";
			zMultiLineTextBoxColumnInfo3.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6837210c-fb6f-4068-b5bf-03c5890d650a", "Consol?", "Is a Consolidation?", "");
			zCheckBoxColumnStyleInfo3.ColumnName = "BW_IsMasterHouse";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.HouseBillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.HouseBillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.HouseBillsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.HouseBillsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.HouseBillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.HouseBillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.HouseBillsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.HouseBillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.HouseBillsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.HouseBillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.HouseBillsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.HouseBillsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.HouseBillsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.HouseBillsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.HouseBillsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo3);
			this.HouseBillsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.HouseBillsGrid.CopySelectedRowsAllowed = true;
			this.HouseBillsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseBillsGrid.GridId = "7edffb24-8308-41ce-953f-33b45586b4b5";
			this.HouseBillsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HouseBillsGrid.LayoutKey = "HouseBillsGrid";
			this.HouseBillsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseBillsGrid.Name = "HouseBillsGrid";
			this.HouseBillsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(908, 91, true);
			this.HouseBillsGrid.TabIndex = 0;
			// 
			// HouseTabControl
			// 
			this.HouseTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.HouseTabControl.Controls.Add(this.DetailsTabPage);
			this.HouseTabControl.Controls.Add(this.ItemsTabPage);
			this.HouseTabControl.Controls.Add(this.AddressesTabPage);
			this.HouseTabControl.Controls.Add(this.HouseMessagesTabPage);
			this.HouseTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseTabControl.Name = "HouseTabControl";
			this.HouseTabControl.SelectedIndex = 0;
			this.HouseTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(908, 370, true);
			this.HouseTabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("01995086-e1ae-4c72-a540-42fe3f7dd947", "Details");
			this.DetailsTabPage.Controls.Add(this.DetailsSplitContainer);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 343, true);
			this.DetailsTabPage.TabIndex = 0;
			this.DetailsTabPage.UseVisualStyleBackColor = true;
			// 
			// DetailsSplitContainer
			// 
			this.DetailsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.DetailsSplitContainer.IsSplitterFixed = true;
			this.DetailsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DetailsSplitContainer.Name = "DetailsSplitContainer";
			// 
			// DetailsSplitContainer.Panel1
			// 
			this.DetailsSplitContainer.Panel1.Controls.Add(this.LatestNoticeProcessingDateDateEdit);
			this.DetailsSplitContainer.Panel1.Controls.Add(this.LatestD4NoticeTextBox);
			this.DetailsSplitContainer.Panel1.Controls.Add(this.HouseBillOverrideCheckBox);
			this.DetailsSplitContainer.Panel1.Controls.Add(this.IsMasterHouseCheckBox);
			this.DetailsSplitContainer.Panel1.Controls.Add(this.CustomsStatusDropEdit);
			this.DetailsSplitContainer.Panel1.Controls.Add(this.MessageStatusDropEdit);
			this.DetailsSplitContainer.Panel1.Controls.Add(this.DGInstructionsTextBox);
			this.DetailsSplitContainer.Panel1.Controls.Add(this.HandlingInstructionsTextBox);
			this.DetailsSplitContainer.Panel1.Controls.Add(this.ReleaseSubLocationCodeFindBox);
			this.DetailsSplitContainer.Panel1.Controls.Add(this.ReleasePortCodeFindBox);
			this.DetailsSplitContainer.Panel1.Controls.Add(this.VolumeCalcDropEdit);
			this.DetailsSplitContainer.Panel1.Controls.Add(this.B2BCommentsTextBox);
			this.DetailsSplitContainer.Panel1.Controls.Add(this.MovementTypeDropEdit);
			this.DetailsSplitContainer.Panel1.Controls.Add(this.WeightCalcDropEdit);
			this.DetailsSplitContainer.Panel1.Controls.Add(this.AmendmentReasonDropEdit);
			this.DetailsSplitContainer.Panel1.Controls.Add(this.HouseBillNumberTextBox);
			this.DetailsSplitContainer.Panel1.Controls.Add(this.CCNTextBox);
			this.DetailsSplitContainer.Panel1.Controls.Add(this.UCRTextBox);
			// 
			// DetailsSplitContainer.Panel2
			// 
			this.DetailsSplitContainer.Panel2.Controls.Add(this.PackLinesGrid);
			this.DetailsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(894, 337, true);
			this.DetailsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(715);
			this.DetailsSplitContainer.TabIndex = 4;
			// 
			// LatestNoticeProcessingDateDateEdit
			// 
			this.LatestNoticeProcessingDateDateEdit.AllowDrop = true;
			this.LatestNoticeProcessingDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.LatestNoticeProcessingDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LatestNoticeProcessingDateDateEdit, "BW_RNSProcessingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_RNSProcessingDate)));
			this.LatestNoticeProcessingDateDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("47181ec9-2627-402f-88f2-a7f39402cfec", "Processing Date");
			this.LatestNoticeProcessingDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LatestNoticeProcessingDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 52, true);
			this.LatestNoticeProcessingDateDateEdit.Name = "LatestNoticeProcessingDateDateEdit";
			this.LatestNoticeProcessingDateDateEdit.TabIndex = 4;
			// 
			// LatestD4NoticeTextBox
			// 
			this.BindingSource.SetBindingMember(this.LatestD4NoticeTextBox, "FormattedLatestD4MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).FormattedLatestD4MessageStatus)));
			this.LatestD4NoticeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("32d1e41d-2941-408c-8a2f-f44019d3bb87", "Latest D4 Notice");
			this.LatestD4NoticeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LatestD4NoticeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 52, true);
			this.LatestD4NoticeTextBox.Name = "LatestD4NoticeTextBox";
			this.LatestD4NoticeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.LatestD4NoticeTextBox.TabIndex = 3;
			// 
			// HouseBillOverrideCheckBox
			// 
			this.HouseBillOverrideCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HouseBillOverrideCheckBox, "BW_OverrideFreightDefaults");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_OverrideFreightDefaults)));
			this.HouseBillOverrideCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b6c94025-86a9-4eb0-8bf3-13ef6fbc5876", "Override");
			this.HouseBillOverrideCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HouseBillOverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 3, true);
			this.HouseBillOverrideCheckBox.Name = "HouseBillOverrideCheckBox";
			this.HouseBillOverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 17, true);
			this.HouseBillOverrideCheckBox.TabIndex = 0;
			this.HouseBillOverrideCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsMasterHouseCheckBox
			// 
			this.IsMasterHouseCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsMasterHouseCheckBox, "BW_IsMasterHouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_IsMasterHouse)));
			this.IsMasterHouseCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8c3d99d6-afdc-46ea-b7b7-373d8e2c085b", "Is a Consolidation?");
			this.IsMasterHouseCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsMasterHouseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 184, true);
			this.IsMasterHouseCheckBox.Name = "IsMasterHouseCheckBox";
			this.IsMasterHouseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 17, true);
			this.IsMasterHouseCheckBox.TabIndex = 14;
			this.IsMasterHouseCheckBox.UseVisualStyleBackColor = true;
			// 
			// CustomsStatusDropEdit
			// 
			this.CustomsStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsStatusDropEdit, "BW_CustomsStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_CustomsStatus)));
			this.CustomsStatusDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9a8dee10-2119-4631-adf1-ad7d2268eb69", "Customs Status");
			this.CustomsStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 26, true);
			this.CustomsStatusDropEdit.Name = "CustomsStatusDropEdit";
			this.CustomsStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CustomsStatusDropEdit.TabIndex = 2;
			this.CustomsStatusDropEdit.TabStop = false;
			// 
			// MessageStatusDropEdit
			// 
			this.MessageStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "BW_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_MessageStatus)));
			this.MessageStatusDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d002cd8e-0864-4ab8-96e2-6601975ef107", "Mess. Status", "Message Status", "");
			this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 26, true);
			this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
			this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.MessageStatusDropEdit.TabIndex = 1;
			this.MessageStatusDropEdit.TabStop = false;
			// 
			// DGInstructionsTextBox
			// 
			this.BindingSource.SetBindingMember(this.DGInstructionsTextBox, "BW_DGSpecialInstructions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_DGSpecialInstructions)));
			this.DGInstructionsTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("85b4f30b-3387-484e-9266-6b9cfce06040", "DG Special Instructions");
			this.DGInstructionsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 208, true);
			this.DGInstructionsTextBox.Multiline = true;
			this.DGInstructionsTextBox.Name = "DGInstructionsTextBox";
			this.DGInstructionsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(283, 58, true);
			this.DGInstructionsTextBox.TabIndex = 16;
			// 
			// HandlingInstructionsTextBox
			// 
			this.BindingSource.SetBindingMember(this.HandlingInstructionsTextBox, "BW_HandlingInstructions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_HandlingInstructions)));
			this.HandlingInstructionsTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c086359e-0c6b-4dbe-b789-ece368a3395e", "Special Instructions");
			this.HandlingInstructionsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 208, true);
			this.HandlingInstructionsTextBox.Multiline = true;
			this.HandlingInstructionsTextBox.Name = "HandlingInstructionsTextBox";
			this.HandlingInstructionsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 58, true);
			this.HandlingInstructionsTextBox.TabIndex = 15;
			// 
			// ReleaseSubLocationCodeFindBox
			// 
			this.ReleaseSubLocationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReleaseSubLocationCodeFindBox, "BW_CBSAReleaseSubLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_CBSAReleaseSubLocation)));
			this.ReleaseSubLocationCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("cf5a2767-4187-4e13-8ab5-9bd8a3546ed6", "Dest./Exit Sub-location", "Port of Destination/Exit Sub-location");
			this.ReleaseSubLocationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 156, true);
			this.ReleaseSubLocationCodeFindBox.Name = "ReleaseSubLocationCodeFindBox";
			this.ReleaseSubLocationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(283, 20, true);
			this.ReleaseSubLocationCodeFindBox.TabIndex = 12;
			// 
			// ReleasePortCodeFindBox
			// 
			this.ReleasePortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReleasePortCodeFindBox, "BW_CBSAReleasePort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_CBSAReleasePort)));
			this.ReleasePortCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("bf48756c-b495-41b6-b952-026ff5c64919", "Port of Dest./Exit", "Port of Destination/Exit");
			this.ReleasePortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 130, true);
			this.ReleasePortCodeFindBox.Name = "ReleasePortCodeFindBox";
			this.ReleasePortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(283, 20, true);
			this.ReleasePortCodeFindBox.TabIndex = 10;
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_VolumeUQ)));
			this.VolumeCalcDropEdit.BindToAmount = "BW_Volume";
			this.VolumeCalcDropEdit.BindToUnit = "BW_VolumeUQ";
			this.VolumeCalcDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c3eb2f0c-2374-4bdd-b925-24b164effeb4", "Volume");
			this.VolumeCalcDropEdit.Decimals = 0;
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 182, true);
			this.VolumeCalcDropEdit.Name = "VolumeCalcDropEdit";
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 20, true);
			this.VolumeCalcDropEdit.TabIndex = 13;
			// 
			// B2BCommentsTextBox
			// 
			this.BindingSource.SetBindingMember(this.B2BCommentsTextBox, "BW_B2BComments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_B2BComments)));
			this.B2BCommentsTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("83bbf95a-af6d-43bc-bc5f-3ff42260b564", "B to B Comments");
			this.B2BCommentsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 272, true);
			this.B2BCommentsTextBox.Multiline = true;
			this.B2BCommentsTextBox.Name = "B2BCommentsTextBox";
			this.B2BCommentsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(614, 50, true);
			this.B2BCommentsTextBox.TabIndex = 17;
			// 
			// MovementTypeDropEdit
			// 
			this.MovementTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MovementTypeDropEdit, "BW_MovementType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_MovementType)));
			this.MovementTypeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("dccb05b4-8d14-4449-b79e-7c2a1431e59c", "Movement Type");
			this.MovementTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 78, true);
			this.MovementTypeDropEdit.Name = "MovementTypeDropEdit";
			this.MovementTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(283, 20, true);
			this.MovementTypeDropEdit.TabIndex = 6;
			// 
			// WeightCalcDropEdit
			// 
			this.WeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_WeightUQ)));
			this.WeightCalcDropEdit.BindToAmount = "BW_Weight";
			this.WeightCalcDropEdit.BindToUnit = "BW_WeightUQ";
			this.WeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("45a3e75e-cb4c-4fca-8837-0f326fd1ee64", "Weight");
			this.WeightCalcDropEdit.Decimals = 3;
			this.WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 156, true);
			this.WeightCalcDropEdit.Name = "WeightCalcDropEdit";
			this.WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 20, true);
			this.WeightCalcDropEdit.TabIndex = 11;
			// 
			// AmendmentReasonDropEdit
			// 
			this.AmendmentReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AmendmentReasonDropEdit, "BW_AmendReasonCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_AmendReasonCode)));
			this.AmendmentReasonDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8c72a85c-915c-4f72-8402-10c8aba43777", "Amendment Reason");
			this.AmendmentReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 104, true);
			this.AmendmentReasonDropEdit.Name = "AmendmentReasonDropEdit";
			this.AmendmentReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(283, 20, true);
			this.AmendmentReasonDropEdit.TabIndex = 8;
			// 
			// HouseBillNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.HouseBillNumberTextBox, "BW_HouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_HouseBill)));
			this.HouseBillNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("145d022a-cd7b-42f1-9e58-1230a303fddd", "Bill No.", "House Bill No.", "");
			this.HouseBillNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 78, true);
			this.HouseBillNumberTextBox.Name = "HouseBillNumberTextBox";
			this.HouseBillNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			this.HouseBillNumberTextBox.TabIndex = 5;
			// 
			// CCNTextBox
			// 
			this.BindingSource.SetBindingMember(this.CCNTextBox, "BW_HouseCCN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_HouseCCN)));
			this.CCNTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("dea51d36-c43e-4ad8-b6d5-07ec0a8a30f6", "CCN");
			this.CCNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 104, true);
			this.CCNTextBox.Name = "CCNTextBox";
			this.CCNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CCNTextBox.TabIndex = 7;
			// 
			// UCRTextBox
			// 
			this.BindingSource.SetBindingMember(this.UCRTextBox, "BW_UCR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).BW_UCR)));
			this.UCRTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2e0a4e2b-289b-4f8f-ab51-3005bdfbd9a0", "UCR");
			this.UCRTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 130, true);
			this.UCRTextBox.Name = "UCRTextBox";
			this.UCRTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			this.UCRTextBox.TabIndex = 9;
			// 
			// PackLinesGrid
			// 
			this.PackLinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackLinesGrid, "Pivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).Pivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.CusCAeMHHouseContainerPivot)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).Pivots)).SyncRoot)).BPA_BQ_Container)));
			this.PackLinesGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f42672a8-19d0-4fd1-b425-7960162382d1", "Container");
			zGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo1.ColumnName = "BPA_BQ_Container";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PackLinesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.PackLinesGrid.CopySelectedRowsAllowed = true;
			this.PackLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackLinesGrid.GridId = "cd0e6bd0-f574-4ecf-acd3-3f73414b6d94";
			this.PackLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackLinesGrid.LayoutKey = "PackLinesGrid";
			this.PackLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackLinesGrid.Name = "PackLinesGrid";
			this.PackLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 337, true);
			this.PackLinesGrid.TabIndex = 0;
			// 
			// ItemsTabPage
			// 
			this.ItemsTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("dfbc80aa-35ad-4994-b068-d873a054ace6", "Pack Line Items");
			this.ItemsTabPage.Controls.Add(this.cusCAeMHItemUserControl);
			this.ItemsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ItemsTabPage.Name = "ItemsTabPage";
			this.ItemsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ItemsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 343, true);
			this.ItemsTabPage.TabIndex = 1;
			this.ItemsTabPage.UseVisualStyleBackColor = true;
			// 
			// cusCAeMHItemUserControl
			// 
			this.cusCAeMHItemUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cusCAeMHItemUserControl, "Items");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.CusCAeMHItemCollection)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).Items)));
			this.cusCAeMHItemUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cusCAeMHItemUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.cusCAeMHItemUserControl.Name = "cusCAeMHItemUserControl";
			this.cusCAeMHItemUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(894, 337, true);
			this.cusCAeMHItemUserControl.TabIndex = 0;
			// 
			// AddressesTabPage
			// 
			this.AddressesTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8e3d5015-0d72-48f3-b58d-f8fb195d1f02", "Addresses");
			this.AddressesTabPage.Controls.Add(this.cusCAeMHAddressesUserControl);
			this.AddressesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AddressesTabPage.Name = "AddressesTabPage";
			this.AddressesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AddressesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 343, true);
			this.AddressesTabPage.TabIndex = 2;
			this.AddressesTabPage.UseVisualStyleBackColor = true;
			// 
			// cusCAeMHAddressesUserControl
			// 
			this.cusCAeMHAddressesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cusCAeMHAddressesUserControl, "DocAddresses");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.CAeMHDocAddressDependentCollection)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)).DocAddresses)));
			this.cusCAeMHAddressesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cusCAeMHAddressesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.cusCAeMHAddressesUserControl.Name = "cusCAeMHAddressesUserControl";
			this.cusCAeMHAddressesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(894, 337, true);
			this.cusCAeMHAddressesUserControl.TabIndex = 0;
			// 
			// HouseMessagesTabPage
			// 
			this.HouseMessagesTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f8585882-229d-4d2b-bb75-99fa1a3968cd", "House Messages");
			this.HouseMessagesTabPage.Controls.Add(this.messagesUserControl);
			this.HouseMessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HouseMessagesTabPage.Name = "HouseMessagesTabPage";
			this.HouseMessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.HouseMessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 343, true);
			this.HouseMessagesTabPage.TabIndex = 3;
			this.HouseMessagesTabPage.UseVisualStyleBackColor = true;
			// 
			// messagesUserControl
			// 
			this.messagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.messagesUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(((Enterprise.Customs.CA.Business.CusCAeMHHouse)(null)))));
			this.messagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.messagesUserControl.Name = "messagesUserControl";
			this.messagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(894, 337, true);
			this.messagesUserControl.TabIndex = 0;
			// 
			// CusCAeMHHouseUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b3fb1486-485f-472a-b143-277c18f1800b", "House Bill No.");
			this.Controls.Add(this.HouseSplitContainer);
			this.Name = "CusCAeMHHouseUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(908, 465, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.HouseSplitContainer.Panel1.ResumeLayout(false);
			this.HouseSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.HouseSplitContainer)).EndInit();
			this.HouseSplitContainer.ResumeLayout(false);
			this.HouseSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsGrid)).EndInit();
			this.HouseBillsGrid.ResumeLayout(false);
			this.HouseBillsGrid.PerformLayout();
			this.HouseTabControl.ResumeLayout(false);
			this.HouseTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.DetailsSplitContainer.Panel1.ResumeLayout(false);
			this.DetailsSplitContainer.Panel1.PerformLayout();
			this.DetailsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.DetailsSplitContainer)).EndInit();
			this.DetailsSplitContainer.ResumeLayout(false);
			this.DetailsSplitContainer.PerformLayout();
			this.LatestNoticeProcessingDateDateEdit.ResumeLayout(true);
			this.LatestNoticeProcessingDateDateEdit.PerformLayout();
			this.CustomsStatusDropEdit.ResumeLayout(true);
			this.CustomsStatusDropEdit.PerformLayout();
			this.MessageStatusDropEdit.ResumeLayout(true);
			this.MessageStatusDropEdit.PerformLayout();
			this.ReleaseSubLocationCodeFindBox.ResumeLayout(true);
			this.ReleaseSubLocationCodeFindBox.PerformLayout();
			this.ReleasePortCodeFindBox.ResumeLayout(true);
			this.ReleasePortCodeFindBox.PerformLayout();
			this.VolumeCalcDropEdit.ResumeLayout(true);
			this.VolumeCalcDropEdit.PerformLayout();
			this.MovementTypeDropEdit.ResumeLayout(true);
			this.MovementTypeDropEdit.PerformLayout();
			this.WeightCalcDropEdit.ResumeLayout(true);
			this.WeightCalcDropEdit.PerformLayout();
			this.AmendmentReasonDropEdit.ResumeLayout(true);
			this.AmendmentReasonDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackLinesGrid)).EndInit();
			this.PackLinesGrid.ResumeLayout(false);
			this.PackLinesGrid.PerformLayout();
			this.ItemsTabPage.ResumeLayout(false);
			this.ItemsTabPage.PerformLayout();
			this.cusCAeMHItemUserControl.ResumeLayout(true);
			this.cusCAeMHItemUserControl.PerformLayout();
			this.AddressesTabPage.ResumeLayout(false);
			this.AddressesTabPage.PerformLayout();
			this.cusCAeMHAddressesUserControl.ResumeLayout(true);
			this.cusCAeMHAddressesUserControl.PerformLayout();
			this.HouseMessagesTabPage.ResumeLayout(false);
			this.HouseMessagesTabPage.PerformLayout();
			this.messagesUserControl.ResumeLayout(true);
			this.messagesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer HouseSplitContainer;
		internal ZArchitecture.ZGrid HouseBillsGrid;
		private ZArchitecture.GUI.ZTabPage AddressesTabPage;
		private ZArchitecture.GUI.ZTabControl HouseTabControl;
		private ZArchitecture.GUI.ZTabPage DetailsTabPage;
		private CargoWise.Windows.UI.KSplitContainer DetailsSplitContainer;
		private ZArchitecture.ZTextBox B2BCommentsTextBox;
		private ZArchitecture.GUI.ZDropEdit MovementTypeDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit WeightCalcDropEdit;
		private ZArchitecture.GUI.ZDropEdit AmendmentReasonDropEdit;
		private ZArchitecture.ZTextBox HouseBillNumberTextBox;
		private ZArchitecture.ZTextBox CCNTextBox;
		private ZArchitecture.ZTextBox UCRTextBox;
		private ZArchitecture.ZGrid PackLinesGrid;
		private ZArchitecture.GUI.ZTabPage ItemsTabPage;
		private CusCAeMHItemUserControl cusCAeMHItemUserControl;
		private CusCAeMHAddressesUserControl cusCAeMHAddressesUserControl;
		private ZArchitecture.GUI.ZCodeFindBox ReleaseSubLocationCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox ReleasePortCodeFindBox;
		private ZArchitecture.GUI.ZCalcDropEdit VolumeCalcDropEdit;
		private ZArchitecture.ZTextBox DGInstructionsTextBox;
		private ZArchitecture.ZTextBox HandlingInstructionsTextBox;
		private ZArchitecture.GUI.ZTabPage HouseMessagesTabPage;
		private CAMessagesUserControl messagesUserControl;
		private ZArchitecture.GUI.ZDropEdit CustomsStatusDropEdit;
		private ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
		private ZArchitecture.GUI.ZCheckBox IsMasterHouseCheckBox;
		internal ZArchitecture.GUI.ZCheckBox HouseBillOverrideCheckBox;
		private ZArchitecture.ZTextBox LatestD4NoticeTextBox;
		private ZArchitecture.GUI.ZDateEdit LatestNoticeProcessingDateDateEdit;
	}
}
