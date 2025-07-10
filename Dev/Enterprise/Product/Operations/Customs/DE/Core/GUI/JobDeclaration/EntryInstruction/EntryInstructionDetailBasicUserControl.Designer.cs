using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	partial class EntryInstructionDetailBasicUserControl
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
			this.OtherPartiesDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OtherPartiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
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
			this.DetailsLayoutControl = new Enterprise.Customs.DE.GUI.EntryInstructionDetailsLayoutControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OtherPartiesDetailsPanel.SuspendLayout();
			this.OtherPartiesGroupBox.SuspendLayout();
			this.BondHolderRemoverPanel.SuspendLayout();
			this.NewOwnerOrganisationControl.SuspendLayout();
			this.RemoverOrganisationControl.SuspendLayout();
			this.BondHolderOrganisationControl.SuspendLayout();
			this.FromWarehouseGroupBox.SuspendLayout();
			this.FromWarehouseAddressControl.SuspendLayout();
			this.ToWarehouseGroupBox.SuspendLayout();
			this.ToWarehouseAddressControl.SuspendLayout();
			this.DetailsLayoutControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.JobDeclaration);
			// 
			// OtherPartiesDetailsPanel
			// 
			this.OtherPartiesDetailsPanel.AutoScroll = true;
			this.OtherPartiesDetailsPanel.Controls.Add(this.OtherPartiesGroupBox);
			this.OtherPartiesDetailsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.OtherPartiesDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 116, true);
			this.OtherPartiesDetailsPanel.Name = "OtherPartiesDetailsPanel";
			this.OtherPartiesDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1097, 180, true);
			this.OtherPartiesDetailsPanel.TabIndex = 2;
			// 
			// OtherPartiesGroupBox
			// 
			this.OtherPartiesGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("B1CF6F18-B6FA-4ABD-89E8-ECB5B4D3A344", "Other Parties");
			this.OtherPartiesGroupBox.Controls.Add(this.BondHolderRemoverPanel);
			this.OtherPartiesGroupBox.Controls.Add(this.FromWarehouseGroupBox);
			this.OtherPartiesGroupBox.Controls.Add(this.ToWarehouseGroupBox);
			this.OtherPartiesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OtherPartiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OtherPartiesGroupBox.Name = "OtherPartiesGroupBox";
			this.OtherPartiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1097, 180, true);
			this.OtherPartiesGroupBox.TabIndex = 2;
			this.OtherPartiesGroupBox.TabStop = false;
			// 
			// BondHolderRemoverPanel
			// 
			this.BondHolderRemoverPanel.Controls.Add(this.NewOwnerOrganisationControl);
			this.BondHolderRemoverPanel.Controls.Add(this.RemoverOrganisationControl);
			this.BondHolderRemoverPanel.Controls.Add(this.BondHolderOrganisationControl);
			this.BondHolderRemoverPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.BondHolderRemoverPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 98, true);
			this.BondHolderRemoverPanel.Name = "BondHolderRemoverPanel";
			this.BondHolderRemoverPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1091, 80, true);
			this.BondHolderRemoverPanel.TabIndex = 3;
			// 
			// NewOwnerOrganisationControl
			// 
			this.NewOwnerOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NewOwnerOrganisationControl, "CustomsEntryInstructions.CEI_OH_Owner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OH_Owner)));
			this.NewOwnerOrganisationControl.BindToOrganisations = "Lookups+Organisations";
			this.NewOwnerOrganisationControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("FC6515E1-141A-430D-93FA-4B4EA3E0E998", "New Owner");
			this.NewOwnerOrganisationControl.Captions = new string[] {
        "New Owner"};
			this.NewOwnerOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
			this.NewOwnerOrganisationControl.IsCaptionOverridden = false;
			this.NewOwnerOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
			this.NewOwnerOrganisationControl.Name = "NewOwnerOrganisationControl";
			this.NewOwnerOrganisationControl.OrgAddressFormatter = null;
			this.NewOwnerOrganisationControl.PopupCaption = "";
			this.NewOwnerOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 39, true);
			this.NewOwnerOrganisationControl.TabIndex = 2;
			// 
			// RemoverOrganisationControl
			// 
			this.RemoverOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RemoverOrganisationControl, "CustomsEntryInstructions.CEI_OH_Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OH_Carrier)));
			this.RemoverOrganisationControl.BindToOrganisations = "Lookups+CarrierOrganisations";
			this.RemoverOrganisationControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("C0F80376-AC9F-4131-AD76-1FCE0A20F3D1", "Remover");
			this.RemoverOrganisationControl.Captions = new string[] {
        "Remover"};
			this.RemoverOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
			this.RemoverOrganisationControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.RemoverOrganisationControl.IsCaptionOverridden = false;
			this.RemoverOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 0, true);
			this.RemoverOrganisationControl.Name = "RemoverOrganisationControl";
			this.RemoverOrganisationControl.OrgAddressFormatter = null;
			this.RemoverOrganisationControl.PopupCaption = "";
			this.RemoverOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 39, true);
			this.RemoverOrganisationControl.TabIndex = 1;
			// 
			// BondHolderOrganisationControl
			// 
			this.BondHolderOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BondHolderOrganisationControl, "CustomsEntryInstructions.CEI_OH_BondHolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OH_BondHolder)));
			this.BondHolderOrganisationControl.BindToOrganisations = "Lookups+Organisations";
			this.BondHolderOrganisationControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("4D4B91D9-7E7E-4C79-8D94-9BC4A3073EB4", "Bond Holder");
			this.BondHolderOrganisationControl.Captions = new string[] {
        "Bond Holder"};
			this.BondHolderOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
			this.BondHolderOrganisationControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.BondHolderOrganisationControl.IsCaptionOverridden = false;
			this.BondHolderOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BondHolderOrganisationControl.Name = "BondHolderOrganisationControl";
			this.BondHolderOrganisationControl.OrgAddressFormatter = null;
			this.BondHolderOrganisationControl.PopupCaption = "";
			this.BondHolderOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 39, true);
			this.BondHolderOrganisationControl.TabIndex = 0;
			// 
			// FromWarehouseGroupBox
			// 
			this.FromWarehouseGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("7B7DE5AA-998D-475E-98AC-729AD025EE5E", "From Warehouse");
			this.FromWarehouseGroupBox.Controls.Add(this.FromWarehouseCodeTextBox);
			this.FromWarehouseGroupBox.Controls.Add(this.FromWarehouseAddressControl);
			this.FromWarehouseGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.FromWarehouseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 57, true);
			this.FromWarehouseGroupBox.Name = "FromWarehouseGroupBox";
			this.FromWarehouseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1091, 41, true);
			this.FromWarehouseGroupBox.TabIndex = 1;
			this.FromWarehouseGroupBox.TabStop = false;
			// 
			// FromWarehouseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.FromWarehouseCodeTextBox, "CustomsEntryInstructions.FromWarehouseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).FromWarehouseCode)));
			this.FromWarehouseCodeTextBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("6412FBDA-CFF0-445C-93CA-8047C8DF7BBC", "From Warehouse Code");
			this.FromWarehouseCodeTextBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.FromWarehouseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 16, true);
			this.FromWarehouseCodeTextBox.Name = "FromWarehouseCodeTextBox";
			this.FromWarehouseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.FromWarehouseCodeTextBox.TabIndex = 3;
			// 
			// FromWarehouseAddressControl
			// 
			this.FromWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FromWarehouseAddressControl, "CustomsEntryInstructions.CEI_OA_Warehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OA_Warehouse)));
			this.FromWarehouseAddressControl.BindToOrgList = "Lookups.BondedWarehouseCollection";
			this.FromWarehouseAddressControl.Dock = System.Windows.Forms.DockStyle.Left;
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
			this.ToWarehouseGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("A1BF6A34-53CF-4CFA-A888-4FB4DF4C846F", "To Warehouse");
			this.ToWarehouseGroupBox.Controls.Add(this.ToWarehouseCodeTextBox);
			this.ToWarehouseGroupBox.Controls.Add(this.ToWarehouseAddressControl);
			this.ToWarehouseGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ToWarehouseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ToWarehouseGroupBox.Name = "ToWarehouseGroupBox";
			this.ToWarehouseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1091, 41, true);
			this.ToWarehouseGroupBox.TabIndex = 0;
			this.ToWarehouseGroupBox.TabStop = false;
			// 
			// ToWarehouseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ToWarehouseCodeTextBox, "CustomsEntryInstructions.ToWarehouseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ToWarehouseCode)));
			this.ToWarehouseCodeTextBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("FDE6BCE3-9CBA-446A-BA31-38940098CCFC", "To Warehouse Code");
			this.ToWarehouseCodeTextBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ToWarehouseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(294, 16, true);
			this.ToWarehouseCodeTextBox.Name = "ToWarehouseCodeTextBox";
			this.ToWarehouseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.ToWarehouseCodeTextBox.TabIndex = 3;
			// 
			// ToWarehouseAddressControl
			// 
			this.ToWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ToWarehouseAddressControl, "CustomsEntryInstructions.CEI_OA_Warehouse2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OA_Warehouse2)));
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
			// DetailsLayoutControl
			// 
			this.DetailsLayoutControl.AllowDrop = true;
			this.DetailsLayoutControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.DetailsLayoutControl, "CustomsEntryInstructions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)))));
			this.DetailsLayoutControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.DetailsLayoutControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsLayoutControl.Name = "DetailsLayoutControl";
			this.DetailsLayoutControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1097, 116, true);
			this.DetailsLayoutControl.TabIndex = 0;
			// 
			// EntryInstructionDetailBasicUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OtherPartiesDetailsPanel);
			this.Controls.Add(this.DetailsLayoutControl);
			this.Name = "EntryInstructionDetailBasicUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1097, 405, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OtherPartiesDetailsPanel.ResumeLayout(false);
			this.OtherPartiesDetailsPanel.PerformLayout();
			this.OtherPartiesGroupBox.ResumeLayout(false);
			this.OtherPartiesGroupBox.PerformLayout();
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
			this.DetailsLayoutControl.ResumeLayout(true);
			this.DetailsLayoutControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private EntryInstructionDetailsLayoutControl DetailsLayoutControl;
		protected ZArchitecture.GUI.ZPanel OtherPartiesDetailsPanel;
		protected ZArchitecture.ZTextBox ToWarehouseCodeTextBox;
		protected ZArchitecture.ZTextBox FromWarehouseCodeTextBox;
		protected ZArchitecture.GUI.ZGroupBox FromWarehouseGroupBox;
		protected ZArchitecture.GUI.ZPanel BondHolderRemoverPanel;
		private ZGroupBox OtherPartiesGroupBox;
		private ZGroupBox ToWarehouseGroupBox;
		private ZAddressControl ToWarehouseAddressControl;
		private ZAddressControl FromWarehouseAddressControl;
		private MasterFiles.GUI.ZOrganisationControl BondHolderOrganisationControl;
		private MasterFiles.GUI.ZOrganisationControl RemoverOrganisationControl;
		private MasterFiles.GUI.ZOrganisationControl NewOwnerOrganisationControl;

		#endregion
	}
}
