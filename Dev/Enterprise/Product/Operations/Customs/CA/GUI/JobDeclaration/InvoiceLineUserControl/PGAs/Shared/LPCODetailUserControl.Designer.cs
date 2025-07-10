namespace Enterprise.Customs.CA.GUI
{
	partial class LPCODetailUserControl
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
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelDocumentType = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelIssuanceCountryCaption = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelCountryOfAuthCaption = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelMixedCaption = new Enterprise.ZArchitecture.ZLabel();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabelHolderContactEmail = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelHolderContactPhone = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelHolderContactName = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelAddress = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelHolderType = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelHolderOrg = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelHolderName = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel20 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel19 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel18 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel12 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel11 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel10 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel9 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel13 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel14 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel1ExpiryDateCaption = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel16 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel17 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelDocumentDescription = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelRefNo = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelEndUseDescription = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelDIFURN = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelCommodityType = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelIssuanceCountry = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelCountryOfAuth = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelMixed = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelQuantity = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelUQ = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelStartDate = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelIssueDate = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelExpiryDate = new Enterprise.ZArchitecture.ZLabel();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabelApplicantContactEmail = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelApplicantContactPhone = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelApplicantContactName = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelApplicantAddress = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelApplicantType = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelApplicantOrg = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelApplicantName = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel28 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel29 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel30 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel31 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel32 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel33 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel34 = new Enterprise.ZArchitecture.ZLabel();
			this.zGroupBox3 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.LPCOViewCollection);
			// 
			// zLabel1
			// 
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 0, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 16, true);
			this.zLabel1.TabIndex = 0;
			this.zLabel1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("849b9e2a-2919-44d8-90a4-9b34d9fc18df", "Document Type:");
			this.zLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabelDocumentType
			// 
			this.BindingSource.SetBindingMember(this.zLabelDocumentType, "CLP_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_Type)));
			this.zLabelDocumentType.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelDocumentType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 0, true);
			this.zLabelDocumentType.Name = "zLabelDocumentType";
			this.zLabelDocumentType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 16, true);
			this.zLabelDocumentType.TabIndex = 1;
			this.zLabelDocumentType.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7bcfbb84-9894-4e6a-9242-2626aad0ec5a", "<Type>");
			// 
			// zLabel2
			// 
			this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 33, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 16, true);
			this.zLabel2.TabIndex = 2;
			this.zLabel2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e3c9dc00-8847-4814-bf30-8995115dda7a", "Ref No:");
			this.zLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel3
			// 
			this.zLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 49, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 16, true);
			this.zLabel3.TabIndex = 3;
			this.zLabel3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5de3a4b3-4ebc-4a61-94ec-aab9b4d8cfc4", "End Use Description:");
			this.zLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel4
			// 
			this.zLabel4.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 65, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 16, true);
			this.zLabel4.TabIndex = 4;
			this.zLabel4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7dde103e-bc2f-4b29-bd7c-4dc784dbb887", "DIF URN:");
			this.zLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel5
			// 
			this.zLabel5.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 81, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 16, true);
			this.zLabel5.TabIndex = 5;
			this.zLabel5.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2db895e6-8089-48fa-bb73-a71fd5738cdc", "Commodity Type:");
			this.zLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabelIssuanceCountryCaption
			// 
			this.zLabelIssuanceCountryCaption.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelIssuanceCountryCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 97, true);
			this.zLabelIssuanceCountryCaption.Name = "zLabelIssuanceCountryCaption";
			this.zLabelIssuanceCountryCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 16, true);
			this.zLabelIssuanceCountryCaption.TabIndex = 6;
			this.zLabelIssuanceCountryCaption.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("570a8ccc-4234-4a3b-b96f-054207e70ac7", "Issuance Country/Region:");
			this.zLabelIssuanceCountryCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabelCountryOfAuthCaption
			// 
			this.zLabelCountryOfAuthCaption.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelCountryOfAuthCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 113, true);
			this.zLabelCountryOfAuthCaption.Name = "zLabelCountryOfAuthCaption";
			this.zLabelCountryOfAuthCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 16, true);
			this.zLabelCountryOfAuthCaption.TabIndex = 7;
			this.zLabelCountryOfAuthCaption.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0710f23d-15cd-4b29-94ea-1673657bd350", "Country/Region of Auth.:");
			this.zLabelCountryOfAuthCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabelMixedCaption
			// 
			this.zLabelMixedCaption.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelMixedCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 129, true);
			this.zLabelMixedCaption.Name = "zLabelMixedCaption";
			this.zLabelMixedCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 16, true);
			this.zLabelMixedCaption.TabIndex = 8;
			this.zLabelMixedCaption.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("4d2118c2-32a7-42a7-9023-875e88585b0b", "Mixed:");
			this.zLabelMixedCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.zLabelHolderContactEmail);
			this.zGroupBox1.Controls.Add(this.zLabelHolderContactPhone);
			this.zGroupBox1.Controls.Add(this.zLabelHolderContactName);
			this.zGroupBox1.Controls.Add(this.zLabelAddress);
			this.zGroupBox1.Controls.Add(this.zLabelHolderType);
			this.zGroupBox1.Controls.Add(this.zLabelHolderOrg);
			this.zGroupBox1.Controls.Add(this.zLabelHolderName);
			this.zGroupBox1.Controls.Add(this.zLabel20);
			this.zGroupBox1.Controls.Add(this.zLabel19);
			this.zGroupBox1.Controls.Add(this.zLabel18);
			this.zGroupBox1.Controls.Add(this.zLabel12);
			this.zGroupBox1.Controls.Add(this.zLabel11);
			this.zGroupBox1.Controls.Add(this.zLabel10);
			this.zGroupBox1.Controls.Add(this.zLabel9);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(503, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 151, true);
			this.zGroupBox1.TabIndex = 9;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b4b3279a-cd36-4c32-a071-5d9b51a31143", "Authorized Party");
			// 
			// zLabelHolderContactEmail
			// 
			this.BindingSource.SetBindingMember(this.zLabelHolderContactEmail, "CLP_HolderContactEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_HolderContactEmail)));
			this.zLabelHolderContactEmail.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelHolderContactEmail.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 132, true);
			this.zLabelHolderContactEmail.Name = "zLabelHolderContactEmail";
			this.zLabelHolderContactEmail.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 16, true);
			this.zLabelHolderContactEmail.TabIndex = 34;
			this.zLabelHolderContactEmail.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("dc8101cd-01b6-4bfb-8ab8-70024ab48edc", "<Contact Email>");
			// 
			// zLabelHolderContactPhone
			// 
			this.BindingSource.SetBindingMember(this.zLabelHolderContactPhone, "CLP_HolderContactPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_HolderContactPhone)));
			this.zLabelHolderContactPhone.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelHolderContactPhone.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 116, true);
			this.zLabelHolderContactPhone.Name = "zLabelHolderContactPhone";
			this.zLabelHolderContactPhone.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 16, true);
			this.zLabelHolderContactPhone.TabIndex = 33;
			this.zLabelHolderContactPhone.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("fdc82873-6048-4dcb-bcd2-ce8a8a9444b5", "<Contact Phone>");
			// 
			// zLabelHolderContactName
			// 
			this.BindingSource.SetBindingMember(this.zLabelHolderContactName, "CLP_HolderContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_HolderContactName)));
			this.zLabelHolderContactName.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelHolderContactName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 100, true);
			this.zLabelHolderContactName.Name = "zLabelHolderContactName";
			this.zLabelHolderContactName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 16, true);
			this.zLabelHolderContactName.TabIndex = 32;
			this.zLabelHolderContactName.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("93f4e35f-86e1-43b9-9f3f-73e2cecf2d1a", "<Contact Name>");
			// 
			// zLabelAddress
			// 
			this.BindingSource.SetBindingMember(this.zLabelAddress, "CLP_OA_LPCOHolder_Address");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_OA_LPCOHolder_Address)));
			this.zLabelAddress.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 66, true);
			this.zLabelAddress.Name = "zLabelAddress";
			this.zLabelAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 33, true);
			this.zLabelAddress.TabIndex = 31;
			this.zLabelAddress.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("761e0091-2dda-492e-81a7-39bb852dc5ab", "<Address>");
			this.zLabelAddress.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// zLabelHolderType
			// 
			this.BindingSource.SetBindingMember(this.zLabelHolderType, "CLP_HolderType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_HolderType)));
			this.zLabelHolderType.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelHolderType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 32, true);
			this.zLabelHolderType.Name = "zLabelHolderType";
			this.zLabelHolderType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 16, true);
			this.zLabelHolderType.TabIndex = 30;
			this.zLabelHolderType.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9a3731c3-575b-41da-9461-d46eb2f5f3cd", "<Type>");
			// 
			// zLabelHolderOrg
			// 
			this.BindingSource.SetBindingMember(this.zLabelHolderOrg, "LPCOHolderOrgCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).LPCOHolderOrgCode)));
			this.zLabelHolderOrg.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelHolderOrg.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 48, true);
			this.zLabelHolderOrg.Name = "zLabelHolderOrg";
			this.zLabelHolderOrg.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 16, true);
			this.zLabelHolderOrg.TabIndex = 29;
			this.zLabelHolderOrg.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("4a016e84-8fa8-4ecb-b051-340469eccc30", "<Organization>");
			// 
			// zLabelHolderName
			// 
			this.BindingSource.SetBindingMember(this.zLabelHolderName, "CLP_HolderName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_HolderName)));
			this.zLabelHolderName.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelHolderName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 16, true);
			this.zLabelHolderName.Name = "zLabelHolderName";
			this.zLabelHolderName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 16, true);
			this.zLabelHolderName.TabIndex = 28;
			this.zLabelHolderName.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("07321591-16b9-4333-be81-1402f26ab4f4", "<Name>");
			// 
			// zLabel20
			// 
			this.zLabel20.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel20.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 132, true);
			this.zLabel20.Name = "zLabel20";
			this.zLabel20.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 16, true);
			this.zLabel20.TabIndex = 9;
			this.zLabel20.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ac319772-7525-4276-a10b-f09f56843900", "Contact Email:");
			this.zLabel20.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel19
			// 
			this.zLabel19.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel19.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 116, true);
			this.zLabel19.Name = "zLabel19";
			this.zLabel19.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 16, true);
			this.zLabel19.TabIndex = 8;
			this.zLabel19.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("4f9de8e8-8f5d-4752-9729-6dd5063838f6", "Contact Phone:");
			this.zLabel19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel18
			// 
			this.zLabel18.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel18.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 100, true);
			this.zLabel18.Name = "zLabel18";
			this.zLabel18.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 16, true);
			this.zLabel18.TabIndex = 7;
			this.zLabel18.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("33f9b429-872b-4511-aca0-c44cc0c5bc96", "Contact Name:");
			this.zLabel18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel12
			// 
			this.zLabel12.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.zLabel12.Name = "zLabel12";
			this.zLabel12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 16, true);
			this.zLabel12.TabIndex = 6;
			this.zLabel12.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("98c48426-a905-45f2-bdb4-dac292505ec3", "Name:");
			this.zLabel12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel11
			// 
			this.zLabel11.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 64, true);
			this.zLabel11.Name = "zLabel11";
			this.zLabel11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 16, true);
			this.zLabel11.TabIndex = 5;
			this.zLabel11.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("981b733a-21ac-4353-bb15-739367397e38", "Address:");
			this.zLabel11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel10
			// 
			this.zLabel10.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 48, true);
			this.zLabel10.Name = "zLabel10";
			this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 16, true);
			this.zLabel10.TabIndex = 4;
			this.zLabel10.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("35a7a51a-1a28-4e25-80ac-35f7ce79b58c", "Organization:");
			this.zLabel10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel9
			// 
			this.zLabel9.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 32, true);
			this.zLabel9.Name = "zLabel9";
			this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 16, true);
			this.zLabel9.TabIndex = 3;
			this.zLabel9.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("cbd975fa-eb41-45df-b27e-8178b4b2d54f", "Type:");
			this.zLabel9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel13
			// 
			this.zLabel13.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 65, true);
			this.zLabel13.Name = "zLabel13";
			this.zLabel13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 16, true);
			this.zLabel13.TabIndex = 10;
			this.zLabel13.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2d2be2f1-364e-492c-8289-66cfafe0a636", "Start Date:");
			this.zLabel13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel14
			// 
			this.zLabel14.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel14.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 81, true);
			this.zLabel14.Name = "zLabel14";
			this.zLabel14.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 16, true);
			this.zLabel14.TabIndex = 11;
			this.zLabel14.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2330dc91-4b9d-4ed1-8d6f-797d9e373dfa", "Issue Date:");
			this.zLabel14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel1ExpiryDateCaption
			// 
			this.zLabel1ExpiryDateCaption.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1ExpiryDateCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 97, true);
			this.zLabel1ExpiryDateCaption.Name = "zLabel1ExpiryDateCaption";
			this.zLabel1ExpiryDateCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 16, true);
			this.zLabel1ExpiryDateCaption.TabIndex = 12;
			this.zLabel1ExpiryDateCaption.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e6c9c7d2-0bbc-4795-a22b-6281d72247ab", "Expiry Date:");
			this.zLabel1ExpiryDateCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel16
			// 
			this.zLabel16.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel16.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 33, true);
			this.zLabel16.Name = "zLabel16";
			this.zLabel16.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 16, true);
			this.zLabel16.TabIndex = 13;
			this.zLabel16.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b7de02a9-d909-4734-9a49-ba468992256c", "Quantity:");
			this.zLabel16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel17
			// 
			this.zLabel17.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel17.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 49, true);
			this.zLabel17.Name = "zLabel17";
			this.zLabel17.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 16, true);
			this.zLabel17.TabIndex = 14;
			this.zLabel17.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("05258544-a2fd-43a6-9420-c85b6770882b", "UQ:");
			this.zLabel17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabelDocumentDescription
			// 
			this.BindingSource.SetBindingMember(this.zLabelDocumentDescription, "LPCO+TypeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).LPCO.TypeDescription)));
			this.zLabelDocumentDescription.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelDocumentDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 2, true);
			this.zLabelDocumentDescription.Name = "zLabelDocumentDescription";
			this.zLabelDocumentDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 30, true);
			this.zLabelDocumentDescription.TabIndex = 15;
			this.zLabelDocumentDescription.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("628d04ad-614a-472c-8355-084d9a58efd7", "<Document Description>");
			this.zLabelDocumentDescription.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// zLabelRefNo
			// 
			this.BindingSource.SetBindingMember(this.zLabelRefNo, "CLP_RefNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_RefNo)));
			this.zLabelRefNo.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelRefNo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 33, true);
			this.zLabelRefNo.Name = "zLabelRefNo";
			this.zLabelRefNo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 16, true);
			this.zLabelRefNo.TabIndex = 16;
			this.zLabelRefNo.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7d844818-d505-422a-8efc-b92d3ceacad8", "<Ref No>");
			// 
			// zLabelEndUseDescription
			// 
			this.BindingSource.SetBindingMember(this.zLabelEndUseDescription, "CLP_SecondaryRefNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_SecondaryRefNo)));
			this.zLabelEndUseDescription.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelEndUseDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 49, true);
			this.zLabelEndUseDescription.Name = "zLabelEndUseDescription";
			this.zLabelEndUseDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 16, true);
			this.zLabelEndUseDescription.TabIndex = 17;
			this.zLabelEndUseDescription.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("200721e7-fffa-4dd0-8619-9cedb41d5875", "<End Use Description>");
			// 
			// zLabelDIFURN
			// 
			this.BindingSource.SetBindingMember(this.zLabelDIFURN, "CLP_DIFRefNumberOrLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_DIFRefNumberOrLocation)));
			this.zLabelDIFURN.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelDIFURN.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 65, true);
			this.zLabelDIFURN.Name = "zLabelDIFURN";
			this.zLabelDIFURN.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 16, true);
			this.zLabelDIFURN.TabIndex = 18;
			this.zLabelDIFURN.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9da96b32-4c82-46ab-89e3-07a20faee4ca", "<DIF URN>");
			// 
			// zLabelCommodityType
			// 
			this.BindingSource.SetBindingMember(this.zLabelCommodityType, "CLP_CommodityTypeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_CommodityTypeCode)));
			this.zLabelCommodityType.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelCommodityType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 81, true);
			this.zLabelCommodityType.Name = "zLabelCommodityType";
			this.zLabelCommodityType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 16, true);
			this.zLabelCommodityType.TabIndex = 19;
			this.zLabelCommodityType.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8e25c46c-b7b4-462c-a449-4e9df8655c8f", "<Commodity Type>");
			// 
			// zLabelIssuanceCountry
			// 
			this.BindingSource.SetBindingMember(this.zLabelIssuanceCountry, "CLP_RN_NKIssuanceCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_RN_NKIssuanceCountryCode)));
			this.zLabelIssuanceCountry.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelIssuanceCountry.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 97, true);
			this.zLabelIssuanceCountry.Name = "zLabelIssuanceCountry";
			this.zLabelIssuanceCountry.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 16, true);
			this.zLabelIssuanceCountry.TabIndex = 20;
			this.zLabelIssuanceCountry.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("988a77db-855b-43ff-9ebd-0263632f2401", "<Issuance Country/Region>");
			// 
			// zLabelCountryOfAuth
			// 
			this.BindingSource.SetBindingMember(this.zLabelCountryOfAuth, "CLP_RN_NKOriginCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_RN_NKOriginCountryCode)));
			this.zLabelCountryOfAuth.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelCountryOfAuth.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 113, true);
			this.zLabelCountryOfAuth.Name = "zLabelCountryOfAuth";
			this.zLabelCountryOfAuth.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 16, true);
			this.zLabelCountryOfAuth.TabIndex = 21;
			this.zLabelCountryOfAuth.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e5ea0188-3b6c-4ccd-a30f-f1f7eac31bd1", "<Country/Region of Auth.>");
			// 
			// zLabelMixed
			// 
			this.BindingSource.SetBindingMember(this.zLabelMixed, "CLP_IsMixedCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_IsMixedCountryOfOrigin)));
			this.zLabelMixed.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelMixed.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 129, true);
			this.zLabelMixed.Name = "zLabelMixed";
			this.zLabelMixed.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 16, true);
			this.zLabelMixed.TabIndex = 22;
			this.zLabelMixed.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c47554aa-6fd6-4284-bf1a-79b8b4ebcfc6", "<Mixed>");
			// 
			// zLabelQuantity
			// 
			this.BindingSource.SetBindingMember(this.zLabelQuantity, "CLP_AlternativeQuotaQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_AlternativeQuotaQuantity)));
			this.zLabelQuantity.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelQuantity.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 33, true);
			this.zLabelQuantity.Name = "zLabelQuantity";
			this.zLabelQuantity.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 16, true);
			this.zLabelQuantity.TabIndex = 23;
			this.zLabelQuantity.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("035b2dfb-c840-4e76-9a4f-5d9c6d041023", "<Quantity>");
			// 
			// zLabelUQ
			// 
			this.BindingSource.SetBindingMember(this.zLabelUQ, "CLP_AlternativeQuotaUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_AlternativeQuotaUQ)));
			this.zLabelUQ.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelUQ.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 49, true);
			this.zLabelUQ.Name = "zLabelUQ";
			this.zLabelUQ.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 16, true);
			this.zLabelUQ.TabIndex = 24;
			this.zLabelUQ.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b4270b7f-5de7-4dc7-801c-6b575517579c", "<UQ>");
			// 
			// zLabelStartDate
			// 
			this.BindingSource.SetBindingMember(this.zLabelStartDate, "CLP_StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_StartDate)));
			this.zLabelStartDate.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelStartDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 65, true);
			this.zLabelStartDate.Name = "zLabelStartDate";
			this.zLabelStartDate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 16, true);
			this.zLabelStartDate.TabIndex = 25;
			this.zLabelStartDate.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e748c492-9982-4a7e-a869-6415f511c2a1", "<Start Date>");
			// 
			// zLabelIssueDate
			// 
			this.BindingSource.SetBindingMember(this.zLabelIssueDate, "CLP_IssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_IssueDate)));
			this.zLabelIssueDate.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelIssueDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 81, true);
			this.zLabelIssueDate.Name = "zLabelIssueDate";
			this.zLabelIssueDate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 16, true);
			this.zLabelIssueDate.TabIndex = 26;
			this.zLabelIssueDate.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("76b9cbf9-5e7f-42ab-b9ce-486cd5791421", "<Issue Date>");
			// 
			// zLabelExpiryDate
			// 
			this.BindingSource.SetBindingMember(this.zLabelExpiryDate, "CLP_EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_EndDate)));
			this.zLabelExpiryDate.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelExpiryDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 97, true);
			this.zLabelExpiryDate.Name = "zLabelExpiryDate";
			this.zLabelExpiryDate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 16, true);
			this.zLabelExpiryDate.TabIndex = 27;
			this.zLabelExpiryDate.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ed6a8ad1-90e6-40c7-be7d-fbe43c1cf94e", "<Expiry Date>");
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Controls.Add(this.zLabelApplicantContactEmail);
			this.zGroupBox2.Controls.Add(this.zLabelApplicantContactPhone);
			this.zGroupBox2.Controls.Add(this.zLabelApplicantContactName);
			this.zGroupBox2.Controls.Add(this.zLabelApplicantAddress);
			this.zGroupBox2.Controls.Add(this.zLabelApplicantType);
			this.zGroupBox2.Controls.Add(this.zLabelApplicantOrg);
			this.zGroupBox2.Controls.Add(this.zLabelApplicantName);
			this.zGroupBox2.Controls.Add(this.zLabel28);
			this.zGroupBox2.Controls.Add(this.zLabel29);
			this.zGroupBox2.Controls.Add(this.zLabel30);
			this.zGroupBox2.Controls.Add(this.zLabel31);
			this.zGroupBox2.Controls.Add(this.zLabel32);
			this.zGroupBox2.Controls.Add(this.zLabel33);
			this.zGroupBox2.Controls.Add(this.zLabel34);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(822, 0, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 151, true);
			this.zGroupBox2.TabIndex = 35;
			this.zGroupBox2.TabStop = false;
			this.zGroupBox2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5fbc2db6-2798-44c4-ae48-e36a63724462", "Applicant");
			// 
			// zLabelApplicantContactEmail
			// 
			this.BindingSource.SetBindingMember(this.zLabelApplicantContactEmail, "CLP_ApplicantContactEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_ApplicantContactEmail)));
			this.zLabelApplicantContactEmail.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelApplicantContactEmail.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 132, true);
			this.zLabelApplicantContactEmail.Name = "zLabelApplicantContactEmail";
			this.zLabelApplicantContactEmail.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 16, true);
			this.zLabelApplicantContactEmail.TabIndex = 34;
			this.zLabelApplicantContactEmail.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a827c13b-f6b7-4fdf-9f11-45285cd00476", "<Contact Email>");
			// 
			// zLabelApplicantContactPhone
			// 
			this.BindingSource.SetBindingMember(this.zLabelApplicantContactPhone, "CLP_ApplicantContactPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_ApplicantContactPhone)));
			this.zLabelApplicantContactPhone.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelApplicantContactPhone.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 116, true);
			this.zLabelApplicantContactPhone.Name = "zLabelApplicantContactPhone";
			this.zLabelApplicantContactPhone.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 16, true);
			this.zLabelApplicantContactPhone.TabIndex = 33;
			this.zLabelApplicantContactPhone.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6c6a38b4-d1ca-4fda-9a6d-64f989be16d0", "<Contact Phone>");
			// 
			// zLabelApplicantContactName
			// 
			this.BindingSource.SetBindingMember(this.zLabelApplicantContactName, "CLP_ApplicantContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_ApplicantContactName)));
			this.zLabelApplicantContactName.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelApplicantContactName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 100, true);
			this.zLabelApplicantContactName.Name = "zLabelApplicantContactName";
			this.zLabelApplicantContactName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 16, true);
			this.zLabelApplicantContactName.TabIndex = 32;
			this.zLabelApplicantContactName.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("bc4d3e11-2b72-42d0-aece-327535c71eea", "<Contact Name>");
			// 
			// zLabelApplicantAddress
			// 
			this.BindingSource.SetBindingMember(this.zLabelApplicantAddress, "CLP_OA_LPCOApplicant_Address");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_OA_LPCOApplicant_Address)));
			this.zLabelApplicantAddress.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelApplicantAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 66, true);
			this.zLabelApplicantAddress.Name = "zLabelApplicantAddress";
			this.zLabelApplicantAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 35, true);
			this.zLabelApplicantAddress.TabIndex = 31;
			this.zLabelApplicantAddress.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ecee39ea-c036-4709-a177-a8b6a437f25c", "<Address>");
			this.zLabelApplicantAddress.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// zLabelApplicantType
			// 
			this.BindingSource.SetBindingMember(this.zLabelApplicantType, "CLP_ApplicantType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_ApplicantType)));
			this.zLabelApplicantType.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelApplicantType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 32, true);
			this.zLabelApplicantType.Name = "zLabelApplicantType";
			this.zLabelApplicantType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 16, true);
			this.zLabelApplicantType.TabIndex = 30;
			this.zLabelApplicantType.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7940d7af-066b-4616-932e-eb7ebf54033b", "<Type>");
			// 
			// zLabelApplicantOrg
			// 
			this.BindingSource.SetBindingMember(this.zLabelApplicantOrg, "LPCOApplicantOrgCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).LPCOApplicantOrgCode)));
			this.zLabelApplicantOrg.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelApplicantOrg.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 48, true);
			this.zLabelApplicantOrg.Name = "zLabelApplicantOrg";
			this.zLabelApplicantOrg.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 16, true);
			this.zLabelApplicantOrg.TabIndex = 29;
			this.zLabelApplicantOrg.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9d4521ce-b5d2-4ef5-a7a5-a3f060b0b6a2", "<Organization>");
			// 
			// zLabelApplicantName
			// 
			this.BindingSource.SetBindingMember(this.zLabelApplicantName, "CLP_ApplicantName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_ApplicantName)));
			this.zLabelApplicantName.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelApplicantName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 16, true);
			this.zLabelApplicantName.Name = "zLabelApplicantName";
			this.zLabelApplicantName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 16, true);
			this.zLabelApplicantName.TabIndex = 28;
			this.zLabelApplicantName.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("161d765b-e067-4dc1-a1c0-f1a80ceabb7d", "<Name>");
			// 
			// zLabel28
			// 
			this.zLabel28.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel28.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 132, true);
			this.zLabel28.Name = "zLabel28";
			this.zLabel28.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 16, true);
			this.zLabel28.TabIndex = 9;
			this.zLabel28.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("cfc05ed2-3500-4d9d-baef-2ab14d4dd02f", "Contact Email:");
			this.zLabel28.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel29
			// 
			this.zLabel29.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel29.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 116, true);
			this.zLabel29.Name = "zLabel29";
			this.zLabel29.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 16, true);
			this.zLabel29.TabIndex = 8;
			this.zLabel29.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("fa007aa7-abdd-47b0-9ff4-3e9627a94abb", "Contact Phone:");
			this.zLabel29.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel30
			// 
			this.zLabel30.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel30.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 100, true);
			this.zLabel30.Name = "zLabel30";
			this.zLabel30.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 16, true);
			this.zLabel30.TabIndex = 7;
			this.zLabel30.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0cb53227-106d-47cb-a538-a9d8091ff3a3", "Contact Name:");
			this.zLabel30.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel31
			// 
			this.zLabel31.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel31.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.zLabel31.Name = "zLabel31";
			this.zLabel31.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 16, true);
			this.zLabel31.TabIndex = 6;
			this.zLabel31.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("812a9f21-2ff7-4ae8-99ae-0b6cd4074e10", "Name:");
			this.zLabel31.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel32
			// 
			this.zLabel32.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel32.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 64, true);
			this.zLabel32.Name = "zLabel32";
			this.zLabel32.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 16, true);
			this.zLabel32.TabIndex = 5;
			this.zLabel32.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("01a76c95-0b3f-4bf9-8f92-f4a6a99e591e", "Address:");
			this.zLabel32.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel33
			// 
			this.zLabel33.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel33.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 48, true);
			this.zLabel33.Name = "zLabel33";
			this.zLabel33.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 16, true);
			this.zLabel33.TabIndex = 4;
			this.zLabel33.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("27b5cc7b-7c43-47ff-b8ec-d6f47873a9bb", "Organization:");
			this.zLabel33.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel34
			// 
			this.zLabel34.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel34.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 32, true);
			this.zLabel34.Name = "zLabel34";
			this.zLabel34.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 16, true);
			this.zLabel34.TabIndex = 3;
			this.zLabel34.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e52795ca-8c2d-4428-a7c0-aed7ebbe7533", "Type:");
			this.zLabel34.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zGroupBox3
			// 
			this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 35, true);
			this.zGroupBox3.Name = "zGroupBox3";
			this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(2, 100, true);
			this.zGroupBox3.TabIndex = 36;
			this.zGroupBox3.TabStop = false;
			// 
			// LPCODetailUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.zGroupBox3);
			this.Controls.Add(this.zGroupBox2);
			this.Controls.Add(this.zLabelExpiryDate);
			this.Controls.Add(this.zLabelIssueDate);
			this.Controls.Add(this.zLabelStartDate);
			this.Controls.Add(this.zLabelUQ);
			this.Controls.Add(this.zLabelQuantity);
			this.Controls.Add(this.zLabelMixed);
			this.Controls.Add(this.zLabelCountryOfAuth);
			this.Controls.Add(this.zLabelIssuanceCountry);
			this.Controls.Add(this.zLabelCommodityType);
			this.Controls.Add(this.zLabelDIFURN);
			this.Controls.Add(this.zLabelEndUseDescription);
			this.Controls.Add(this.zLabelRefNo);
			this.Controls.Add(this.zLabelDocumentDescription);
			this.Controls.Add(this.zLabel17);
			this.Controls.Add(this.zLabel16);
			this.Controls.Add(this.zLabel1ExpiryDateCaption);
			this.Controls.Add(this.zLabel14);
			this.Controls.Add(this.zLabel13);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.zLabelMixedCaption);
			this.Controls.Add(this.zLabelCountryOfAuthCaption);
			this.Controls.Add(this.zLabelIssuanceCountryCaption);
			this.Controls.Add(this.zLabel5);
			this.Controls.Add(this.zLabel4);
			this.Controls.Add(this.zLabel3);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.zLabelDocumentType);
			this.Controls.Add(this.zLabel1);
			this.Name = "LPCODetailUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1170, 154, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZLabel zLabelDocumentType;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.ZLabel zLabel3;
		private ZArchitecture.ZLabel zLabel4;
		private ZArchitecture.ZLabel zLabel5;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.ZLabel zLabel12;
		private ZArchitecture.ZLabel zLabel11;
		private ZArchitecture.ZLabel zLabel10;
		private ZArchitecture.ZLabel zLabel9;
		private ZArchitecture.ZLabel zLabel20;
		private ZArchitecture.ZLabel zLabel19;
		private ZArchitecture.ZLabel zLabel18;
		private ZArchitecture.ZLabel zLabel13;
		private ZArchitecture.ZLabel zLabel14;
		private ZArchitecture.ZLabel zLabel16;
		private ZArchitecture.ZLabel zLabel17;
		private ZArchitecture.ZLabel zLabelDocumentDescription;
		private ZArchitecture.ZLabel zLabelRefNo;
		private ZArchitecture.ZLabel zLabelEndUseDescription;
		private ZArchitecture.ZLabel zLabelDIFURN;
		private ZArchitecture.ZLabel zLabelCommodityType;
		private ZArchitecture.ZLabel zLabelQuantity;
		private ZArchitecture.ZLabel zLabelUQ;
		private ZArchitecture.ZLabel zLabelStartDate;
		private ZArchitecture.ZLabel zLabelIssueDate;
		private ZArchitecture.ZLabel zLabelHolderOrg;
		private ZArchitecture.ZLabel zLabelHolderName;
		private ZArchitecture.ZLabel zLabelHolderType;
		private ZArchitecture.ZLabel zLabelAddress;
		private ZArchitecture.ZLabel zLabelHolderContactName;
		private ZArchitecture.ZLabel zLabelHolderContactPhone;
		private ZArchitecture.ZLabel zLabelHolderContactEmail;
		private ZArchitecture.GUI.ZGroupBox zGroupBox2;
		private ZArchitecture.ZLabel zLabelApplicantContactEmail;
		private ZArchitecture.ZLabel zLabelApplicantContactPhone;
		private ZArchitecture.ZLabel zLabelApplicantContactName;
		private ZArchitecture.ZLabel zLabelApplicantAddress;
		private ZArchitecture.ZLabel zLabelApplicantType;
		private ZArchitecture.ZLabel zLabelApplicantOrg;
		private ZArchitecture.ZLabel zLabelApplicantName;
		private ZArchitecture.ZLabel zLabel28;
		private ZArchitecture.ZLabel zLabel29;
		private ZArchitecture.ZLabel zLabel30;
		private ZArchitecture.ZLabel zLabel31;
		private ZArchitecture.ZLabel zLabel32;
		private ZArchitecture.ZLabel zLabel33;
		private ZArchitecture.ZLabel zLabel34;
		internal ZArchitecture.ZLabel zLabelIssuanceCountryCaption;
		internal ZArchitecture.ZLabel zLabelCountryOfAuthCaption;
		internal ZArchitecture.ZLabel zLabelMixedCaption;
		internal ZArchitecture.ZLabel zLabel1ExpiryDateCaption;
		internal ZArchitecture.ZLabel zLabelIssuanceCountry;
		internal ZArchitecture.ZLabel zLabelCountryOfAuth;
		internal ZArchitecture.ZLabel zLabelMixed;
		internal ZArchitecture.ZLabel zLabelExpiryDate;
		private ZArchitecture.GUI.ZGroupBox zGroupBox3;
	}
}
