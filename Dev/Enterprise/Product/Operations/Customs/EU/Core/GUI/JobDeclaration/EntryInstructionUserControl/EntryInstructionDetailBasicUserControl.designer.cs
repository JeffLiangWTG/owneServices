using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
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
			this.DetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.StyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
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
			this.DetailsPanel.SuspendLayout();
			this.StyleDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// OtherPartiesDetailsPanel
			// 
			this.OtherPartiesDetailsPanel.AutoScroll = true;
			this.OtherPartiesDetailsPanel.Controls.Add(this.OtherPartiesGroupBox);
			this.OtherPartiesDetailsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.OtherPartiesDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 42, true);
			this.OtherPartiesDetailsPanel.Name = "OtherPartiesDetailsPanel";
			this.OtherPartiesDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1097, 180, true);
			this.OtherPartiesDetailsPanel.TabIndex = 6;
			// 
			// OtherPartiesGroupBox
			// 
			this.OtherPartiesGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("fc9bb418-e121-4e11-8002-8c88a4bf2dd5", "Other Parties");
			this.OtherPartiesGroupBox.Controls.Add(this.BondHolderRemoverPanel);
			this.OtherPartiesGroupBox.Controls.Add(this.FromWarehouseGroupBox);
			this.OtherPartiesGroupBox.Controls.Add(this.ToWarehouseGroupBox);
			this.OtherPartiesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OtherPartiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OtherPartiesGroupBox.Name = "OtherPartiesGroupBox";
			this.OtherPartiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1097, 180, true);
			this.OtherPartiesGroupBox.TabIndex = 1;
			this.OtherPartiesGroupBox.TabStop = false;
			// 
			// BondHolderRemoverPanel
			// 
			this.BondHolderRemoverPanel.Controls.Add(this.NewOwnerOrganisationControl);
			this.BondHolderRemoverPanel.Controls.Add(this.RemoverOrganisationControl);
			this.BondHolderRemoverPanel.Controls.Add(this.BondHolderOrganisationControl);
			this.BondHolderRemoverPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.BondHolderRemoverPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 97, true);
			this.BondHolderRemoverPanel.Name = "BondHolderRemoverPanel";
			this.BondHolderRemoverPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1094, 80, true);
			this.BondHolderRemoverPanel.TabIndex = 2;
			// 
			// NewOwnerOrganisationControl
			// 
			this.NewOwnerOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NewOwnerOrganisationControl, "CustomsEntryInstructions.CEI_OH_Owner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OH_Owner)));
			this.NewOwnerOrganisationControl.BindToOrganisations = "Lookups+Organisations";
			this.NewOwnerOrganisationControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("fddd5257-6618-4d82-9da5-90216c7602f7", "New Owner");
			this.NewOwnerOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
			this.NewOwnerOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
			this.NewOwnerOrganisationControl.Name = "NewOwnerOrganisationControl";
			this.NewOwnerOrganisationControl.PopupCaption = "";
			this.NewOwnerOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 35, true);
			this.NewOwnerOrganisationControl.TabIndex = 3;
			// 
			// RemoverOrganisationControl
			// 
			this.RemoverOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RemoverOrganisationControl, "CustomsEntryInstructions.CEI_OH_Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OH_Carrier)));
			this.RemoverOrganisationControl.BindToOrganisations = "Lookups+CarrierOrganisations";
			this.RemoverOrganisationControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("19f5f1b1-e008-43cc-a5c7-a1c57279f8fe", "Remover");
			this.RemoverOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
			this.RemoverOrganisationControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.RemoverOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 0, true);
			this.RemoverOrganisationControl.Name = "RemoverOrganisationControl";
			this.RemoverOrganisationControl.PopupCaption = "";
			this.RemoverOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 35, true);
			this.RemoverOrganisationControl.TabIndex = 2;
			// 
			// BondHolderOrganisationControl
			// 
			this.BondHolderOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BondHolderOrganisationControl, "CustomsEntryInstructions.CEI_OH_BondHolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OH_BondHolder)));
			this.BondHolderOrganisationControl.BindToOrganisations = "Lookups+Organisations";
			this.BondHolderOrganisationControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("1136ecfe-8100-427d-9758-a27d5bde644a", "Bond Holder");
			this.BondHolderOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
			this.BondHolderOrganisationControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.BondHolderOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BondHolderOrganisationControl.Name = "BondHolderOrganisationControl";
			this.BondHolderOrganisationControl.PopupCaption = "";
			this.BondHolderOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 35, true);
			this.BondHolderOrganisationControl.TabIndex = 1;
			// 
			// FromWarehouseGroupBox
			// 
			this.FromWarehouseGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("081b31f7-54ee-4c8d-a0bb-7f3a24e1e65d", "From Warehouse");
			this.FromWarehouseGroupBox.Controls.Add(this.FromWarehouseCodeTextBox);
			this.FromWarehouseGroupBox.Controls.Add(this.FromWarehouseAddressControl);
			this.FromWarehouseGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.FromWarehouseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 55, true);
			this.FromWarehouseGroupBox.Name = "FromWarehouseGroupBox";
			this.FromWarehouseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1094, 41, true);
			this.FromWarehouseGroupBox.TabIndex = 4;
			this.FromWarehouseGroupBox.TabStop = false;
			// 
			// FromWarehouseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.FromWarehouseCodeTextBox, "CustomsEntryInstructions.FromWarehouseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).FromWarehouseCode)));
			this.FromWarehouseCodeTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("c15a6ee8-68b7-4a47-9ad6-44929408a51b", "From Warehouse Code");
			this.FromWarehouseCodeTextBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.FromWarehouseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(409, 14, true);
			this.FromWarehouseCodeTextBox.Name = "FromWarehouseCodeTextBox";
			this.FromWarehouseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 17, true);
			this.FromWarehouseCodeTextBox.TabIndex = 3;
			// 
			// FromWarehouseAddressControl
			// 
			this.FromWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FromWarehouseAddressControl, "CustomsEntryInstructions.CEI_OA_Warehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OA_Warehouse)));
			this.FromWarehouseAddressControl.BindToOrgList = "Lookups.BondedWarehouseCollection";
			this.FromWarehouseAddressControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FromWarehouseAddressControl, false);
			this.FromWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.FromWarehouseAddressControl.Name = "FromWarehouseAddressControl";
			this.FromWarehouseAddressControl.PopupCaption = "";
			this.FromWarehouseAddressControl.ShowAddress = false;
			this.FromWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 25, true);
			this.FromWarehouseAddressControl.TabIndex = 0;
			// 
			// ToWarehouseGroupBox
			// 
			this.ToWarehouseGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("38ca5966-ac68-4ed6-a9d7-617d6e395cd4", "To Warehouse");
			this.ToWarehouseGroupBox.Controls.Add(this.ToWarehouseCodeTextBox);
			this.ToWarehouseGroupBox.Controls.Add(this.ToWarehouseAddressControl);
			this.ToWarehouseGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ToWarehouseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ToWarehouseGroupBox.Name = "ToWarehouseGroupBox";
			this.ToWarehouseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1094, 41, true);
			this.ToWarehouseGroupBox.TabIndex = 5;
			this.ToWarehouseGroupBox.TabStop = false;
			// 
			// ToWarehouseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ToWarehouseCodeTextBox, "CustomsEntryInstructions.ToWarehouseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ToWarehouseCode)));
			this.ToWarehouseCodeTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("5b087474-e0c2-404d-af1f-67de0c168cbc", "To Warehouse Code");
			this.ToWarehouseCodeTextBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ToWarehouseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(409, 14, true);
			this.ToWarehouseCodeTextBox.Name = "ToWarehouseCodeTextBox";
			this.ToWarehouseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 17, true);
			this.ToWarehouseCodeTextBox.TabIndex = 3;
			// 
			// ToWarehouseAddressControl
			// 
			this.ToWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ToWarehouseAddressControl, "CustomsEntryInstructions.CEI_OA_Warehouse2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OA_Warehouse2)));
			this.ToWarehouseAddressControl.BindToOrgList = "Lookups.BondedWarehouseCollection";
			this.ToWarehouseAddressControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ToWarehouseAddressControl, false);
			this.ToWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ToWarehouseAddressControl.Name = "ToWarehouseAddressControl";
			this.ToWarehouseAddressControl.PopupCaption = "";
			this.ToWarehouseAddressControl.ShowAddress = false;
			this.ToWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 25, true);
			this.ToWarehouseAddressControl.TabIndex = 0;
			// 
			// DetailsPanel
			// 
			this.DetailsPanel.AutoScroll = true;
			this.DetailsPanel.Controls.Add(this.StyleDropEdit);
			this.DetailsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.DetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsPanel.Name = "DetailsPanel";
			this.DetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1097, 42, true);
			this.DetailsPanel.TabIndex = 6;
			// 
			// StyleDropEdit
			// 
			this.StyleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StyleDropEdit, "CustomsEntryInstructions.CEI_Style");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_Style)));
			this.StyleDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.StyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 12, true);
			this.StyleDropEdit.Name = "StyleDropEdit";
			this.StyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 17, true);
			this.StyleDropEdit.TabIndex = 0;
			// 
			// EntryInstructionDetailBasicUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OtherPartiesDetailsPanel);
			this.Controls.Add(this.DetailsPanel);
			this.Name = "EntryInstructionDetailBasicUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1097, 264, true);
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
			this.DetailsPanel.ResumeLayout(false);
			this.DetailsPanel.PerformLayout();
			this.StyleDropEdit.ResumeLayout(true);
			this.StyleDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZPanel DetailsPanel;
		protected ZArchitecture.GUI.ZPanel OtherPartiesDetailsPanel;
		protected internal ZArchitecture.ZTextBox ToWarehouseCodeTextBox;
		protected ZArchitecture.GUI.ZGroupBox ToWarehouseGroupBox;
		protected internal ZArchitecture.ZTextBox FromWarehouseCodeTextBox;
		protected ZArchitecture.GUI.ZGroupBox FromWarehouseGroupBox;
		protected ZArchitecture.GUI.ZPanel BondHolderRemoverPanel;
		internal ZAddressControl ToWarehouseAddressControl;
		internal ZAddressControl FromWarehouseAddressControl;
		internal MasterFiles.GUI.ZOrganisationControl BondHolderOrganisationControl;
		internal MasterFiles.GUI.ZOrganisationControl RemoverOrganisationControl;
		internal MasterFiles.GUI.ZOrganisationControl NewOwnerOrganisationControl;
		protected internal ZDropEditWithFixedWidth StyleDropEdit;
		protected ZGroupBox OtherPartiesGroupBox;
	}
}
