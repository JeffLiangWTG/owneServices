namespace Enterprise.Customs.CA.GUI
{
	partial class OrganisationLVSConsolidationUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.LVSStrategyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LVSInvoiceDetailDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IsCreateIndividualLVSCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsConsolidateByImporterCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsConsolidateByBranchCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsConsolidateByProvinceofClearanceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsConsolidateByBrokerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsConsolidateToOneFTypePerCLVSEntry = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SubHeaderSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.CommentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PrintDeliveryAddressForLVXCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SubHeaderSplitContainer)).BeginInit();
			this.LVSInvoiceDetailDropEdit.SuspendLayout();
			this.LVSStrategyGroupBox.SuspendLayout();
			this.SubHeaderSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.OrgImpAddInfo);
			// 
			// SubHeaderSplitContainer
			// 
			this.SubHeaderSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SubHeaderSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SubHeaderSplitContainer.Name = "SubHeaderSplitContainer";
			this.SubHeaderSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SubHeaderSplitContainer.Panel1
			// 
			this.SubHeaderSplitContainer.Panel1.Controls.Add(this.PrintDeliveryAddressForLVXCheckBox);
			this.SubHeaderSplitContainer.Panel1.Controls.Add(this.LVSInvoiceDetailDropEdit);
			this.SubHeaderSplitContainer.Panel2.Controls.Add(this.LVSStrategyGroupBox);
			this.SubHeaderSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 926, true);
			this.SubHeaderSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.SubHeaderSplitContainer.TabIndex = 1;
			// 
			// LVSInvoiceDetailDropEdit
			// 
			this.LVSInvoiceDetailDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LVSInvoiceDetailDropEdit, "ZO_LVSInvoiceDetailCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_LVSInvoiceDetailCode)));
			this.LVSInvoiceDetailDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("fab92dfa-e74d-4176-af07-29d64f5652f3", "LVS Invoice Details Code");
			this.LVSInvoiceDetailDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 31, true);
			this.LVSInvoiceDetailDropEdit.Name = "LVSInvoiceDetailDropEdit";
			this.LVSInvoiceDetailDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 17, true);
			this.LVSInvoiceDetailDropEdit.TabIndex = 1;
			// 
			// PrintDeliveryAddressForLVXCheckBox
			// 
			this.PrintDeliveryAddressForLVXCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PrintDeliveryAddressForLVXCheckBox, "ZO_PrintDeliveryAddressForLVX");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_PrintDeliveryAddressForLVX)));
			this.PrintDeliveryAddressForLVXCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("F3C62A8E-FB6C-41F0-B4D3-BC3CAE373FEB", "Print Delivery Address on Invoices for Courier LVS Declaration jobs");
			this.PrintDeliveryAddressForLVXCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PrintDeliveryAddressForLVXCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 10, true);
			this.PrintDeliveryAddressForLVXCheckBox.Name = "PrintDeliveryAddressForLVXCheckBox";
			this.PrintDeliveryAddressForLVXCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.PrintDeliveryAddressForLVXCheckBox.TabIndex = 0;
			this.PrintDeliveryAddressForLVXCheckBox.UseVisualStyleBackColor = true;
			// 
			// LVSStrategyGroupBox
			// 
			this.LVSStrategyGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("B7E5AA54-031C-406D-9801-D37F28B062C4", "LVS Consolidation Strategy for this Importer");
			this.LVSStrategyGroupBox.Controls.Add(this.IsCreateIndividualLVSCheckBox);
			this.LVSStrategyGroupBox.Controls.Add(this.IsConsolidateByImporterCheckBox);
			this.LVSStrategyGroupBox.Controls.Add(this.IsConsolidateByBranchCheckBox);
			this.LVSStrategyGroupBox.Controls.Add(this.IsConsolidateByProvinceofClearanceCheckBox);
			this.LVSStrategyGroupBox.Controls.Add(this.IsConsolidateByBrokerCheckBox);
			this.LVSStrategyGroupBox.Controls.Add(this.IsConsolidateToOneFTypePerCLVSEntry);
			this.LVSStrategyGroupBox.Controls.Add(this.CommentLabel);
			this.LVSStrategyGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LVSStrategyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 10, true);
			this.LVSStrategyGroupBox.Name = "LVSStrategyGroupBox";
			this.LVSStrategyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 323, true);
			this.LVSStrategyGroupBox.TabIndex = 1;
			this.LVSStrategyGroupBox.TabStop = false;
			// 
			// IsCreateIndividualLVSCheckBox
			// 
			this.IsCreateIndividualLVSCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsCreateIndividualLVSCheckBox, "ZO_IsCreateIndividualLVS");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_IsCreateIndividualLVS)));
			this.IsCreateIndividualLVSCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2f0b15da-d45c-429a-b70b-d02e835f9877", "Create Individual LVS Shipments (Applicable for CLVS Entry Wizard)");
			this.IsCreateIndividualLVSCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsCreateIndividualLVSCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 20, true);
			this.IsCreateIndividualLVSCheckBox.Name = "IsCreateIndividualLVSCheckBox";
			this.IsCreateIndividualLVSCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 17, true);
			this.IsCreateIndividualLVSCheckBox.TabIndex = 0;
			this.IsCreateIndividualLVSCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsConsolidateByImporterCheckBox
			// 
			this.IsConsolidateByImporterCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsConsolidateByImporterCheckBox, "ZO_IsLVSConsolidated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_IsLVSConsolidated)));
			this.IsConsolidateByImporterCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("439cff0e-9b04-4aac-9089-0240c60ebf6b", "Consolidate by Importer");
			this.IsConsolidateByImporterCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsConsolidateByImporterCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 44, true);
			this.IsConsolidateByImporterCheckBox.Name = "IsConsolidateByImporterCheckBox";
			this.IsConsolidateByImporterCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.IsConsolidateByImporterCheckBox.TabIndex = 1;
			this.IsConsolidateByImporterCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsConsolidateByBranchCheckBox
			// 
			this.IsConsolidateByBranchCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsConsolidateByBranchCheckBox, "ZO_IsConsolidateByBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_IsConsolidateByBranch)));
			this.IsConsolidateByBranchCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("92b9d875-4c59-4c16-8fce-59b4aa27182b", "Consolidate by Branch");
			this.IsConsolidateByBranchCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsConsolidateByBranchCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 68, true);
			this.IsConsolidateByBranchCheckBox.Name = "IsConsolidateByBranchCheckBox";
			this.IsConsolidateByBranchCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 17, true);
			this.IsConsolidateByBranchCheckBox.TabIndex = 2;
			this.IsConsolidateByBranchCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsConsolidateByProvinceofClearanceCheckBox
			// 
			this.IsConsolidateByProvinceofClearanceCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsConsolidateByProvinceofClearanceCheckBox, "ZO_IsConsolidateByProvinceofClearance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_IsConsolidateByProvinceofClearance)));
			this.IsConsolidateByProvinceofClearanceCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("da104d85-e70c-4da7-8bf2-44ffa728b821", "Consolidate by Province of Clearance");
			this.IsConsolidateByProvinceofClearanceCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsConsolidateByProvinceofClearanceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 92, true);
			this.IsConsolidateByProvinceofClearanceCheckBox.Name = "IsConsolidateByProvinceofClearanceCheckBox";
			this.IsConsolidateByProvinceofClearanceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 17, true);
			this.IsConsolidateByProvinceofClearanceCheckBox.TabIndex = 3;
			this.IsConsolidateByProvinceofClearanceCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsConsolidateByBrokerCheckBox
			// 
			this.IsConsolidateByBrokerCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsConsolidateByBrokerCheckBox, "ZO_IsConsolidateByBroker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_IsConsolidateByBroker)));
			this.IsConsolidateByBrokerCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f4e96adb-0678-49bc-896e-c2fdfcda446f", "Consolidate by Broker");
			this.IsConsolidateByBrokerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsConsolidateByBrokerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 116, true);
			this.IsConsolidateByBrokerCheckBox.Name = "IsConsolidateByBrokerCheckBox";
			this.IsConsolidateByBrokerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 17, true);
			this.IsConsolidateByBrokerCheckBox.TabIndex = 4;
			this.IsConsolidateByBrokerCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsConsolidateToOneFTypePerCLVSEntry
			// 
			this.IsConsolidateToOneFTypePerCLVSEntry.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsConsolidateToOneFTypePerCLVSEntry, "ZO_IsConsolidateToOneFTypePerCLVSEntry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_IsConsolidateToOneFTypePerCLVSEntry)));
			this.IsConsolidateToOneFTypePerCLVSEntry.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0F7A8BFD-1B7D-415E-B8F4-9DC9002E6B3", "Consolidate to one F type per CLVS entry");
			this.IsConsolidateToOneFTypePerCLVSEntry.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsConsolidateToOneFTypePerCLVSEntry.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 140, true);
			this.IsConsolidateToOneFTypePerCLVSEntry.Name = "IsConsolidateToOneFTypePerCLVSEntry";
			this.IsConsolidateToOneFTypePerCLVSEntry.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 17, true);
			this.IsConsolidateToOneFTypePerCLVSEntry.TabIndex = 5;
			this.IsConsolidateToOneFTypePerCLVSEntry.UseVisualStyleBackColor = true;
			// 
			// CommentLabel
			// 
			this.CommentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CommentLabel.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("65626678-f680-4f6e-92e7-f69b41445f59", "See the registry under Customs->Canada->Import->Declaration-> Default LVS Consolidation Strategy for default fall back settings");
			this.CommentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 158, true);
			this.CommentLabel.Name = "CommentLabel";
			this.CommentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 23, true);
			this.CommentLabel.TabIndex = 6;
			this.CommentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.CommentLabel.UseMnemonic = false;
			// 
			// OrganisationLVSConsolidationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SubHeaderSplitContainer);
			this.Name = "OrganisationLVSConsolidationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 423, true);
			this.LVSStrategyGroupBox.ResumeLayout(false);
			this.LVSStrategyGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SubHeaderSplitContainer.Panel1.ResumeLayout(false);
			this.SubHeaderSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SubHeaderSplitContainer)).EndInit();
			this.SubHeaderSplitContainer.ResumeLayout(false);
			this.LVSInvoiceDetailDropEdit.ResumeLayout(true);
			this.LVSInvoiceDetailDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZGroupBox LVSStrategyGroupBox;
		private ZArchitecture.GUI.ZCheckBox IsCreateIndividualLVSCheckBox;
		private ZArchitecture.GUI.ZCheckBox IsConsolidateByImporterCheckBox;
		private ZArchitecture.GUI.ZCheckBox IsConsolidateByBranchCheckBox;
		private ZArchitecture.GUI.ZCheckBox IsConsolidateByProvinceofClearanceCheckBox;
		private ZArchitecture.GUI.ZCheckBox IsConsolidateByBrokerCheckBox;
		private ZArchitecture.GUI.ZCheckBox IsConsolidateToOneFTypePerCLVSEntry;
		private ZArchitecture.ZLabel CommentLabel;
		private ZArchitecture.GUI.ZCheckBox PrintDeliveryAddressForLVXCheckBox;
		private ZArchitecture.GUI.ZDropEdit LVSInvoiceDetailDropEdit;
		private CargoWise.Windows.UI.KSplitContainer SubHeaderSplitContainer;
	}
}
