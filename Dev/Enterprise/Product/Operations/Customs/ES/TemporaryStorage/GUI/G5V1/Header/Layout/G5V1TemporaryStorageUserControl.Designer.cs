using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.TemporaryStorage.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	partial class G5V1TemporaryStorageUserControl
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
			this.components = new System.ComponentModel.Container();
			this.DeclarationDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DynamicDetailsUserControlLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.PreviousDocumentsLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.PreviousDocumentUserControlTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SupportingDocumentsLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.SupportingDocumentUserControlTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdditionalInfosLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.AdditionalInfoUserControlTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DocumentsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.DestinationCustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ManualLocationOfGoodsCodeFindBox = new ManualLocationOfGoodsCodeFindBox();
			this.DestinationLocationOfGoodsUserControl = new DestinationLocationOfGoodsUserControl();
			this.GuaranteeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DynamicGuaranteePanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.CertificateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TrainingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsSimplifiedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MovementOfContainersOnlyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TransportDocumentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TransportDocumentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UnionGoodsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DeclarationDetailsGroupBox.SuspendLayout();
			this.PreviousDocumentUserControlTabPage.SuspendLayout();
			this.SupportingDocumentUserControlTabPage.SuspendLayout();
			this.AdditionalInfoUserControlTabPage.SuspendLayout();
			this.DocumentsTabControl.SuspendLayout();
			this.DestinationCustomsOfficeCodeFindBox.SuspendLayout();
			this.DestinationLocationOfGoodsUserControl.SuspendLayout();
			this.CertificateDropEdit.SuspendLayout();
			this.TrainingCheckBox.SuspendLayout();
			this.IsSimplifiedCheckBox.SuspendLayout();
			this.TransportDocumentTypeDropEdit.SuspendLayout();
			this.TransportDocumentTextBox.SuspendLayout();
			this.UnionGoodsCheckBox.SuspendLayout();
			this.MovementOfContainersOnlyCheckBox.SuspendLayout();
			this.DynamicGuaranteePanel.SuspendLayout();
			this.GuaranteeGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader);
			// 
			// DeclarationDetailsGroupBox
			// 
			this.DeclarationDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DeclarationDetailsGroupBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("6AD2C300-BB29-434D-8EEC-E35225D4AA75", "Declaration Details");
			this.DeclarationDetailsGroupBox.Controls.Add(this.DynamicDetailsUserControlLayoutPanel);
			this.DeclarationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 0, true);
			this.DeclarationDetailsGroupBox.Name = "DeclarationDetailsGroupBox";
			this.DeclarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 150, true);
			this.DeclarationDetailsGroupBox.TabIndex = 2;
			this.DeclarationDetailsGroupBox.TabStop = false;
			// 
			// DynamicDetailsUserControlLayoutPanel
			// 
			this.DynamicDetailsUserControlLayoutPanel.AllowDrop = true;
			this.DynamicDetailsUserControlLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicDetailsUserControlLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicDetailsUserControlLayoutPanel.Name = "DynamicDetailsUserControlLayoutPanel";
			this.DynamicDetailsUserControlLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 100, true);
			this.DynamicDetailsUserControlLayoutPanel.TabIndex = 3;
			// 
			// GuaranteeGroupBox
			// 
			this.GuaranteeGroupBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("3F8B29A6-C970-4EAF-9868-D38DF8061D2C", "Guarantee");
			this.GuaranteeGroupBox.Controls.Add(this.DynamicGuaranteePanel);
			this.GuaranteeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(790, 16, true);
			this.GuaranteeGroupBox.Name = "GuaranteeGroupBox";
			this.GuaranteeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 82, true);
			this.GuaranteeGroupBox.TabIndex = 2;
			this.GuaranteeGroupBox.TabStop = false;
			// 
			// DynamicGuaranteePanel
			//
			this.BindingSource.SetBindingMember(this.DynamicGuaranteePanel, "Guarantee");
			this.DynamicGuaranteePanel.AllowDrop = true;
			this.DynamicGuaranteePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicGuaranteePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicGuaranteePanel.Name = "DynamicGuaranteePanel";
			this.DynamicGuaranteePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 100, true);
			this.DynamicGuaranteePanel.TabIndex = 3;
			// 
			// PreviousDocumentsLayoutPanel
			// 
			this.PreviousDocumentsLayoutPanel.AllowDrop = true;
			this.PreviousDocumentsLayoutPanel.AutoScroll = true;
			this.PreviousDocumentsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviousDocumentsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PreviousDocumentsLayoutPanel.Name = "PreviousDocumentsLayoutPanel";
			this.PreviousDocumentsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 228, true);
			this.PreviousDocumentsLayoutPanel.TabIndex = 0;
			// 
			// PreviousDocumentUserControlTabPage
			// 
			this.PreviousDocumentUserControlTabPage.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("BF6857AA-6F9A-4322-A9FC-CB09F8EBFBDB", "Previous Docs");
			this.PreviousDocumentUserControlTabPage.Controls.Add(this.PreviousDocumentsLayoutPanel);
			this.PreviousDocumentUserControlTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.PreviousDocumentUserControlTabPage.Name = "PreviousDocumentUserControlTabPage";
			this.PreviousDocumentUserControlTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 229, true);
			this.PreviousDocumentUserControlTabPage.TabIndex = 0;
			// 
			// SupportingDocumentUserControlTabPage
			// 
			this.SupportingDocumentUserControlTabPage.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("AFC2B8BC-2C29-4C47-9BFA-7BE6CD715435", "Supporting Documents");
			this.SupportingDocumentUserControlTabPage.Controls.Add(this.SupportingDocumentsLayoutPanel);
			this.SupportingDocumentUserControlTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.SupportingDocumentUserControlTabPage.Name = "SupportingDocumentUserControlTabPage";
			this.SupportingDocumentUserControlTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 229, true);
			this.SupportingDocumentUserControlTabPage.TabIndex = 0;
			// 
			// SupportingDocumentsLayoutPanel
			// 
			this.SupportingDocumentsLayoutPanel.AllowDrop = true;
			this.SupportingDocumentsLayoutPanel.AutoScroll = true;
			this.SupportingDocumentsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentsLayoutPanel.Name = "SupportingDocumentsLayoutPanel";
			this.SupportingDocumentsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 228, true);
			this.SupportingDocumentsLayoutPanel.TabIndex = 0;
			// 
			// AdditionalInfoUserControlTabPage
			// 
			this.AdditionalInfoUserControlTabPage.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("39F36CD0-6315-4E89-871A-6FECF819CAD6", "Additional Information");
			this.AdditionalInfoUserControlTabPage.Controls.Add(this.AdditionalInfosLayoutPanel);
			this.AdditionalInfoUserControlTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.AdditionalInfoUserControlTabPage.Name = "AdditionalInfoUserControlTabPage";
			this.AdditionalInfoUserControlTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 229, true);
			this.AdditionalInfoUserControlTabPage.TabIndex = 0;
			//
			// AdditionalInfosLayoutPanel
			//
			this.AdditionalInfosLayoutPanel.AllowDrop = true;
			this.AdditionalInfosLayoutPanel.AutoScroll = true;
			this.AdditionalInfosLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInfosLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInfosLayoutPanel.Name = "AdditionalInfosLayoutPanel";
			this.AdditionalInfosLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 228, true);
			this.AdditionalInfosLayoutPanel.TabIndex = 0;
			// 
			// DocumentsTabControl
			// 
			this.DocumentsTabControl.Controls.Add(this.PreviousDocumentUserControlTabPage);
			this.DocumentsTabControl.Controls.Add(this.SupportingDocumentUserControlTabPage);
			this.DocumentsTabControl.Controls.Add(this.AdditionalInfoUserControlTabPage);
			this.DocumentsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 251, true);
			this.DocumentsTabControl.Name = "DocumentsTabControl";
			this.DocumentsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 251, true);
			this.DocumentsTabControl.TabIndex = 49;
			// 
			// DestinationCustomsOfficeCodeFindBox
			// 
			this.DestinationCustomsOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationCustomsOfficeCodeFindBox, "DestinationCustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)).DestinationCustomsOffice)));
			this.DestinationCustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 276, true);
			this.DestinationCustomsOfficeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.DestinationCustomsOfficeCodeFindBox.Name = "DestinationCustomsOfficeCodeFindBox";
			this.DestinationCustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DestinationCustomsOfficeCodeFindBox.ParentType = null;
			this.DestinationCustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 18, true);
			this.DestinationCustomsOfficeCodeFindBox.TabIndex = 23;
			// 
			// ManualLocationOfGoodsCodeFindBox
			// 
			this.ManualLocationOfGoodsCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManualLocationOfGoodsCodeFindBox, "ManualDestinationCustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)).ManualDestinationCustomsOffice)));
			this.ManualLocationOfGoodsCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 276, true);
			this.ManualLocationOfGoodsCodeFindBox.ModuleID = ModuleIDs.Customs.EU.TempStoragePremises;
			this.ManualLocationOfGoodsCodeFindBox.Name = "ManualLocationOfGoodsCodeFindBox";
			this.ManualLocationOfGoodsCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ManualLocationOfGoodsCodeFindBox.ParentType = null;
			this.ManualLocationOfGoodsCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 18, true);
			this.ManualLocationOfGoodsCodeFindBox.TabIndex = 46;

			// 
			// DestinationLocationOfGoodsUserControl
			// 
			this.DestinationLocationOfGoodsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationLocationOfGoodsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.ICusGoodsLocationProvider)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)))));
			this.DestinationLocationOfGoodsUserControl.CusGoodsLocationProviderType = null;
			this.DestinationLocationOfGoodsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 411, true);
			this.DestinationLocationOfGoodsUserControl.Name = "DestinationLocationOfGoodsUserControl";
			this.DestinationLocationOfGoodsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 23, true);
			this.DestinationLocationOfGoodsUserControl.TabIndex = 46;
			// 
			// CertificateDropEdit
			// 
			this.CertificateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateDropEdit, "AMA_CustomsProfile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).AMA_CustomsProfile)));
			this.CertificateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 72, true);
			this.CertificateDropEdit.Name = "CertificateDropEdit";
			this.CertificateDropEdit.ShouldResizeByMaxLength = false;
			this.CertificateDropEdit.ShowDescriptionBox = false;
			this.CertificateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.CertificateDropEdit.TabIndex = 36;
			this.CertificateDropEdit.UseFullWidthForCodeBox = true;
			// 
			// TrainingCheckBox
			// 
			this.BindingSource.SetBindingMember(this.TrainingCheckBox, "TrainingEntry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)).TrainingEntry)));
			this.TrainingCheckBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("3C8E066A-F25C-4592-8BA7-2195410872CB", englishCaption: "Training Entry", englishMediumCaption: "Training Entry", englishShortCaption: "Training Entry", englishFullDescription: "When checked the declaration will be sent to Test");
			this.TrainingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TrainingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 86, true);
			this.TrainingCheckBox.Name = "TrainingCheckBox";
			this.TrainingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 19, true);
			this.TrainingCheckBox.TabIndex = 4;
			this.TrainingCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsSimplifiedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsSimplifiedCheckBox, "IsSimplified");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)).IsSimplified)));
			this.IsSimplifiedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSimplifiedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 86, true);
			this.IsSimplifiedCheckBox.Name = "IsSimplifiedCheckBox";
			this.IsSimplifiedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 19, true);
			this.IsSimplifiedCheckBox.TabIndex = 4;
			this.IsSimplifiedCheckBox.UseVisualStyleBackColor = true;
			//	
			// MovementOfContainersOnlyCheckBox
			// 
			this.BindingSource.SetBindingMember(this.MovementOfContainersOnlyCheckBox, "MovementOfContainersOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)).MovementOfContainersOnly)));
			this.MovementOfContainersOnlyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MovementOfContainersOnlyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 86, true);
			this.MovementOfContainersOnlyCheckBox.Name = "MovementOfContainersOnlyCheckBox";
			this.MovementOfContainersOnlyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 19, true);
			this.MovementOfContainersOnlyCheckBox.TabIndex = 4;
			this.MovementOfContainersOnlyCheckBox.UseVisualStyleBackColor = true;
			// 
			// TransportDocumentTypeDropEdit
			// 
			this.TransportDocumentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportDocumentTypeDropEdit, "Bills.TypeOfBillDocument");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).TypeOfBillDocument)));
			this.TransportDocumentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 107, true);
			this.TransportDocumentTypeDropEdit.Name = "TransportDocumentTypeDropEdit";
			this.TransportDocumentTypeDropEdit.PreBoundMaxLength = 4;
			this.TransportDocumentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.TransportDocumentTypeDropEdit.TabIndex = 1;
			// 
			// TransportDocumentTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransportDocumentTextBox, "Bills.ABL_BillNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_BillNumber)));
			this.TransportDocumentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 142, true);
			this.TransportDocumentTextBox.Name = "TransportDocumentTextBox";
			this.TransportDocumentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.TransportDocumentTextBox.TabIndex = 2;
			// 
			// UnionGoodsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.UnionGoodsCheckBox, "UnionGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)).UnionGoods)));
			this.UnionGoodsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UnionGoodsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 86, true);
			this.UnionGoodsCheckBox.Name = "UnionGoodsCheckBox";
			this.UnionGoodsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 19, true);
			this.UnionGoodsCheckBox.TabIndex = 4;
			this.UnionGoodsCheckBox.UseVisualStyleBackColor = true;
			// 
			// G5V1TemporaryStorageUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DeclarationDetailsGroupBox);
			this.Controls.Add(this.DocumentsTabControl);
			this.Controls.Add(this.DestinationCustomsOfficeCodeFindBox);
			this.Controls.Add(this.DestinationLocationOfGoodsUserControl);
			this.Controls.Add(this.ManualLocationOfGoodsCodeFindBox);
			this.Controls.Add(this.CertificateDropEdit);
			this.Controls.Add(this.TrainingCheckBox);
			this.Controls.Add(this.IsSimplifiedCheckBox);
			this.Controls.Add(this.MovementOfContainersOnlyCheckBox);
			this.Controls.Add(this.TransportDocumentTypeDropEdit);
			this.Controls.Add(this.TransportDocumentTextBox);
			this.Controls.Add(this.UnionGoodsCheckBox);
			this.Controls.Add(this.GuaranteeGroupBox);
			this.Name = "G5V1TemporaryStorageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1360, 567, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PreviousDocumentUserControlTabPage.ResumeLayout(false);
			this.PreviousDocumentUserControlTabPage.PerformLayout();
			this.SupportingDocumentUserControlTabPage.ResumeLayout(false);
			this.SupportingDocumentUserControlTabPage.PerformLayout();
			this.AdditionalInfoUserControlTabPage.ResumeLayout(false);
			this.AdditionalInfoUserControlTabPage.PerformLayout();
			this.DeclarationDetailsGroupBox.ResumeLayout(false);
			this.DeclarationDetailsGroupBox.PerformLayout();
			this.DocumentsTabControl.ResumeLayout(false);
			this.DocumentsTabControl.PerformLayout();
			this.DestinationCustomsOfficeCodeFindBox.ResumeLayout(true);
			this.DestinationCustomsOfficeCodeFindBox.PerformLayout();
			this.ManualLocationOfGoodsCodeFindBox.ResumeLayout(true);
			this.ManualLocationOfGoodsCodeFindBox.PerformLayout();
			this.DestinationLocationOfGoodsUserControl.ResumeLayout(true);
			this.DestinationLocationOfGoodsUserControl.PerformLayout();
			this.CertificateDropEdit.ResumeLayout(true);
			this.CertificateDropEdit.PerformLayout();
			this.TrainingCheckBox.ResumeLayout(true);
			this.TrainingCheckBox.PerformLayout();
			this.IsSimplifiedCheckBox.ResumeLayout(true);
			this.IsSimplifiedCheckBox.PerformLayout();
			this.TransportDocumentTypeDropEdit.ResumeLayout(true);
			this.TransportDocumentTypeDropEdit.PerformLayout();
			this.TransportDocumentTextBox.ResumeLayout(true);
			this.TransportDocumentTextBox.PerformLayout();
			this.UnionGoodsCheckBox.ResumeLayout(true);
			this.UnionGoodsCheckBox.PerformLayout();
			this.MovementOfContainersOnlyCheckBox.ResumeLayout(true);
			this.MovementOfContainersOnlyCheckBox.PerformLayout();
			this.DynamicGuaranteePanel.ResumeLayout(false);
			this.DynamicGuaranteePanel.PerformLayout();
			this.GuaranteeGroupBox.ResumeLayout(false);
			this.GuaranteeGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.DynamicLayoutPanel PreviousDocumentsLayoutPanel;
		internal ZTabPage PreviousDocumentUserControlTabPage;
		private ZArchitecture.GUI.DynamicLayoutPanel SupportingDocumentsLayoutPanel;
		internal ZTabPage SupportingDocumentUserControlTabPage;
		private ZArchitecture.GUI.DynamicLayoutPanel AdditionalInfosLayoutPanel;
		internal ZTabPage AdditionalInfoUserControlTabPage;
		internal ZTemplateTabControl DocumentsTabControl;
		internal ZArchitecture.GUI.ZCodeFindBox DestinationCustomsOfficeCodeFindBox;
		internal ManualLocationOfGoodsCodeFindBox ManualLocationOfGoodsCodeFindBox;
		internal DestinationLocationOfGoodsUserControl DestinationLocationOfGoodsUserControl;
		internal ZArchitecture.GUI.ZDropEdit CertificateDropEdit;
		internal ZArchitecture.GUI.ZCheckBox TrainingCheckBox;
		internal ZArchitecture.GUI.ZCheckBox IsSimplifiedCheckBox;
		internal ZArchitecture.GUI.ZCheckBox MovementOfContainersOnlyCheckBox;
		internal ZArchitecture.GUI.ZDropEdit TransportDocumentTypeDropEdit;
		internal ZArchitecture.ZTextBox TransportDocumentTextBox;
		internal ZArchitecture.GUI.ZCheckBox UnionGoodsCheckBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel DynamicDetailsUserControlLayoutPanel;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox DeclarationDetailsGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel DynamicGuaranteePanel;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox GuaranteeGroupBox;
	}
}
