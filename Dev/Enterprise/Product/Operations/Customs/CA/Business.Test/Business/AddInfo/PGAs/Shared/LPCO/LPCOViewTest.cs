using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(LPCOView))]
	sealed class LPCOViewTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCLP_RN_NKSmeltAndPourCountryCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = "Y";
			var gacPGAHeader = invoiceLine.GACPGAHeader;
			var lpcoview = gacPGAHeader.LPCOViews.AddNew();
			AssertEquals(true, lpcoview.CLP_RN_NKSmeltAndPourCountryCode.IsEmpty);
		}

		public void TestHasPGAHeader()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "2001", "2001 DESC", startDate, endDate);
			var attributeNamePGA = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, "PGA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributePGA = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.GAC);
			var attributeNameValuesAllowed = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypeValuesAllowed, "ValuesAllowed", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributeValuesAllowed01 = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameValuesAllowed.ZXE_Name, "YYY");
			newFactory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Centilitre;
			invoiceLine.CA_GACInd = "Y";

			var gacLpcoview = GetLPCOWithValues(declaration);
			gacLpcoview.CLP_Type = "2001";

			var gacPGAHeader = invoiceLine.GACPGAHeader;
			var lpcoview = gacPGAHeader.LPCOViews.AddNew();
			lpcoview.CLP_Type = "2002";

			var lpcoViewList = gacPGAHeader.LPCOViews.OfType<LPCOView>().ToList();
			AssertEquals(2, lpcoViewList.Count);
			var invLPCO = lpcoViewList.FirstOrDefault(x => x.CLP_Type == "2002");
			Assert(!invLPCO.HasPGAHeader);
			var decLpcoview = lpcoViewList.FirstOrDefault(x => x.CLP_Type == "2001");
			Assert(decLpcoview.HasPGAHeader);
		}

		public void TestClearOtherValuesAndReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Centilitre;
			invoiceLine.CA_GACInd = "Y";

			var gacPGAHeader = invoiceLine.GACPGAHeader;
			var lpcoview = gacPGAHeader.LPCOViews.AddNew();
			lpcoview.CLP_HolderType = LPCOHolderPartyTypeCodes.Codes.Other;
			Assert(!lpcoview.LPCOHolderOrgPKInfo.ReadOnly);
			Assert(!lpcoview.CLP_OA_HolderInfo.ReadOnly);
			Assert(lpcoview.CLP_HolderNameInfo.ReadOnly);

			var lpcoHolder = Factory.New<OrgHeader>();
			lpcoview.LPCOHolderOrgPK = lpcoHolder.PK;
			lpcoview.CLP_OA_Holder = lpcoHolder.MainAddress.PK;
			lpcoview.CLP_IsHolderOverridden = true;
			lpcoview.CLP_HolderName = "Lilly";

			lpcoview.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Other;
			Assert(!lpcoview.LPCOApplicantOrgPKInfo.ReadOnly);
			Assert(!lpcoview.CLP_OA_ApplicantInfo.ReadOnly);
			Assert(lpcoview.CLP_ApplicantNameInfo.ReadOnly);

			var lpcoApplicant = Factory.New<OrgHeader>();
			lpcoview.LPCOApplicantOrgPK = lpcoApplicant.PK;
			lpcoview.CLP_OA_Applicant = lpcoApplicant.MainAddress.PK;

			lpcoview.CLP_HolderType = LPCOHolderPartyTypeCodes.Codes.Importer;
			AssertEquals(ZGuid.Empty, lpcoview.LPCOHolderOrgPK);
			AssertEquals(ZGuid.Empty, lpcoview.CLP_OA_Holder);
			AssertEquals(ZString.Empty, lpcoview.CLP_HolderName);
			Assert(lpcoview.LPCOHolderOrgPKInfo.ReadOnly);
			Assert(lpcoview.CLP_OA_HolderInfo.ReadOnly);
			Assert(!lpcoview.CLP_HolderNameInfo.ReadOnly);

			AssertNotEquals(ZGuid.Empty, lpcoview.LPCOApplicantOrgPK);
			AssertNotEquals(ZGuid.Empty, lpcoview.CLP_OA_Applicant);
			Assert(!lpcoview.LPCOApplicantOrgPKInfo.ReadOnly);
			Assert(!lpcoview.CLP_OA_ApplicantInfo.ReadOnly);

			lpcoview.CLP_IsApplicantOverridden = true;
			lpcoview.CLP_ApplicantName = "Lilly2";
			AssertNotEquals(ZString.Empty, lpcoview.CLP_ApplicantName);
			Assert(!lpcoview.CLP_ApplicantNameInfo.ReadOnly);

			lpcoview.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Importer;
			AssertEquals(ZGuid.Empty, lpcoview.LPCOApplicantOrgPK);
			AssertEquals(ZGuid.Empty, lpcoview.CLP_OA_Applicant);
			Assert(lpcoview.LPCOApplicantOrgPKInfo.ReadOnly);
			Assert(lpcoview.CLP_OA_ApplicantInfo.ReadOnly);

			lpcoview.CLP_IsApplicantOverridden = true;
			lpcoview.CLP_ApplicantName = "Lilly2";
			AssertEquals("Lilly2", lpcoview.CLP_ApplicantName);
			Assert(!lpcoview.CLP_ApplicantNameInfo.ReadOnly);
		}

		public void TestReadOnlyAndClearValueFromReadOnlyFields()
		{
			RegistrationNumberHelperTest.PrepareGlobalData(Factory);

			var gacHeader = Factory.New<GACPGAHeader>();
			var gaclpco = gacHeader.LPCOViews.AddNew();

			Assert("Is_SecondaryRefNo_ReadOnly", gACLPCOView.CLP_SecondaryRefNoInfo.ReadOnly);
			Assert("Is_LPCOIssueDate_ReadOnly", gACLPCOView.CLP_IssueDateInfo.ReadOnly);

			gACLPCOView.CLP_Type = CusCALPCO.ForeignExportLicenseCode;

			Assert("Is_SecondaryRefNo_ReadOnly", !gACLPCOView.CLP_SecondaryRefNoInfo.ReadOnly);
			Assert("Is_LPCOIssueDate_ReadOnly", !gACLPCOView.CLP_IssueDateInfo.ReadOnly);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.CA_ServiceOption = "IID";

			var gacLpcoview = GetLPCOWithValues(declaration);
			var cfialpcoview = GetLPCOWithValues(declaration);
			var tclpcoview = GetLPCOWithValues(declaration);
			var nrcanlpcoview = GetLPCOWithValues(declaration);

			gacLpcoview.CLP_Type = "2001";
			cfialpcoview.CLP_Type = "C01";
			tclpcoview.CLP_Type = "4001";
			nrcanlpcoview.CLP_Type = "3001";

			Assert("Contains CLP_RefNo", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_RefNo));
			Assert("CLP_RefNo not Readonly", !gacLpcoview.CLP_RefNoInfo.ReadOnly);
			Assert(!gacLpcoview.CLP_RefNo.IsEmpty);

			Assert("Contains CLP_AlternativeQuotaQuantity", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_AlternativeQuotaQuantity));
			Assert("CLP_AlternativeQuotaQuantity not Readonly", !gacLpcoview.CLP_AlternativeQuotaQuantityInfo.ReadOnly);
			Assert(!gacLpcoview.CLP_AlternativeQuotaQuantity.IsEmpty);
			Assert("Contains no CLP_AlternativeQuotaQuantity", !CFIAPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_AlternativeQuotaQuantity));
			Assert("CLP_AlternativeQuotaQuantity Readonly", cfialpcoview.CLP_AlternativeQuotaQuantityInfo.ReadOnly);
			Assert(cfialpcoview.CLP_AlternativeQuotaQuantity.IsEmpty);

			Assert("Contains CLP_HolderType", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_HolderType));
			Assert("CLP_HolderType not Readonly", !gacLpcoview.CLP_HolderTypeInfo.ReadOnly);
			Assert(!gacLpcoview.CLP_HolderType.IsEmpty);
			Assert("Contains no CLP_HolderType", !CFIAPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_HolderType));
			Assert("CLP_HolderType Readonly", cfialpcoview.CLP_HolderTypeInfo.ReadOnly);
			Assert(cfialpcoview.CLP_HolderType.IsEmpty);

			Assert("Contains CLP_ApplicantType", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_ApplicantType));
			Assert("CLP_ApplicantType not Readonly", !gacLpcoview.CLP_ApplicantTypeInfo.ReadOnly);
			Assert(!gacLpcoview.CLP_ApplicantType.IsEmpty);
			Assert("Contains no CLP_ApplicantType", !CFIAPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_ApplicantType));
			Assert("CLP_ApplicantType Readonly", cfialpcoview.CLP_ApplicantTypeInfo.ReadOnly);
			Assert(cfialpcoview.CLP_ApplicantType.IsEmpty);

			Assert("Contains CLP_SecondaryRefNo", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_SecondaryRefNo));
			Assert("CLP_SecondaryRefNo not Readonly", !gacLpcoview.CLP_SecondaryRefNoInfo.ReadOnly);
			Assert(!gacLpcoview.CLP_SecondaryRefNo.IsEmpty);
			Assert("Contains no CLP_SecondaryRefNo", !CFIAPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_SecondaryRefNo));
			Assert("CLP_SecondaryRefNo Readonly", cfialpcoview.CLP_SecondaryRefNoInfo.ReadOnly);
			Assert(cfialpcoview.CLP_SecondaryRefNo.IsEmpty);

			Assert("Contains CLP_LPCOIssueDate", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_IssueDate));
			Assert("CLP_LPCOIssueDate not Readonly", !gacLpcoview.CLP_IssueDateInfo.ReadOnly);
			Assert(!gacLpcoview.CLP_IssueDate.IsEmpty);
			Assert("Contains no CLP_LPCOIssueDate", !CFIAPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_IssueDate));
			Assert("CLP_LPCOIssueDate Readonly", cfialpcoview.CLP_IssueDateInfo.ReadOnly);
			Assert(cfialpcoview.CLP_IssueDate.IsEmpty);

			Assert("Contains no CLP_AuthorizationCountry", !GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_RN_NKAuthorizationCountry));
			Assert("CLP_AuthorizationCountry Readonly", gacLpcoview.CLP_RN_NKAuthorizationCountryInfo.ReadOnly);
			Assert(gacLpcoview.CLP_RN_NKAuthorizationCountry.IsEmpty);
			Assert("Contains CLP_AuthorizationCountry", TCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_RN_NKAuthorizationCountry));
			Assert("CLP_AuthorizationCountry not Readonly", !tclpcoview.CLP_RN_NKAuthorizationCountryInfo.ReadOnly);
			Assert(!tclpcoview.CLP_RN_NKAuthorizationCountry.IsEmpty);

			Assert("Contains CLP_CommodityTypeCode", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_CommodityTypeCode));
			Assert("CLP_CommodityTypeCode not Readonly", !gacLpcoview.CLP_CommodityTypeCodeInfo.ReadOnly);
			Assert(!gacLpcoview.CLP_CommodityTypeCode.IsEmpty);
			Assert("Contains no CLP_CommodityTypeCode", !TCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_CommodityTypeCode));
			Assert("CLP_CommodityTypeCode Readonly", tclpcoview.CLP_CommodityTypeCodeInfo.ReadOnly);
			Assert(tclpcoview.CLP_CommodityTypeCode.IsEmpty);

			Assert("Contains LPCOHolderEmail", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_HolderContactEmail));
			Assert("LPCOHolderEmail not Readonly", !gacLpcoview.CLP_HolderContactEmailInfo.ReadOnly);
			Assert(!gacLpcoview.CLP_HolderContactEmail.IsEmpty);
			Assert("Contains no LPCOHolderEmail", !TCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_HolderContactEmail));
			Assert("LPCOHolderEmail Readonly", tclpcoview.CLP_HolderContactEmailInfo.ReadOnly);
			Assert(tclpcoview.CLP_HolderContactEmail.IsEmpty);

			Assert("Contains LPCOHolderContactName", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_HolderContactName));
			Assert("LPCOHolderContactName not Readonly", !gacLpcoview.CLP_HolderContactNameInfo.ReadOnly);
			Assert(!gacLpcoview.CLP_HolderContactName.IsEmpty);
			Assert("Contains no LPCOHolderContactName", !TCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_HolderContactName));
			Assert("LPCOHolderContactName Readonly", tclpcoview.CLP_HolderContactNameInfo.ReadOnly);
			Assert(tclpcoview.CLP_HolderContactName.IsEmpty);

			Assert("Contains LPCOHolderPhone", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_HolderContactPhone));
			Assert("CLP_HolderContactPhone not Readonly", !gacLpcoview.CLP_HolderContactPhoneInfo.ReadOnly);
			Assert(!gacLpcoview.CLP_HolderContactPhone.IsEmpty);
			Assert("Contains no LPCOHolderPhone", !TCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_HolderContactPhone));
			Assert("CLP_HolderContactPhone Readonly", tclpcoview.CLP_HolderContactPhoneInfo.ReadOnly);
			Assert(tclpcoview.CLP_HolderContactPhone.IsEmpty);

			Assert("Contains LPCOApplicantEmail", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_ApplicantContactEmail));
			Assert("CLP_ApplicantContactEmail not Readonly", !gacLpcoview.CLP_ApplicantContactEmailInfo.ReadOnly);
			Assert(!gacLpcoview.CLP_ApplicantContactEmail.IsEmpty);
			Assert("Contains no LPCOApplicantEmail", !TCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_ApplicantContactEmail));
			Assert("LPCOApplicantEmail Readonly", tclpcoview.CLP_ApplicantContactEmailInfo.ReadOnly);
			Assert(tclpcoview.CLP_ApplicantContactEmail.IsEmpty);

			Assert("Contains LPCOApplicantContactName", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_ApplicantContactName));
			Assert("CLP_ApplicantContactName not Readonly", !gacLpcoview.CLP_ApplicantContactNameInfo.ReadOnly);
			Assert(!gacLpcoview.CLP_ApplicantContactName.IsEmpty);
			Assert("Contains no LPCOApplicantContactName", !TCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_ApplicantContactName));
			Assert("LPCOApplicantContactName Readonly", tclpcoview.CLP_ApplicantContactNameInfo.ReadOnly);
			Assert(tclpcoview.CLP_ApplicantContactName.IsEmpty);

			Assert("Contains LPCOApplicantPhone", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_ApplicantContactPhone));
			Assert("CLP_ApplicantContactPhone not Readonly", !gacLpcoview.CLP_ApplicantContactPhoneInfo.ReadOnly);
			Assert(!gacLpcoview.CLP_ApplicantContactPhone.IsEmpty);
			Assert("Contains no LPCOApplicantPhone", !TCPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_ApplicantContactPhone));
			Assert("CLP_ApplicantContactPhone Readonly", tclpcoview.CLP_ApplicantContactPhoneInfo.ReadOnly);
			Assert(tclpcoview.CLP_ApplicantContactPhone.IsEmpty);

			Assert("Contains no CLP_CountryOfIssuance", !GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_RN_NKIssuanceCountryCode));
			Assert("CLP_CountryOfIssuance Readonly", gacLpcoview.CLP_RN_NKIssuanceCountryCodeInfo.ReadOnly);
			Assert(gacLpcoview.CLP_RN_NKIssuanceCountryCode.IsEmpty);
			Assert("Contains CLP_CountryOfIssuance", NRCanPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_RN_NKIssuanceCountryCode));
			Assert("CLP_CountryOfIssuance not Readonly", !nrcanlpcoview.CLP_RN_NKIssuanceCountryCodeInfo.ReadOnly);
			Assert(!nrcanlpcoview.CLP_RN_NKIssuanceCountryCode.IsEmpty);

			Assert("Contains no CLP_CountryOfOrigin", !GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_RN_NKOriginCountryCode));
			Assert("CLP_CountryOfOrigin Readonly", gacLpcoview.CLP_RN_NKOriginCountryCodeInfo.ReadOnly);
			Assert(gacLpcoview.CLP_RN_NKOriginCountryCode.IsEmpty);
			Assert("Contains CLP_CountryOfOrigin", NRCanPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_RN_NKOriginCountryCode));
			Assert("CLP_CountryOfOrigin not Readonly", !nrcanlpcoview.CLP_RN_NKOriginCountryCodeInfo.ReadOnly);
			Assert(!nrcanlpcoview.CLP_RN_NKOriginCountryCode.IsEmpty);

			Assert("Contains CLP_DIFRefNumberOrLocation", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation));
			Assert("CLP_DIFRefNumberOrLocation not Readonly", !gacLpcoview.CLP_DIFRefNumberOrLocationInfo.ReadOnly);
			Assert(!gacLpcoview.CLP_DIFRefNumberOrLocation.IsEmpty);

			Assert("Contains no CLP_IsMixedCountryOfOrigin", !GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_IsMixedCountryOfOrigin));
			Assert("CLP_IsMixedCountryOfOrigin Readonly", gacLpcoview.CLP_IsMixedCountryOfOriginInfo.ReadOnly);
			Assert("Contains CLP_IsMixedCountryOfOrigin", NRCanPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_IsMixedCountryOfOrigin));
			Assert("CLP_IsMixedCountryOfOrigin not Readonly", !nrcanlpcoview.CLP_IsMixedCountryOfOriginInfo.ReadOnly);

			Assert("Contains no CLP_EndDate", !GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_EndDate));
			Assert("CLP_EndDate Readonly", gacLpcoview.CLP_EndDateInfo.ReadOnly);
			Assert(!gacLpcoview.CLP_IsMixedCountryOfOrigin);
			Assert("Contains CLP_EndDate", NRCanPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_EndDate));
			Assert("CLP_EndDate not Readonly", !nrcanlpcoview.CLP_EndDateInfo.ReadOnly);
			Assert(nrcanlpcoview.CLP_IsMixedCountryOfOrigin);

			Assert("Contains CLP_StartDate", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_StartDate));
			Assert("CLP_StartDate not Readonly", !gacLpcoview.CLP_StartDateInfo.ReadOnly);
			Assert(!gacLpcoview.CLP_StartDate.IsEmpty);
			Assert("Contains no CLP_StartDate", !CFIAPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_StartDate));
			Assert("CLP_StartDate Readonly", cfialpcoview.CLP_StartDateInfo.ReadOnly);
			Assert(cfialpcoview.CLP_StartDate.IsEmpty);

			Assert("Contains CLP_AlternativeQuotaUQ", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_AlternativeQuotaUQ));
			Assert("CLP_AlternativeQuotaUQ not Readonly", !gacLpcoview.CLP_AlternativeQuotaUQInfo.ReadOnly);
			Assert(!gacLpcoview.CLP_AlternativeQuotaUQ.IsEmpty);
			Assert("Contains no CLP_AlternativeQuotaUQ", !CFIAPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_AlternativeQuotaUQ));
			Assert("CLP_AlternativeQuotaUQ Readonly", cfialpcoview.CLP_AlternativeQuotaUQInfo.ReadOnly);
			Assert(cfialpcoview.CLP_AlternativeQuotaUQ.IsEmpty);
		}

		LPCOView GetLPCOWithValues(JobDeclaration dec)
		{
			var lpcoview = dec.LPCOViews.AddNew();
			lpcoview.CLP_AlternativeQuotaUQ = "KG";
			lpcoview.CLP_SecondaryRefNo = "SNO";
			lpcoview.CLP_RefNo = "00001";
			lpcoview.CLP_AlternativeQuotaQuantity = 1.5m;
			lpcoview.CLP_StartDate = new ZDate(2018, 3, 29);
			lpcoview.CLP_IssueDate = new ZDate(2018, 3, 29);
			lpcoview.CLP_EndDate = new ZDate(2018, 3, 29);
			lpcoview.CLP_IsMixedCountryOfOrigin = true;
			lpcoview.CLP_DIFRefNumberOrLocation = "DIF00001";
			lpcoview.CLP_RN_NKOriginCountryCode = "CA";
			lpcoview.CLP_RN_NKIssuanceCountryCode = "US";
			lpcoview.CLP_HolderType = "IMP";
			lpcoview.CLP_IsHolderOverridden = true;
			lpcoview.CLP_HolderContactPhone = "000001";
			lpcoview.CLP_HolderContactName = "TST";
			lpcoview.CLP_HolderContactEmail = "123@test.com";
			lpcoview.CLP_CommodityTypeCode = "XX";
			lpcoview.CLP_RN_NKAuthorizationCountry = "CN";
			lpcoview.CLP_ApplicantType = "IMP";
			lpcoview.CLP_IsApplicantOverridden = true;
			lpcoview.CLP_ApplicantContactPhone = "000002";
			lpcoview.CLP_ApplicantContactName = "APP";
			lpcoview.CLP_ApplicantContactEmail = "abc@test.com";
			return lpcoview;
		}

		public void TestRunPreSaveValidationCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_HCInd = YesNoList.Codes.Yes;

			var hc = invoiceLine.HCPGAHeader;
			hc.CA_APIProgramInd = YesNoList.Codes.Yes;
			var lpcoView = hc.LPCOViews.AddNew();
			lpcoView.CLP_Type = "5001";
			lpcoView.RunPreSaveValidation();
			AssertHasMessageErrorContaining(lpcoView.CLP_RefNoInfo, MandatoryValidation.YouHaveNotEntered);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var invoice2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.CA_HCInd = YesNoList.Codes.Yes;

			var hc2 = invoiceLine2.HCPGAHeader;
			hc2.CA_APIProgramInd = YesNoList.Codes.Yes;
			var lpcoView2 = hc2.LPCOViews.AddNew();
			lpcoView2.CLP_Type = "5001";
			lpcoView2.RunPreSaveValidation();
			AssertNoMessageErrorContaining(lpcoView2.CLP_RefNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;

			var gac = invoiceLine.GACPGAHeader;
			gac.CA_AllProgramInd = YesNoList.Codes.Yes;
			var lpcoView = gac.LPCOViews.AddNew();
			lpcoView.CLP_Type = "5001";
			lpcoView.CLP_HolderType = LPCOHolderPartyTypeCodes.Codes.Other;
			lpcoView.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Other;
			lpcoView.CLP_IsHolderOverridden = true;
			lpcoView.CLP_IsApplicantOverridden = true;
			return new LPCOView(lpcoView.LPCO);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			this.gACCHeader = invoiceLine.GACPGAHeader;
			gACCHeader.CA_AllProgramInd = YesNoList.Codes.Yes;
			this.gACLPCOView = gACCHeader.LPCOViews.AddNew();
		}

		LPCOView gACLPCOView;
		GACPGAHeader gACCHeader;

		#endregion
	}
}
