using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public partial class EntryInstructionDetailBasicUserControl
{
	#region Component Designer generated code

	/// <summary> 
	/// Required method for Designer support - do not modify 
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		this.ActivateByOperatorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
		this.IncludeRoutingSecurityDataCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
		this.LocationOfGoodsUserControl = new Enterprise.Customs.EU.GUI.LocationOfGoodsUserControl();
		this.NewOwnerOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
		this.RemoverOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
		this.BondHolderOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
		this.RequestLabel = new Enterprise.ZArchitecture.ZLabel();
		this.RequestTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.IndirectTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.NationalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
		this.NumberOfDaysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
		this.JustificationTextBox = new Enterprise.ZArchitecture.ZTextBox();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.LocationOfGoodsUserControl.SuspendLayout();
		this.NewOwnerOrganisationControl.SuspendLayout();
		this.RemoverOrganisationControl.SuspendLayout();
		this.BondHolderOrganisationControl.SuspendLayout();
		this.RequestTypeDropEdit.SuspendLayout();
		this.IndirectTypeDropEdit.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction);
		// 
		// ActivateByOperatorCheckBox
		// 
		this.ActivateByOperatorCheckBox.AutoSize = true;
		this.BindingSource.SetBindingMember(this.ActivateByOperatorCheckBox, "ZG_ActivateByOperator");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction)(null)).ZG_ActivateByOperator)));
		this.ActivateByOperatorCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
		this.ActivateByOperatorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 231, true);
		this.ActivateByOperatorCheckBox.Name = "ActivateByOperatorCheckBox";
		this.ActivateByOperatorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
		this.ActivateByOperatorCheckBox.TabIndex = 6;
		this.ActivateByOperatorCheckBox.UseVisualStyleBackColor = true;
		// 
		// IncludeRoutingSecurityDataCheckBox
		// 
		this.IncludeRoutingSecurityDataCheckBox.AutoSize = true;
		this.BindingSource.SetBindingMember(this.IncludeRoutingSecurityDataCheckBox, "IncludeRoutingSecurityData");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction)(null)).IncludeRoutingSecurityData)));
		this.IncludeRoutingSecurityDataCheckBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("A037887D-880D-446B-9CEC-8CB9624E8860", "Include Routing Security Data");
		this.IncludeRoutingSecurityDataCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
		this.IncludeRoutingSecurityDataCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 231, true);
		this.IncludeRoutingSecurityDataCheckBox.Name = "IncludeRoutingSecurityDataCheckBox";
		this.IncludeRoutingSecurityDataCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
		this.IncludeRoutingSecurityDataCheckBox.TabIndex = 5;
		this.IncludeRoutingSecurityDataCheckBox.UseVisualStyleBackColor = true;
		// 
		// LocationOfGoodsUserControl
		// 
		this.LocationOfGoodsUserControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.LocationOfGoodsUserControl, ".");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.ICusGoodsLocationProvider)(((Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction)(null)))));
		this.LocationOfGoodsUserControl.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("6BE4E415-7627-46FD-8838-664F918AAB59", "Location of Goods");
		this.LocationOfGoodsUserControl.CusGoodsLocationProviderType = null;
		this.LocationOfGoodsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 151, true);
		this.LocationOfGoodsUserControl.Name = "LocationOfGoodsUserControl";
		this.LocationOfGoodsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 22, true);
		this.LocationOfGoodsUserControl.TabIndex = 7;
		// 
		// NewOwnerOrganisationControl
		// 
		this.NewOwnerOrganisationControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.NewOwnerOrganisationControl, "CEI_OH_Owner");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction)(null)).CEI_OH_Owner)));
		this.NewOwnerOrganisationControl.BindToOrganisations = "Lookups+Organisations";
		this.NewOwnerOrganisationControl.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("609C5D3A-A799-4A8D-A6B2-F642DCE514CF", "New Owner");
		this.NewOwnerOrganisationControl.Captions = new string[] {
        "New Owner"};
		this.NewOwnerOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
		this.NewOwnerOrganisationControl.IsCaptionOverridden = false;
		this.NewOwnerOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 106, true);
		this.NewOwnerOrganisationControl.Name = "NewOwnerOrganisationControl";
		this.NewOwnerOrganisationControl.OrgAddressFormatter = null;
		this.NewOwnerOrganisationControl.PopupCaption = "";
		this.NewOwnerOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 39, true);
		this.NewOwnerOrganisationControl.TabIndex = 3;
		// 
		// RemoverOrganisationControl
		// 
		this.RemoverOrganisationControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.RemoverOrganisationControl, "CEI_OH_Carrier");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction)(null)).CEI_OH_Carrier)));
		this.RemoverOrganisationControl.BindToOrganisations = "Lookups+CarrierOrganisations";
		this.RemoverOrganisationControl.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("46F97297-8172-4FE4-89C5-DC3167EE0EB2", "Remover");
		this.RemoverOrganisationControl.Captions = new string[] {
        "Remover"};
		this.RemoverOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
		this.RemoverOrganisationControl.IsCaptionOverridden = false;
		this.RemoverOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(283, 69, true);
		this.RemoverOrganisationControl.Name = "RemoverOrganisationControl";
		this.RemoverOrganisationControl.OrgAddressFormatter = null;
		this.RemoverOrganisationControl.PopupCaption = "";
		this.RemoverOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 39, true);
		this.RemoverOrganisationControl.TabIndex = 2;
		// 
		// BondHolderOrganisationControl
		// 
		this.BondHolderOrganisationControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.BondHolderOrganisationControl, "CEI_OH_BondHolder");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction)(null)).CEI_OH_BondHolder)));
		this.BondHolderOrganisationControl.BindToOrganisations = "Lookups+Organisations";
		this.BondHolderOrganisationControl.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("578A3D64-CD7E-4999-8533-A8D5374EE0D7", "Bond Holder");
		this.BondHolderOrganisationControl.Captions = new string[] {
        "Bond Holder"};
		this.BondHolderOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
		this.BondHolderOrganisationControl.IsCaptionOverridden = false;
		this.BondHolderOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 63, true);
		this.BondHolderOrganisationControl.Name = "BondHolderOrganisationControl";
		this.BondHolderOrganisationControl.OrgAddressFormatter = null;
		this.BondHolderOrganisationControl.PopupCaption = "";
		this.BondHolderOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 39, true);
		this.BondHolderOrganisationControl.TabIndex = 1;
		// 
		// RequestLabel
		// 
		this.RequestLabel.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("F8627880-AFE6-4222-BA53-4616335C8BE9", "Request");
		this.RequestLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
		this.RequestLabel.IsFontBold = true;
		this.RequestLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 130, true);
		this.RequestLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 10, true);
		this.RequestLabel.Name = "RequestLabel";
		this.RequestLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
		this.RequestLabel.TabIndex = 1;
		// 
		// RequestTypeDropEdit
		// 
		this.RequestTypeDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.RequestTypeDropEdit, "ZG_RequestType");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction)(null)).ZG_RequestType)));
		this.RequestTypeDropEdit.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("A6C97E5C-575E-4AE1-BFA7-E191894C48A8", "Type");
		this.RequestTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 179, true);
		this.RequestTypeDropEdit.Name = "RequestTypeDropEdit";
		this.RequestTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
		this.RequestTypeDropEdit.TabIndex = 1;
		// 
		// IndirectTypeDropEdit
		// 
		this.IndirectTypeDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.IndirectTypeDropEdit, "ZG_IndirectType");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction)(null)).ZG_IndirectType)));
		this.IndirectTypeDropEdit.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("2E8F4CCF-F979-49C2-BEE4-B42169AE3534", "Indirect Type");
		this.IndirectTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 179, true);
		this.IndirectTypeDropEdit.Name = "IndirectTypeDropEdit";
		this.IndirectTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
		this.IndirectTypeDropEdit.TabIndex = 1;
		// 
		// NationalCheckBox
		// 
		this.BindingSource.SetBindingMember(this.NationalCheckBox, "ZG_National");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction)(null)).ZG_National)));
		this.NationalCheckBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("57D08304-5EB5-4C1D-9ECF-1EA3630C071D", "National");
		this.NationalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 251, true);
		this.NationalCheckBox.Name = "NationalCheckBox";
		this.NationalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 24, true);
		this.NationalCheckBox.TabIndex = 1;
		// 
		// NumberOfDaysCalcEdit
		// 
		this.BindingSource.SetBindingMember(this.NumberOfDaysCalcEdit, "ZG_NumberOfDays");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction)(null)).ZG_NumberOfDays)));
		this.NumberOfDaysCalcEdit.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("85AC64E2-F687-4C2F-8B6F-4A8FB994452C", "Number of days");
		this.NumberOfDaysCalcEdit.DecimalPlaces = 0;
		this.NumberOfDaysCalcEdit.Decimals = 0;
		this.NumberOfDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 80, true);
		this.NumberOfDaysCalcEdit.Name = "NumberOfDaysCalcEdit";
		this.NumberOfDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
		this.NumberOfDaysCalcEdit.TabIndex = 1;
		this.NumberOfDaysCalcEdit.Text = "0";
		this.NumberOfDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.NumberOfDaysCalcEdit.TrackDisposedAccess = true;
		// 
		// JustificationTextBox
		// 
		this.BindingSource.SetBindingMember(this.JustificationTextBox, "ZG_Justification");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction)(null)).ZG_Justification)));
		this.JustificationTextBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("266BAA0B-F463-4FBE-9F80-53D0473B54A6", "Justification");
		this.JustificationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 205, true);
		this.JustificationTextBox.Name = "JustificationTextBox";
		this.JustificationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		this.JustificationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
		this.JustificationTextBox.TabIndex = 1;
		// 
		// EntryInstructionDetailBasicUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.Controls.Add(this.LocationOfGoodsUserControl);
		this.Controls.Add(this.ActivateByOperatorCheckBox);
		this.Controls.Add(this.IncludeRoutingSecurityDataCheckBox);
		this.Controls.Add(this.BondHolderOrganisationControl);
		this.Controls.Add(this.NewOwnerOrganisationControl);
		this.Controls.Add(this.RemoverOrganisationControl);
		this.Controls.Add(this.RequestLabel);
		this.Controls.Add(this.RequestTypeDropEdit);
		this.Controls.Add(this.IndirectTypeDropEdit);
		this.Controls.Add(this.NationalCheckBox);
		this.Controls.Add(this.NumberOfDaysCalcEdit);
		this.Controls.Add(this.JustificationTextBox);
		this.Name = "EntryInstructionDetailBasicUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(713, 281, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.LocationOfGoodsUserControl.ResumeLayout(true);
		this.LocationOfGoodsUserControl.PerformLayout();
		this.NewOwnerOrganisationControl.ResumeLayout(true);
		this.NewOwnerOrganisationControl.PerformLayout();
		this.RemoverOrganisationControl.ResumeLayout(true);
		this.RemoverOrganisationControl.PerformLayout();
		this.BondHolderOrganisationControl.ResumeLayout(true);
		this.BondHolderOrganisationControl.PerformLayout();
		this.RequestTypeDropEdit.ResumeLayout(true);
		this.RequestTypeDropEdit.PerformLayout();
		this.IndirectTypeDropEdit.ResumeLayout(true);
		this.IndirectTypeDropEdit.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	internal ZCheckBox ActivateByOperatorCheckBox;
	internal ZCheckBox IncludeRoutingSecurityDataCheckBox;
	internal Enterprise.Customs.EU.GUI.LocationOfGoodsUserControl LocationOfGoodsUserControl;
	internal MasterFiles.GUI.ZOrganisationControl NewOwnerOrganisationControl;
	internal MasterFiles.GUI.ZOrganisationControl RemoverOrganisationControl;
	internal MasterFiles.GUI.ZOrganisationControl BondHolderOrganisationControl;
	internal ZLabel RequestLabel;
	internal ZDropEdit RequestTypeDropEdit;
	internal ZCheckBox NationalCheckBox;
	internal ZCalcEdit NumberOfDaysCalcEdit;
	internal ZTextBox JustificationTextBox;
	internal ZDropEdit IndirectTypeDropEdit;
}
