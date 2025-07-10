using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.GUI
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
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.EntryInstructionGridUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.EntryInstructionTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DetailsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.FiscalReferencesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.FiscalReferencesUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.AuthorisationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AuthorisationsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.DV1DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.entryInstructionDV1DetailsUserControl = new Enterprise.Customs.EU.GUI.EntryInstructionDV1DetailsUserControl();
			this.SupplyChainActorTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SupplyChainActorReferencesUserControl = new Enterprise.Customs.EU.GUI.PlugIn.SupplyChainActorReferencesUserControl();
			this.GuaranteesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GuaranteesUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.SealsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SealsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.AdditionalInfoTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdditionalInfoUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.SupportingDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SupportingDocumentsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.PreviousDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PreviousDocumentsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.SpecialProceduresTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SpecialProceduresUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.EntryInstructionTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.FiscalReferencesTabPage.SuspendLayout();
			this.AuthorisationsTabPage.SuspendLayout();
			this.DV1DetailsTabPage.SuspendLayout();
			this.entryInstructionDV1DetailsUserControl.SuspendLayout();
			this.SupplyChainActorTabPage.SuspendLayout();
			this.SupplyChainActorReferencesUserControl.SuspendLayout();
			this.GuaranteesTabPage.SuspendLayout();
			this.SealsTabPage.SuspendLayout();
			this.AdditionalInfoTabPage.SuspendLayout();
			this.SupportingDocumentsTabPage.SuspendLayout();
			this.PreviousDocumentsTabPage.SuspendLayout();
			this.SpecialProceduresTabPage.SuspendLayout();
			this.SpecialProceduresUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
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
			this.SplitContainer.Panel1.Controls.Add(this.EntryInstructionGridUserControl);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.AutoScroll = true;
			this.SplitContainer.Panel2.Controls.Add(this.EntryInstructionTabControl);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1337, 725, true);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(432);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(290);
			this.SplitContainer.TabIndex = 3;
			// 
			// EntryInstructionGridUserControl
			// 
			this.EntryInstructionGridUserControl.AllowDrop = true;
			this.EntryInstructionGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryInstructionGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryInstructionGridUserControl.Name = "EntryInstructionGridUserControl";
			this.EntryInstructionGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1337, 290, true);
			this.EntryInstructionGridUserControl.TabIndex = 1;
			// 
			// EntryInstructionTabControl
			// 
			this.EntryInstructionTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.EntryInstructionTabControl.Controls.Add(this.DetailsTabPage);
			this.EntryInstructionTabControl.Controls.Add(this.FiscalReferencesTabPage);
			this.EntryInstructionTabControl.Controls.Add(this.AuthorisationsTabPage);
			this.EntryInstructionTabControl.Controls.Add(this.DV1DetailsTabPage);
			this.EntryInstructionTabControl.Controls.Add(this.SupplyChainActorTabPage);
			this.EntryInstructionTabControl.Controls.Add(this.GuaranteesTabPage);
			this.EntryInstructionTabControl.Controls.Add(this.SealsTabPage);
			this.EntryInstructionTabControl.Controls.Add(this.AdditionalInfoTabPage);
			this.EntryInstructionTabControl.Controls.Add(this.SupportingDocumentsTabPage);
			this.EntryInstructionTabControl.Controls.Add(this.PreviousDocumentsTabPage);
			this.EntryInstructionTabControl.Controls.Add(this.SpecialProceduresTabPage);
			this.EntryInstructionTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryInstructionTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryInstructionTabControl.Name = "EntryInstructionTabControl";
			this.EntryInstructionTabControl.SelectedIndex = 0;
			this.EntryInstructionTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1337, 432, true);
			this.EntryInstructionTabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("5bea5aea-d0f4-494f-a264-72818c013b3e", "Details");
			this.DetailsTabPage.Controls.Add(this.DetailsUserControl);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.DetailsTabPage.TabIndex = 0;
			// 
			// DetailsUserControl
			// 
			this.DetailsUserControl.AllowDrop = true;
			this.DetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsUserControl.Name = "DetailsUserControl";
			this.DetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.DetailsUserControl.TabIndex = 0;
			// 
			// FiscalReferencesTabPage
			// 
			this.FiscalReferencesTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("caca7e71-3b5d-417b-828e-c11fe13996c5", "Fiscal References");
			this.FiscalReferencesTabPage.Controls.Add(this.FiscalReferencesUserControl);
			this.FiscalReferencesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.FiscalReferencesTabPage.Name = "FiscalReferencesTabPage";
			this.FiscalReferencesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.FiscalReferencesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.FiscalReferencesTabPage.TabIndex = 1;
			this.FiscalReferencesTabPage.UseVisualStyleBackColor = true;
			// 
			// FiscalReferencesUserControl
			// 
			this.FiscalReferencesUserControl.AllowDrop = true;
			this.FiscalReferencesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FiscalReferencesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.FiscalReferencesUserControl.Name = "FiscalReferencesUserControl";
			this.FiscalReferencesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1323, 399, true);
			this.FiscalReferencesUserControl.TabIndex = 0;
			// 
			// AuthorisationsTabPage
			// 
			this.AuthorisationsTabPage.Controls.Add(this.AuthorisationsUserControl);
			this.AuthorisationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AuthorisationsTabPage.Name = "AuthorisationsTabPage";
			this.AuthorisationsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AuthorisationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.AuthorisationsTabPage.TabIndex = 2;
			this.AuthorisationsTabPage.UseVisualStyleBackColor = true;
			// 
			// AuthorisationsUserControl
			// 
			this.AuthorisationsUserControl.AllowDrop = true;
			this.AuthorisationsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AuthorisationsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AuthorisationsUserControl.Name = "AuthorisationsUserControl";
			this.AuthorisationsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1323, 399, true);
			this.AuthorisationsUserControl.TabIndex = 0;
			// 
			// DV1DetailsTabPage
			// 
			this.DV1DetailsTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("ae933a58-1f00-4271-87c9-fdce203604b0", "D.V.1 Details");
			this.DV1DetailsTabPage.Controls.Add(this.entryInstructionDV1DetailsUserControl);
			this.DV1DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DV1DetailsTabPage.Name = "DV1DetailsTabPage";
			this.DV1DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.DV1DetailsTabPage.TabIndex = 3;
			this.DV1DetailsTabPage.Text = "D.V.1 Details";
			// 
			// entryInstructionDV1DetailsUserControl
			// 
			this.entryInstructionDV1DetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.entryInstructionDV1DetailsUserControl, ".");
			this.entryInstructionDV1DetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.entryInstructionDV1DetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.entryInstructionDV1DetailsUserControl.Name = "entryInstructionDV1DetailsUserControl";
			this.entryInstructionDV1DetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.entryInstructionDV1DetailsUserControl.TabIndex = 0;
			// 
			// SupplyChainActorTabPage
			// 
			this.SupplyChainActorTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("3b531d41-f4b8-4f89-a61b-b35a3af6310a", "Add. Supply Chain Actors");
			this.SupplyChainActorTabPage.Controls.Add(this.SupplyChainActorReferencesUserControl);
			this.SupplyChainActorTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SupplyChainActorTabPage.Name = "SupplyChainActorTabPage";
			this.SupplyChainActorTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SupplyChainActorTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.SupplyChainActorTabPage.TabIndex = 4;
			this.SupplyChainActorTabPage.UseVisualStyleBackColor = true;
			// 
			// SupplyChainActorReferencesUserControl
			// 
			this.SupplyChainActorReferencesUserControl.AllowDrop = true;
			this.SupplyChainActorReferencesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BindingSource.SetBindingMember(this.SupplyChainActorReferencesUserControl, "CustomsEntryInstructions.CusSupplyChainActorReferences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.Declaration.ICusSupplyChainActorReferenceCollection<Enterprise.Customs.EU.Business.Declaration.CusSupplyChainActorReference>)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CusSupplyChainActorReferences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.Declaration.ICusSupplyChainActorReferenceCollection<Enterprise.Customs.EU.Business.Declaration.CusSupplyChainActorReference>)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CusSupplyChainActorReferences)));
			this.SupplyChainActorReferencesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SupplyChainActorReferencesUserControl.Name = "SupplyChainActorReferencesUserControl";
			this.SupplyChainActorReferencesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1323, 399, true);
			this.SupplyChainActorReferencesUserControl.TabIndex = 0;
			// 
			// GuaranteesTabPage
			// 
			this.GuaranteesTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("790C4589-23F6-497C-8BFE-417C70727D0D", "Guarantees");
			this.GuaranteesTabPage.Controls.Add(this.GuaranteesUserControl);
			this.GuaranteesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GuaranteesTabPage.Name = "GuaranteesTabPage";
			this.GuaranteesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.GuaranteesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.GuaranteesTabPage.TabIndex = 5;
			this.GuaranteesTabPage.UseVisualStyleBackColor = true;
			// 
			// GuaranteesUserControl
			// 
			this.GuaranteesUserControl.AllowDrop = true;
			this.GuaranteesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GuaranteesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.GuaranteesUserControl.Name = "GuaranteesUserControl";
			this.GuaranteesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1323, 399, true);
			this.GuaranteesUserControl.TabIndex = 0;
			// 
			// SealsTabPage
			// 
			this.SealsTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("4543b830-4d0e-4c9e-bbf2-bd5728259422", "Seals");
			this.SealsTabPage.Controls.Add(this.SealsUserControl);
			this.SealsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SealsTabPage.Name = "SealsTabPage";
			this.SealsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.SealsTabPage.TabIndex = 5;
			// 
			// SealsUserControl
			// 
			this.SealsUserControl.AllowDrop = true;
			this.SealsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SealsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SealsUserControl.Name = "SealsUserControl";
			this.SealsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.SealsUserControl.TabIndex = 1;
			// 
			// AdditionalInfoTabPage
			// 
			this.AdditionalInfoTabPage.Controls.Add(this.AdditionalInfoUserControl);
			this.AdditionalInfoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdditionalInfoTabPage.Name = "AdditionalInfoTabPage";
			this.AdditionalInfoTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AdditionalInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 243, true);
			this.AdditionalInfoTabPage.TabIndex = 6;
			this.AdditionalInfoTabPage.UseVisualStyleBackColor = true;
			// 
			// AdditionalInfoUserControl
			// 
			this.AdditionalInfoUserControl.AllowDrop = true;
			this.AdditionalInfoUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInfoUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AdditionalInfoUserControl.Name = "AdditionalInfoUserControl";
			this.AdditionalInfoUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 237, true);
			this.AdditionalInfoUserControl.TabIndex = 1;
			// 
			// SupportingDocumentsTabPage
			// 
			this.SupportingDocumentsTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("21CA8D03-FF1A-4109-BF1A-65C4798E1DC8", "Supporting Documents");
			this.SupportingDocumentsTabPage.Controls.Add(this.SupportingDocumentsUserControl);
			this.SupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SupportingDocumentsTabPage.Name = "SupportingDocumentsTabPage";
			this.SupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.SupportingDocumentsTabPage.TabIndex = 0;
			this.SupportingDocumentsTabPage.UseVisualStyleBackColor = true;
			// 
			// SupportingDocumentsUserControl
			// 
			this.SupportingDocumentsUserControl.AllowDrop = true;
			this.SupportingDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentsUserControl.Name = "SupportingDocumentsUserControl";
			this.SupportingDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.SupportingDocumentsUserControl.TabIndex = 0;
			// 
			// PreviousDocumentsTabPage
			// 
			this.PreviousDocumentsTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("77b2fecb-f6c7-4164-99a0-3ebc5017908d", "Previous Documents");
			this.PreviousDocumentsTabPage.Controls.Add(this.PreviousDocumentsUserControl);
			this.PreviousDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PreviousDocumentsTabPage.Name = "PreviousDocumentsTabPage";
			this.PreviousDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.PreviousDocumentsTabPage.TabIndex = 6;
			// 
			// PreviousDocumentsUserControl
			// 
			this.PreviousDocumentsUserControl.AllowDrop = true;
			this.PreviousDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviousDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PreviousDocumentsUserControl.Name = "PreviousDocumentsUserControl";
			this.PreviousDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.PreviousDocumentsUserControl.TabIndex = 0;
			// 
			// SpecialProceduresTabPage
			// 
			this.SpecialProceduresTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("f5231480-f575-4096-abb7-2d3311b02f22", "Special Procedures");
			this.SpecialProceduresTabPage.Controls.Add(this.SpecialProceduresUserControl);
			this.SpecialProceduresTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.SpecialProceduresTabPage.Name = "SpecialProceduresTabPage";
			this.SpecialProceduresTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 408, true);
			this.SpecialProceduresTabPage.TabIndex = 7;
			// 
			// SpecialProceduresUserControl
			// 
			this.SpecialProceduresUserControl.AllowDrop = true;
			this.SpecialProceduresUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SpecialProceduresUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SpecialProceduresUserControl.Name = "SpecialProceduresUserControl";
			this.SpecialProceduresUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 408, true);
			this.SpecialProceduresUserControl.TabIndex = 0;
			// 
			// EntryInstructionDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "EntryInstructionDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1337, 725, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.EntryInstructionTabControl.ResumeLayout(false);
			this.EntryInstructionTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.FiscalReferencesTabPage.ResumeLayout(false);
			this.FiscalReferencesTabPage.PerformLayout();
			this.AuthorisationsTabPage.ResumeLayout(false);
			this.AuthorisationsTabPage.PerformLayout();
			this.DV1DetailsTabPage.ResumeLayout(false);
			this.DV1DetailsTabPage.PerformLayout();
			this.entryInstructionDV1DetailsUserControl.ResumeLayout(true);
			this.entryInstructionDV1DetailsUserControl.PerformLayout();
			this.SupplyChainActorTabPage.ResumeLayout(false);
			this.SupplyChainActorTabPage.PerformLayout();
			this.SupplyChainActorReferencesUserControl.ResumeLayout(true);
			this.SupplyChainActorReferencesUserControl.PerformLayout();
			this.GuaranteesTabPage.ResumeLayout(false);
			this.GuaranteesTabPage.PerformLayout();
			this.SealsTabPage.ResumeLayout(false);
			this.SealsTabPage.PerformLayout();
			this.AdditionalInfoTabPage.ResumeLayout(false);
			this.AdditionalInfoTabPage.PerformLayout();
			this.SupportingDocumentsTabPage.ResumeLayout(false);
			this.SupportingDocumentsTabPage.PerformLayout();
			this.PreviousDocumentsTabPage.ResumeLayout(false);
			this.PreviousDocumentsTabPage.PerformLayout();
			this.SpecialProceduresTabPage.ResumeLayout(false);
			this.SpecialProceduresTabPage.PerformLayout();
			this.SpecialProceduresUserControl.ResumeLayout(true);
			this.SpecialProceduresUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		internal protected ZArchitecture.GUI.ZTabControl EntryInstructionTabControl;
		private ZArchitecture.GUI.ZTabPage DetailsTabPage;
		protected internal ZArchitecture.GUI.ZTabPage FiscalReferencesTabPage;
		protected internal ZArchitecture.GUI.ZTabPage AuthorisationsTabPage;
		protected internal ZArchitecture.GUI.ZDynamicControlCreationUserControl DetailsUserControl;
		internal ZArchitecture.GUI.ZDynamicControlCreationUserControl FiscalReferencesUserControl;
		internal ZArchitecture.GUI.ZDynamicControlCreationUserControl AuthorisationsUserControl;
		protected internal ZArchitecture.GUI.ZDynamicControlCreationUserControl EntryInstructionGridUserControl;
		internal ZArchitecture.GUI.ZTabPage DV1DetailsTabPage;
		private EntryInstructionDV1DetailsUserControl entryInstructionDV1DetailsUserControl;
		protected internal ZArchitecture.GUI.ZTabPage SupplyChainActorTabPage;
		private PlugIn.SupplyChainActorReferencesUserControl SupplyChainActorReferencesUserControl;
		protected ZArchitecture.GUI.ZTabPage GuaranteesTabPage;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl GuaranteesUserControl;
		internal ZArchitecture.GUI.ZTabPage SealsTabPage;
		internal ZArchitecture.GUI.ZDynamicControlCreationUserControl SealsUserControl;
		protected internal ZArchitecture.GUI.ZTabPage AdditionalInfoTabPage;
		internal ZArchitecture.GUI.ZDynamicControlCreationUserControl AdditionalInfoUserControl;
		protected internal ZArchitecture.GUI.ZTabPage SupportingDocumentsTabPage;
		internal ZArchitecture.GUI.ZDynamicControlCreationUserControl SupportingDocumentsUserControl;
		protected internal ZArchitecture.GUI.ZTabPage PreviousDocumentsTabPage;
		internal ZArchitecture.GUI.ZDynamicControlCreationUserControl PreviousDocumentsUserControl;
		protected internal ZArchitecture.GUI.ZTabPage SpecialProceduresTabPage;
		internal ZArchitecture.GUI.ZDynamicControlCreationUserControl SpecialProceduresUserControl;
	}
}
