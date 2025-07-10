using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	partial class DV1DetailsUserControl
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
			this.CustomsDecisionNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ResaleDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ResaleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RoyalitiesLicenceDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RoyalitiesLicenceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RestrictionsConsiderationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsiderationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RestrictionsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RelationDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PriceInfluenceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RelationshipDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ContractNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContractDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CloseApproximationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ResaleDropEdit.SuspendLayout();
			this.RoyalitiesLicenceDropEdit.SuspendLayout();
			this.ConsiderationDropEdit.SuspendLayout();
			this.RestrictionsDropEdit.SuspendLayout();
			this.PriceInfluenceDropEdit.SuspendLayout();
			this.RelationshipDropEdit.SuspendLayout();
			this.ContractDateDateEdit.SuspendLayout();
			this.CloseApproximationDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// CustomsDecisionNumberTextBox
			//
			this.CustomsDecisionNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
			this.BindingSource.SetBindingMember(this.CustomsDecisionNumberTextBox, "DV1Details.DV1_CustomsDecisionNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusDV1Detail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).DV1Details)).SyncRoot)).DV1_CustomsDecisionNumber)));
			this.CustomsDecisionNumberTextBox.CaptionResourceString = null;
			this.CustomsDecisionNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CustomsDecisionNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 169, true);
			this.CustomsDecisionNumberTextBox.Name = "CustomsDecisionNumberTextBox";
			this.CustomsDecisionNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.CustomsDecisionNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1202, 20, true);
			this.CustomsDecisionNumberTextBox.TabIndex = 11;
			// 
			// ResaleDetailsTextBox
			// 
			this.ResaleDetailsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
			this.BindingSource.SetBindingMember(this.ResaleDetailsTextBox, "DV1Details.DV1_ResaleDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusDV1Detail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).DV1Details)).SyncRoot)).DV1_ResaleDetails)));
			this.ResaleDetailsTextBox.CaptionResourceString = null;
			this.ResaleDetailsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ResaleDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 147, true);
			this.ResaleDetailsTextBox.Name = "ResaleDetailsTextBox";
			this.ResaleDetailsTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ResaleDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1091, 20, true);
			this.ResaleDetailsTextBox.TabIndex = 10;
			// 
			// ResaleDropEdit
			// 
			this.ResaleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ResaleDropEdit, "DV1Details.DV1_Resale");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.CusDV1Detail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).DV1Details)).SyncRoot)).DV1_Resale)));
			this.ResaleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 147, true);
			this.ResaleDropEdit.Name = "ResaleDropEdit";
			this.ResaleDropEdit.PreBoundMaxLength = 1;
			this.ResaleDropEdit.ShouldResizeByMaxLength = true;
			this.ResaleDropEdit.ShowDescriptionBox = false;
			this.ResaleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.ResaleDropEdit.TabIndex = 9;
			// 
			// RoyalitiesLicenceDetailsTextBox
			// 
			this.RoyalitiesLicenceDetailsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
			this.BindingSource.SetBindingMember(this.RoyalitiesLicenceDetailsTextBox, "DV1Details.DV1_RoyaltiesLicenceDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusDV1Detail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).DV1Details)).SyncRoot)).DV1_RoyaltiesLicenceDetails)));
			this.RoyalitiesLicenceDetailsTextBox.CaptionResourceString = null;
			this.RoyalitiesLicenceDetailsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RoyalitiesLicenceDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 126, true);
			this.RoyalitiesLicenceDetailsTextBox.Name = "RoyalitiesLicenceDetailsTextBox";
			this.RoyalitiesLicenceDetailsTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.RoyalitiesLicenceDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1091, 20, true);
			this.RoyalitiesLicenceDetailsTextBox.TabIndex = 8;
			// 
			// RoyalitiesLicenceDropEdit
			// 
			this.RoyalitiesLicenceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RoyalitiesLicenceDropEdit, "DV1Details.DV1_RoyaltiesLicence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.CusDV1Detail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).DV1Details)).SyncRoot)).DV1_RoyaltiesLicence)));
			this.RoyalitiesLicenceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 126, true);
			this.RoyalitiesLicenceDropEdit.Name = "RoyalitiesLicenceDropEdit";
			this.RoyalitiesLicenceDropEdit.PreBoundMaxLength = 1;
			this.RoyalitiesLicenceDropEdit.ShouldResizeByMaxLength = true;
			this.RoyalitiesLicenceDropEdit.ShowDescriptionBox = false;
			this.RoyalitiesLicenceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RoyalitiesLicenceDropEdit.TabIndex = 7;
			// 
			// RestrictionsConsiderationTextBox
			// 
			this.RestrictionsConsiderationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
			this.BindingSource.SetBindingMember(this.RestrictionsConsiderationTextBox, "DV1Details.DV1_RestrictionConsiderationDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusDV1Detail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).DV1Details)).SyncRoot)).DV1_RestrictionConsiderationDetails)));
			this.RestrictionsConsiderationTextBox.CaptionResourceString = null;
			this.RestrictionsConsiderationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RestrictionsConsiderationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 105, true);
			this.RestrictionsConsiderationTextBox.Name = "RestrictionsConsiderationTextBox";
			this.RestrictionsConsiderationTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.RestrictionsConsiderationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1091, 20, true);
			this.RestrictionsConsiderationTextBox.TabIndex = 6;
			// 
			// ConsiderationDropEdit
			// 
			this.ConsiderationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsiderationDropEdit, "DV1Details.DV1_Consideration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.CusDV1Detail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).DV1Details)).SyncRoot)).DV1_Consideration)));
			this.ConsiderationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 105, true);
			this.ConsiderationDropEdit.Name = "ConsiderationDropEdit";
			this.ConsiderationDropEdit.PreBoundMaxLength = 1;
			this.ConsiderationDropEdit.ShouldResizeByMaxLength = true;
			this.ConsiderationDropEdit.ShowDescriptionBox = false;
			this.ConsiderationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.ConsiderationDropEdit.TabIndex = 5;
			// 
			// RestrictionsDropEdit
			// 
			this.RestrictionsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RestrictionsDropEdit, "DV1Details.DV1_Restrictions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.CusDV1Detail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).DV1Details)).SyncRoot)).DV1_Restrictions)));
			this.RestrictionsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 83, true);
			this.RestrictionsDropEdit.Name = "RestrictionsDropEdit";
			this.RestrictionsDropEdit.PreBoundMaxLength = 1;
			this.RestrictionsDropEdit.ShouldResizeByMaxLength = true;
			this.RestrictionsDropEdit.ShowDescriptionBox = false;
			this.RestrictionsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RestrictionsDropEdit.TabIndex = 4;
			// 
			// RelationDetailsTextBox
			// 
			this.RelationDetailsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left))));
			this.BindingSource.SetBindingMember(this.RelationDetailsTextBox, "DV1Details.DV1_RelationDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusDV1Detail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).DV1Details)).SyncRoot)).DV1_RelationDetails)));
			this.RelationDetailsTextBox.CaptionResourceString = null;
			this.RelationDetailsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RelationDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 40, true);
			this.RelationDetailsTextBox.Name = "RelationDetailsTextBox";
			this.RelationDetailsTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.RelationDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1091, 20, true);
			this.RelationDetailsTextBox.TabIndex = 2;
			// 
			// PriceInfluenceDropEdit
			// 
			this.PriceInfluenceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PriceInfluenceDropEdit, "DV1Details.DV1_PriceInfluence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.CusDV1Detail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).DV1Details)).SyncRoot)).DV1_PriceInfluence)));
			this.PriceInfluenceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 40, true);
			this.PriceInfluenceDropEdit.Name = "PriceInfluenceDropEdit";
			this.PriceInfluenceDropEdit.PreBoundMaxLength = 1;
			this.PriceInfluenceDropEdit.ShouldResizeByMaxLength = true;
			this.PriceInfluenceDropEdit.ShowDescriptionBox = false;
			this.PriceInfluenceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.PriceInfluenceDropEdit.TabIndex = 1;
			// 
			// RelationshipDropEdit
			//
			this.RelationshipDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RelationshipDropEdit, "DV1Details.DV1_Relationship");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.CusDV1Detail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).DV1Details)).SyncRoot)).DV1_Relationship)));
			this.RelationshipDropEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("76c02aeb-9035-48b2-aafd-7ff416b2c2d9", "Buyer / Seller Relationship");
			this.RelationshipDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 19, true);
			this.RelationshipDropEdit.Name = "RelationshipDropEdit";
			this.RelationshipDropEdit.PreBoundMaxLength = 1;
			this.RelationshipDropEdit.ShouldResizeByMaxLength = true;
			this.RelationshipDropEdit.ShowDescriptionBox = false;
			this.RelationshipDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RelationshipDropEdit.TabIndex = 0;
			// 
			// ContractNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.ContractNumberTextBox, "DV1Details.DV1_ContractNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusDV1Detail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).DV1Details)).SyncRoot)).DV1_ContractNumber)));
			this.ContractNumberTextBox.CaptionResourceString = null;
			this.ContractNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 191, true);
			this.ContractNumberTextBox.Name = "ContractNumberTextBox";
			this.ContractNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ContractNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1202, 20, true);
			this.ContractNumberTextBox.TabIndex = 12;
			// 
			// ContractDateDateEdit
			// 
			this.ContractDateDateEdit.AllowDrop = true;
			this.ContractDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ContractDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ContractDateDateEdit, "DV1Details.DV1_ContractDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.CusDV1Detail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).DV1Details)).SyncRoot)).DV1_ContractDate)));
			this.ContractDateDateEdit.CaptionResourceString = null;
			this.ContractDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 214, true);
			this.ContractDateDateEdit.Name = "ContractDateDateEdit";
			this.ContractDateDateEdit.TabIndex = 13;
			// 
			// CloseApproximationDropEdit
			// 
			this.CloseApproximationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CloseApproximationDropEdit, "DV1Details.DV1_CloseApproximation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.CusDV1Detail)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).DV1Details)).SyncRoot)).DV1_CloseApproximation)));
			this.CloseApproximationDropEdit.CaptionResourceString = null;
			this.CloseApproximationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 61, true);
			this.CloseApproximationDropEdit.Name = "CloseApproximationDropEdit";
			this.CloseApproximationDropEdit.PreBoundMaxLength = 1;
			this.CloseApproximationDropEdit.ShouldResizeByMaxLength = true;
			this.CloseApproximationDropEdit.ShowDescriptionBox = false;
			this.CloseApproximationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.CloseApproximationDropEdit.TabIndex = 3;
			// 
			// DV1DetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CloseApproximationDropEdit);
			this.Controls.Add(this.ContractDateDateEdit);
			this.Controls.Add(this.ContractNumberTextBox);
			this.Controls.Add(this.CustomsDecisionNumberTextBox);
			this.Controls.Add(this.ResaleDetailsTextBox);
			this.Controls.Add(this.ResaleDropEdit);
			this.Controls.Add(this.RoyalitiesLicenceDetailsTextBox);
			this.Controls.Add(this.RoyalitiesLicenceDropEdit);
			this.Controls.Add(this.RestrictionsConsiderationTextBox);
			this.Controls.Add(this.ConsiderationDropEdit);
			this.Controls.Add(this.RestrictionsDropEdit);
			this.Controls.Add(this.RelationDetailsTextBox);
			this.Controls.Add(this.PriceInfluenceDropEdit);
			this.Controls.Add(this.RelationshipDropEdit);
			this.Name = "DV1DetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1361, 240, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResaleDropEdit.ResumeLayout(true);
			this.ResaleDropEdit.PerformLayout();
			this.RoyalitiesLicenceDropEdit.ResumeLayout(true);
			this.RoyalitiesLicenceDropEdit.PerformLayout();
			this.ConsiderationDropEdit.ResumeLayout(true);
			this.ConsiderationDropEdit.PerformLayout();
			this.RestrictionsDropEdit.ResumeLayout(true);
			this.RestrictionsDropEdit.PerformLayout();
			this.PriceInfluenceDropEdit.ResumeLayout(true);
			this.PriceInfluenceDropEdit.PerformLayout();
			this.RelationshipDropEdit.ResumeLayout(true);
			this.RelationshipDropEdit.PerformLayout();
			this.ContractDateDateEdit.ResumeLayout(true);
			this.ContractDateDateEdit.PerformLayout();
			this.CloseApproximationDropEdit.ResumeLayout(true);
			this.CloseApproximationDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit PriceInfluenceDropEdit;
		internal ZArchitecture.GUI.ZDropEdit RelationshipDropEdit;
		internal ZArchitecture.ZTextBox RelationDetailsTextBox;
		internal ZArchitecture.GUI.ZDropEdit ConsiderationDropEdit;
		internal ZArchitecture.GUI.ZDropEdit RestrictionsDropEdit;
		internal ZArchitecture.ZTextBox RestrictionsConsiderationTextBox;
		internal ZArchitecture.GUI.ZDropEdit RoyalitiesLicenceDropEdit;
		internal ZArchitecture.ZTextBox RoyalitiesLicenceDetailsTextBox;
		internal ZArchitecture.GUI.ZDropEdit ResaleDropEdit;
		internal ZArchitecture.ZTextBox ResaleDetailsTextBox;
		internal ZArchitecture.ZTextBox CustomsDecisionNumberTextBox;
		internal ZArchitecture.ZTextBox ContractNumberTextBox;
		internal ZDateEdit ContractDateDateEdit;
		internal ZDropEdit CloseApproximationDropEdit;
	}
}
