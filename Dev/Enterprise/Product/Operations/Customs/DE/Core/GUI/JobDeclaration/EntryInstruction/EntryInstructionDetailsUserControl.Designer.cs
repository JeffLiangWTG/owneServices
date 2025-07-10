using Enterprise.Customs.EU.GUI;

namespace Enterprise.Customs.DE.GUI
{
	partial class EntryInstructionDetailsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

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
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.EntryInstructionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.EntryInstructionTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BasicUserControl = new Enterprise.Customs.DE.GUI.EntryInstructionDetailBasicUserControl();
			this.OutwardProcessingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.outwardProcessingUserControl = new Enterprise.Customs.DE.GUI.OutwardProcessingUserControl();
			this.InwardProcessingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InwardProcessingUserControl = new Enterprise.Customs.DE.GUI.InwardProcessingUserControl();
			this.PreviousDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PreviousDocumentsUserControl = new Enterprise.Customs.DE.GUI.ImportPreviousDocumentsUserControl();
			this.DV1DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntryInstructionDV1DetailsUserControl = new Enterprise.Customs.EU.GUI.EntryInstructionDV1DetailsUserControl();
			this.FiscalReferencesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.FiscalReferencesUserControl = new Enterprise.Customs.EU.GUI.EntryInstructionFiscalReferencesUserControl();
			this.SupplyChainActorReferencesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SupplyChainActorReferencesUserControl = new Enterprise.Customs.EU.GUI.PlugIn.SupplyChainActorReferencesUserControl();
			this.AuthorizationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AuthorizationsUserControl = new Enterprise.Customs.EU.GUI.EntryInstructionAuthorisationsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EntryInstructionsGrid)).BeginInit();
			this.EntryInstructionsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.EntryInstructionTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.BasicUserControl.SuspendLayout();
			this.OutwardProcessingTabPage.SuspendLayout();
			this.outwardProcessingUserControl.SuspendLayout();
			this.InwardProcessingTabPage.SuspendLayout();
			this.InwardProcessingUserControl.SuspendLayout();
			this.PreviousDocumentsTabPage.SuspendLayout();
			this.PreviousDocumentsUserControl.SuspendLayout();
			this.DV1DetailsTabPage.SuspendLayout();
			this.EntryInstructionDV1DetailsUserControl.SuspendLayout();
			this.FiscalReferencesTabPage.SuspendLayout();
			this.FiscalReferencesUserControl.SuspendLayout();
			this.SupplyChainActorReferencesTabPage.SuspendLayout();
			this.SupplyChainActorReferencesUserControl.SuspendLayout();
			this.AuthorizationsTabPage.SuspendLayout();
			this.AuthorizationsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.JobDeclaration);
			// 
			// EntryInstructionsGrid
			// 
			this.EntryInstructionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryInstructionsGrid, "CustomsEntryInstructions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_Style)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_SubStyle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ZG_PartyConstellation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_DateForDuty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_LocalClearanceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ZG_ExitDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_EarlyClearanceFlag)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_Procedure)));
			this.EntryInstructionsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zDropEditColumnStyleInfo1.ColumnName = "CEI_Style";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107);
			zDropEditColumnStyleInfo2.ColumnName = "CEI_SubStyle";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68);
			zDropEditColumnStyleInfo3.ColumnName = "ZG_PartyConstellation";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114);
			zTextBoxColumnStyleInfo1.ColumnName = "CEI_Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(290);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("9ccfe104-c8ec-4ee1-be94-d7610b63519d", "Decisive Date");
			zDateEditColumnStyleInfo1.ColumnName = "CEI_DateForDuty";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(89);
			zDateEditColumnStyleInfo2.ColumnName = "CEI_LocalClearanceDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			zDateEditColumnStyleInfo3.ColumnName = "ZG_ExitDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(67);
			zDropEditColumnStyleInfo4.ColumnName = "CEI_EarlyClearanceFlag";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(123);
			zDropEditColumnStyleInfo5.ColumnName = "CEI_Procedure";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.EntryInstructionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.EntryInstructionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.EntryInstructionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryInstructionsGrid.GridId = "ad340f7f-fd6b-4196-998c-ee4c975da660";
			this.EntryInstructionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryInstructionsGrid.LayoutKey = "EntryInstructionsGrid";
			this.EntryInstructionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryInstructionsGrid.Name = "EntryInstructionsGrid";
			this.EntryInstructionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1337, 291, true);
			this.EntryInstructionsGrid.TabIndex = 0;
			this.EntryInstructionsGrid.AfterBind += new System.EventHandler(this.EntryInstructionsGridOnAfterBind);
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
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.AutoScroll = true;
			this.SplitContainer.Panel2.Controls.Add(this.EntryInstructionTabControl);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1337, 727, true);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(432);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(291);
			this.SplitContainer.TabIndex = 3;
			// 
			// EntryInstructionTabControl
			// 
			this.EntryInstructionTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.EntryInstructionTabControl.Controls.Add(this.DetailsTabPage);
			this.EntryInstructionTabControl.Controls.Add(this.OutwardProcessingTabPage);
			this.EntryInstructionTabControl.Controls.Add(this.InwardProcessingTabPage);
			this.EntryInstructionTabControl.Controls.Add(this.PreviousDocumentsTabPage);
			this.EntryInstructionTabControl.Controls.Add(this.DV1DetailsTabPage);
			this.EntryInstructionTabControl.Controls.Add(this.FiscalReferencesTabPage);
			this.EntryInstructionTabControl.Controls.Add(this.SupplyChainActorReferencesTabPage);
			this.EntryInstructionTabControl.Controls.Add(this.AuthorizationsTabPage);
			this.EntryInstructionTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryInstructionTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryInstructionTabControl.Name = "EntryInstructionTabControl";
			this.EntryInstructionTabControl.SelectedIndex = 0;
			this.EntryInstructionTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1337, 432, true);
			this.EntryInstructionTabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("cc4930b3-63ee-4272-89c6-27489e5efe82", "Details");
			this.DetailsTabPage.Controls.Add(this.BasicUserControl);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.DetailsTabPage.TabIndex = 0;
			// 
			// BasicUserControl
			// 
			this.BasicUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BasicUserControl, ".");
			this.BasicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BasicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BasicUserControl.Name = "BasicUserControl";
			this.BasicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.BasicUserControl.TabIndex = 0;
			// 
			// OutwardProcessingTabPage
			// 
			this.OutwardProcessingTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("956711e4-c92c-460f-944b-fd7eea34fa7c", "Outward Processing");
			this.OutwardProcessingTabPage.Controls.Add(this.outwardProcessingUserControl);
			this.OutwardProcessingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OutwardProcessingTabPage.Name = "OutwardProcessingTabPage";
			this.OutwardProcessingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.OutwardProcessingTabPage.TabIndex = 1;
			// 
			// outwardProcessingUserControl
			// 
			this.outwardProcessingUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.outwardProcessingUserControl, ".");
			this.outwardProcessingUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.outwardProcessingUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.outwardProcessingUserControl.Name = "outwardProcessingUserControl";
			this.outwardProcessingUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.outwardProcessingUserControl.TabIndex = 1;
			// 
			// InwardProcessingTabPage
			// 
			this.InwardProcessingTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("0845a817-a510-4df9-aa15-c3d60c6bccfe", "Inward Processing");
			this.InwardProcessingTabPage.Controls.Add(this.InwardProcessingUserControl);
			this.InwardProcessingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InwardProcessingTabPage.Name = "InwardProcessingTabPage";
			this.InwardProcessingTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.InwardProcessingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.InwardProcessingTabPage.TabIndex = 2;
			this.InwardProcessingTabPage.UseVisualStyleBackColor = true;
			// 
			// InwardProcessingUserControl
			// 
			this.InwardProcessingUserControl.AllowDrop = true;
			this.InwardProcessingUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.InwardProcessingUserControl, ".");
			this.InwardProcessingUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InwardProcessingUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.InwardProcessingUserControl.Name = "InwardProcessingUserControl";
			this.InwardProcessingUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1323, 399, true);
			this.InwardProcessingUserControl.TabIndex = 0;
			// 
			// PreviousDocumentsTabPage
			// 
			this.PreviousDocumentsTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("D3E4D453-7425-47CD-A932-A0016F29F7C2", "[40] Previous Docs");
			this.PreviousDocumentsTabPage.Controls.Add(this.PreviousDocumentsUserControl);
			this.PreviousDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PreviousDocumentsTabPage.Name = "PreviousDocumentsTabPage";
			this.PreviousDocumentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PreviousDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.PreviousDocumentsTabPage.TabIndex = 2;
			this.PreviousDocumentsTabPage.UseVisualStyleBackColor = true;
			// 
			// PreviousDocumentsUserControl
			// 
			this.PreviousDocumentsUserControl.AllowDrop = true;
			this.PreviousDocumentsUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.PreviousDocumentsUserControl, ".");
			this.PreviousDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviousDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PreviousDocumentsUserControl.Name = "PreviousDocumentsUserControl";
			this.PreviousDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1323, 399, true);
			this.PreviousDocumentsUserControl.TabIndex = 0;
			// 
			// DV1DetailsTabPage
			// 
			this.DV1DetailsTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("B017C802-3BF8-42FE-90F8-B47AFE2E8B7D", "D.V.1 Details");
			this.DV1DetailsTabPage.Controls.Add(this.EntryInstructionDV1DetailsUserControl);
			this.DV1DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DV1DetailsTabPage.Name = "DV1DetailsTabPage";
			this.DV1DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.DV1DetailsTabPage.TabIndex = 3;
			// 
			// EntryInstructionDV1DetailsUserControl
			// 
			this.EntryInstructionDV1DetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryInstructionDV1DetailsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)))));
			this.EntryInstructionDV1DetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryInstructionDV1DetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryInstructionDV1DetailsUserControl.Name = "EntryInstructionDV1DetailsUserControl";
			this.EntryInstructionDV1DetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.EntryInstructionDV1DetailsUserControl.TabIndex = 0;
			// 
			// FiscalReferencesTabPage
			// 
			this.FiscalReferencesTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("ab83fbde-a8ee-4849-a0ae-41126362af1d", "Fiscal References");
			this.FiscalReferencesTabPage.Controls.Add(this.FiscalReferencesUserControl);
			this.FiscalReferencesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.FiscalReferencesTabPage.Name = "FiscalReferencesTabPage";
			this.FiscalReferencesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.FiscalReferencesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.FiscalReferencesTabPage.TabIndex = 4;
			this.FiscalReferencesTabPage.UseVisualStyleBackColor = true;
			// 
			// FiscalReferencesUserControl
			// 
			this.FiscalReferencesUserControl.AllowDrop = true;
			this.FiscalReferencesUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.FiscalReferencesUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)))));
			this.FiscalReferencesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FiscalReferencesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.FiscalReferencesUserControl.Name = "FiscalReferencesUserControl";
			this.FiscalReferencesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1323, 399, true);
			this.FiscalReferencesUserControl.TabIndex = 0;
			// 
			// SupplyChainActorReferencesTabPage
			// 
			this.SupplyChainActorReferencesTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("D322FDE2-CCF7-490C-9618-9DD1BA0B70C6", "Add. Supply Chain Actor");
			this.SupplyChainActorReferencesTabPage.Controls.Add(this.SupplyChainActorReferencesUserControl);
			this.SupplyChainActorReferencesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SupplyChainActorReferencesTabPage.Name = "SupplyChainActorReferencesTabPage";
			this.SupplyChainActorReferencesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SupplyChainActorReferencesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.SupplyChainActorReferencesTabPage.TabIndex = 5;
			this.SupplyChainActorReferencesTabPage.UseVisualStyleBackColor = true;
			// 
			// SupplyChainActorReferencesUserControl
			// 
			this.SupplyChainActorReferencesUserControl.AllowDrop = true;
			this.SupplyChainActorReferencesUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.SupplyChainActorReferencesUserControl, "CustomsEntryInstructions.CusSupplyChainActorReferences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.Declaration.ICusSupplyChainActorReferenceCollection<Enterprise.Customs.EU.Business.Declaration.CusSupplyChainActorReference>)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CusSupplyChainActorReferences)));
			this.SupplyChainActorReferencesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupplyChainActorReferencesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SupplyChainActorReferencesUserControl.Name = "SupplyChainActorReferencesUserControl";
			this.SupplyChainActorReferencesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1323, 399, true);
			this.SupplyChainActorReferencesUserControl.TabIndex = 0;
			// 
			// AuthorizationsTabPage
			// 
			this.AuthorizationsTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("1c43f2fc-fa95-4ac2-9202-d4a91600f9d3", "Authorizations");
			this.AuthorizationsTabPage.Controls.Add(this.AuthorizationsUserControl);
			this.AuthorizationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AuthorizationsTabPage.Name = "AuthorizationsTabPage";
			this.AuthorizationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.AuthorizationsTabPage.TabIndex = 6;
			// 
			// AuthorizationsUserControl
			// 
			this.AuthorizationsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorizationsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)))));
			this.AuthorizationsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AuthorizationsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AuthorizationsUserControl.Name = "AuthorizationsUserControl";
			this.AuthorizationsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.AuthorizationsUserControl.TabIndex = 1;
			// 
			// EntryInstructionDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "EntryInstructionDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1337, 727, true);
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
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.BasicUserControl.ResumeLayout(true);
			this.BasicUserControl.PerformLayout();
			this.OutwardProcessingTabPage.ResumeLayout(false);
			this.OutwardProcessingTabPage.PerformLayout();
			this.outwardProcessingUserControl.ResumeLayout(true);
			this.outwardProcessingUserControl.PerformLayout();
			this.InwardProcessingTabPage.ResumeLayout(false);
			this.InwardProcessingTabPage.PerformLayout();
			this.InwardProcessingUserControl.ResumeLayout(true);
			this.InwardProcessingUserControl.PerformLayout();
			this.PreviousDocumentsTabPage.ResumeLayout(false);
			this.PreviousDocumentsTabPage.PerformLayout();
			this.PreviousDocumentsUserControl.ResumeLayout(true);
			this.PreviousDocumentsUserControl.PerformLayout();
			this.DV1DetailsTabPage.ResumeLayout(false);
			this.DV1DetailsTabPage.PerformLayout();
			this.EntryInstructionDV1DetailsUserControl.ResumeLayout(true);
			this.EntryInstructionDV1DetailsUserControl.PerformLayout();
			this.FiscalReferencesTabPage.ResumeLayout(false);
			this.FiscalReferencesTabPage.PerformLayout();
			this.FiscalReferencesUserControl.ResumeLayout(true);
			this.FiscalReferencesUserControl.PerformLayout();
			this.SupplyChainActorReferencesTabPage.ResumeLayout(false);
			this.SupplyChainActorReferencesTabPage.PerformLayout();
			this.SupplyChainActorReferencesUserControl.ResumeLayout(true);
			this.SupplyChainActorReferencesUserControl.PerformLayout();
			this.AuthorizationsTabPage.ResumeLayout(false);
			this.AuthorizationsTabPage.PerformLayout();
			this.AuthorizationsUserControl.ResumeLayout(true);
			this.AuthorizationsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.ZGrid EntryInstructionsGrid;
		CargoWise.Windows.UI.KSplitContainer SplitContainer;
		EntryInstructionDetailBasicUserControl BasicUserControl;
		ZArchitecture.GUI.ZTabPage OutwardProcessingTabPage;
		ZArchitecture.GUI.ZTabControl EntryInstructionTabControl;
		ZArchitecture.GUI.ZTabPage DetailsTabPage;
		OutwardProcessingUserControl outwardProcessingUserControl;
		ZArchitecture.GUI.ZTabPage InwardProcessingTabPage;
		InwardProcessingUserControl InwardProcessingUserControl;
		ZArchitecture.GUI.ZTabPage FiscalReferencesTabPage;
		Enterprise.Customs.EU.GUI.EntryInstructionFiscalReferencesUserControl FiscalReferencesUserControl;
		ZArchitecture.GUI.ZTabPage SupplyChainActorReferencesTabPage;
		EU.GUI.PlugIn.SupplyChainActorReferencesUserControl SupplyChainActorReferencesUserControl;
		ZArchitecture.GUI.ZTabPage PreviousDocumentsTabPage;
		ImportPreviousDocumentsUserControl PreviousDocumentsUserControl;
		ZArchitecture.GUI.ZTabPage DV1DetailsTabPage;
		EntryInstructionDV1DetailsUserControl EntryInstructionDV1DetailsUserControl;
		private ZArchitecture.GUI.ZTabPage AuthorizationsTabPage;
		private EntryInstructionAuthorisationsUserControl AuthorizationsUserControl;
	}
}
