namespace Enterprise.Customs.DE.GUI
{
	partial class NewSplitLinesUserControl
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
			this.NewSplitLineDetailsMainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OrganisationAndItemsDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ItemDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GoodsLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoodsTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DestinationPlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OrganisationDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DisposalEntitledTraderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GoodsOwnerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.CustodianGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustodianAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ClassificationKeyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OwnerReferenceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OwneRefNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NewSplitLineDetailsMainPanel.SuspendLayout();
			this.OrganisationAndItemsDetailsPanel.SuspendLayout();
			this.ItemDetailsGroupBox.SuspendLayout();
			this.GoodsLocationDropEdit.SuspendLayout();
			this.GoodsTypeDropEdit.SuspendLayout();
			this.OrganisationDetailsPanel.SuspendLayout();
			this.DisposalEntitledTraderGroupBox.SuspendLayout();
			this.GoodsOwnerAddressControl.SuspendLayout();
			this.CustodianGroupBox.SuspendLayout();
			this.CustodianAddressControl.SuspendLayout();
			this.ClassificationKeyGroupBox.SuspendLayout();
			this.OwnerReferenceTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader);
			// 
			// NewSplitLineDetailsMainPanel
			// 
			this.NewSplitLineDetailsMainPanel.Controls.Add(this.OrganisationAndItemsDetailsPanel);
			this.NewSplitLineDetailsMainPanel.Controls.Add(this.ClassificationKeyGroupBox);
			this.NewSplitLineDetailsMainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NewSplitLineDetailsMainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NewSplitLineDetailsMainPanel.Name = "NewSplitLineDetailsMainPanel";
			this.NewSplitLineDetailsMainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 136, true);
			this.NewSplitLineDetailsMainPanel.TabIndex = 0;
			// 
			// OrganisationAndItemsDetailsPanel
			// 
			this.OrganisationAndItemsDetailsPanel.Controls.Add(this.ItemDetailsGroupBox);
			this.OrganisationAndItemsDetailsPanel.Controls.Add(this.OrganisationDetailsPanel);
			this.OrganisationAndItemsDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrganisationAndItemsDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 47, true);
			this.OrganisationAndItemsDetailsPanel.Name = "OrganisationAndItemsDetailsPanel";
			this.OrganisationAndItemsDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 89, true);
			this.OrganisationAndItemsDetailsPanel.TabIndex = 1;
			// 
			// ItemDetailsGroupBox
			// 
			this.ItemDetailsGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("69992e41-0bf2-453a-a5fb-1cbd9a37d1aa", "Item Details");
			this.ItemDetailsGroupBox.Controls.Add(this.GoodsLocationDropEdit);
			this.ItemDetailsGroupBox.Controls.Add(this.GoodsTypeDropEdit);
			this.ItemDetailsGroupBox.Controls.Add(this.DestinationPlaceTextBox);
			this.ItemDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 0, true);
			this.ItemDetailsGroupBox.Name = "ItemDetailsGroupBox";
			this.ItemDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 89, true);
			this.ItemDetailsGroupBox.TabIndex = 3;
			this.ItemDetailsGroupBox.TabStop = false;
			// 
			// GoodsLocationDropEdit
			// 
			this.GoodsLocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsLocationDropEdit, "CUSPCSCusTempStorageDecs.ConsolidatedCusTempStorageLine.CusTempStorageLinesTo.TSL" +
        "_LocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSSplitCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPCSCusTempStorageDecs)).SyncRoot)).ConsolidatedCusTempStorageLine.CusTempStorageLinesTo)).SyncRoot)).TSL_LocationOfGoods)));
			this.GoodsLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 39, true);
			this.GoodsLocationDropEdit.Name = "GoodsLocationDropEdit";
			this.GoodsLocationDropEdit.ShouldResizeByMaxLength = true;
			this.GoodsLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 20, true);
			this.GoodsLocationDropEdit.TabIndex = 2;
			// 
			// GoodsTypeDropEdit
			// 
			this.GoodsTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsTypeDropEdit, "CUSPCSCusTempStorageDecs.ConsolidatedCusTempStorageLine.CusTempStorageLinesTo.TSL" +
        "_GoodsType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSSplitCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPCSCusTempStorageDecs)).SyncRoot)).ConsolidatedCusTempStorageLine.CusTempStorageLinesTo)).SyncRoot)).TSL_GoodsType)));
			this.GoodsTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 17, true);
			this.GoodsTypeDropEdit.Name = "GoodsTypeDropEdit";
			this.GoodsTypeDropEdit.ShouldResizeByMaxLength = true;
			this.GoodsTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 20, true);
			this.GoodsTypeDropEdit.TabIndex = 1;
			// 
			// DestinationPlaceTextBox
			// 
			this.BindingSource.SetBindingMember(this.DestinationPlaceTextBox, "CUSPCSCusTempStorageDecs.ConsolidatedCusTempStorageLine.CusTempStorageLinesTo.TSL" +
        "_DestinationPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSSplitCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPCSCusTempStorageDecs)).SyncRoot)).ConsolidatedCusTempStorageLine.CusTempStorageLinesTo)).SyncRoot)).TSL_DestinationPlace)));
			this.DestinationPlaceTextBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("c4d960db-d092-4a86-a0e0-d9e029c3c74c", "Destination Place");
			this.DestinationPlaceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DestinationPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 62, true);
			this.DestinationPlaceTextBox.Name = "DestinationPlaceTextBox";
			this.DestinationPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 20, true);
			this.DestinationPlaceTextBox.TabIndex = 3;
			// 
			// OrganisationDetailsPanel
			// 
			this.OrganisationDetailsPanel.Controls.Add(this.DisposalEntitledTraderGroupBox);
			this.OrganisationDetailsPanel.Controls.Add(this.CustodianGroupBox);
			this.OrganisationDetailsPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.OrganisationDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrganisationDetailsPanel.Name = "OrganisationDetailsPanel";
			this.OrganisationDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 89, true);
			this.OrganisationDetailsPanel.TabIndex = 2;
			// 
			// DisposalEntitledTraderGroupBox
			// 
			this.DisposalEntitledTraderGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("5b7da6a3-6623-4ab3-bf41-05876d69118f", "Disposal Entitled Trader");
			this.DisposalEntitledTraderGroupBox.Controls.Add(this.GoodsOwnerAddressControl);
			this.DisposalEntitledTraderGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DisposalEntitledTraderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 43, true);
			this.DisposalEntitledTraderGroupBox.Name = "DisposalEntitledTraderGroupBox";
			this.DisposalEntitledTraderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 46, true);
			this.DisposalEntitledTraderGroupBox.TabIndex = 2;
			this.DisposalEntitledTraderGroupBox.TabStop = false;
			// 
			// GoodsOwnerAddressControl
			// 
			this.GoodsOwnerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsOwnerAddressControl, "CUSPCSCusTempStorageDecs.ConsolidatedCusTempStorageLine.CusTempStorageLinesTo.TSL" +
        "_OA_GoodsOwner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSSplitCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPCSCusTempStorageDecs)).SyncRoot)).ConsolidatedCusTempStorageLine.CusTempStorageLinesTo)).SyncRoot)).TSL_OA_GoodsOwner)));
			this.GoodsOwnerAddressControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("b7034de7-22d2-4413-87d2-122560832011", "Disposal Entitled Trader");
			this.GoodsOwnerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 17, true);
			this.GoodsOwnerAddressControl.Name = "GoodsOwnerAddressControl";
			this.GoodsOwnerAddressControl.PopupCaption = "";
			this.GoodsOwnerAddressControl.ReadOnly = false;
			this.GoodsOwnerAddressControl.ShowAddress = false;
			this.GoodsOwnerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.GoodsOwnerAddressControl.TabIndex = 1;
			// 
			// CustodianGroupBox
			// 
			this.CustodianGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("979c1e6a-295d-405e-a70a-16eb8bb52ebf", "Custodian");
			this.CustodianGroupBox.Controls.Add(this.CustodianAddressControl);
			this.CustodianGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.CustodianGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustodianGroupBox.Name = "CustodianGroupBox";
			this.CustodianGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 43, true);
			this.CustodianGroupBox.TabIndex = 1;
			this.CustodianGroupBox.TabStop = false;
			// 
			// CustodianAddressControl
			// 
			this.CustodianAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustodianAddressControl, "CUSPCSCusTempStorageDecs.ConsolidatedCusTempStorageLine.CusTempStorageLinesTo.TSL" +
        "_OA_Custodian");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSSplitCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPCSCusTempStorageDecs)).SyncRoot)).ConsolidatedCusTempStorageLine.CusTempStorageLinesTo)).SyncRoot)).TSL_OA_Custodian)));
			this.CustodianAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 17, true);
			this.CustodianAddressControl.Name = "CustodianAddressControl";
			this.CustodianAddressControl.PopupCaption = "";
			this.CustodianAddressControl.ReadOnly = false;
			this.CustodianAddressControl.ShowAddress = false;
			this.CustodianAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CustodianAddressControl.TabIndex = 1;
			// 
			// ClassificationKeyGroupBox
			// 
			this.ClassificationKeyGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("eee32331-3b9f-4e0b-bb96-a0f1fcc55784", "Classification Key");
			this.ClassificationKeyGroupBox.Controls.Add(this.OwnerReferenceTypeDropEdit);
			this.ClassificationKeyGroupBox.Controls.Add(this.OwneRefNumberTextBox);
			this.ClassificationKeyGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ClassificationKeyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClassificationKeyGroupBox.Name = "ClassificationKeyGroupBox";
			this.ClassificationKeyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 47, true);
			this.ClassificationKeyGroupBox.TabIndex = 0;
			this.ClassificationKeyGroupBox.TabStop = false;
			// 
			// OwnerReferenceTypeDropEdit
			// 
			this.OwnerReferenceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OwnerReferenceTypeDropEdit, "CUSPCSCusTempStorageDecs.ConsolidatedCusTempStorageLine.CusTempStorageLinesTo.TSL" +
        "_OwnerReferenceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSSplitCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPCSCusTempStorageDecs)).SyncRoot)).ConsolidatedCusTempStorageLine.CusTempStorageLinesTo)).SyncRoot)).TSL_OwnerReferenceType)));
			this.OwnerReferenceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 19, true);
			this.OwnerReferenceTypeDropEdit.Name = "OwnerReferenceTypeDropEdit";
			this.OwnerReferenceTypeDropEdit.PreBoundMaxLength = 3;
			this.OwnerReferenceTypeDropEdit.ShouldResizeByMaxLength = true;
			this.OwnerReferenceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.OwnerReferenceTypeDropEdit.TabIndex = 1;
			// 
			// OwneRefNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.OwneRefNumberTextBox, "CUSPCSCusTempStorageDecs.ConsolidatedCusTempStorageLine.CusTempStorageLinesTo.TSL" +
        "_OwnerReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSSplitCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CUSPCSCusTempStorageDecs)).SyncRoot)).ConsolidatedCusTempStorageLine.CusTempStorageLinesTo)).SyncRoot)).TSL_OwnerReferenceNumber)));
			this.OwneRefNumberTextBox.CaptionResourceString = null;
			this.OwneRefNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OwneRefNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(494, 19, true);
			this.OwneRefNumberTextBox.Name = "OwneRefNumberTextBox";
			this.OwneRefNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.OwneRefNumberTextBox.TabIndex = 2;
			// 
			// NewSplitLinesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NewSplitLineDetailsMainPanel);
			this.Name = "NewSplitLinesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 136, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NewSplitLineDetailsMainPanel.ResumeLayout(false);
			this.NewSplitLineDetailsMainPanel.PerformLayout();
			this.OrganisationAndItemsDetailsPanel.ResumeLayout(false);
			this.OrganisationAndItemsDetailsPanel.PerformLayout();
			this.ItemDetailsGroupBox.ResumeLayout(false);
			this.ItemDetailsGroupBox.PerformLayout();
			this.GoodsLocationDropEdit.ResumeLayout(true);
			this.GoodsLocationDropEdit.PerformLayout();
			this.GoodsTypeDropEdit.ResumeLayout(true);
			this.GoodsTypeDropEdit.PerformLayout();
			this.OrganisationDetailsPanel.ResumeLayout(false);
			this.OrganisationDetailsPanel.PerformLayout();
			this.DisposalEntitledTraderGroupBox.ResumeLayout(false);
			this.DisposalEntitledTraderGroupBox.PerformLayout();
			this.GoodsOwnerAddressControl.ResumeLayout(true);
			this.GoodsOwnerAddressControl.PerformLayout();
			this.CustodianGroupBox.ResumeLayout(false);
			this.CustodianGroupBox.PerformLayout();
			this.CustodianAddressControl.ResumeLayout(true);
			this.CustodianAddressControl.PerformLayout();
			this.ClassificationKeyGroupBox.ResumeLayout(false);
			this.ClassificationKeyGroupBox.PerformLayout();
			this.OwnerReferenceTypeDropEdit.ResumeLayout(true);
			this.OwnerReferenceTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel NewSplitLineDetailsMainPanel;
		private ZArchitecture.GUI.ZPanel OrganisationAndItemsDetailsPanel;
		private ZArchitecture.GUI.ZPanel OrganisationDetailsPanel;
		private ZArchitecture.GUI.ZGroupBox DisposalEntitledTraderGroupBox;
		private ZArchitecture.GUI.ZAddressControl GoodsOwnerAddressControl;
		private ZArchitecture.GUI.ZGroupBox CustodianGroupBox;
		private ZArchitecture.GUI.ZAddressControl CustodianAddressControl;
		private ZArchitecture.GUI.ZGroupBox ItemDetailsGroupBox;
		private ZArchitecture.GUI.ZDropEdit GoodsLocationDropEdit;
		private ZArchitecture.GUI.ZDropEdit GoodsTypeDropEdit;
		private ZArchitecture.ZTextBox DestinationPlaceTextBox;
		private ZArchitecture.GUI.ZGroupBox ClassificationKeyGroupBox;
		private ZArchitecture.GUI.ZDropEdit OwnerReferenceTypeDropEdit;
		private ZArchitecture.ZTextBox OwneRefNumberTextBox;
	}
}
