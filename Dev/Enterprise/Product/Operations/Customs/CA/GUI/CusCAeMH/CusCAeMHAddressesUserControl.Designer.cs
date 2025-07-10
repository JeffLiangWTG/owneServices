namespace Enterprise.Customs.CA.GUI
{
	partial class CusCAeMHAddressesUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AddressSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.AddressesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AddressDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AddressSplitContainer)).BeginInit();
			this.AddressSplitContainer.Panel1.SuspendLayout();
			this.AddressSplitContainer.Panel2.SuspendLayout();
			this.AddressSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AddressesGrid)).BeginInit();
			this.AddressesGrid.SuspendLayout();
			this.AddressDocAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.CAeMHDocAddressDependentCollection);
			// 
			// AddressSplitContainer
			// 
			this.AddressSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.AddressSplitContainer.IsSplitterFixed = true;
			this.AddressSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AddressSplitContainer.Name = "AddressSplitContainer";
			// 
			// AddressSplitContainer.Panel1
			// 
			this.AddressSplitContainer.Panel1.Controls.Add(this.AddressesGrid);
			// 
			// AddressSplitContainer.Panel2
			// 
			this.AddressSplitContainer.Panel2.Controls.Add(this.AddressDocAddressControl);
			this.AddressSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(788, 187, true);
			this.AddressSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(530);
			this.AddressSplitContainer.TabIndex = 0;
			// 
			// AddressesGrid
			// 
			this.AddressesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AddressesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)).E2_AddressType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)).AddressDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)).E2_AddressOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)).OrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)).Lookups.ThirdParties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)).E2_OA_Address)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)).E2_CompanyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)).E2_Address1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)).E2_Address2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)).E2_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)).E2_State)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)).E2_City)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)).E2_Postcode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)).E2_GovRegNumType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)).E2_GovRegNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)).E2_Contact)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)).ContactDataFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)).Organisation.Contacts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)).E2_Phone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)).E2_Fax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)).E2_Email)));
			this.AddressesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("97a38472-9025-4bf6-9e1f-f79fe81bc79e", "Address Type");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "E2_AddressType";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("14d1face-4c24-4af4-93eb-a72d46e79311", "Add. Desc.", "Address Type Description", "");
			zTextBoxColumnStyleInfo1.ColumnName = "AddressDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6ec9573a-f3a7-4b8c-b54c-e195010469d1", "Override");
			zCheckBoxColumnStyleInfo1.ColumnName = "E2_AddressOverride";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups.ThirdParties";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e43be9ec-3313-4103-96bc-729c4a9dc977", "Org.", "Organization", "");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OrganisationPK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6dbcbeda-40ff-4e63-9f83-abfd26917bd5", "Address");
			zGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo1.ColumnName = "E2_OA_Address";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(171);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("97ed9cb1-cfce-47fb-91f6-bc2727b73e43", "Company Name");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "E2_CompanyName";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(143);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3b4b47e4-1551-4c2e-91e2-be7fd6d60b6e", "Address 1");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "E2_Address1";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(163);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("865cab4b-47f4-465c-91c7-3989b3e27391", "Address 2");
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "E2_Address2";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(127);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("4afc7973-58fb-44e5-8d4c-9e775db817c2", "Country/Region");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "E2_RN_NKCountryCode";
			zCodeFindBoxColumnStyleInfo1.IsVisible = false;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("dadc037b-3306-4660-ad6e-8ad4a256e9c8", "State");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "E2_State";
			zDropEditColumnStyleInfo2.IsVisible = false;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(71);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5a2c3774-5526-4ab5-8ba8-e84290a1e97f", "City");
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "E2_City";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(54);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("44faefbc-c39a-4e43-85da-2d831124fd1c", "Post Code");
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "E2_Postcode";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("68df8d1a-24b5-4f84-837b-3c3e98dee8be", "Code Type");
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "E2_GovRegNumType";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6db90e92-885c-40ae-bbae-45b715f4b1f9", "Code");
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "E2_GovRegNum";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.BindToList = "Organisation+Contacts";
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("32e3c1dd-3e06-467d-a2bc-a6d70d38762d", "Contact");
			zMultiControlColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiControlColumnStyleInfo1.ColumnName = "E2_Contact";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "ContactDataFieldType";
			zMultiControlColumnStyleInfo1.IsVisible = false;
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b2f00b5c-61ff-4756-a648-3c7b93f82d89", "Phone");
			zTextBoxColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo8.ColumnName = "E2_Phone";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(103);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e4726e76-f8b6-42dc-b751-5d2039c80c38", "Fax");
			zTextBoxColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo9.ColumnName = "E2_Fax";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("008d9f66-4648-4772-9c20-724567725385", "Email");
			zTextBoxColumnStyleInfo10.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo10.ColumnName = "E2_Email";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			this.AddressesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AddressesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AddressesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.AddressesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.AddressesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.AddressesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AddressesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AddressesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.AddressesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.AddressesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.AddressesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.AddressesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.AddressesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.AddressesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.AddressesGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.AddressesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.AddressesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.AddressesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.AddressesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressesGrid.GridId = "936474b3-7096-49d4-89ab-2c60a2769526";
			this.AddressesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AddressesGrid.LayoutKey = "AddressesGrid";
			this.AddressesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AddressesGrid.Name = "AddressesGrid";
			this.AddressesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 187, true);
			this.AddressesGrid.TabIndex = 0;
			this.AddressesGrid.CurrentCellChanged += new System.EventHandler(this.AddressesGrid_CurrentCellChanged);
			// 
			// AddressDocAddressControl
			// 
			this.AddressDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressDocAddressControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.CA.Business.CAeMHDocAddress)(null)))));
			this.AddressDocAddressControl.BindToOrganisations = "Lookups+ThirdParties";
			this.AddressDocAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f48c9de9-be0f-4afa-889d-9fd90cbd67bd", "Related Party");
			this.AddressDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.AddressDocAddressControl.Name = "AddressDocAddressControl";
			this.AddressDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.AddressDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.AddressDocAddressControl.TabIndex = 0;
			this.AddressDocAddressControl.ValidationJustForced = false;
			// 
			// CusCAeMHAddressesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AddressSplitContainer);
			this.Name = "CusCAeMHAddressesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(788, 187, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AddressSplitContainer.Panel1.ResumeLayout(false);
			this.AddressSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AddressSplitContainer)).EndInit();
			this.AddressSplitContainer.ResumeLayout(false);
			this.AddressSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AddressesGrid)).EndInit();
			this.AddressesGrid.ResumeLayout(false);
			this.AddressesGrid.PerformLayout();
			this.AddressDocAddressControl.ResumeLayout(true);
			this.AddressDocAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer AddressSplitContainer;
		private ZArchitecture.ZGrid AddressesGrid;
		private MasterFiles.GUI.ZDocAddressControl AddressDocAddressControl;
	}
}
