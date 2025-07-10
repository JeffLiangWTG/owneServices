using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(GACPGAHeader))]
	sealed class GACPGAHeaderTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<GACPGAHeader>
	{
		public void TestLPCODefaulter()
		{
			var header = Factory.New<GACPGAHeader>();
			var lpcoDefaulter = header as ILPCODefaulter;
			AssertNotNull(lpcoDefaulter);
			Assert(lpcoDefaulter.ShouldDefaultLPCOFields);
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_Type));
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_RefNo));
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_StartDate));
			Assert(lpcoDefaulter.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_HolderType));
		}

		public void TestAvailableLPCOFields()
		{
			AssertEquals(26, GACPGAHeader.AvailableLPCOFields.Count);
			Assert("CLP_CommodityTypeCode", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_CommodityTypeCode));
			Assert("CLP_HolderContactEmail", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_HolderContactEmail));
			Assert("CLP_HolderContactName", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_HolderContactName));
			Assert("CLP_HolderContactPhone", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_HolderContactPhone));
			Assert("CLP_IsHolderOverridden", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_IsHolderOverridden));
			Assert("CLP_DIFRefNumberOrLocation", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation));
			Assert("CLP_ApplicantType", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_ApplicantType));
			Assert("LPCOApplicantOrgPK", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.LPCOApplicantOrgPK));
			Assert("LPCOApplicantAddressPK", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_OA_Applicant));
			Assert("CLP_ApplicantName", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_ApplicantName));
			Assert("CLP_HolderType", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_HolderType));
			Assert("LPCOHolderOrgPK", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.LPCOHolderOrgPK));
			Assert("LPCOHolderAddressPK", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_OA_Holder));
			Assert("LPCOHolderCompanyName", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_HolderName));
			Assert("CLP_StartDate", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_StartDate));
			Assert("CLP_IssueDate", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_IssueDate));
			Assert("LPCOApplicantEmail", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_ApplicantContactEmail));
			Assert("LPCOApplicantContactName", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_ApplicantContactName));
			Assert("LPCOApplicantPhone", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_ApplicantContactPhone));
			Assert("CLP_IsApplicantOverridden", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_IsApplicantOverridden));
			Assert("CLP_AlternativeQuotaQuantity", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_AlternativeQuotaQuantity));
			Assert("CLP_RefNo", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_RefNo));
			Assert("CLP_Type", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_Type));
			Assert("CLP_SecondaryRefNo", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_SecondaryRefNo));
			Assert("CLP_AlternativeQuotaUQ", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_AlternativeQuotaUQ));
			Assert("CLP_RN_NKSmeltAndPourCountryCode", GACPGAHeader.AvailableLPCOFields.Contains(CusCALPCO.Schema.CLP_RN_NKSmeltAndPourCountryCode));
		}

		#region Purge Values

		public void TestPurgeValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_PermitApplication = false;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			invoiceLine.JI_Tariff = string.Empty;

			var pgaHeader = invoiceLine.GACPGAHeader;

			PopulateValuesAndAssertPurgeResult(pgaHeader, "48000012");
			PopulateValuesAndAssertPurgeResult(pgaHeader, "50000012");
		}

		public void TestPurgeValuesForCusClassification()
		{
			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = ClassificationType.IMP;
			classification.CCA_GACIndicator = YesNoList.Codes.Yes;
			classification.CC_TariffNum = string.Empty;

			var pgaHeader = invoiceLine.GACPGAHeader;

			PopulateValuesAndAssertPurgeResult(pgaHeader, "48000012");
			PopulateValuesAndAssertPurgeResult(pgaHeader, "50000012");
		}

		void PopulateValuesAndAssertPurgeResult(GACPGAHeader pgaHeader, string tariff)
		{
			pgaHeader.CA_CommodityCode = "TEST CODE";
			pgaHeader.CA_ComplianceStatement = true;
			pgaHeader.CA_FTACode = FTAProcessingCodes.Codes.FA01;
			pgaHeader.CA_FibreCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			pgaHeader.CA_FabricCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			pgaHeader.CA_YarnCountryOfOrigin = Core.Constants.CountryCodes.Afghanistan;

			pgaHeader.InvoiceLine.JI_Tariff = tariff;

			CombineAssertions(() =>
			{
				AssertEquals("Should not purge CA_CommodityCode", "TEST CODE", pgaHeader.CA_CommodityCode);
				AssertEquals("Should not purge CA_ComplianceStatement", true, pgaHeader.CA_ComplianceStatement);

				var shouldNotPurge = pgaHeader.AreClothingAndTextileDetailsVisibility;

				var expected = shouldNotPurge ? FTAProcessingCodes.Codes.FA01 : string.Empty;
				AssertEquals("Should keep CA_FTACode when clothing and textile details are visibility, or purge.", expected, pgaHeader.CA_FTACode);

				expected = shouldNotPurge ? Core.Constants.CountryCodes.Australia : string.Empty;
				AssertEquals("Should keep CA_FibreCountryOfOrigin when clothing and textile details are visibility, or purge.", expected, pgaHeader.CA_FibreCountryOfOrigin);

				expected = shouldNotPurge ? Core.Constants.CountryCodes.UnitedStates : string.Empty;
				AssertEquals("Should keep CA_FabricCountryOfOrigin when clothing and textile details are visibility, or purge.", expected, pgaHeader.CA_FabricCountryOfOrigin);

				expected = shouldNotPurge ? Core.Constants.CountryCodes.Afghanistan : string.Empty;
				AssertEquals("Should keep CA_YarnCountryOfOrigin when clothing and textile details are visibility, or purge.", expected, pgaHeader.CA_YarnCountryOfOrigin);
			});
		}

		#endregion

		public void TestClothingAndTextileDetailsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_PermitApplication = false;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;
			invoiceLine.JI_Tariff = string.Empty;

			var pgaHeader = invoiceLine.GACPGAHeader;
			pgaHeader.CA_CommodityCode = string.Empty;

			AssertEquals(false, pgaHeader.AreClothingAndTextileDetailsVisibility);
			AssertEquals(false, header.IsFTAProcessingCodeFA01);

			invoiceLine.JI_Tariff = "50000012";
			AssertEquals(true, pgaHeader.AreClothingAndTextileDetailsVisibility);
			AssertEquals(false, pgaHeader.IsFTAProcessingCodeFA01);

			pgaHeader.CA_AllProgramInd = Customs.Business.YesNoList.Codes.Yes;
			pgaHeader.CA_FTACode = FTAProcessingCodes.Codes.FA01;
			AssertEquals(true, pgaHeader.AreClothingAndTextileDetailsVisibility);
			AssertEquals(true, pgaHeader.IsFTAProcessingCodeFA01);

			pgaHeader.CA_FTACode = FTAProcessingCodes.Codes.FA02;
			AssertEquals(true, pgaHeader.AreClothingAndTextileDetailsVisibility);
			AssertEquals(false, pgaHeader.IsFTAProcessingCodeFA01);

			invoiceLine.JI_Tariff = "48000012";
			AssertEquals(false, pgaHeader.AreClothingAndTextileDetailsVisibility);

			invoiceLine.JI_Tariff = "A2000012";
			AssertEquals(false, pgaHeader.AreClothingAndTextileDetailsVisibility);
		}

		public void TestClothingAndTextileDetailsVisibilityForCusClassification()
		{
			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = ClassificationType.IMP;
			classification.CCA_GACIndicator = YesNoList.Codes.Yes;
			classification.CC_TariffNum = string.Empty;

			var pgaHeader = classification.GACPGAHeader;
			pgaHeader.CA_CommodityCode = string.Empty;

			AssertEquals(false, pgaHeader.AreClothingAndTextileDetailsVisibility);

			classification.CC_TariffNum = "50000012";
			AssertEquals(true, pgaHeader.AreClothingAndTextileDetailsVisibility);

			classification.CC_TariffNum = "48000012";
			AssertEquals(false, pgaHeader.AreClothingAndTextileDetailsVisibility);

			classification.CC_TariffNum = "A2000012";
			AssertEquals(false, pgaHeader.AreClothingAndTextileDetailsVisibility);
		}

		public void TestSupportsNotes()
		{
			var bo = (GACPGAHeader)GetNewBusinessObject();

			AssertEquals(false, bo.SupportsNotes);
		}

		public void TestGetContactDetails()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "test@mail.com";

			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "TestOrgName";

			var contact = importer.Contacts.AddNew();
			contact.OC_Email = "xxx@yyy.com";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = OrgConstants.ContactAllocationType.CAPGA;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_GS_NKCusAgent = staff.GS_Code;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.CA_GACInd = "Y";
			var gacPGAHeader = invoiceLine.GACPGAHeader;
			gacPGAHeader.CA_AllProgramInd = "Y";

			AssertEquals("Should get value from the importer.", contact.OC_Email, ((ILPCOContactParent)gacPGAHeader).GetContactDetails(LPCOHolderPartyTypeCodes.Codes.Importer).EmailAddress);
			AssertEquals("Should get value from the broker.", staff.GS_EmailAddress, ((ILPCOContactParent)gacPGAHeader).GetContactDetails(LPCOHolderPartyTypeCodes.Codes.Broker).EmailAddress);
		}

		public void TestLPCOViewCollection()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "2005", "2005 DESC", startDate, endDate);
			var attributeNamePGA = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, "PGA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributePGA = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.GAC);
			newFactory.Save();

			AssertEquals("LpcoViews on GAC", 0, header.LPCOViews.Count);
			AssertEquals(typeof(LPCOViewCollection), header.LPCOViews.GetType());

			var headerLPCO = declaration.LPCOViews.AddNew();
			headerLPCO.CLP_Type = "2005";
			header.LPCOViews.AddNew();
			AssertEquals("LpcoViews on GAC", 2, header.LPCOViews.Count);

			var headerLPCO2 = declaration.LPCOViews.AddNew();
			headerLPCO2.CLP_Type = "2005";
			AssertEquals("LpcoViews on GAC", 3, header.LPCOViews.Count);
		}

		protected override IEnumerable<GACPGAHeader> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = "Y";
			yield return invoiceLine.GACPGAHeader;

			var product = factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = factory.New<CusClassPartPivot>();
			pivot.CI_OP = product.PK;
			pivot.CI_TariffNum = "1111.11.11";
			pivot.CCA_GACIndicator = Customs.Business.YesNoList.Codes.Yes;
			yield return pivot.GACPGAHeader;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = "Y";
			header = invoiceLine.GACPGAHeader;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return header;
		}
		GACPGAHeader header;
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = "Y";
			return invoiceLine.GACPGAHeader;
		}
	}
}
