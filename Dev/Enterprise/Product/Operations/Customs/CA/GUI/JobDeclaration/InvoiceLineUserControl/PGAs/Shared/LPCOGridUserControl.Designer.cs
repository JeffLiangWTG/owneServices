using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CA.GUI
{
	partial class LPCOGridUserControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.LpcoGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AddOrEditDIFButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LpcoGrid)).BeginInit();
			this.LpcoGrid.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.LPCOViewCollection);
			// 
			// LpcoGrid
			// 
			this.LpcoGrid.AllowNavigation = false;
			this.LpcoGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LpcoGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.LPCOView)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.LPCOView)(null)).LPCO.Lookups.DocumentTypeCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).LPCO.TypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_RefNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.LPCOView)(null)).LPCO.Lookups.RefNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_SecondaryRefNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_DIFRefNumberOrLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.LPCOView)(null)).LPCO.Lookups.DIFDocumentIDList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_CommodityTypeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_RN_NKIssuanceCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.LPCOView)(null)).LPCO.Lookups.IssuanceCountryCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_RN_NKOriginCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.LPCOView)(null)).LPCO.Lookups.OriginCountryCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_IsMixedCountryOfOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_IssueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_EndDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_AlternativeQuotaQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_AlternativeQuotaUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.LPCOView)(null)).LPCO.Lookups.UQList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_RN_NKAuthorizationCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.LPCOView)(null)).LPCO.Lookups.AuthorizationCountries)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_HolderType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.LPCOView)(null)).LPCO.Lookups.HolderPartyTypeCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.LPCOView)(null)).LPCOHolderOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.LPCOView)(null)).LPCO.Lookups.LPCOHolders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_HolderName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_OA_Holder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.LPCOView)(null)).LPCO.CLP_OA_Holder_ZAddress.OrgAddress_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_IsHolderOverridden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_HolderContactName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_HolderContactPhone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_HolderContactEmail)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_ApplicantType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.LPCOView)(null)).LPCO.Lookups.LPCOApplicantCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.LPCOView)(null)).LPCOApplicantOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.LPCOView)(null)).LPCO.Lookups.LPCOApplicants)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_ApplicantName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_OA_Applicant)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.LPCOView)(null)).LPCO.CLP_OA_Applicant_ZAddress.OrgAddress_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_IsApplicantOverridden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_ApplicantContactName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_ApplicantContactPhone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LPCOView)(null)).CLP_ApplicantContactEmail)));
			this.LpcoGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo5.BindToList = "LPCO+Lookups.DocumentTypeCodes";
			zCodeFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a71c6ee5-6b77-4682-be9a-7548135814fd", "Document Type");
			zCodeFindBoxColumnStyleInfo5.ColumnName = "CLP_Type";
			zCodeFindBoxColumnStyleInfo5.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("253bf541-ca47-497b-afb6-283b434e64ac", "Document Type");
			zCodeFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("baf27284-21a5-4888-9c50-ac9641d7b613", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "LPCO+TypeDescription";
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("253bf541-ca47-497b-afb6-283b434e64ac", "Document Type");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.BindToList = "LPCO+Lookups.RefNumbers";
			zDropEditColumnStyleInfo4.ColumnName = "CLP_RefNo";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("42A871A1-45F9-473F-A590-696643C1CE15", "End Use Description");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "CLP_SecondaryRefNo";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCodeFindBoxColumnStyleInfo1.BindToList = "LPCO+Lookups.DIFDocumentIDList";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CF96FE02-B8DB-4F7D-92B1-A730CA2F80FF", "DIF URN");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CLP_DIFRefNumberOrLocation";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("4997b5fc-2e96-4988-85b7-e036f33bdda5", "Commodity Type");
			zTextBoxColumnStyleInfo4.ColumnName = "CLP_CommodityTypeCode";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo2.BindToList = "LPCO+Lookups.IssuanceCountryCodes";
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("354CFB70-B94C-466B-9737-B1EA470F4D5D", "Issuance Country/region");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "CLP_RN_NKIssuanceCountryCode";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(123);
			zCodeFindBoxColumnStyleInfo3.BindToList = "LPCO+Lookups.OriginCountryCodes";
			zCodeFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("52D515EA-8DD5-43C4-BEEB-15F3E728909F", "Country/Region of Authentication");
			zCodeFindBoxColumnStyleInfo3.ColumnName = "CLP_RN_NKOriginCountryCode";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("A20FDF17-D587-45CC-8D7E-345DAF4C62D2", "Mixed?");
			zCheckBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCheckBoxColumnStyleInfo1.ColumnName = "CLP_IsMixedCountryOfOrigin";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("EC0CCEEC-9439-47D8-889F-526C0EB8D332", "Start Date");
			zDateEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo1.ColumnName = "CLP_StartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ab7c0449-968f-45a0-87dc-b35cf3534a01", "Issue Date");
			zDateEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo2.ColumnName = "CLP_IssueDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BFA7C894-7067-4DC9-806A-0F22043CD09C", "Expiry Date");
			zDateEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo3.ColumnName = "CLP_EndDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("B874AC60-FFAC-4812-BF3A-1395538A4162", "Quantity");
			zCalcEditColumnStyleInfo1.ColumnName = "CLP_AlternativeQuotaQuantity";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.BindToList = "LPCO+Lookups.UQList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b76a3214-e1e6-41b4-9449-a103a04d84d8", "UQ");
			zDropEditColumnStyleInfo1.ColumnName = "CLP_AlternativeQuotaUQ";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCodeFindBoxColumnStyleInfo4.BindToList = "LPCO+Lookups.AuthorizationCountries";
			zCodeFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8db3a485-fdcc-47b8-ae86-b7811583510b", "Authorization Country/region");
			zCodeFindBoxColumnStyleInfo4.ColumnName = "CLP_RN_NKAuthorizationCountry";
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			zDropEditColumnStyleInfo2.BindToList = "LPCO+Lookups.HolderPartyTypeCodes";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("DA4611CF-F8B2-4243-AEDC-7F17295F941B", "Authorized Party Type");
			zDropEditColumnStyleInfo2.ColumnName = "CLP_HolderType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidFindBoxColumnStyleInfo1.BindToList = "LPCO+Lookups.LPCOHolders";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7f71010a-39e4-4aa0-9a44-b64b10f64178", "Authorized Party Org.");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "LPCOHolderOrgPK";
			zGuidFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LPCOGridUserControl|67fa0903-6f49-4339-9c2f-47f94560ece7", "Authorized Party");
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e8122438-2634-4eb3-ad44-1ed70c38a558", "Authorized Party Name");
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo5.ColumnName = "CLP_HolderName";
			zTextBoxColumnStyleInfo5.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LPCOGridUserControl|67fa0903-6f49-4339-9c2f-47f94560ece7", "Authorized Party");
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zGuidDropEditColumnStyleInfo1.BindToList = "LPCO+CLP_OA_Holder_ZAddress.OrgAddress_List";
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ed4a8ae7-62d5-4f60-9a31-5e0bacc1afdc", "Authorized Party Address");
			zGuidDropEditColumnStyleInfo1.ColumnName = "CLP_OA_Holder";
			zGuidDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LPCOGridUserControl|67fa0903-6f49-4339-9c2f-47f94560ece7", "Authorized Party");
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8f1447d6-2baa-4993-ad77-3148f9d3aa18", "Authorized Party Override");
			zCheckBoxColumnStyleInfo2.ColumnName = "CLP_IsHolderOverridden";
			zCheckBoxColumnStyleInfo2.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LPCOGridUserControl|67fa0903-6f49-4339-9c2f-47f94560ece7", "Authorized Party");
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1521cb9f-0a99-4e88-8c46-6dd6e30a2cc4", "Authorized Party Contact Name");
			zTextBoxColumnStyleInfo6.ColumnName = "CLP_HolderContactName";
			zTextBoxColumnStyleInfo6.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LPCOGridUserControl|67fa0903-6f49-4339-9c2f-47f94560ece7", "Authorized Party");
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f83b3bb4-3344-4395-9c23-70146fe7b1b2", "Authorized Party Contact Phone");
			zTextBoxColumnStyleInfo7.ColumnName = "CLP_HolderContactPhone";
			zTextBoxColumnStyleInfo7.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LPCOGridUserControl|67fa0903-6f49-4339-9c2f-47f94560ece7", "Authorized Party");
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("73d3c642-f2df-40c2-9aa1-10d1dbb00c3d", "Authorized Party Contact Email");
			zTextBoxColumnStyleInfo8.ColumnName = "CLP_HolderContactEmail";
			zTextBoxColumnStyleInfo8.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LPCOGridUserControl|67fa0903-6f49-4339-9c2f-47f94560ece7", "Authorized Party");
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo3.BindToList = "LPCO+Lookups.LPCOApplicantCodes";
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a5c805f0-f598-45aa-ac50-e21bebca1b8f", "Applicant Type");
			zDropEditColumnStyleInfo3.ColumnName = "CLP_ApplicantType";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.BindToList = "LPCO+Lookups.LPCOApplicants";
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9f913cd3-a15a-4c54-8cd2-6a73c3e8a6c7", "Applicant Org.");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "LPCOApplicantOrgPK";
			zGuidFindBoxColumnStyleInfo2.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LPCOGridUserControl|f483c685-e57f-4bf1-a6bb-cfe32f228ac", "Applicant");
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f849d9ba-a7ee-41ad-bbd2-122ab6f32efb", "Applicant Name");
			zTextBoxColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo9.ColumnName = "CLP_ApplicantName";
			zTextBoxColumnStyleInfo9.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LPCOGridUserControl|f483c685-e57f-4bf1-a6bb-cfe32f228ac", "Applicant");
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidDropEditColumnStyleInfo2.BindToList = "LPCO+CLP_OA_Applicant_ZAddress.OrgAddress_List";
			zGuidDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("314e7e0c-436c-4024-b431-93cb3303abd7", "Applicant Address");
			zGuidDropEditColumnStyleInfo2.ColumnName = "CLP_OA_Applicant";
			zGuidDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LPCOGridUserControl|f483c685-e57f-4bf1-a6bb-cfe32f228ac", "Applicant");
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("dccef5b1-729d-4d67-a48e-6e64307b9841", "Applicant Override");
			zCheckBoxColumnStyleInfo3.ColumnName = "CLP_IsApplicantOverridden";
			zCheckBoxColumnStyleInfo3.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LPCOGridUserControl|f483c685-e57f-4bf1-a6bb-cfe32f228ac", "Applicant");
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("335a56ec-d81a-4cb6-b5bc-a7f3d26dd146", "Applicant Contact Name");
			zTextBoxColumnStyleInfo10.ColumnName = "CLP_ApplicantContactName";
			zTextBoxColumnStyleInfo10.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LPCOGridUserControl|f483c685-e57f-4bf1-a6bb-cfe32f228ac", "Applicant");
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("96e71a20-b3cc-48b2-af52-93a807e679c2", "Applicant Contact Phone");
			zTextBoxColumnStyleInfo11.ColumnName = "CLP_ApplicantContactPhone";
			zTextBoxColumnStyleInfo11.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LPCOGridUserControl|f483c685-e57f-4bf1-a6bb-cfe32f228ac", "Applicant");
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d1b7a4ec-1ad0-4932-873a-d8482c639fdb", "Applicant Contact Email");
			zTextBoxColumnStyleInfo12.ColumnName = "CLP_ApplicantContactEmail";
			zTextBoxColumnStyleInfo12.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LPCOGridUserControl|f483c685-e57f-4bf1-a6bb-cfe32f228ac", "Applicant");
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCodeFindBoxColumnStyleInfo6.BindToList = "LPCO+Lookups.SmeltAndPourCountryCodes";
			zCodeFindBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c20ca70a-7d19-4952-a5b9-4aaf586f68ab", "CMP", "Ctry. of Melt & Pour", "Country of Melt and Pour");
			zCodeFindBoxColumnStyleInfo6.ColumnName = "CLP_RN_NKSmeltAndPourCountryCode";
			zCodeFindBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			this.LpcoGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo5);
			this.LpcoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LpcoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.LpcoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LpcoGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.LpcoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.LpcoGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.LpcoGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.LpcoGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.LpcoGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.LpcoGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.LpcoGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.LpcoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LpcoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LpcoGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.LpcoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.LpcoGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.LpcoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.LpcoGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.LpcoGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.LpcoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.LpcoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.LpcoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.LpcoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.LpcoGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.LpcoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.LpcoGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.LpcoGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.LpcoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.LpcoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.LpcoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.LpcoGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo6);
			this.LpcoGrid.GridId = "ED8CCAEF-2A00-46D8-8EF9-17E6B420E9F3";
			this.LpcoGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LpcoGrid.LayoutKey = "LpcoGrid";
			this.LpcoGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LpcoGrid.Name = "LpcoGrid";
			this.LpcoGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1268, 489, true);
			this.LpcoGrid.TabIndex = 0;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.AddOrEditDIFButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 492, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1268, 27, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// AddOrEditDIFButton
			// 
			this.AddOrEditDIFButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("057465aa-badc-4622-91cc-59f5cd109c7c", "Add/Edit DIF");
			this.AddOrEditDIFButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AddOrEditDIFButton.Name = "AddOrEditDIFButton";
			this.AddOrEditDIFButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.AddOrEditDIFButton.TabIndex = 5;
			this.AddOrEditDIFButton.ToolTipCaption = null;
			this.AddOrEditDIFButton.Click += new System.EventHandler(this.AddOrEditDIFButton_Click);
			// 
			// LPCOGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.LpcoGrid);
			this.Name = "LPCOGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1268, 519, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LpcoGrid)).EndInit();
			this.LpcoGrid.ResumeLayout(false);
			this.LpcoGrid.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.ZGrid LpcoGrid;
		internal ZArchitecture.GUI.ZPanel BottomPanel;
		private ZArchitecture.GUI.ZButton AddOrEditDIFButton;
	}
}
