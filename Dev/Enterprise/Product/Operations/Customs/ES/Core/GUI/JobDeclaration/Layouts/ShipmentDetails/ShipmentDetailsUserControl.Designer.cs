namespace Enterprise.Customs.ES.GUI;

partial class ShipmentDetailsUserControl
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
		this.RegionOrTerritoryOfDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
		this.RegionOrTerritoryOfDestinationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.DestinationStateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.PartialWriteoffCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
		this.GoodsLocationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
		this.ShipmentDetailsOriginUserControl = new Enterprise.Customs.ES.GUI.ShipmentDetailsOriginUserControl();
		this.ShipmentDetailsFinalDestinationUserControl = new Enterprise.Customs.ES.GUI.ShipmentDetailsFinalDestinationUserControl();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.RegionOrTerritoryOfDestinationCodeFindBox.SuspendLayout();
		this.RegionOrTerritoryOfDestinationDropEdit.SuspendLayout();
		this.DestinationStateDropEdit.SuspendLayout();
		this.GoodsLocationCodeFindBox.SuspendLayout();
		this.ShipmentDetailsOriginUserControl.SuspendLayout();
		this.ShipmentDetailsFinalDestinationUserControl.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.JobDeclaration);
		// 
		// RegionOrTerritoryOfDestinationCodeFindBox
		// 
		this.RegionOrTerritoryOfDestinationCodeFindBox.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.RegionOrTerritoryOfDestinationCodeFindBox, "EUD_RegionOrTerritoryOfDestination");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).EUD_RegionOrTerritoryOfDestination)));
		this.RegionOrTerritoryOfDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 251, true);
		this.RegionOrTerritoryOfDestinationCodeFindBox.Name = "RegionOrTerritoryOfDestinationCodeFindBox";
		this.RegionOrTerritoryOfDestinationCodeFindBox.BindToList = "AddInfoChildLookups+RegionOrTerritoryOfDestinationList";
		this.RegionOrTerritoryOfDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 17, true);
		this.RegionOrTerritoryOfDestinationCodeFindBox.TabIndex = 10;
		// 
		// RegionOrTerritoryOfDestinationDropEdit
		// 
		this.RegionOrTerritoryOfDestinationDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.RegionOrTerritoryOfDestinationDropEdit, "EUD_RegionOrTerritoryOfDestination");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).EUD_RegionOrTerritoryOfDestination)));
		this.RegionOrTerritoryOfDestinationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 251, true);
		this.RegionOrTerritoryOfDestinationDropEdit.Name = "RegionOrTerritoryOfDestinationDropEdit";
		this.RegionOrTerritoryOfDestinationDropEdit.BindToList = "AddInfoChildLookups+RegionOrTerritoryOfDestinationList";
		this.RegionOrTerritoryOfDestinationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 17, true);
		this.RegionOrTerritoryOfDestinationDropEdit.TabIndex = 9;
		// 
		// DestinationStateDropEdit
		// 
		this.DestinationStateDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.DestinationStateDropEdit, "ZG_DestinationState");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).ZG_DestinationState)));
		this.DestinationStateDropEdit.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("C7C6CDCD-296A-4DA6-A222-DEADA1B588C1", "[17b] Dest. State/Island");
		this.DestinationStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 13, true);
		this.DestinationStateDropEdit.Name = "DestinationStateDropEdit";
		this.DestinationStateDropEdit.PreBoundMaxLength = 2;
		this.DestinationStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
		this.DestinationStateDropEdit.TabIndex = 8;
		// 
		// PartialWriteoffCheckBox
		// 
		this.PartialWriteoffCheckBox.AutoSize = true;
		this.BindingSource.SetBindingMember(this.PartialWriteoffCheckBox, "ZG_PartialWriteoff");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).ZG_PartialWriteoff)));
		this.PartialWriteoffCheckBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("B03A8475-0482-481F-89F3-33B5F32A5750", "Ceuta/Melilla: B/L write off");
		this.PartialWriteoffCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 51, true);
		this.PartialWriteoffCheckBox.Name = "PartialWriteoffCheckBox";
		this.PartialWriteoffCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 17, true);
		this.PartialWriteoffCheckBox.TabIndex = 2;
		this.PartialWriteoffCheckBox.UseVisualStyleBackColor = true;
		// 
		// GoodsLocationCodeFindBox
		// 
		this.GoodsLocationCodeFindBox.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.GoodsLocationCodeFindBox, "JE_LocationOfGoods");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).JE_LocationOfGoods)));
		this.GoodsLocationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 85, true);
		this.GoodsLocationCodeFindBox.Name = "GoodsLocationCodeFindBox";
		this.GoodsLocationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
		this.GoodsLocationCodeFindBox.ParentType = null;
		this.GoodsLocationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 20, true);
		this.GoodsLocationCodeFindBox.TabIndex = 11;
		// 
		// ShipmentDetailsOriginUserControl
		// 
		this.ShipmentDetailsOriginUserControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.ShipmentDetailsOriginUserControl, ".");
		this.ShipmentDetailsOriginUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 29, true);
		this.ShipmentDetailsOriginUserControl.Name = "ShipmentDetailsOriginUserControl";
		this.ShipmentDetailsOriginUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
		this.ShipmentDetailsOriginUserControl.TabIndex = 0;
		// 
		// ShipmentDetailsFinalDestinationUserControl
		// 
		this.ShipmentDetailsFinalDestinationUserControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.ShipmentDetailsFinalDestinationUserControl, ".");
		this.ShipmentDetailsFinalDestinationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.ShipmentDetailsFinalDestinationUserControl.Name = "ShipmentDetailsFinalDestinationUserControl";
		this.ShipmentDetailsFinalDestinationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
		this.ShipmentDetailsFinalDestinationUserControl.TabIndex = 1;
		// 
		// ShipmentDetailsUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.RegionOrTerritoryOfDestinationCodeFindBox);
		this.Controls.Add(this.RegionOrTerritoryOfDestinationDropEdit);
		this.Controls.Add(this.DestinationStateDropEdit);
		this.Controls.Add(this.PartialWriteoffCheckBox);
		this.Controls.Add(this.GoodsLocationCodeFindBox);
		this.Controls.Add(this.ShipmentDetailsOriginUserControl);
		this.Controls.Add(this.ShipmentDetailsFinalDestinationUserControl);
		this.Name = "ShipmentDetailsUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 193, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.RegionOrTerritoryOfDestinationCodeFindBox.ResumeLayout(true);
		this.RegionOrTerritoryOfDestinationCodeFindBox.PerformLayout();
		this.RegionOrTerritoryOfDestinationDropEdit.ResumeLayout(true);
		this.RegionOrTerritoryOfDestinationDropEdit.PerformLayout();
		this.DestinationStateDropEdit.ResumeLayout(true);
		this.DestinationStateDropEdit.PerformLayout();
		this.GoodsLocationCodeFindBox.ResumeLayout(true);
		this.GoodsLocationCodeFindBox.PerformLayout();
		this.ShipmentDetailsOriginUserControl.ResumeLayout(true);
		this.ShipmentDetailsOriginUserControl.PerformLayout();
		this.ShipmentDetailsFinalDestinationUserControl.ResumeLayout(true);
		this.ShipmentDetailsFinalDestinationUserControl.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	internal ZArchitecture.GUI.ZCodeFindBox RegionOrTerritoryOfDestinationCodeFindBox;
	internal ZArchitecture.GUI.ZDropEdit RegionOrTerritoryOfDestinationDropEdit;
	internal ZArchitecture.GUI.ZDropEdit DestinationStateDropEdit;
	internal ZArchitecture.GUI.ZCheckBox PartialWriteoffCheckBox;
	internal ZArchitecture.GUI.ZCodeFindBox GoodsLocationCodeFindBox;
	internal ES.GUI.ShipmentDetailsOriginUserControl ShipmentDetailsOriginUserControl;
	internal ES.GUI.ShipmentDetailsFinalDestinationUserControl ShipmentDetailsFinalDestinationUserControl;
}
