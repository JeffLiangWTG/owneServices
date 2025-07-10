using Enterprise.Customs.CH.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

partial class EntryInstructionDetailsUserControl
{
    #region Component Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.EntryInstructionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.EntryInstructionTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.EntryInstructionDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DetailsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.PreviousDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PreviousDocumentsUserControl = new Enterprise.Customs.CH.GUI.PlugIn.PreviousDocumentsUserControl();
			this.SupportingDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SupportingDocumentsUserControl = new Enterprise.Customs.CH.GUI.SupportingDocumentsUserControl();
			this.TransportDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TransportDocumentsUserControl = new Enterprise.Customs.CH.GUI.TransportDocumentsUserControl();
			this.CusSupplyChainActorsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CusSupplyChainActorsUserControl = new Enterprise.Customs.CH.GUI.CusSupplyChainActorsUserControl();
			this.AdditionalInformationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdditionalInformationUserControl = new Enterprise.Customs.CH.GUI.AdditionalInformationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EntryInstructionsGrid)).BeginInit();
			this.EntryInstructionsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.EntryInstructionTabControl.SuspendLayout();
			this.EntryInstructionDetailsTabPage.SuspendLayout();
			this.PreviousDocumentsTabPage.SuspendLayout();
			this.PreviousDocumentsUserControl.SuspendLayout();
			this.SupportingDocumentsTabPage.SuspendLayout();
			this.SupportingDocumentsUserControl.SuspendLayout();
			this.TransportDocumentsTabPage.SuspendLayout();
			this.TransportDocumentsUserControl.SuspendLayout();
			this.CusSupplyChainActorsTabPage.SuspendLayout();
			this.CusSupplyChainActorsUserControl.SuspendLayout();
			this.AdditionalInformationTabPage.SuspendLayout();
			this.AdditionalInformationUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.JobDeclaration);
			// 
			// EntryInstructionsGrid
			// 
			this.EntryInstructionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryInstructionsGrid, "CustomsEntryInstructions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryInstructions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_Procedure)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_Style)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_SubStyle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_DeclarationReason)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_NextProcedure)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CH.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_DateForDuty)));
			this.EntryInstructionsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CEI_Procedure";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "CEI_Style";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "CEI_SubStyle";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo1.ColumnName = "CEI_Description";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "CEI_DeclarationReason";
			zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "CEI_NextProcedure";
			zDropEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.ColumnName = "CEI_DateForDuty";
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.EntryInstructionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.EntryInstructionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryInstructionsGrid.GridId = "ad340f7f-fd6b-4196-998c-ee4c975da660";
			this.EntryInstructionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryInstructionsGrid.LayoutKey = "EntryInstructionsGrid";
			this.EntryInstructionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryInstructionsGrid.Name = "EntryInstructionsGrid";
			this.EntryInstructionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 80, true);
			this.EntryInstructionsGrid.TabIndex = 0;
		// 
		// SplitContainer
		// 
		this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
		this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.SplitContainer.Name = "SplitContainer";
		this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
		// 
		// SplitContainer.Panel1
		// 
		this.SplitContainer.Panel1.Controls.Add(this.EntryInstructionsGrid);
		this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 385, true);
		this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
		// 
		// SplitContainer.Panel2
		// 
		this.SplitContainer.Panel2.AutoScroll = true;
		this.SplitContainer.Panel2.Controls.Add(this.EntryInstructionTabControl);
		this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
		this.SplitContainer.SplitterWidth = 7;
		this.SplitContainer.TabIndex = 3;
		// 
		// EntryInstructionTabControl
		// 
		this.EntryInstructionTabControl.Controls.Add(this.EntryInstructionDetailsTabPage);
		this.EntryInstructionTabControl.Controls.Add(this.PreviousDocumentsTabPage);
		this.EntryInstructionTabControl.Controls.Add(this.SupportingDocumentsTabPage);
		this.EntryInstructionTabControl.Controls.Add(this.TransportDocumentsTabPage);
		this.EntryInstructionTabControl.Controls.Add(this.CusSupplyChainActorsTabPage);
		this.EntryInstructionTabControl.Controls.Add(this.AdditionalInformationTabPage);
		this.EntryInstructionTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
		this.EntryInstructionTabControl.Name = "EntryInstructionTabControl";
		this.EntryInstructionTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1274, 296, true);
		this.EntryInstructionTabControl.TabIndex = 0;
		// 
		// EntryInstructionDetailsTabPage
		// 
		this.EntryInstructionDetailsTabPage.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("3D4AC58F-BDA4-4F99-81A2-DAC1DA9CE441", "Details");
		this.EntryInstructionDetailsTabPage.Controls.Add(this.DetailsUserControl);
		this.EntryInstructionDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
		this.EntryInstructionDetailsTabPage.Name = "EntryInstructionDetailsTabPage";
		this.EntryInstructionDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
		this.EntryInstructionDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1269, 274, true);
		this.EntryInstructionDetailsTabPage.TabIndex = 0;
		this.EntryInstructionDetailsTabPage.UseVisualStyleBackColor = true;
		// 
		// DetailsUserControl
		// 
		this.DetailsUserControl.AllowDrop = true;
		this.DetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.DetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
		this.DetailsUserControl.Name = "DetailsUserControl";
		this.DetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1263, 269, true);
		this.DetailsUserControl.TabIndex = 0;
		// 
		// PreviousDocumentsTabPage
		// 
		this.PreviousDocumentsTabPage.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("2346BA33-49DD-474B-A2C0-67F6CF986F87", "[40] Previous Docs");
		this.PreviousDocumentsTabPage.Controls.Add(this.PreviousDocumentsUserControl);
		this.PreviousDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
		this.PreviousDocumentsTabPage.Name = "PreviousDocumentsTabPage";
		this.PreviousDocumentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
		this.PreviousDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1269, 274, true);
		this.PreviousDocumentsTabPage.TabIndex = 1;
		this.PreviousDocumentsTabPage.UseVisualStyleBackColor = true;
		// 
		// PreviousDocumentsUserControl
		// 
		this.PreviousDocumentsUserControl.AllowDrop = true;
		this.PreviousDocumentsUserControl.AutoSize = true;
		this.BindingSource.SetBindingMember(this.PreviousDocumentsUserControl, "CustomsEntryInstructions.PreviousDocuments");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CH.Business.PreviousDocument)(((Enterprise.Customs.CH.Business.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).PreviousDocuments)).SyncRoot)))));
		this.PreviousDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.PreviousDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
		this.PreviousDocumentsUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
		this.PreviousDocumentsUserControl.Name = "PreviousDocumentsUserControl";
		this.PreviousDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1263, 269, true);
		this.PreviousDocumentsUserControl.TabIndex = 2;
		// 
		// SupportingDocumentsTabPage
		// 
		this.SupportingDocumentsTabPage.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("ED86E70A-2876-4312-83D7-95DD84AAB794", "[44] Supporting Documents");
		this.SupportingDocumentsTabPage.Controls.Add(this.SupportingDocumentsUserControl);
		this.SupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
		this.SupportingDocumentsTabPage.Name = "SupportingDocumentsTabPage";
		this.SupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1269, 274, true);
		this.SupportingDocumentsTabPage.TabIndex = 1;
		// 
		// SupportingDocumentsUserControl
		// 
		this.SupportingDocumentsUserControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.SupportingDocumentsUserControl, "CustomsEntryInstructions.SupportingDocuments");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CH.Business.SupportingDocument)(((Enterprise.Customs.CH.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).SupportingDocuments)).SyncRoot)))));
		this.SupportingDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.SupportingDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.SupportingDocumentsUserControl.Name = "SupportingDocumentsUserControl";
		this.SupportingDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1269, 274, true);
		this.SupportingDocumentsUserControl.TabIndex = 0;
		// 
		// TransportDocumentsTabPage
		// 
		this.TransportDocumentsTabPage.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("2C9050C4-8B5F-487C-884E-E7A922023D48", "Transport Documents");
		this.TransportDocumentsTabPage.Controls.Add(this.TransportDocumentsUserControl);
		this.TransportDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
		this.TransportDocumentsTabPage.Name = "TransportDocumentsTabPage";
		this.TransportDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1269, 271, true);
		this.TransportDocumentsTabPage.TabIndex = 1;
		// 
		// TransportDocumentsUserControl
		// 
		this.TransportDocumentsUserControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.TransportDocumentsUserControl, "CustomsEntryInstructions.TransportDocuments");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CH.Business.TransportDocument)(((Enterprise.Customs.CH.Business.TransportDocument)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).TransportDocuments)).SyncRoot)))));
		this.TransportDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.TransportDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.TransportDocumentsUserControl.Name = "TransportDocumentsUserControl";
		this.TransportDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1269, 274, true);
		this.TransportDocumentsUserControl.TabIndex = 0;
		// 
		// CusSupplyChainActorsTabPage
		// 
		this.CusSupplyChainActorsTabPage.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("F696B947-2465-47D2-BA4B-2FC6F2B2D3CD", "Supply Chain Actors");
		this.CusSupplyChainActorsTabPage.Controls.Add(this.CusSupplyChainActorsUserControl);
		this.CusSupplyChainActorsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
		this.CusSupplyChainActorsTabPage.Name = "CusSupplyChainActorsTabPage";
		this.CusSupplyChainActorsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
		this.CusSupplyChainActorsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1269, 274, true);
		this.CusSupplyChainActorsTabPage.TabIndex = 2;
		this.CusSupplyChainActorsTabPage.UseVisualStyleBackColor = true;
		// 
		// CusSupplyChainActorsUserControl
		// 
		this.CusSupplyChainActorsUserControl.AllowDrop = true;
		this.CusSupplyChainActorsUserControl.AutoSize = true;
		this.BindingSource.SetBindingMember(this.CusSupplyChainActorsUserControl, "CustomsEntryInstructions");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CH.Business.CusEntryInstruction)(((Enterprise.Customs.CH.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)))));
		this.CusSupplyChainActorsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.CusSupplyChainActorsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
		this.CusSupplyChainActorsUserControl.Name = "CusSupplyChainActorsUserControl";
		this.CusSupplyChainActorsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1263, 269, true);
		this.CusSupplyChainActorsUserControl.TabIndex = 0;
		// 
		// AdditionalInformationTabPage
		// 
		this.AdditionalInformationTabPage.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("0657F592-8B37-407F-9DEF-85DA744110DD", "Additional Information");
		this.AdditionalInformationTabPage.Controls.Add(this.AdditionalInformationUserControl);
		this.AdditionalInformationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
		this.AdditionalInformationTabPage.Name = "AdditionalInformationTabPage";
		this.AdditionalInformationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
		this.AdditionalInformationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1266, 270, true);
		this.AdditionalInformationTabPage.TabIndex = 0;
		this.AdditionalInformationTabPage.UseVisualStyleBackColor = true;
		// 
		// AdditionalInformationUserControl
		// 
		this.AdditionalInformationUserControl.AllowDrop = true;
		this.AdditionalInformationUserControl.AutoSize = true;
		this.BindingSource.SetBindingMember(this.AdditionalInformationUserControl, "CustomsEntryInstructions.AdditionalInformations");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CH.Business.AdditionalInformation)(((Enterprise.Customs.CH.Business.AdditionalInformation)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.CH.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AdditionalInformations)).SyncRoot)))));
		this.AdditionalInformationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.AdditionalInformationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
		this.AdditionalInformationUserControl.Name = "AdditionalInformationUserControl";
		this.AdditionalInformationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1260, 264, true);
		this.AdditionalInformationUserControl.TabIndex = 0;
		// 
		// EntryInstructionDetailsUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.SplitContainer);
		this.Name = "EntryInstructionDetailsUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 385, true);
		this.Controls.SetChildIndex(this.SplitContainer, 0);
		this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.EntryInstructionsGrid)).EndInit();
		this.EntryInstructionsGrid.ResumeLayout(false);
		this.EntryInstructionsGrid.PerformLayout();
		this.SplitContainer.Panel1.ResumeLayout(false);
		this.SplitContainer.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
		this.SplitContainer.ResumeLayout(false);
		this.SplitContainer.PerformLayout();
		this.EntryInstructionTabControl.ResumeLayout(false);
		this.EntryInstructionTabControl.PerformLayout();
		this.EntryInstructionDetailsTabPage.ResumeLayout(false);
		this.EntryInstructionDetailsTabPage.PerformLayout();
		this.PreviousDocumentsTabPage.ResumeLayout(false);
		this.PreviousDocumentsTabPage.PerformLayout();
		this.PreviousDocumentsUserControl.ResumeLayout(true);
		this.PreviousDocumentsUserControl.PerformLayout();
		this.SupportingDocumentsTabPage.ResumeLayout(false);
		this.SupportingDocumentsTabPage.PerformLayout();
		this.SupportingDocumentsUserControl.ResumeLayout(true);
		this.SupportingDocumentsUserControl.PerformLayout();
		this.TransportDocumentsTabPage.ResumeLayout(false);
		this.TransportDocumentsTabPage.PerformLayout();
		this.TransportDocumentsUserControl.ResumeLayout(true);
		this.TransportDocumentsUserControl.PerformLayout();
		this.CusSupplyChainActorsTabPage.ResumeLayout(false);
		this.CusSupplyChainActorsTabPage.PerformLayout();
		this.CusSupplyChainActorsUserControl.ResumeLayout(true);
		this.CusSupplyChainActorsUserControl.PerformLayout();
		this.AdditionalInformationTabPage.ResumeLayout(false);
		this.AdditionalInformationTabPage.PerformLayout();
		this.AdditionalInformationUserControl.ResumeLayout(true);
		this.AdditionalInformationUserControl.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}
    #endregion

    internal ZArchitecture.ZGrid EntryInstructionsGrid;
    internal CargoWise.Windows.UI.KSplitContainer SplitContainer;
    internal ZTabControl EntryInstructionTabControl;
    internal ZTabPage EntryInstructionDetailsTabPage;
    internal ZTabPage CusSupplyChainActorsTabPage;
    internal ZTabPage PreviousDocumentsTabPage;
    internal ZTabPage AdditionalInformationTabPage;
    internal AdditionalInformationUserControl AdditionalInformationUserControl;
    internal Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl DetailsUserControl;
    internal CusSupplyChainActorsUserControl CusSupplyChainActorsUserControl;
    internal PreviousDocumentsUserControl PreviousDocumentsUserControl;
    internal SupportingDocumentsUserControl SupportingDocumentsUserControl;
    internal Enterprise.ZArchitecture.GUI.ZTabPage SupportingDocumentsTabPage;
    internal ZTabPage TransportDocumentsTabPage;
    internal TransportDocumentsUserControl TransportDocumentsUserControl;
    private System.ComponentModel.IContainer components;
}
