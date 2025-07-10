namespace Enterprise.Registry.GUI
{
	partial class AWBLabelCustomisationRegistryControl
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
		void InitializeComponent()
		{
			this.CustomDesignDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OptionalInfo1DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OptionalInfo1Label = new Enterprise.ZArchitecture.ZLabel();
			this.OptionalInfo2Label = new Enterprise.ZArchitecture.ZLabel();
			this.OptionalInfo2DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OptionalInfo3Label = new Enterprise.ZArchitecture.ZLabel();
			this.OptionalInfo3DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OptionalInfo6Label = new Enterprise.ZArchitecture.ZLabel();
			this.OptionalInfo6DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OptionalInfo5Label = new Enterprise.ZArchitecture.ZLabel();
			this.OptionalInfo5DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OptionalInfo4Label = new Enterprise.ZArchitecture.ZLabel();
			this.OptionalInfo4DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SecondaryBarcodeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ConsolDestinationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ShipmentWeightCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ShipmentHandlingInformationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConsolPieceCountCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ShipmentPieceCountCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ShipmentPieceNumberCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConsolWeightCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HousebillNumberCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConsolOriginCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OptionalDescription1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OptionalInformationTopLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OptionalDescriptionTopLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OptionalDescription2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OptionalDescription3TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OptionalDescription6TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OptionalDescription5TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OptionalDescription4TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DesignLayoutGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DesignTemplateLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SecondaryBarcodeGroupBox.SuspendLayout();
			this.DesignLayoutGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.AWBLabelCustomisation);
			// 
			// CustomDesignDropEdit
			// 
			this.BindingSource.SetBindingMember(this.CustomDesignDropEdit, "CustomDesign");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).CustomDesign)));
			this.CustomDesignDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CustomDesignDropEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBLabelCustomisationRegistryControl|34ba0354-3357-4d01-9308-4958ed177397", "Optional Information Design");
			this.CustomDesignDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 3, true);
			this.CustomDesignDropEdit.Name = "CustomDesignDropEdit";
			this.CustomDesignDropEdit.ShowDescriptionBox = false;
			this.CustomDesignDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.CustomDesignDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.CustomDesignDropEdit.TabIndex = 0;
			// 
			// OptionalInfo1DropEdit
			// 
			this.BindingSource.SetBindingMember(this.OptionalInfo1DropEdit, "OptionalInformation1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).OptionalInformation1)));
			this.OptionalInfo1DropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OptionalInfo1DropEdit, false);
			this.OptionalInfo1DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 136, true);
			this.OptionalInfo1DropEdit.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.OptionalInfo1DropEdit.Name = "OptionalInfo1DropEdit";
			this.OptionalInfo1DropEdit.PreBoundMaxLength = 30;
			this.OptionalInfo1DropEdit.ShowDescriptionBox = false;
			this.OptionalInfo1DropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.OptionalInfo1DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.OptionalInfo1DropEdit.TabIndex = 5;
			// 
			// OptionalInfo1Label
			// 
			this.OptionalInfo1Label.AutoSize = true;
			this.OptionalInfo1Label.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBLabelCustomisationRegistryControl|69977938-f7d4-416c-94a6-9085daa6786c", "1");
			this.OptionalInfo1Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 139, true);
			this.OptionalInfo1Label.Name = "OptionalInfo1Label";
			this.OptionalInfo1Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.OptionalInfo1Label.TabIndex = 4;
			// 
			// OptionalInfo2Label
			// 
			this.OptionalInfo2Label.AutoSize = true;
			this.OptionalInfo2Label.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBLabelCustomisationRegistryControl|e2c0b735-714d-417f-b516-709ad8408b16", "2");
			this.OptionalInfo2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 165, true);
			this.OptionalInfo2Label.Name = "OptionalInfo2Label";
			this.OptionalInfo2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.OptionalInfo2Label.TabIndex = 7;
			// 
			// OptionalInfo2DropEdit
			// 
			this.BindingSource.SetBindingMember(this.OptionalInfo2DropEdit, "OptionalInformation2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).OptionalInformation2)));
			this.OptionalInfo2DropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OptionalInfo2DropEdit, false);
			this.OptionalInfo2DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 162, true);
			this.OptionalInfo2DropEdit.Name = "OptionalInfo2DropEdit";
			this.OptionalInfo2DropEdit.PreBoundMaxLength = 30;
			this.OptionalInfo2DropEdit.ShowDescriptionBox = false;
			this.OptionalInfo2DropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.OptionalInfo2DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.OptionalInfo2DropEdit.TabIndex = 8;
			// 
			// OptionalInfo3Label
			// 
			this.OptionalInfo3Label.AutoSize = true;
			this.OptionalInfo3Label.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBLabelCustomisationRegistryControl|bc14239d-c140-414e-b267-5a192f4c9616", "3");
			this.OptionalInfo3Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 191, true);
			this.OptionalInfo3Label.Name = "OptionalInfo3Label";
			this.OptionalInfo3Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.OptionalInfo3Label.TabIndex = 10;
			// 
			// OptionalInfo3DropEdit
			// 
			this.BindingSource.SetBindingMember(this.OptionalInfo3DropEdit, "OptionalInformation3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).OptionalInformation3)));
			this.OptionalInfo3DropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OptionalInfo3DropEdit, false);
			this.OptionalInfo3DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 188, true);
			this.OptionalInfo3DropEdit.Name = "OptionalInfo3DropEdit";
			this.OptionalInfo3DropEdit.PreBoundMaxLength = 30;
			this.OptionalInfo3DropEdit.ShowDescriptionBox = false;
			this.OptionalInfo3DropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.OptionalInfo3DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.OptionalInfo3DropEdit.TabIndex = 11;
			// 
			// OptionalInfo6Label
			// 
			this.OptionalInfo6Label.AutoSize = true;
			this.OptionalInfo6Label.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBLabelCustomisationRegistryControl|9203feb4-1287-43c9-a76b-83663f334ebc", "6");
			this.OptionalInfo6Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 269, true);
			this.OptionalInfo6Label.Name = "OptionalInfo6Label";
			this.OptionalInfo6Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.OptionalInfo6Label.TabIndex = 19;
			// 
			// OptionalInfo6DropEdit
			// 
			this.BindingSource.SetBindingMember(this.OptionalInfo6DropEdit, "OptionalInformation6");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).OptionalInformation6)));
			this.OptionalInfo6DropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OptionalInfo6DropEdit, false);
			this.OptionalInfo6DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 266, true);
			this.OptionalInfo6DropEdit.Name = "OptionalInfo6DropEdit";
			this.OptionalInfo6DropEdit.PreBoundMaxLength = 30;
			this.OptionalInfo6DropEdit.ShowDescriptionBox = false;
			this.OptionalInfo6DropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.OptionalInfo6DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.OptionalInfo6DropEdit.TabIndex = 20;
			// 
			// OptionalInfo5Label
			// 
			this.OptionalInfo5Label.AutoSize = true;
			this.OptionalInfo5Label.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBLabelCustomisationRegistryControl|e9f8880b-cc83-4ae0-810d-c1d944b4e99a", "5");
			this.OptionalInfo5Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 243, true);
			this.OptionalInfo5Label.Name = "OptionalInfo5Label";
			this.OptionalInfo5Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.OptionalInfo5Label.TabIndex = 16;
			// 
			// OptionalInfo5DropEdit
			// 
			this.BindingSource.SetBindingMember(this.OptionalInfo5DropEdit, "OptionalInformation5");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).OptionalInformation5)));
			this.OptionalInfo5DropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OptionalInfo5DropEdit, false);
			this.OptionalInfo5DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 240, true);
			this.OptionalInfo5DropEdit.Name = "OptionalInfo5DropEdit";
			this.OptionalInfo5DropEdit.PreBoundMaxLength = 30;
			this.OptionalInfo5DropEdit.ShowDescriptionBox = false;
			this.OptionalInfo5DropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.OptionalInfo5DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.OptionalInfo5DropEdit.TabIndex = 17;
			// 
			// OptionalInfo4Label
			// 
			this.OptionalInfo4Label.AutoSize = true;
			this.OptionalInfo4Label.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBLabelCustomisationRegistryControl|851fa659-1148-4d32-b59d-ed89991571d6", "4");
			this.OptionalInfo4Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 217, true);
			this.OptionalInfo4Label.Name = "OptionalInfo4Label";
			this.OptionalInfo4Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.OptionalInfo4Label.TabIndex = 13;
			// 
			// OptionalInfo4DropEdit
			// 
			this.BindingSource.SetBindingMember(this.OptionalInfo4DropEdit, "OptionalInformation4");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).OptionalInformation4)));
			this.OptionalInfo4DropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OptionalInfo4DropEdit, false);
			this.OptionalInfo4DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 214, true);
			this.OptionalInfo4DropEdit.Name = "OptionalInfo4DropEdit";
			this.OptionalInfo4DropEdit.PreBoundMaxLength = 30;
			this.OptionalInfo4DropEdit.ShowDescriptionBox = false;
			this.OptionalInfo4DropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.OptionalInfo4DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.OptionalInfo4DropEdit.TabIndex = 14;
			// 
			// SecondaryBarcodeGroupBox
			// 
			this.SecondaryBarcodeGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBLabelCustomisationRegistryControl|9cd6df05-d461-4dba-a29f-96e1d284d02f", "Secondary Barcode");
			this.SecondaryBarcodeGroupBox.Controls.Add(this.ConsolDestinationCheckBox);
			this.SecondaryBarcodeGroupBox.Controls.Add(this.ShipmentWeightCheckBox);
			this.SecondaryBarcodeGroupBox.Controls.Add(this.ShipmentHandlingInformationCheckBox);
			this.SecondaryBarcodeGroupBox.Controls.Add(this.ConsolPieceCountCheckBox);
			this.SecondaryBarcodeGroupBox.Controls.Add(this.ShipmentPieceCountCheckBox);
			this.SecondaryBarcodeGroupBox.Controls.Add(this.ShipmentPieceNumberCheckBox);
			this.SecondaryBarcodeGroupBox.Controls.Add(this.ConsolWeightCheckBox);
			this.SecondaryBarcodeGroupBox.Controls.Add(this.HousebillNumberCheckBox);
			this.SecondaryBarcodeGroupBox.Controls.Add(this.ConsolOriginCheckBox);
			this.SecondaryBarcodeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 293, true);
			this.SecondaryBarcodeGroupBox.Name = "SecondaryBarcodeGroupBox";
			this.SecondaryBarcodeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 129, true);
			this.SecondaryBarcodeGroupBox.TabIndex = 22;
			this.SecondaryBarcodeGroupBox.TabStop = false;
			// 
			// ConsolDestinationCheckBox
			// 
			this.ConsolDestinationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ConsolDestinationCheckBox, "BarcodeConsolDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).BarcodeConsolDestination)));
			this.ConsolDestinationCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBLabelCustomisationRegistryControl|98d305af-bebd-4f48-991d-c245f985222e", "Consol Destination");
			this.ConsolDestinationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ConsolDestinationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 39, true);
			this.ConsolDestinationCheckBox.Name = "ConsolDestinationCheckBox";
			this.ConsolDestinationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
			this.ConsolDestinationCheckBox.TabIndex = 1;
			this.ConsolDestinationCheckBox.UseVisualStyleBackColor = true;
			// 
			// ShipmentWeightCheckBox
			// 
			this.ShipmentWeightCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShipmentWeightCheckBox, "BarcodeShipmentWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).BarcodeShipmentWeight)));
			this.ShipmentWeightCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBLabelCustomisationRegistryControl|bcfe2b24-8e0f-4ed7-9761-5f7b533bf7b7", "Shipment Weight");
			this.ShipmentWeightCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShipmentWeightCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 85, true);
			this.ShipmentWeightCheckBox.Name = "ShipmentWeightCheckBox";
			this.ShipmentWeightCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 17, true);
			this.ShipmentWeightCheckBox.TabIndex = 7;
			this.ShipmentWeightCheckBox.UseVisualStyleBackColor = true;
			// 
			// ShipmentHandlingInformationCheckBox
			// 
			this.ShipmentHandlingInformationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShipmentHandlingInformationCheckBox, "BarcodeShipmentHandlingInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).BarcodeShipmentHandlingInformation)));
			this.ShipmentHandlingInformationCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBLabelCustomisationRegistryControl|6ca79eb2-3a85-4273-b8ee-d9fc820a7c95", "Shipment Handling Information");
			this.ShipmentHandlingInformationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShipmentHandlingInformationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 108, true);
			this.ShipmentHandlingInformationCheckBox.Name = "ShipmentHandlingInformationCheckBox";
			this.ShipmentHandlingInformationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 17, true);
			this.ShipmentHandlingInformationCheckBox.TabIndex = 8;
			this.ShipmentHandlingInformationCheckBox.UseVisualStyleBackColor = true;
			// 
			// ConsolPieceCountCheckBox
			// 
			this.ConsolPieceCountCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ConsolPieceCountCheckBox, "BarcodeConsolPieceCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).BarcodeConsolPieceCount)));
			this.ConsolPieceCountCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBLabelCustomisationRegistryControl|2eaf097c-3fab-4a1d-8376-d8d511e694fd", "Consol Total No. of Pieces");
			this.ConsolPieceCountCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ConsolPieceCountCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 62, true);
			this.ConsolPieceCountCheckBox.Name = "ConsolPieceCountCheckBox";
			this.ConsolPieceCountCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 17, true);
			this.ConsolPieceCountCheckBox.TabIndex = 2;
			this.ConsolPieceCountCheckBox.UseVisualStyleBackColor = true;
			// 
			// ShipmentPieceCountCheckBox
			// 
			this.ShipmentPieceCountCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShipmentPieceCountCheckBox, "BarcodeShipmentPieceCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).BarcodeShipmentPieceCount)));
			this.ShipmentPieceCountCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBLabelCustomisationRegistryControl|d815cf75-7f19-4d3d-b9cc-38dbc4452ed1", "Shipment Total No. of Pieces");
			this.ShipmentPieceCountCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShipmentPieceCountCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 62, true);
			this.ShipmentPieceCountCheckBox.Name = "ShipmentPieceCountCheckBox";
			this.ShipmentPieceCountCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 17, true);
			this.ShipmentPieceCountCheckBox.TabIndex = 6;
			this.ShipmentPieceCountCheckBox.UseVisualStyleBackColor = true;
			// 
			// ShipmentPieceNumberCheckBox
			// 
			this.ShipmentPieceNumberCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShipmentPieceNumberCheckBox, "BarcodeShipmentPieceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).BarcodeShipmentPieceNumber)));
			this.ShipmentPieceNumberCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBLabelCustomisationRegistryControl|81b3296a-708a-40e8-834e-7c4ecaa401ff", "Shipment Piece Number");
			this.ShipmentPieceNumberCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShipmentPieceNumberCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 39, true);
			this.ShipmentPieceNumberCheckBox.Name = "ShipmentPieceNumberCheckBox";
			this.ShipmentPieceNumberCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 17, true);
			this.ShipmentPieceNumberCheckBox.TabIndex = 5;
			this.ShipmentPieceNumberCheckBox.UseVisualStyleBackColor = true;
			// 
			// ConsolWeightCheckBox
			// 
			this.ConsolWeightCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ConsolWeightCheckBox, "BarcodeConsolWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).BarcodeConsolWeight)));
			this.ConsolWeightCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBLabelCustomisationRegistryControl|0dcc6095-a3b0-49bf-8ae8-ae70d6e60d2f", "Consol Weight");
			this.ConsolWeightCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ConsolWeightCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 85, true);
			this.ConsolWeightCheckBox.Name = "ConsolWeightCheckBox";
			this.ConsolWeightCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 17, true);
			this.ConsolWeightCheckBox.TabIndex = 3;
			this.ConsolWeightCheckBox.UseVisualStyleBackColor = true;
			// 
			// HousebillNumberCheckBox
			// 
			this.HousebillNumberCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HousebillNumberCheckBox, "BarcodeHousebillNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).BarcodeHousebillNumber)));
			this.HousebillNumberCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBLabelCustomisationRegistryControl|0073f9f7-1b10-4f22-9783-8ea2b1dc555b", "House Bill Number");
			this.HousebillNumberCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HousebillNumberCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 16, true);
			this.HousebillNumberCheckBox.Name = "HousebillNumberCheckBox";
			this.HousebillNumberCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 17, true);
			this.HousebillNumberCheckBox.TabIndex = 4;
			this.HousebillNumberCheckBox.UseVisualStyleBackColor = true;
			// 
			// ConsolOriginCheckBox
			// 
			this.ConsolOriginCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ConsolOriginCheckBox, "BarcodeConsolOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).BarcodeConsolOrigin)));
			this.ConsolOriginCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBLabelCustomisationRegistryControl|6d574b8a-2964-4787-8846-e58aed0d9a71", "Consol Origin");
			this.ConsolOriginCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ConsolOriginCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ConsolOriginCheckBox.Name = "ConsolOriginCheckBox";
			this.ConsolOriginCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 17, true);
			this.ConsolOriginCheckBox.TabIndex = 0;
			this.ConsolOriginCheckBox.UseVisualStyleBackColor = true;
			// 
			// OptionalDescription1TextBox
			// 
			this.BindingSource.SetBindingMember(this.OptionalDescription1TextBox, "OptionalDescription1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).OptionalDescription1)));
			this.OptionalDescription1TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OptionalDescription1TextBox, false);
			this.OptionalDescription1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 136, true);
			this.OptionalDescription1TextBox.Name = "OptionalDescription1TextBox";
			this.OptionalDescription1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.OptionalDescription1TextBox.TabIndex = 6;
			// 
			// OptionalInformationTopLabel
			// 
			this.OptionalInformationTopLabel.AutoSize = true;
			this.OptionalInformationTopLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBLabelCustomisationRegistryControl|3befda20-162a-408f-995d-50744ecb539a", "Optional Information");
			this.OptionalInformationTopLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 120, true);
			this.OptionalInformationTopLabel.Name = "OptionalInformationTopLabel";
			this.OptionalInformationTopLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 13, true);
			this.OptionalInformationTopLabel.TabIndex = 2;
			// 
			// OptionalDescriptionTopLabel
			// 
			this.OptionalDescriptionTopLabel.AutoSize = true;
			this.OptionalDescriptionTopLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBLabelCustomisationRegistryControl|30efe5db-77f6-437e-b734-74a84f373960", "Optional Description");
			this.OptionalDescriptionTopLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 120, true);
			this.OptionalDescriptionTopLabel.Name = "OptionalDescriptionTopLabel";
			this.OptionalDescriptionTopLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 13, true);
			this.OptionalDescriptionTopLabel.TabIndex = 3;
			// 
			// OptionalDescription2TextBox
			// 
			this.BindingSource.SetBindingMember(this.OptionalDescription2TextBox, "OptionalDescription2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).OptionalDescription2)));
			this.OptionalDescription2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OptionalDescription2TextBox, false);
			this.OptionalDescription2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 162, true);
			this.OptionalDescription2TextBox.Name = "OptionalDescription2TextBox";
			this.OptionalDescription2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.OptionalDescription2TextBox.TabIndex = 9;
			// 
			// OptionalDescription3TextBox
			// 
			this.BindingSource.SetBindingMember(this.OptionalDescription3TextBox, "OptionalDescription3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).OptionalDescription3)));
			this.OptionalDescription3TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OptionalDescription3TextBox, false);
			this.OptionalDescription3TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 188, true);
			this.OptionalDescription3TextBox.Name = "OptionalDescription3TextBox";
			this.OptionalDescription3TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.OptionalDescription3TextBox.TabIndex = 12;
			// 
			// OptionalDescription6TextBox
			// 
			this.BindingSource.SetBindingMember(this.OptionalDescription6TextBox, "OptionalDescription6");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).OptionalDescription6)));
			this.OptionalDescription6TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OptionalDescription6TextBox, false);
			this.OptionalDescription6TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 266, true);
			this.OptionalDescription6TextBox.Name = "OptionalDescription6TextBox";
			this.OptionalDescription6TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.OptionalDescription6TextBox.TabIndex = 21;
			// 
			// OptionalDescription5TextBox
			// 
			this.BindingSource.SetBindingMember(this.OptionalDescription5TextBox, "OptionalDescription5");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).OptionalDescription5)));
			this.OptionalDescription5TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OptionalDescription5TextBox, false);
			this.OptionalDescription5TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 240, true);
			this.OptionalDescription5TextBox.Name = "OptionalDescription5TextBox";
			this.OptionalDescription5TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.OptionalDescription5TextBox.TabIndex = 18;
			// 
			// OptionalDescription4TextBox
			// 
			this.BindingSource.SetBindingMember(this.OptionalDescription4TextBox, "OptionalDescription4");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.AWBLabelCustomisation)(null)).OptionalDescription4)));
			this.OptionalDescription4TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OptionalDescription4TextBox, false);
			this.OptionalDescription4TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 214, true);
			this.OptionalDescription4TextBox.Name = "OptionalDescription4TextBox";
			this.OptionalDescription4TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.OptionalDescription4TextBox.TabIndex = 15;
			// 
			// DesignLayoutGroupBox
			// 
			this.DesignLayoutGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBLabelCustomisationRegistryControl|56bf91f0-3f7e-4010-98ef-4f0dbc94e1ff", "Layout");
			this.DesignLayoutGroupBox.Controls.Add(this.DesignTemplateLabel);
			this.DesignLayoutGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 26, true);
			this.DesignLayoutGroupBox.Name = "DesignLayoutGroupBox";
			this.DesignLayoutGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 91, true);
			this.DesignLayoutGroupBox.TabIndex = 1;
			this.DesignLayoutGroupBox.TabStop = false;
			// 
			// DesignTemplateLabel
			// 
			this.DesignTemplateLabel.AutoSize = true;
			this.DesignTemplateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.DesignTemplateLabel.Name = "DesignTemplateLabel";
			this.DesignTemplateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.DesignTemplateLabel.TabIndex = 0;
			this.DesignTemplateLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// AWBLabelCustomisationRegistryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DesignLayoutGroupBox);
			this.Controls.Add(this.OptionalDescription6TextBox);
			this.Controls.Add(this.OptionalDescription5TextBox);
			this.Controls.Add(this.OptionalDescription4TextBox);
			this.Controls.Add(this.OptionalDescription3TextBox);
			this.Controls.Add(this.OptionalDescription2TextBox);
			this.Controls.Add(this.OptionalDescriptionTopLabel);
			this.Controls.Add(this.OptionalInformationTopLabel);
			this.Controls.Add(this.OptionalDescription1TextBox);
			this.Controls.Add(this.SecondaryBarcodeGroupBox);
			this.Controls.Add(this.OptionalInfo6Label);
			this.Controls.Add(this.OptionalInfo6DropEdit);
			this.Controls.Add(this.OptionalInfo5Label);
			this.Controls.Add(this.OptionalInfo5DropEdit);
			this.Controls.Add(this.OptionalInfo4Label);
			this.Controls.Add(this.OptionalInfo4DropEdit);
			this.Controls.Add(this.OptionalInfo3Label);
			this.Controls.Add(this.OptionalInfo3DropEdit);
			this.Controls.Add(this.OptionalInfo2Label);
			this.Controls.Add(this.OptionalInfo2DropEdit);
			this.Controls.Add(this.OptionalInfo1Label);
			this.Controls.Add(this.OptionalInfo1DropEdit);
			this.Controls.Add(this.CustomDesignDropEdit);
			this.Name = "AWBLabelCustomisationRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 430, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SecondaryBarcodeGroupBox.ResumeLayout(false);
			this.SecondaryBarcodeGroupBox.PerformLayout();
			this.DesignLayoutGroupBox.ResumeLayout(false);
			this.DesignLayoutGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit CustomDesignDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OptionalInfo1DropEdit;
		private Enterprise.ZArchitecture.ZLabel OptionalInfo1Label;
		private Enterprise.ZArchitecture.ZLabel OptionalInfo2Label;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OptionalInfo2DropEdit;
		private Enterprise.ZArchitecture.ZLabel OptionalInfo3Label;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OptionalInfo3DropEdit;
		private Enterprise.ZArchitecture.ZLabel OptionalInfo6Label;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OptionalInfo6DropEdit;
		private Enterprise.ZArchitecture.ZLabel OptionalInfo5Label;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OptionalInfo5DropEdit;
		private Enterprise.ZArchitecture.ZLabel OptionalInfo4Label;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OptionalInfo4DropEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox SecondaryBarcodeGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ConsolDestinationCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ShipmentWeightCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ShipmentHandlingInformationCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ConsolPieceCountCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ShipmentPieceCountCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ShipmentPieceNumberCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ConsolWeightCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox HousebillNumberCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ConsolOriginCheckBox;
		private Enterprise.ZArchitecture.ZTextBox OptionalDescription1TextBox;
		private Enterprise.ZArchitecture.ZLabel OptionalInformationTopLabel;
		private Enterprise.ZArchitecture.ZLabel OptionalDescriptionTopLabel;
		private Enterprise.ZArchitecture.ZTextBox OptionalDescription2TextBox;
		private Enterprise.ZArchitecture.ZTextBox OptionalDescription3TextBox;
		private Enterprise.ZArchitecture.ZTextBox OptionalDescription6TextBox;
		private Enterprise.ZArchitecture.ZTextBox OptionalDescription5TextBox;
		private Enterprise.ZArchitecture.ZTextBox OptionalDescription4TextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DesignLayoutGroupBox;
		private Enterprise.ZArchitecture.ZLabel DesignTemplateLabel;

	}
}
