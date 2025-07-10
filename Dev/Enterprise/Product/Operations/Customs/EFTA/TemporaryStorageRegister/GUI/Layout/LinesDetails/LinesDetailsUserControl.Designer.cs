namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI;

partial class LinesDetailsUserControl
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
		this.UnionStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.PackagesRemainingCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
		this.LineNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
		this.CustomsStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.PackageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.OwnerReferenceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.LimitDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
		this.CustodianEORIBranchUserControl = new CustodianEORIBranchUserControl();
		this.DisposalEntitledTraderEORIBranchUserControl = new DisposalEntitledTraderEORIBranchUserControl();
		this.GoodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.LocationofGoodsTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.OwnerReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.UnionStatusDropEdit.SuspendLayout();
		this.PackagesRemainingCalcEdit.SuspendLayout();
		this.LineNumberCalcEdit.SuspendLayout();
		this.CustomsStatusDropEdit.SuspendLayout();
		this.PackageTypeDropEdit.SuspendLayout();
		this.OwnerReferenceTypeDropEdit.SuspendLayout();
		this.LimitDateEdit.SuspendLayout();
		this.CustodianEORIBranchUserControl.SuspendLayout();
		this.DisposalEntitledTraderEORIBranchUserControl.SuspendLayout();
		this.GoodsDescriptionTextBox.SuspendLayout();
		this.LocationofGoodsTextBox.SuspendLayout();
		this.OwnerReferenceNumberTextBox.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader);
		// 
		// UnionStatusDropEdit
		// 
		this.UnionStatusDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.UnionStatusDropEdit, "CusTempStorageRegLines.SRL_UnionStatus");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_UnionStatus)));
		this.UnionStatusDropEdit.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("f171c7bd-e2f5-4dc9-baa2-80738c083c3e", "Union Status");
		this.UnionStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 136, true);
		this.UnionStatusDropEdit.Name = "UnionStatusDropEdit";
		this.UnionStatusDropEdit.PreBoundMaxLength = 3;
		this.UnionStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 18, true);
		this.UnionStatusDropEdit.TabIndex = 0;
		// 
		// PackagesRemainingCalcEdit
		// 
		this.BindingSource.SetBindingMember(this.PackagesRemainingCalcEdit, "CusTempStorageRegLines.SRL_PackagesRemaining");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_PackagesRemaining)));
		this.PackagesRemainingCalcEdit.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("4c8a24c2-c18b-46a7-b767-f7af55671fd5", "Packages Remaining");
		this.PackagesRemainingCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(532, 32, true);
		this.PackagesRemainingCalcEdit.Name = "PackagesRemainingCalcEdit";
		this.PackagesRemainingCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 18, true);
		this.PackagesRemainingCalcEdit.TabIndex = 1;
		// 
		// LineNumberCalcEdit
		// 
		this.BindingSource.SetBindingMember(this.LineNumberCalcEdit, "CusTempStorageRegLines.SRL_LineNumber");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_LineNumber)));
		this.LineNumberCalcEdit.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("0b546bb6-56be-4b02-921e-4dfff0cb56fd", "Line Number");
		this.LineNumberCalcEdit.DecimalPlaces = 0;
		this.LineNumberCalcEdit.Decimals = 0;
		this.LineNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 6, true);
		this.LineNumberCalcEdit.Name = "LineNumberCalcEdit";
		this.LineNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 18, true);
		this.LineNumberCalcEdit.TabIndex = 2;
		this.LineNumberCalcEdit.Text = "0";
		this.LineNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		// 
		// CustomsStatusDropEdit
		// 
		this.CustomsStatusDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.CustomsStatusDropEdit, "CusTempStorageRegLines.SRL_CustomsStatus");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_CustomsStatus)));
		this.CustomsStatusDropEdit.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("921fc36e-f2f2-4889-965e-c989116459fd", "Customs Status");
		this.CustomsStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(532, 110, true);
		this.CustomsStatusDropEdit.Name = "CustomsStatusDropEdit";
		this.CustomsStatusDropEdit.PreBoundMaxLength = 3;
		this.CustomsStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 18, true);
		this.CustomsStatusDropEdit.TabIndex = 3;
		// 
		// PackageTypeDropEdit
		// 
		this.PackageTypeDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.PackageTypeDropEdit, "CusTempStorageRegLines.SRL_PackageType");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_PackageType)));
		this.PackageTypeDropEdit.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("e41aab3c-e2b1-4816-9971-a64b057143b3", "Package Type");
		this.PackageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(532, 58, true);
		this.PackageTypeDropEdit.Name = "PackageTypeDropEdit";
		this.PackageTypeDropEdit.PreBoundMaxLength = 3;
		this.PackageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 18, true);
		this.PackageTypeDropEdit.TabIndex = 4;
		// 
		// OwnerReferenceTypeDropEdit
		// 
		this.OwnerReferenceTypeDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.OwnerReferenceTypeDropEdit, "CusTempStorageRegLines.SRL_OwnerReferenceType");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_OwnerReferenceType)));
		this.OwnerReferenceTypeDropEdit.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("c2c9a9f4-afbc-49e2-bbdd-e27e83d980d2", "Owner Reference Type");
		this.OwnerReferenceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(532, 84, true);
		this.OwnerReferenceTypeDropEdit.Name = "OwnerReferenceTypeDropEdit";
		this.OwnerReferenceTypeDropEdit.PreBoundMaxLength = 3;
		this.OwnerReferenceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 18, true);
		this.OwnerReferenceTypeDropEdit.TabIndex = 5;
		// 
		// LimitDateEdit
		// 
		this.LimitDateEdit.AllowDrop = true;
		this.LimitDateEdit.AutoCompleteMonthThreshold = 1;
		this.BindingSource.SetBindingMember(this.LimitDateEdit, "CusTempStorageRegLines.SRL_LimitDate");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_LimitDate)));
		this.LimitDateEdit.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("30487013-ac4d-4444-b20c-d2d0784b9fd3", "Limit Date");
		this.LimitDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(532, 6, true);
		this.LimitDateEdit.Name = "LimitDateEdit";
		this.LimitDateEdit.TabIndex = 6;
		// 
		// CustodianEORIBranchUserControl
		//
		this.CustodianEORIBranchUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 110, true);
		this.CustodianEORIBranchUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
		this.CustodianEORIBranchUserControl.Name = "CustodianEORIBranchUserControl";
		this.CustodianEORIBranchUserControl.TabIndex = 7;
		// 
		// DisposalEntitledTraderEORIBranchUserControl
		//
		this.DisposalEntitledTraderEORIBranchUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 210, true);
		this.DisposalEntitledTraderEORIBranchUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
		this.DisposalEntitledTraderEORIBranchUserControl.Name = "DisposalEntitledTraderEORIBranchUserControl";
		this.DisposalEntitledTraderEORIBranchUserControl.TabIndex = 8;
		// 
		// GoodsDescriptionTextBox
		// 
		this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "CusTempStorageRegLines.SRL_GoodsDescription");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_GoodsDescription)));
		this.GoodsDescriptionTextBox.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("b967c82b-d0ab-41a4-b37c-902119ea85c1", "Goods Description");
		this.GoodsDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 84, true);
		this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
		this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 18, true);
		this.GoodsDescriptionTextBox.TabIndex = 8;
		// 
		// LocationofGoodsTextBox
		// 
		this.BindingSource.SetBindingMember(this.LocationofGoodsTextBox, "CusTempStorageRegLines.SRL_LocationOfGoods");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_LocationOfGoods)));
		this.LocationofGoodsTextBox.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("ffa0a1df-86ca-4fe5-a0a4-21f268b84e3f", "Location of Goods");
		this.LocationofGoodsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 32, true);
		this.LocationofGoodsTextBox.Name = "LocationofGoodsTextBox";
		this.LocationofGoodsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 18, true);
		this.LocationofGoodsTextBox.TabIndex = 9;
		// 
		// OwnerReferenceNumberTextBox
		// 
		this.BindingSource.SetBindingMember(this.OwnerReferenceNumberTextBox, "CusTempStorageRegLines.SRL_OwnerReference");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).SRL_OwnerReference)));
		this.OwnerReferenceNumberTextBox.CaptionResourceString = Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Res.GetData("83ff98e2-004b-434b-b0f9-6861e52b3f60", "Owner Reference Number");
		this.OwnerReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		this.OwnerReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 58, true);
		this.OwnerReferenceNumberTextBox.Name = "OwnerReferenceNumberTextBox";
		this.OwnerReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 18, true);
		this.OwnerReferenceNumberTextBox.TabIndex = 10;
		// 
		// LinesDetailsUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.UnionStatusDropEdit);
		this.Controls.Add(this.PackagesRemainingCalcEdit);
		this.Controls.Add(this.LineNumberCalcEdit);
		this.Controls.Add(this.CustomsStatusDropEdit);
		this.Controls.Add(this.PackageTypeDropEdit);
		this.Controls.Add(this.OwnerReferenceTypeDropEdit);
		this.Controls.Add(this.LimitDateEdit);
		this.Controls.Add(this.CustodianEORIBranchUserControl);
		this.Controls.Add(this.DisposalEntitledTraderEORIBranchUserControl);
		this.Controls.Add(this.GoodsDescriptionTextBox);
		this.Controls.Add(this.LocationofGoodsTextBox);
		this.Controls.Add(this.OwnerReferenceNumberTextBox);
		this.Name = "LinesDetailsUserControl";
		this.ShouldSerializeTabPageMethods = true;
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 177, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.UnionStatusDropEdit.ResumeLayout(true);
		this.UnionStatusDropEdit.PerformLayout();
		this.PackagesRemainingCalcEdit.ResumeLayout(true);
		this.PackagesRemainingCalcEdit.PerformLayout();
		this.LineNumberCalcEdit.ResumeLayout(true);
		this.LineNumberCalcEdit.PerformLayout();
		this.CustomsStatusDropEdit.ResumeLayout(true);
		this.CustomsStatusDropEdit.PerformLayout();
		this.PackageTypeDropEdit.ResumeLayout(true);
		this.PackageTypeDropEdit.PerformLayout();
		this.OwnerReferenceTypeDropEdit.ResumeLayout(true);
		this.OwnerReferenceTypeDropEdit.PerformLayout();
		this.LimitDateEdit.ResumeLayout(true);
		this.LimitDateEdit.PerformLayout();
		this.CustodianEORIBranchUserControl.ResumeLayout(true);
		this.CustodianEORIBranchUserControl.PerformLayout();
		this.DisposalEntitledTraderEORIBranchUserControl.ResumeLayout(true);
		this.DisposalEntitledTraderEORIBranchUserControl.PerformLayout();
		this.GoodsDescriptionTextBox.ResumeLayout(true);
		this.GoodsDescriptionTextBox.PerformLayout();
		this.LocationofGoodsTextBox.ResumeLayout(true);
		this.LocationofGoodsTextBox.PerformLayout();
		this.OwnerReferenceNumberTextBox.ResumeLayout(true);
		this.OwnerReferenceNumberTextBox.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();
	}

	#endregion

	internal ZArchitecture.GUI.ZDropEdit UnionStatusDropEdit;
	internal ZArchitecture.ZCalcEdit PackagesRemainingCalcEdit;
	internal ZArchitecture.ZCalcEdit LineNumberCalcEdit;
	internal ZArchitecture.GUI.ZDropEdit CustomsStatusDropEdit;
	internal ZArchitecture.GUI.ZDropEdit PackageTypeDropEdit;
	internal ZArchitecture.GUI.ZDropEdit OwnerReferenceTypeDropEdit;
	internal ZArchitecture.GUI.ZDateEdit LimitDateEdit;
	internal ZArchitecture.GUI.ZUserControl CustodianEORIBranchUserControl;
	internal ZArchitecture.GUI.ZUserControl DisposalEntitledTraderEORIBranchUserControl;
	internal ZArchitecture.ZTextBox GoodsDescriptionTextBox;
	internal ZArchitecture.ZTextBox LocationofGoodsTextBox;
	internal ZArchitecture.ZTextBox OwnerReferenceNumberTextBox;
}
