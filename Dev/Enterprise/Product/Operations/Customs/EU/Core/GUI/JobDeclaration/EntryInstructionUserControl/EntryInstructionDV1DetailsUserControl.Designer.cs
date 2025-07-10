namespace Enterprise.Customs.EU.GUI
{
	partial class EntryInstructionDV1DetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.dv1DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.customsDecisionNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.resaleDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.resaleTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.royalitiesLicenceDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.royalitiesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.restrictionsConsiderationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.considerationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.restrictionsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.relationDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.priceInfluenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.relationshipTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.dv1DetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DetailsGrid)).BeginInit();
			this.DetailsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// dv1DetailsGroupBox
			// 
			this.dv1DetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("ABD679D0-A143-4FF7-B553-A830E909FB34", "D.V.1 Details");
			this.dv1DetailsGroupBox.Controls.Add(this.customsDecisionNumberTextBox);
			this.dv1DetailsGroupBox.Controls.Add(this.resaleDetailsTextBox);
			this.dv1DetailsGroupBox.Controls.Add(this.resaleTextBox);
			this.dv1DetailsGroupBox.Controls.Add(this.royalitiesLicenceDetailsTextBox);
			this.dv1DetailsGroupBox.Controls.Add(this.royalitiesTextBox);
			this.dv1DetailsGroupBox.Controls.Add(this.restrictionsConsiderationTextBox);
			this.dv1DetailsGroupBox.Controls.Add(this.considerationTextBox);
			this.dv1DetailsGroupBox.Controls.Add(this.restrictionsTextBox);
			this.dv1DetailsGroupBox.Controls.Add(this.relationDetailsTextBox);
			this.dv1DetailsGroupBox.Controls.Add(this.priceInfluenceTextBox);
			this.dv1DetailsGroupBox.Controls.Add(this.relationshipTextBox);
			this.dv1DetailsGroupBox.Controls.Add(this.DetailsGrid);
			this.dv1DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dv1DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.dv1DetailsGroupBox.Name = "dv1DetailsGroupBox";
			this.dv1DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1114, 400, true);
			this.dv1DetailsGroupBox.TabIndex = 0;
			this.dv1DetailsGroupBox.TabStop = false;
			// 
			// customsDecisionNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.customsDecisionNumberTextBox, "CustomsEntryInstructions.DV1DetailsPivots.CustomsDecisionNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).CustomsDecisionNumber)));
			this.customsDecisionNumberTextBox.CaptionResourceString = null;
			this.customsDecisionNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 358, true);
			this.customsDecisionNumberTextBox.Name = "customsDecisionNumberTextBox";
			this.customsDecisionNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 20, true);
			this.customsDecisionNumberTextBox.TabIndex = 11;
			// 
			// resaleDetailsTextBox
			// 
			this.BindingSource.SetBindingMember(this.resaleDetailsTextBox, "CustomsEntryInstructions.DV1DetailsPivots.ResaleDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).ResaleDetails)));
			this.resaleDetailsTextBox.CaptionResourceString = null;
			this.resaleDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 336, true);
			this.resaleDetailsTextBox.Name = "resaleDetailsTextBox";
			this.resaleDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 20, true);
			this.resaleDetailsTextBox.TabIndex = 10;
			// 
			// resaleTextBox
			// 
			this.resaleTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.resaleTextBox, "CustomsEntryInstructions.DV1DetailsPivots.Resale");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).Resale)));
			this.resaleTextBox.CaptionResourceString = null;
			this.resaleTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 336, true);
			this.resaleTextBox.Name = "resaleTextBox";
			this.resaleTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.resaleTextBox.TabIndex = 9;
			// 
			// royalitiesLicenceDetailsTextBox
			// 
			this.BindingSource.SetBindingMember(this.royalitiesLicenceDetailsTextBox, "CustomsEntryInstructions.DV1DetailsPivots.RoyaltiesLicenceDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).RoyaltiesLicenceDetails)));
			this.royalitiesLicenceDetailsTextBox.CaptionResourceString = null;
			this.royalitiesLicenceDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 315, true);
			this.royalitiesLicenceDetailsTextBox.Name = "royalitiesLicenceDetailsTextBox";
			this.royalitiesLicenceDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 20, true);
			this.royalitiesLicenceDetailsTextBox.TabIndex = 8;
			// 
			// royalitiesTextBox
			// 
			this.royalitiesTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.royalitiesTextBox, "CustomsEntryInstructions.DV1DetailsPivots.RoyaltiesLicence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).RoyaltiesLicence)));
			this.royalitiesTextBox.CaptionResourceString = null;
			this.royalitiesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 315, true);
			this.royalitiesTextBox.Name = "royalitiesTextBox";
			this.royalitiesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.royalitiesTextBox.TabIndex = 7;
			// 
			// restrictionsConsiderationTextBox
			// 
			this.BindingSource.SetBindingMember(this.restrictionsConsiderationTextBox, "CustomsEntryInstructions.DV1DetailsPivots.RestrictionConsiderationDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).RestrictionConsiderationDetails)));
			this.restrictionsConsiderationTextBox.CaptionResourceString = null;
			this.restrictionsConsiderationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 294, true);
			this.restrictionsConsiderationTextBox.Name = "restrictionsConsiderationTextBox";
			this.restrictionsConsiderationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 20, true);
			this.restrictionsConsiderationTextBox.TabIndex = 6;
			// 
			// considerationTextBox
			// 
			this.considerationTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.considerationTextBox, "CustomsEntryInstructions.DV1DetailsPivots.Consideration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).Consideration)));
			this.considerationTextBox.CaptionResourceString = null;
			this.considerationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 294, true);
			this.considerationTextBox.Name = "considerationTextBox";
			this.considerationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.considerationTextBox.TabIndex = 5;
			// 
			// restrictionsTextBox
			// 
			this.restrictionsTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.restrictionsTextBox, "CustomsEntryInstructions.DV1DetailsPivots.Restrictions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).Restrictions)));
			this.restrictionsTextBox.CaptionResourceString = null;
			this.restrictionsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 273, true);
			this.restrictionsTextBox.Name = "restrictionsTextBox";
			this.restrictionsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.restrictionsTextBox.TabIndex = 3;
			// 
			// relationDetailsTextBox
			// 
			this.BindingSource.SetBindingMember(this.relationDetailsTextBox, "CustomsEntryInstructions.DV1DetailsPivots.RelationDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).RelationDetails)));
			this.relationDetailsTextBox.CaptionResourceString = null;
			this.relationDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 251, true);
			this.relationDetailsTextBox.Name = "relationDetailsTextBox";
			this.relationDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 20, true);
			this.relationDetailsTextBox.TabIndex = 4;
			// 
			// priceInfluenceTextBox
			// 
			this.priceInfluenceTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.priceInfluenceTextBox, "CustomsEntryInstructions.DV1DetailsPivots.PriceInfluence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).PriceInfluence)));
			this.priceInfluenceTextBox.CaptionResourceString = null;
			this.priceInfluenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 251, true);
			this.priceInfluenceTextBox.Name = "priceInfluenceTextBox";
			this.priceInfluenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.priceInfluenceTextBox.TabIndex = 2;
			// 
			// relationshipTextBox
			// 
			this.BindingSource.SetBindingMember(this.relationshipTextBox, "CustomsEntryInstructions.DV1DetailsPivots.Relationship");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).Relationship)));
			this.relationshipTextBox.CaptionResourceString = null;
			this.relationshipTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 230, true);
			this.relationshipTextBox.Name = "relationshipTextBox";
			this.relationshipTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.relationshipTextBox.TabIndex = 1;
			// 
			// DetailsGrid
			// 
			this.DetailsGrid.AllowNavigation = false;
			this.DetailsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DetailsGrid, "CustomsEntryInstructions.DV1DetailsPivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).IsForEntryInstruction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).Relationship)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).PriceInfluence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).RelationDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).Restrictions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).Consideration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).RestrictionConsiderationDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).RoyaltiesLicence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).RoyaltiesLicenceDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).Resale)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).ResaleDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.NonPersistentCusDV1DetailPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).DV1DetailsPivots)).SyncRoot)).CustomsDecisionNumber)));
			this.DetailsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "IsForEntryInstruction";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Sequence";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "Relationship";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "PriceInfluence";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.ColumnName = "RelationDetails";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.ColumnName = "Restrictions";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "Consideration";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "RestrictionConsiderationDetails";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo7.ColumnName = "RoyaltiesLicence";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "RoyaltiesLicenceDetails";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo9.ColumnName = "Resale";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.ColumnName = "ResaleDetails";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo11.ColumnName = "CustomsDecisionNumber";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.DetailsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.DetailsGrid.GridId = "27960194-9d9f-4a24-a704-c331662ae0f1";
			this.DetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DetailsGrid.LayoutKey = "DetailsGrid";
			this.DetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 17, true);
			this.DetailsGrid.Name = "DetailsGrid";
			this.DetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1103, 200, true);
			this.DetailsGrid.TabIndex = 0;
			// 
			// EntryInstructionDV1DetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.dv1DetailsGroupBox);
			this.Name = "EntryInstructionDV1DetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1114, 400, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.dv1DetailsGroupBox.ResumeLayout(false);
			this.dv1DetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DetailsGrid)).EndInit();
			this.DetailsGrid.ResumeLayout(false);
			this.DetailsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox dv1DetailsGroupBox;
		private ZArchitecture.ZGrid DetailsGrid;
		private ZArchitecture.ZTextBox priceInfluenceTextBox;
		private ZArchitecture.ZTextBox relationshipTextBox;
		private ZArchitecture.ZTextBox relationDetailsTextBox;
		private ZArchitecture.ZTextBox considerationTextBox;
		private ZArchitecture.ZTextBox restrictionsTextBox;
		private ZArchitecture.ZTextBox restrictionsConsiderationTextBox;
		private ZArchitecture.ZTextBox royalitiesTextBox;
		private ZArchitecture.ZTextBox royalitiesLicenceDetailsTextBox;
		private ZArchitecture.ZTextBox resaleTextBox;
		private ZArchitecture.ZTextBox resaleDetailsTextBox;
		private ZArchitecture.ZTextBox customsDecisionNumberTextBox;
	}
}
