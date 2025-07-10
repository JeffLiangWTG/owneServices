using System.ComponentModel;
using Enterprise.Customs.IN.Business;

namespace Enterprise.Customs.IN.GUI;

partial class EntryInstructionOtherPartiesUserControl
{
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private IContainer components = null;

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
		this.OtherPartiesDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
		this.BondHolderRemoverPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
		this.NewOwnerOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
		this.RemoverOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
		this.BondHolderOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
		this.FromWarehouseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
		this.FromWarehouseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.FromWarehouseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
		this.ToWarehouseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
		this.ToWarehouseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.ToWarehouseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.OtherPartiesDetailsPanel.SuspendLayout();
		this.BondHolderRemoverPanel.SuspendLayout();
		this.NewOwnerOrganisationControl.SuspendLayout();
		this.RemoverOrganisationControl.SuspendLayout();
		this.BondHolderOrganisationControl.SuspendLayout();
		this.FromWarehouseGroupBox.SuspendLayout();
		this.FromWarehouseAddressControl.SuspendLayout();
		this.ToWarehouseGroupBox.SuspendLayout();
		this.ToWarehouseAddressControl.SuspendLayout();
		this.SuspendLayout();
		//
		// OtherPartiesDetailsPanel
		//
		this.OtherPartiesDetailsPanel.AutoScroll = true;
		this.OtherPartiesDetailsPanel.Controls.Add(this.BondHolderRemoverPanel);
 		this.OtherPartiesDetailsPanel.Controls.Add(this.FromWarehouseGroupBox);
 		this.OtherPartiesDetailsPanel.Controls.Add(this.ToWarehouseGroupBox);
		this.OtherPartiesDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.OtherPartiesDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.OtherPartiesDetailsPanel.Name = "OtherPartiesDetailsPanel";
		this.OtherPartiesDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 259, true);
		this.OtherPartiesDetailsPanel.TabIndex = 2;
		//
		// BondHolderRemoverPanel
		//
		this.BondHolderRemoverPanel.Controls.Add(this.NewOwnerOrganisationControl);
		this.BondHolderRemoverPanel.Controls.Add(this.RemoverOrganisationControl);
		this.BondHolderRemoverPanel.Controls.Add(this.BondHolderOrganisationControl);
		this.BondHolderRemoverPanel.Dock = System.Windows.Forms.DockStyle.Top;
		this.BondHolderRemoverPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 98, true);
		this.BondHolderRemoverPanel.Name = "BondHolderRemoverPanel";
		this.BondHolderRemoverPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 80, true);
		this.BondHolderRemoverPanel.TabIndex = 2;
		//
		// NewOwnerOrganisationControl
		//
		this.NewOwnerOrganisationControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.NewOwnerOrganisationControl, "CEI_OH_Owner");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OH_Owner)));
		this.NewOwnerOrganisationControl.BindToOrganisations = "Lookups+Organisations";
		this.NewOwnerOrganisationControl.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("3782C626-DC13-4C2D-9E66-9EDB4776C7DA", "New Owner");
		this.NewOwnerOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
		this.NewOwnerOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
		this.NewOwnerOrganisationControl.Name = "NewOwnerOrganisationControl";
		this.NewOwnerOrganisationControl.PopupCaption = "";
		this.NewOwnerOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 39, true);
		this.NewOwnerOrganisationControl.TabIndex = 3;
		//
		// RemoverOrganisationControl
		//
		this.RemoverOrganisationControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.RemoverOrganisationControl, "CEI_OH_Carrier");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OH_Carrier)));
		this.RemoverOrganisationControl.BindToOrganisations = "Lookups+CarrierOrganisations";
		this.RemoverOrganisationControl.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("FE835217-C3B1-43B5-B67E-F53A7F111861", "Remover");
		this.RemoverOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
		this.RemoverOrganisationControl.Dock = System.Windows.Forms.DockStyle.Left;
		this.RemoverOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 0, true);
		this.RemoverOrganisationControl.Name = "RemoverOrganisationControl";
		this.RemoverOrganisationControl.PopupCaption = "";
		this.RemoverOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 39, true);
		this.RemoverOrganisationControl.TabIndex = 2;
		//
		// BondHolderOrganisationControl
		//
		this.BondHolderOrganisationControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.BondHolderOrganisationControl, "CEI_OH_BondHolder");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OH_BondHolder)));
		this.BondHolderOrganisationControl.BindToOrganisations = "Lookups+Organisations";
		this.BondHolderOrganisationControl.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("A0E1D749-8121-4A24-90B9-F67DDDEF3DDD", "Bond Holder");
		this.BondHolderOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
		this.BondHolderOrganisationControl.Dock = System.Windows.Forms.DockStyle.Left;
		this.BondHolderOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.BondHolderOrganisationControl.Name = "BondHolderOrganisationControl";
		this.BondHolderOrganisationControl.PopupCaption = "";
		this.BondHolderOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 39, true);
		this.BondHolderOrganisationControl.TabIndex = 1;
		//
		// FromWarehouseGroupBox
		//
		this.FromWarehouseGroupBox.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("20055EBD-7224-4323-88D6-BA64BE04CEF9", "From Warehouse");
		this.FromWarehouseGroupBox.Controls.Add(this.FromWarehouseCodeTextBox);
		this.FromWarehouseGroupBox.Controls.Add(this.FromWarehouseAddressControl);
		this.FromWarehouseGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
		this.FromWarehouseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 57, true);
		this.FromWarehouseGroupBox.Name = "FromWarehouseGroupBox";
		this.FromWarehouseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 41, true);
		this.FromWarehouseGroupBox.TabIndex = 4;
		this.FromWarehouseGroupBox.TabStop = false;
		//
		// FromWarehouseCodeTextBox
		//
		this.BindingSource.SetBindingMember(this.FromWarehouseCodeTextBox, "FromWarehouseCode");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).FromWarehouseCode)));
		this.FromWarehouseCodeTextBox.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("490CEA97-F0D7-4D84-98CB-A841DA06341C", "From Warehouse Code");
		this.FromWarehouseCodeTextBox.Dock = System.Windows.Forms.DockStyle.Left;
		this.FromWarehouseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 16, true);
		this.FromWarehouseCodeTextBox.Name = "FromWarehouseCodeTextBox";
		this.FromWarehouseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
		this.FromWarehouseCodeTextBox.TabIndex = 3;
		//
		// FromWarehouseAddressControl
		//
		this.FromWarehouseAddressControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.FromWarehouseAddressControl, "CEI_OA_Warehouse");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OA_Warehouse)));
		this.FromWarehouseAddressControl.BindToOrgList = "Lookups.BondedWarehouseCollection";
		this.FromWarehouseAddressControl.Dock = System.Windows.Forms.DockStyle.Left;
		this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.FromWarehouseAddressControl, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
		this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FromWarehouseAddressControl, false);
		this.FromWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
		this.FromWarehouseAddressControl.Name = "FromWarehouseAddressControl";
		this.FromWarehouseAddressControl.PopupCaption = "";
		this.FromWarehouseAddressControl.ShowAddress = false;
		this.FromWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 22, true);
		this.FromWarehouseAddressControl.TabIndex = 0;
		//
		// ToWarehouseGroupBox
		//
		this.ToWarehouseGroupBox.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("726974AB-5AFE-4A7B-857C-AC4CF1E15C8C", "To Warehouse");
		this.ToWarehouseGroupBox.Controls.Add(this.ToWarehouseCodeTextBox);
		this.ToWarehouseGroupBox.Controls.Add(this.ToWarehouseAddressControl);
		this.ToWarehouseGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
		this.ToWarehouseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
		this.ToWarehouseGroupBox.Name = "ToWarehouseGroupBox";
		this.ToWarehouseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 41, true);
		this.ToWarehouseGroupBox.TabIndex = 5;
		this.ToWarehouseGroupBox.TabStop = false;
		//
		// ToWarehouseCodeTextBox
		//
		this.BindingSource.SetBindingMember(this.ToWarehouseCodeTextBox, "ToWarehouseCode");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ToWarehouseCode)));
		this.ToWarehouseCodeTextBox.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("0E957D93-081A-4EA2-8591-CFAE3C7D2088", "To Warehouse Code");
		this.ToWarehouseCodeTextBox.Dock = System.Windows.Forms.DockStyle.Left;
		this.ToWarehouseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 16, true);
		this.ToWarehouseCodeTextBox.Name = "ToWarehouseCodeTextBox";
		this.ToWarehouseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
		this.ToWarehouseCodeTextBox.TabIndex = 3;
		//
		// ToWarehouseAddressControl
		//
		this.ToWarehouseAddressControl.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.ToWarehouseAddressControl, "CEI_OA_Warehouse2");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OA_Warehouse2)));
		this.ToWarehouseAddressControl.BindToOrgList = "Lookups.BondedWarehouseCollection";
		this.ToWarehouseAddressControl.Dock = System.Windows.Forms.DockStyle.Left;
		this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ToWarehouseAddressControl, false);
		this.ToWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
		this.ToWarehouseAddressControl.Name = "ToWarehouseAddressControl";
		this.ToWarehouseAddressControl.PopupCaption = "";
		this.ToWarehouseAddressControl.ShowAddress = false;
		this.ToWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 22, true);
		this.ToWarehouseAddressControl.TabIndex = 0;

		//
		// EntryInstructionOtherPartiesUserControl
		//
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(OtherPartiesDetailsPanel);
		this.Name = "EntryInstructionOtherPartiesUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 385, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.OtherPartiesDetailsPanel.ResumeLayout(false);
		this.OtherPartiesDetailsPanel.PerformLayout();
		this.BondHolderRemoverPanel.ResumeLayout(false);
		this.BondHolderRemoverPanel.PerformLayout();
		this.NewOwnerOrganisationControl.ResumeLayout(true);
		this.NewOwnerOrganisationControl.PerformLayout();
		this.RemoverOrganisationControl.ResumeLayout(true);
		this.RemoverOrganisationControl.PerformLayout();
		this.BondHolderOrganisationControl.ResumeLayout(true);
		this.BondHolderOrganisationControl.PerformLayout();
		this.FromWarehouseGroupBox.ResumeLayout(false);
		this.FromWarehouseGroupBox.PerformLayout();
		this.FromWarehouseAddressControl.ResumeLayout(true);
		this.FromWarehouseAddressControl.PerformLayout();
		this.ToWarehouseGroupBox.ResumeLayout(false);
		this.ToWarehouseGroupBox.PerformLayout();
		this.ToWarehouseAddressControl.ResumeLayout(true);
		this.ToWarehouseAddressControl.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion
	protected ZArchitecture.GUI.ZPanel OtherPartiesDetailsPanel;
	protected ZArchitecture.GUI.ZGroupBox ToWarehouseGroupBox;
	protected ZArchitecture.GUI.ZAddressControl ToWarehouseAddressControl;
	protected ZArchitecture.ZTextBox ToWarehouseCodeTextBox;
	protected ZArchitecture.ZTextBox FromWarehouseCodeTextBox;
	protected ZArchitecture.GUI.ZGroupBox FromWarehouseGroupBox;
	protected ZArchitecture.GUI.ZAddressControl FromWarehouseAddressControl;
	protected MasterFiles.GUI.ZOrganisationControl BondHolderOrganisationControl;
	protected MasterFiles.GUI.ZOrganisationControl RemoverOrganisationControl;
	protected MasterFiles.GUI.ZOrganisationControl NewOwnerOrganisationControl;
	protected ZArchitecture.GUI.ZPanel BondHolderRemoverPanel;
}

