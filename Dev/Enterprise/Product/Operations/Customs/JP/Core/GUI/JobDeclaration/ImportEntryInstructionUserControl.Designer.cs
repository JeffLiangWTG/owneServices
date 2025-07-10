using System.Windows.Forms;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	partial class ImportEntryInstructionUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo3 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo11 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo12 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo13 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo14 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo15 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo16 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo17 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo18 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo19 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DetailsLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.CertificateLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.EntryInstructionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CertificateTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.NotesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MarksAndNumbersTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotesForOwnerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotesForBrokerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotesForCustomsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotesForCustomsOverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GuaranteesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GuaranteesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EntryInstructionsGrid)).BeginInit();
			this.EntryInstructionsGrid.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.CertificateTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.GuaranteesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GuaranteesGrid)).BeginInit();
			this.GuaranteesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.JobDeclaration);
			// 
			// DetailsLayoutPanel
			// 
			this.DetailsLayoutPanel.AllowDrop = true;
			this.DetailsLayoutPanel.AutoScroll = true;
			this.DetailsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DetailsLayoutPanel.Name = "DetailsLayoutPanel";
			this.DetailsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1035, 468, true);
			this.DetailsLayoutPanel.TabIndex = 0;
			// 
			// CertificateLayoutPanel
			// 
			this.CertificateLayoutPanel.AllowDrop = true;
			this.CertificateLayoutPanel.AutoScroll = true;
			this.CertificateLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CertificateLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CertificateLayoutPanel.Name = "CertificateLayoutPanel";
			this.CertificateLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1041, 475, true);
			this.CertificateLayoutPanel.TabIndex = 0;
			// 
			// EntryInstructionsGrid
			// 
			this.EntryInstructionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryInstructionsGrid, "CustomsEntryInstructions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_DisplaySequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_ValueType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_Style)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_SubStyle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_CustomsInspectionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).TradeTypeFirstChar)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).TradeTypeSecondChar)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).TradeTypeThirdChar)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).JP_CustomsNotes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).JP_BrokersNotes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).JP_OwnersNotes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_DutyDrawback)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_CommercialValueType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_ContentInspectionResult)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_BeforePermitApplicationReason)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_CommonControlNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_FoodHygieneCertificateType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_PlantProtectionCertificateType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_AnimalQuarantineCertificateType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_DateForDuty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_DeclarationCondition)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DeclarationConditionDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_CustomsOfficeForSpecialDeclarations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_CustomsOfficeDepartmentForSpecialDeclarations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_BondedLocationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_BondedLocationName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_GrossWeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).Lookups.GrossWeightUnitList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_CustomsWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_CustomsWeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_ContainerCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_DeclarationCargoType)));
			this.EntryInstructionsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CEI_DisplaySequence";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zDropEditColumnStyleInfo1.ColumnName = "CEI_ValueType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "CEI_Style";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "CEI_SubStyle";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "CEI_CustomsInspectionCode";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo4.ColumnName = "TradeTypeFirstChar";
			zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo4.GroupName = Enterprise.Customs.JP.GUI.Res.GetData("5DE76E76-95C3-4AAC-9221-174E40BCFFE8", "Trade Type");
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo5.ColumnName = "TradeTypeSecondChar";
			zDropEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo5.GroupName = Enterprise.Customs.JP.GUI.Res.GetData("5DE76E76-95C3-4AAC-9221-174E40BCFFE8", "Trade Type");
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo6.ColumnName = "TradeTypeThirdChar";
			zDropEditColumnStyleInfo6.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo6.GroupName = Enterprise.Customs.JP.GUI.Res.GetData("5DE76E76-95C3-4AAC-9221-174E40BCFFE8", "Trade Type");
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiLineTextBoxColumnInfo1.ColumnName = "JP_CustomsNotes";
			zMultiLineTextBoxColumnInfo1.DefaultCollectionIndex = 0;
			zMultiLineTextBoxColumnInfo1.IsVisible = false;
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiLineTextBoxColumnInfo2.ColumnName = "JP_BrokersNotes";
			zMultiLineTextBoxColumnInfo2.DefaultCollectionIndex = 0;
			zMultiLineTextBoxColumnInfo2.IsVisible = false;
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiLineTextBoxColumnInfo3.ColumnName = "JP_OwnersNotes";
			zMultiLineTextBoxColumnInfo3.DefaultCollectionIndex = 0;
			zMultiLineTextBoxColumnInfo3.IsVisible = false;
			zMultiLineTextBoxColumnInfo3.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo7.ColumnName = "CEI_DutyDrawback";
			zDropEditColumnStyleInfo7.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo8.ColumnName = "CEI_CommercialValueType";
			zDropEditColumnStyleInfo8.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo9.ColumnName = "CEI_ContentInspectionResult";
			zDropEditColumnStyleInfo9.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo10.ColumnName = "CEI_BeforePermitApplicationReason";
			zDropEditColumnStyleInfo10.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.ColumnName = "CEI_CommonControlNumber";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo11.ColumnName = "CEI_FoodHygieneCertificateType";
			zDropEditColumnStyleInfo11.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo12.ColumnName = "CEI_PlantProtectionCertificateType";
			zDropEditColumnStyleInfo12.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo13.ColumnName = "CEI_AnimalQuarantineCertificateType";
			zDropEditColumnStyleInfo13.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.ColumnName = "CEI_DateForDuty";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo14.ColumnName = "CEI_DeclarationCondition";
			zDropEditColumnStyleInfo14.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo14.GroupName = Enterprise.Customs.JP.GUI.Res.GetData("9f2c9ef4-9e8b-42c9-95b1-7dc4053cee4b", "Declaration Condition");
			zDropEditColumnStyleInfo14.IsVisible = false;
			zDropEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "DeclarationConditionDescription";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.GroupName = Enterprise.Customs.JP.GUI.Res.GetData("9f2c9ef4-9e8b-42c9-95b1-7dc4053cee4b", "Declaration Condition");
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo15.ColumnName = "CEI_CustomsOfficeForSpecialDeclarations";
			zDropEditColumnStyleInfo15.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo15.GroupName = Enterprise.Customs.JP.GUI.Res.GetData("9F76FEEB-6ED3-45B0-A72E-F963ADE0F6B2", "Special Declaration Office");
			zDropEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo16.ColumnName = "CEI_CustomsOfficeDepartmentForSpecialDeclarations";
			zDropEditColumnStyleInfo16.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo16.GroupName = Enterprise.Customs.JP.GUI.Res.GetData("9F76FEEB-6ED3-45B0-A72E-F963ADE0F6B2", "Special Declaration Office");
			zDropEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CEI_BondedLocationCode";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.JP.GUI.Res.GetData("FDCC20DC-B8B7-4431-A107-21DADE4CFCE6", "Bonded Location");
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.ColumnName = "CEI_BondedLocationName";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.GroupName = Enterprise.Customs.JP.GUI.Res.GetData("FDCC20DC-B8B7-4431-A107-21DADE4CFCE6", "Bonded Location");
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CEI_GrossWeight";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.JP.GUI.Res.GetData("CCD4C26C-E6C2-47D3-BE02-BA4846ECA52A", "Gross Weight");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo17.BindToList = "Lookups.GrossWeightUnitList";
			zDropEditColumnStyleInfo17.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("B2E8140B-1709-40C5-A2AB-38F3742D1B9D", "UQ");
			zDropEditColumnStyleInfo17.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo17.ColumnName = "CEI_GrossWeightUnit";
			zDropEditColumnStyleInfo17.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo17.GroupName = Enterprise.Customs.JP.GUI.Res.GetData("CCD4C26C-E6C2-47D3-BE02-BA4846ECA52A", "Gross Weight");
			zDropEditColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CEI_CustomsWeight";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.JP.GUI.Res.GetData("fa3ee9fa-5cea-4d12-a17c-813a21999597", "Customs Weight");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo18.ColumnName = "CEI_CustomsWeightUnit";
			zDropEditColumnStyleInfo18.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo18.GroupName = Enterprise.Customs.JP.GUI.Res.GetData("fa3ee9fa-5cea-4d12-a17c-813a21999597", "Customs Weight");
			zDropEditColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(33);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "CEI_ContainerCount";
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			zDropEditColumnStyleInfo19.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo19.ColumnName = "CEI_DeclarationCargoType";
			zDropEditColumnStyleInfo19.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			this.EntryInstructionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.EntryInstructionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.EntryInstructionsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.EntryInstructionsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo3);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.EntryInstructionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo11);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo12);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo13);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo14);
			this.EntryInstructionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo15);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo16);
			this.EntryInstructionsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.EntryInstructionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo17);
			this.EntryInstructionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo18);
			this.EntryInstructionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo19);
			this.EntryInstructionsGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.EntryInstructionsGrid.GridId = "bb235660-1ca0-4884-a9f7-e89cc053e69c";
			this.EntryInstructionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryInstructionsGrid.LayoutKey = "EntryInstructionsGrid";
			this.EntryInstructionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryInstructionsGrid.Name = "EntryInstructionsGrid";
			this.EntryInstructionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 205, true);
			this.EntryInstructionsGrid.TabIndex = 0;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.DetailsTabPage);
			this.MainTabControl.Controls.Add(this.CertificateTabPage);
			this.MainTabControl.Controls.Add(this.NotesTabPage);
			this.MainTabControl.Controls.Add(this.GuaranteesTabPage);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 205, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 500, true);
			this.MainTabControl.TabIndex = 4;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.BackColor = System.Drawing.Color.Transparent;
			this.DetailsTabPage.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("DA100B60-26A1-42DE-BAD9-C4B340E94D92", "Details");
			this.DetailsTabPage.Controls.Add(this.DetailsLayoutPanel);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1041, 475, true);
			this.DetailsTabPage.TabIndex = 0;
			// 
			// CertificateTabPage
			// 
			this.CertificateTabPage.AutoScroll = true;
			this.CertificateTabPage.BackColor = System.Drawing.Color.Transparent;
			this.CertificateTabPage.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("8A4A8567-D197-49C1-BD47-36F4ADC5EF66", "Other Laws and Certificates");
			this.CertificateTabPage.Controls.Add(this.CertificateLayoutPanel);
			this.CertificateTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.CertificateTabPage.Name = "CertificateTabPage";
			this.CertificateTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1041, 475, true);
			this.CertificateTabPage.TabIndex = 2;
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.BackColor = System.Drawing.Color.Transparent;
			this.NotesTabPage.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("A256E7C0-5294-4BBB-8C58-4E5F3471EBD1", "Notes");
			this.NotesTabPage.Controls.Add(this.MarksAndNumbersTextBox);
			this.NotesTabPage.Controls.Add(this.NotesForOwnerTextBox);
			this.NotesTabPage.Controls.Add(this.NotesForBrokerTextBox);
			this.NotesTabPage.Controls.Add(this.NotesForCustomsTextBox);
			this.NotesTabPage.Controls.Add(this.NotesForCustomsOverrideCheckBox);
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.NotesTabPage.Name = "NotesTabPage";
			this.NotesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1041, 475, true);
			this.NotesTabPage.TabIndex = 3;
			// 
			// MarksAndNumbersTextBox
			// 
			this.BindingSource.SetBindingMember(this.MarksAndNumbersTextBox, "CustomsEntryInstructions.JP_MarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).JP_MarksAndNumbers)));
			this.MarksAndNumbersTextBox.ImeMode = System.Windows.Forms.ImeMode.Disable;
			this.MarksAndNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 130, true);
			this.MarksAndNumbersTextBox.Multiline = true;
			this.MarksAndNumbersTextBox.Name = "MarksAndNumbersTextBox";
			this.MarksAndNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 48, true);
			this.MarksAndNumbersTextBox.TabIndex = 6;
			// 
			// NotesForOwnerTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotesForOwnerTextBox, "CustomsEntryInstructions.JP_OwnersNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).JP_OwnersNotes)));
			this.NotesForOwnerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 107, true);
			this.NotesForOwnerTextBox.Name = "NotesForOwnerTextBox";
			this.NotesForOwnerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 16, true);
			this.NotesForOwnerTextBox.TabIndex = 3;
			// 
			// NotesForBrokerTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotesForBrokerTextBox, "CustomsEntryInstructions.JP_BrokersNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).JP_BrokersNotes)));
			this.NotesForBrokerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 85, true);
			this.NotesForBrokerTextBox.Name = "NotesForBrokerTextBox";
			this.NotesForBrokerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 16, true);
			this.NotesForBrokerTextBox.TabIndex = 2;
			// 
			// NotesForCustomsTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotesForCustomsTextBox, "CustomsEntryInstructions.JP_CustomsNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).JP_CustomsNotes)));
			this.NotesForCustomsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 13, true);
			this.NotesForCustomsTextBox.Multiline = true;
			this.NotesForCustomsTextBox.Name = "NotesForCustomsTextBox";
			this.NotesForCustomsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 68, true);
			this.NotesForCustomsTextBox.TabIndex = 1;
			// 
			// NotesForCustomsOverrideCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NotesForCustomsOverrideCheckBox, "CustomsEntryInstructions.JP_CustomsNotes_Override");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).JP_CustomsNotes_Override)));
			this.NotesForCustomsOverrideCheckBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("388D3475-7952-44B1-8BFF-7266E98E757E", "Override");
			this.NotesForCustomsOverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(567, 13, true);
			this.NotesForCustomsOverrideCheckBox.Name = "NotesForCustomsOverrideCheckBox";
			this.NotesForCustomsOverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 25, true);
			this.NotesForCustomsOverrideCheckBox.TabIndex = 1;
			this.NotesForCustomsOverrideCheckBox.Visible = false;
			// 
			// GuaranteesTabPage
			// 
			this.GuaranteesTabPage.BackColor = System.Drawing.Color.Transparent;
			this.GuaranteesTabPage.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("03A08F1F-3572-4AD5-BDA2-48FF43D2FEDD", "Guarantee");
			this.GuaranteesTabPage.Controls.Add(this.GuaranteesGrid);
			this.GuaranteesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.GuaranteesTabPage.Name = "GuaranteesTabPage";
			this.GuaranteesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.GuaranteesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1041, 475, true);
			this.GuaranteesTabPage.TabIndex = 4;
			// 
			// GuaranteesGrid
			// 
			this.GuaranteesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.GuaranteesGrid, "CustomsEntryInstructions.Guarantees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).Guarantees)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusGuaranteeReference)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).Guarantees)).SyncRoot)).CFR_Reference)));
			this.GuaranteesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo6.ColumnName = "CFR_Reference";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.GuaranteesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.GuaranteesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GuaranteesGrid.GridId = "42111B2C-545C-42B4-8F79-EC307AD44FA3";
			this.GuaranteesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GuaranteesGrid.LayoutKey = "GuaranteesGrid";
			this.GuaranteesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.GuaranteesGrid.Name = "GuaranteesGrid";
			this.GuaranteesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1035, 468, true);
			this.GuaranteesGrid.TabIndex = 0;
			// 
			// ImportEntryInstructionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.EntryInstructionsGrid);
			this.Name = "ImportEntryInstructionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 511, true);
			this.Controls.SetChildIndex(this.EntryInstructionsGrid, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.EntryInstructionsGrid)).EndInit();
			this.EntryInstructionsGrid.ResumeLayout(false);
			this.EntryInstructionsGrid.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.CertificateTabPage.ResumeLayout(false);
			this.CertificateTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.GuaranteesTabPage.ResumeLayout(false);
			this.GuaranteesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.GuaranteesGrid)).EndInit();
			this.GuaranteesGrid.ResumeLayout(false);
			this.GuaranteesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZGrid EntryInstructionsGrid;
		ZGrid GuaranteesGrid;
		public ZTabControl MainTabControl;
		ZTabPage DetailsTabPage;
		ZTabPage CertificateTabPage;
		ZTabPage NotesTabPage;
		ZTabPage GuaranteesTabPage;
		ZTextBox NotesForCustomsTextBox;
		ZCheckBox NotesForCustomsOverrideCheckBox;
		ZTextBox NotesForOwnerTextBox;
		ZTextBox NotesForBrokerTextBox;
		ZTextBox MarksAndNumbersTextBox;
		DynamicLayoutPanel DetailsLayoutPanel;
		DynamicLayoutPanel CertificateLayoutPanel;
	}
}
