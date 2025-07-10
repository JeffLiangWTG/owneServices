using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AQISUserControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AQISUserControl));
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.aQISInformation = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.aQISCommodityCodeControl = new Enterprise.Customs.AU.Declaration.GUI.AQISCommodityCodeControl();
			this.aQISPermitNumberControl = new Enterprise.Customs.AU.Declaration.GUI.AQISPermitNumberControl();
			this.aQISEntityIDControl = new Enterprise.Customs.AU.Declaration.GUI.AQISEntityIdControl();
			this.aQISProducerCodeControl = new Enterprise.Customs.AU.Declaration.GUI.AQISProducerCodeControl();
			this.aQISDocumentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.aQISDocumentGrid = new Enterprise.ZArchitecture.ZGrid();
			this.aQISPremisesIdAndPackagesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.aQISPremisesIdProcessingTypeGrid = new Enterprise.ZArchitecture.ZGrid();
			this.aQISInformation.SuspendLayout();
			this.aQISDocumentGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.aQISDocumentGrid)).BeginInit();
			this.aQISPremisesIdAndPackagesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.aQISPremisesIdProcessingTypeGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// AQISInformation
			// 
			this.aQISInformation.Controls.Add(this.aQISCommodityCodeControl);
			this.aQISInformation.Controls.Add(this.aQISPermitNumberControl);
			this.aQISInformation.Controls.Add(this.aQISEntityIDControl);
			this.aQISInformation.Controls.Add(this.aQISProducerCodeControl);
			this.aQISInformation.Dock = System.Windows.Forms.DockStyle.Top;
			this.aQISInformation.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.aQISInformation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.aQISInformation.Name = "AQISInformation";
			this.aQISInformation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 68, true);
			this.aQISInformation.TabIndex = 1;
			this.aQISInformation.TabStop = false;
			this.aQISInformation.Text = "Quarantine Information";
			// 
			// AQISCommodityCodeControl
			// 
			this.aQISCommodityCodeControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).AddInfo.ZA_AQISCommCodes_Hidden)));
			this.BindingSource.SetBindingMember(this.aQISCommodityCodeControl, "PivotsForBinding.AddInfo+ZA_AQISCommCodes_Hidden");
			this.aQISCommodityCodeControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AQISUserControl|E656BA83-E66A-4930-AD9C-E68D08B478D2", "Commodity Code");
			this.aQISCommodityCodeControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 39, true);
			this.aQISCommodityCodeControl.Name = "AQISCommodityCodeControl";
			this.aQISCommodityCodeControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.aQISCommodityCodeControl.TabIndex = 7;
			// 
			// AQISPermitNumberControl
			// 
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).AddInfo.ZA_AQISPermitIds_Hidden)));
			this.BindingSource.SetBindingMember(this.aQISPermitNumberControl, "PivotsForBinding.AddInfo+ZA_AQISPermitIds_Hidden");
			this.aQISPermitNumberControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AQISUserControl|0D29F22B-7FAC-4992-BD46-FC5F87EB1F9F", "Permit Number");
			this.aQISPermitNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 16, true);
			this.aQISPermitNumberControl.Name = "AQISPermitNumberControl";
			this.aQISPermitNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.aQISPermitNumberControl.TabIndex = 1;
			// 
			// AQISEntityIDControl
			// 
			this.aQISEntityIDControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).AddInfo.ZA_AQISEntityIds_Hidden)));
			this.BindingSource.SetBindingMember(this.aQISEntityIDControl, "PivotsForBinding.AddInfo+ZA_AQISEntityIds_Hidden");
			this.aQISEntityIDControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AQISUserControl|17849900-1790-4977-A6C5-8AA6496F6B1F", "Entity ID");
			this.aQISEntityIDControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 15, true);
			this.aQISEntityIDControl.Name = "AQISEntityIDControl";
			this.aQISEntityIDControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.aQISEntityIDControl.TabIndex = 5;
			// 
			// AQISProducerCodeControl
			// 
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).AddInfo.ZA_AQISProducerCodes_Hidden)));
			this.BindingSource.SetBindingMember(this.aQISProducerCodeControl, "PivotsForBinding.AddInfo+ZA_AQISProducerCodes_Hidden");
			this.aQISProducerCodeControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AQISUserControl|46DCE5F4-A1A3-4C0E-8678-4FAC8C4035C9", "Producer Code");
			this.aQISProducerCodeControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 40, true);
			this.aQISProducerCodeControl.Name = "AQISProducerCodeControl";
			this.aQISProducerCodeControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.aQISProducerCodeControl.TabIndex = 3;
			// 
			// AQISDocumentGroupBox
			// 
			this.aQISDocumentGroupBox.Controls.Add(this.aQISDocumentGrid);
			this.aQISDocumentGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.aQISDocumentGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.aQISDocumentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 68, true);
			this.aQISDocumentGroupBox.Name = "AQISDocumentGroupBox";
			this.aQISDocumentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 250, true);
			this.aQISDocumentGroupBox.TabIndex = 2;
			this.aQISDocumentGroupBox.TabStop = false;
			this.aQISDocumentGroupBox.Text = "Quarantine Document";
			// 
			// AQISDocumentGrid
			// 
			this.aQISDocumentGrid.AllowNavigation = false;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).AQISDocuments)));
			this.BindingSource.SetBindingMember(this.aQISDocumentGrid, "PivotsForBinding.AQISDocuments");
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			this.aQISDocumentGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = "Type";
			zDropEditColumnStyleInfo1.ColumnName = "Type";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.ToolTip = resources.GetString("zDropEditColumnStyleInfo1.ToolTip");
			zTextBoxColumnStyleInfo1.Caption = "Number";
			zTextBoxColumnStyleInfo1.ColumnName = "Number";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.ToolTip = resources.GetString("zTextBoxColumnStyleInfo1.ToolTip");
			this.aQISDocumentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.aQISDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.aQISDocumentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.aQISDocumentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.aQISDocumentGrid.LayoutKey = "AQISDocumentGrid";
			this.aQISDocumentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.aQISDocumentGrid.Name = "AQISDocumentGrid";
			this.aQISDocumentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 231, true);
			this.aQISDocumentGrid.TabIndex = 0;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			// 
			// AQISPremisesIdAndPackagesGroupBox
			// 
			this.aQISPremisesIdAndPackagesGroupBox.Controls.Add(this.aQISPremisesIdProcessingTypeGrid);
			this.aQISPremisesIdAndPackagesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.aQISPremisesIdAndPackagesGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.aQISPremisesIdAndPackagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 68, true);
			this.aQISPremisesIdAndPackagesGroupBox.Name = "AQISPremisesIdAndPackagesGroupBox";
			this.aQISPremisesIdAndPackagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 250, true);
			this.aQISPremisesIdAndPackagesGroupBox.TabIndex = 3;
			this.aQISPremisesIdAndPackagesGroupBox.TabStop = false;
			this.aQISPremisesIdAndPackagesGroupBox.Text = "Quarantine Premises Id and AEP Processing Type";
			// 
			// AQISPremisesIdProcessingTypeGrid
			// 
			this.aQISPremisesIdProcessingTypeGrid.AllowNavigation = false;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).AQISPremisesIdAndProcessingTypes)));
			this.BindingSource.SetBindingMember(this.aQISPremisesIdProcessingTypeGrid, "PivotsForBinding.AQISPremisesIdAndProcessingTypes");
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			this.aQISPremisesIdProcessingTypeGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.Caption = "Premises Id";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "PremisesId";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.ToolTip = resources.GetString("zCodeFindBoxColumnStyleInfo1.ToolTip");
			zDropEditColumnStyleInfo2.Caption = "AEP Processing Type";
			zDropEditColumnStyleInfo2.ColumnName = "ProcessingType";
			zDropEditColumnStyleInfo2.IsMandatory = true;
			zDropEditColumnStyleInfo2.ToolTip = "A broker-nominated type indicating the actions that Quarantine should take with a consi" +
				 "gnment. The broker must be accredited with Quarantine as part of the BAS/AEP scheme to" +
				 " use this field";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			this.aQISPremisesIdProcessingTypeGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.aQISPremisesIdProcessingTypeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.aQISPremisesIdProcessingTypeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.aQISPremisesIdProcessingTypeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.aQISPremisesIdProcessingTypeGrid.LayoutKey = "AQISPremisesIdProcessingTypeGrid";
			this.aQISPremisesIdProcessingTypeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.aQISPremisesIdProcessingTypeGrid.Name = "AQISPremisesIdProcessingTypeGrid";
			this.aQISPremisesIdProcessingTypeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 231, true);
			this.aQISPremisesIdProcessingTypeGrid.TabIndex = 0;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			// 
			// AQISUserControl
			// 
			this.Controls.Add(this.aQISPremisesIdAndPackagesGroupBox);
			this.Controls.Add(this.aQISDocumentGroupBox);
			this.Controls.Add(this.aQISInformation);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Business";
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Business.Design.DataSourceTypeRequiredInstructionsType";
			this.Name = "AQISUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 318, true);
			this.aQISInformation.ResumeLayout(false);
			this.aQISDocumentGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.aQISDocumentGrid)).EndInit();
			this.aQISPremisesIdAndPackagesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.aQISPremisesIdProcessingTypeGrid)).EndInit();
			this.ResumeLayout(false);
		}

		ZGroupBox aQISInformation;
		AQISCommodityCodeControl aQISCommodityCodeControl;
		AQISPermitNumberControl aQISPermitNumberControl;
		AQISEntityIdControl aQISEntityIDControl;
		AQISProducerCodeControl aQISProducerCodeControl;
		ZGroupBox aQISDocumentGroupBox;
		ZGrid aQISDocumentGrid;
		ZGroupBox aQISPremisesIdAndPackagesGroupBox;
		internal ZGrid aQISPremisesIdProcessingTypeGrid;
	}
}
