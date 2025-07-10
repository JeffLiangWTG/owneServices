using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusCALPCO))]
	sealed class CusCALPCOTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDIFDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			var docsAndCartage = declaration.DocsAndCartage;
			var requiredDocument = docsAndCartage.RequiredDocuments.AddNew();
			var requiredDocumentAddInfo = requiredDocument.AddInfos.AddNew();
			requiredDocumentAddInfo.EX_GC_Company = declaration.JE_GC;
			requiredDocumentAddInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			requiredDocumentAddInfo.EX_ReferenceNumber = "20220826";
			requiredDocumentAddInfo.EX_AddInfo = @"<DIFDocument xmlns=""http://www.cargowise.com/Schemas/DIFDocument""><PGA>CFIA</PGA><DocumentNumber>1498498498</DocumentNumber></DIFDocument>";
			Factory.Save();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = "Y";
			var pgaHeader = invoiceLine.CFIAPGAHeader;
			var lpcoView = pgaHeader.LPCOViews.AddNew();
			var lpco = lpcoView.LPCO;
			lpco.CLP_DIFRefNumberOrLocation = "20220826";
			var difDocument = lpco.DIFDocument;
			AssertEquals(requiredDocumentAddInfo.PK, difDocument.PK);
		}

		public void TestDefaultCLP_RefNoWhenDocumentTypeIsConfirmation()
		{
			RegistrationNumberHelperTest.PrepareGlobalData(Factory);

			var header = Factory.New<CFIAPGAHeader>();
			var lpco = header.LPCOViews.AddNew();
			AssertEquals(ZString.Empty, lpco.CLP_RefNo);

			lpco.CLP_Type = "C03";
			AssertEquals("Y", lpco.CLP_RefNo);

			lpco.CLP_Type = "";
			lpco.CLP_RefNo = "A";
			AssertEquals("A", lpco.CLP_RefNo);

			lpco.CLP_Type = "0001";
			AssertEquals("A", lpco.CLP_RefNo);
		}

		public void TestCLP_Type()
		{
			var now = ZDate.Today;
			var header = Factory.New<GACPGAHeader>();

			var lpcoView = header.LPCOViews.AddNew();
			lpcoView.CLP_SecondaryRefNo = "123";
			lpcoView.CLP_IssueDate = now;

			var lpco = lpcoView.LPCO;

			lpco.CLP_Type = LPCODocumentTypeQualifier.Codes._2006;

			AssertEquals("CLP_SecondaryRefNo", ZString.Empty, lpco.CLP_SecondaryRefNo);
			AssertEquals("CLP_IssueDate", ZDateTime.Empty, lpco.CLP_IssueDate);

			lpco.CLP_SecondaryRefNo = "123";
			lpco.CLP_IssueDate = now;

			lpco.CLP_Type = LPCODocumentTypeQualifier.Codes._2007;

			AssertEquals("CLP_SecondaryRefNo", "123", lpco.CLP_SecondaryRefNo);
			AssertEquals("CLP_IssueDate", now, lpco.CLP_IssueDate);
		}

		public void TestDefaultRefNo()
		{
			var header = Factory.New<GACPGAHeader>();
			var lpcoView = header.LPCOViews.AddNew();
			var lpco = lpcoView.LPCO;

			lpco.CLP_Type = "0001";
			AssertEquals(string.Empty, lpco.CLP_RefNo);

			lpco.CLP_Type = "0002";
			AssertEquals(string.Empty, lpco.CLP_RefNo);

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "0003", "0003 DESC", startDate, endDate);
			var attributeNamePGA = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, "PGA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributePGA = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.GAC);
			var attributeNameValuesAllowed = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypeValuesAllowed, "ValuesAllowed", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributeValuesAllowed01 = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameValuesAllowed.ZXE_Name, "YYY");
			newFactory.Save();

			lpcoView = header.LPCOViews.AddNew();
			lpco = lpcoView.LPCO;
			lpco.CLP_Type = "0001";
			lpco.CLP_RefNo = "ABC";

			lpco.CLP_Type = "0002";
			AssertEquals("ABC", lpco.CLP_RefNo);

			lpco.CLP_Type = "0001";
			lpco.CLP_RefNo = string.Empty;
			AssertEquals(string.Empty, lpco.CLP_RefNo);

			lpco.CLP_Type = "0003";
			AssertEquals("YYY", lpco.CLP_RefNo);

			Factory.ClearCachedValue<ZZRefCusCodeListCombined>(string.Format("{0}_{1}_{2}", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "0003", ZDateTime.UtcToday.Date.ToShortDateString()));
			var attributeValuesAllowed02 = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameValuesAllowed.ZXE_Name, "ZZZ");
			newFactory.Save();

			lpco.CLP_Type = "0001";
			lpco.CLP_RefNo = string.Empty;
			AssertEquals(string.Empty, lpco.CLP_RefNo);

			lpco.CLP_Type = "0003";
			AssertEquals("YYY", lpco.CLP_RefNo);

			var newGAC = newFactory.New<GACPGAHeader>();
			lpcoView = newGAC.LPCOViews.AddNew();
			lpco = lpcoView.LPCO;
			lpco.CLP_Type = "0003";
			AssertEquals(string.Empty, lpco.CLP_RefNo);
		}

		public void TestDefaultCFIACLP_RefNo()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TSTIMPORTER";
			var orgImpAddInfo = OrgImpAddInfo.Get(importer);
			var license = orgImpAddInfo.SafeFoodLicenses.AddNew();
			license.CY_Code = "SFC";
			license.CY_Data = "SafeFoodLicense";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;

			var lpco = declaration.LPCOs.AddNew();
			lpco.CLP_Type = RegistrationNumberHelper.SafeFoodForCanadiansLicence;
			AssertEquals("SFC", lpco.CLP_RefNo);

			lpco.CLP_RefNo = "TTT";
			lpco.CLP_Type = ZString.Empty;
			lpco.CLP_Type = RegistrationNumberHelper.SafeFoodForCanadiansLicence;
			AssertEquals("TTT", lpco.CLP_RefNo);

			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;
			var cfia = invoiceLine.CFIAPGAHeader;
			var lpcoView = cfia.LPCOViews.AddNew();
			AssertEquals(ZString.Empty, lpcoView.CLP_RefNo);

			lpcoView.CLP_Type = RegistrationNumberHelper.SafeFoodForCanadiansLicence;
			AssertEquals("SFC", lpcoView.CLP_RefNo);

			lpcoView.CLP_RefNo = "TTT";
			lpcoView.CLP_Type = ZString.Empty;
			lpcoView.CLP_Type = RegistrationNumberHelper.SafeFoodForCanadiansLicence;
			AssertEquals("TTT", lpcoView.CLP_RefNo);
		}

		public void TestDefaultUQ()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Centilitre;
			invoiceLine.CA_GACInd = "Y";

			var gacPGAHeader = invoiceLine.GACPGAHeader;
			var lpcoView = gacPGAHeader.LPCOViews.AddNew();
			var lpco = lpcoView.LPCO;

			lpco.CLP_AlternativeQuotaQuantity = 10.55m;
			AssertEquals(CustomsUnitOfMeasureList.Codes.Centilitre, lpco.CLP_AlternativeQuotaUQ);
		}

		public void TestClearOtherValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Centilitre;
			invoiceLine.CA_GACInd = "Y";

			var gacPGAHeader = invoiceLine.GACPGAHeader;
			var lpcoView = gacPGAHeader.LPCOViews.AddNew();
			var lpco = lpcoView.LPCO;
			lpco.CLP_HolderType = LPCOHolderPartyTypeCodes.Codes.Other;

			lpco.CLP_OA_Holder = Factory.New<OrgHeader>().MainAddress.PK;
			lpco.CLP_HolderName = "Lilly";
			lpco.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Other;

			lpco.CLP_OA_Applicant = Factory.New<OrgHeader>().MainAddress.PK;

			lpco.CLP_HolderType = LPCOHolderPartyTypeCodes.Codes.Importer;
			AssertEquals(ZGuid.Empty, lpco.CLP_OA_Holder);
			AssertEquals(ZString.Empty, lpco.CLP_HolderName);

			AssertNotEquals(ZGuid.Empty, lpco.CLP_OA_Applicant);
			lpco.CLP_IsApplicantOverridden = true;
			lpco.CLP_ApplicantName = "Lilly2";
			AssertNotEquals(ZString.Empty, lpco.CLP_ApplicantName);

			lpco.CLP_IsApplicantOverridden = false;
			lpco.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Importer;
			AssertEquals(ZGuid.Empty, lpco.CLP_OA_Applicant);
			AssertEquals(ZString.Empty, lpco.CLP_ApplicantName);
		}

		public void TestDefaultApplicantOrHolderName()
		{
			var party1 = Factory.New<OrgHeader>();
			party1.OH_FullName = "Fireworks r us";

			var partyAddress = party1.Addresses.AddNew();
			partyAddress.OA_CompanyNameOverride = "Cracking fireworks";

			var party2 = Factory.New<OrgHeader>();
			party2.OH_FullName = "Toys r us";

			var partyAddress2 = party2.Addresses.AddNew();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = party1.PK;
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Centilitre;
			invoiceLine.CA_GACInd = "Y";

			var gacPGAHeader = invoiceLine.GACPGAHeader;
			var lpcoView = gacPGAHeader.LPCOViews.AddNew();
			var lpco = lpcoView;
			lpco.CLP_HolderType = LPCOHolderPartyTypeCodes.Codes.Other;
			lpco.CLP_OA_Holder = partyAddress.PK;
			AssertEquals("CLP_HolderName", "Cracking fireworks", lpco.CLP_HolderName);

			lpco.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Other;
			lpco.CLP_OA_Applicant = partyAddress2.PK;
			AssertEquals("ApplicantName", "Toys r us", lpco.CLP_ApplicantName);

			lpco.CLP_ApplicantName = ZString.Empty;
			lpco.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Importer;

			AssertEquals(ZGuid.Empty, lpco.CLP_OA_Applicant);
			AssertEquals("Fireworks r us", lpco.CLP_ApplicantName);

			lpco.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Exporter;
			AssertEquals(ZString.Empty, lpco.CLP_ApplicantName);
		}

		public void TestDefaultHolderContactDetails()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "TestOrgName";
			CreateContactForAllocation(importer, "Eric Cartman", "xxx@yyy.com", "02512345678", OrgConstants.ContactAllocationType.CAPGA);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.CA_GACInd = "Y";
			var gacPGAHeader = invoiceLine.GACPGAHeader;
			gacPGAHeader.CA_AllProgramInd = "Y";

			var lpcoView = gacPGAHeader.LPCOViews.AddNew();
			var lpco = lpcoView;
			lpco.CLP_HolderType = LPCOHolderPartyTypeCodes.Codes.Importer;

			AssertEquals("Eric Cartman", lpco.CLP_HolderContactName);
			AssertEquals("xxx@yyy.com", lpco.CLP_HolderContactEmail);
			AssertEquals("02512345678", lpco.CLP_HolderContactPhone);

			lpco.CLP_HolderType = LPCOHolderPartyTypeCodes.Codes.Exporter;
			AssertEquals(ZString.Empty, lpco.CLP_HolderContactName);
			AssertEquals(ZString.Empty, lpco.CLP_HolderContactEmail);
			AssertEquals(ZString.Empty, lpco.CLP_HolderContactPhone);
		}

		public void TestDefaultApplicantContactDetails()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "TestOrgName";
			CreateContactForAllocation(importer, "Eric Cartman", "xxx@yyy.com", "02512345678", OrgConstants.ContactAllocationType.CAPGA);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.CA_GACInd = "Y";
			var gacPGAHeader = invoiceLine.GACPGAHeader;
			gacPGAHeader.CA_AllProgramInd = "Y";

			var lpcoView = gacPGAHeader.LPCOViews.AddNew();
			var lpco = lpcoView;
			lpco.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Importer;

			AssertEquals("Eric Cartman", lpco.CLP_ApplicantContactName);
			AssertEquals("xxx@yyy.com", lpco.CLP_ApplicantContactEmail);
			AssertEquals("02512345678", lpco.CLP_ApplicantContactPhone);

			lpco.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Exporter;
			AssertEquals(ZString.Empty, lpco.CLP_ApplicantContactName);
			AssertEquals(ZString.Empty, lpco.CLP_ApplicantContactEmail);
			AssertEquals(ZString.Empty, lpco.CLP_ApplicantContactPhone);
		}

		public void TestCLP_AuthorizedPartyAddressOverride()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "TestOrgName";
			importer.OH_Code = "TES";
			CreateContactForAllocation(importer, "Eric Cartman", "xxx@yyy.com", "02512345678", OrgConstants.ContactAllocationType.CAPGA);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_OH_Importer = importer.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;

			var gac = invoiceLine.GACPGAHeader;
			gac.CA_AllProgramInd = YesNoList.Codes.Yes;
			var lpcoView = gac.LPCOViews.AddNew();
			lpcoView.CLP_Type = "5001";
			lpcoView.CLP_HolderType = LPCOHolderPartyTypeCodes.Codes.Importer;
			lpcoView.CLP_IsHolderOverridden = true;
			AssertEquals("Eric Cartman", lpcoView.CLP_HolderContactName);
			AssertEquals("xxx@yyy.com", lpcoView.CLP_HolderContactEmail);
			AssertEquals("02512345678", lpcoView.CLP_HolderContactPhone);
			AssertEquals("TestOrgName", lpcoView.CLP_HolderName);
			AssertEquals(false, lpcoView.CLP_HolderContactNameInfo.ReadOnly);
			AssertEquals(false, lpcoView.CLP_HolderContactEmailInfo.ReadOnly);
			AssertEquals(false, lpcoView.CLP_HolderContactPhoneInfo.ReadOnly);
			AssertEquals(false, lpcoView.CLP_HolderNameInfo.ReadOnly);

			lpcoView.CLP_IsHolderOverridden = false;
			AssertEquals("Eric Cartman", lpcoView.CLP_HolderContactName);
			AssertEquals("xxx@yyy.com", lpcoView.CLP_HolderContactEmail);
			AssertEquals("02512345678", lpcoView.CLP_HolderContactPhone);
			AssertEquals("TestOrgName", lpcoView.CLP_HolderName);
			AssertEquals(true, lpcoView.CLP_HolderContactNameInfo.ReadOnly);
			AssertEquals(true, lpcoView.CLP_HolderContactEmailInfo.ReadOnly);
			AssertEquals(true, lpcoView.CLP_HolderContactPhoneInfo.ReadOnly);
			AssertEquals(true, lpcoView.CLP_HolderNameInfo.ReadOnly);
		}

		public void TestCLP_ApplicantAddressOverride()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "TestOrgName";
			importer.OH_Code = "TES";
			CreateContactForAllocation(importer, "Eric Cartman", "xxx@yyy.com", "02512345678", OrgConstants.ContactAllocationType.CAPGA);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_OH_Importer = importer.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;

			var gac = invoiceLine.GACPGAHeader;
			gac.CA_AllProgramInd = YesNoList.Codes.Yes;
			var lpcoView = gac.LPCOViews.AddNew();
			lpcoView.CLP_Type = "5001";
			lpcoView.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Importer;
			lpcoView.CLP_IsApplicantOverridden = true;
			AssertEquals("Eric Cartman", lpcoView.CLP_ApplicantContactName);
			AssertEquals("xxx@yyy.com", lpcoView.CLP_ApplicantContactEmail);
			AssertEquals("02512345678", lpcoView.CLP_ApplicantContactPhone);
			AssertEquals("TestOrgName", lpcoView.CLP_ApplicantName);
			AssertEquals(false, lpcoView.CLP_ApplicantContactNameInfo.ReadOnly);
			AssertEquals(false, lpcoView.CLP_ApplicantContactEmailInfo.ReadOnly);
			AssertEquals(false, lpcoView.CLP_ApplicantContactPhoneInfo.ReadOnly);
			AssertEquals(false, lpcoView.CLP_ApplicantNameInfo.ReadOnly);

			lpcoView.CLP_IsApplicantOverridden = false;
			AssertEquals("Eric Cartman", lpcoView.CLP_ApplicantContactName);
			AssertEquals("xxx@yyy.com", lpcoView.CLP_ApplicantContactEmail);
			AssertEquals("02512345678", lpcoView.CLP_ApplicantContactPhone);
			AssertEquals("TestOrgName", lpcoView.CLP_ApplicantName);
			AssertEquals(true, lpcoView.CLP_ApplicantContactNameInfo.ReadOnly);
			AssertEquals(true, lpcoView.CLP_ApplicantContactEmailInfo.ReadOnly);
			AssertEquals(true, lpcoView.CLP_ApplicantContactPhoneInfo.ReadOnly);
			AssertEquals(true, lpcoView.CLP_ApplicantNameInfo.ReadOnly);
		}

		public void TestNoExceptionThrownWhenChangeApplicantType()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "TestOrgName";
			importer.OH_Code = "TES";
			CreateContactForAllocation(importer, "Eric Cartman", "xxx@yyy.com", "02512345678", OrgConstants.ContactAllocationType.CAPGA);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_OH_Importer = importer.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;

			var gac = invoiceLine.GACPGAHeader;
			gac.CA_AllProgramInd = YesNoList.Codes.Yes;
			var lpcoView = gac.LPCOViews.AddNew();
			lpcoView.CLP_Type = "5001";
			lpcoView.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Importer;
			lpcoView.CLP_IsApplicantOverridden = true;
			Factory.Save();

			lpcoView.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Exporter;
			lpcoView.CLP_IsApplicantOverridden = false;
			Factory.Save();

			AssertNoExceptionThrown(() =>
			{
				lpcoView.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Importer;
			});
		}

		public void TestNoExceptionThrownWhenResetTheProductCode()
		{
			var factory = new BusinessObjectFactory();
			var importer1 = factory.New<OrgHeader>();
			importer1.OH_FullName = "TestOrgName";
			importer1.OH_Code = "TES";
			CreateContactForAllocation(importer1, "Eric Cartman", "xxx@yyy.com", "02512345678", OrgConstants.ContactAllocationType.CAPGA);
			var part = factory.New<OrgSupplierPart>();
			part.FillWithValidTestData();
			part.OP_PartNum = "OP123";
			var part1Relation1 = part.RelatedOrganisations.AddNew();
			part1Relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			part1Relation1.OU_OH = importer1.PK;
			CusClassPartPivot pivot = factory.New<CusClassPartPivot>();
			pivot.CI_OP = part.PK;
			pivot.CI_TariffNum = "1111.11.11";
			pivot.CCA_CFIAIndicator = YesNoList.Codes.Yes;
			var partCFIA = pivot.CFIAPGAHeader;
			partCFIA.CA_AllProgramInd = YesNoList.Codes.Yes;
			var partLPCOView = partCFIA.LPCOViews.AddNew();
			partLPCOView.CLP_Type = "CLP1";
			partLPCOView.CLP_RefNo = "TEST1";
			factory.Save();

			var factory2 = new BusinessObjectFactory();
			var declaration = factory2.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_OH_Importer = importer1.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;
			var invCFIA = invoiceLine.CFIAPGAHeader;
			invCFIA.CA_AllProgramInd = YesNoList.Codes.Yes;
			var invLPCOView = invCFIA.LPCOViews.AddNew();
			factory2.Save();

			invLPCOView.CLP_Type = "CLP3";
			invLPCOView.CLP_RefNo = "TEST3";
			invoiceLine.JI_PartNo = "OP123";

			var factory3 = new BusinessObjectFactory();
			var partLoadFromInv = factory3.Load<OrgSupplierPart>(part.PK);
			var pivotLoadFromInv = partLoadFromInv.Factory.Load<CusClassPartPivot>(pivot.PK);
			var partCFIALoadFromInv = pivotLoadFromInv.CFIAPGAHeader;
			var partLPCOViewLoadFromInv = partCFIALoadFromInv.LPCOViews[0];
			partLPCOViewLoadFromInv.LPCO.Delete();
			partLPCOViewLoadFromInv = partCFIALoadFromInv.LPCOViews.AddNew();
			partLPCOViewLoadFromInv.CLP_Type = "CLP2";
			partLPCOViewLoadFromInv.CLP_RefNo = "TEST2";
			partLoadFromInv.OP_PartNum = "OP456";

			factory3.Save();

			AssertNoExceptionThrown(() =>
			{
				invoiceLine.UpdateDetailsOnPartChange();
			});
		}

		void CreateContactForAllocation(OrgHeader org, string name, string email, string phone, string type)
		{
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = name;
			contact.OC_Email = email;
			contact.OC_Phone = phone;
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = type;
		}

		public void TestDocumentType()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "0003", "0003 DESC", startDate, endDate);
			var attributeNamePGA = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, "PGA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributePGA = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.GAC);
			var attributeNameValuesAllowed = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypeValuesAllowed, "ValuesAllowed", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributeValuesAllowed01 = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameValuesAllowed.ZXE_Name, "YYY");
			newFactory.Save();

			var header = Factory.New<GACPGAHeader>();
			var lpcoView = header.LPCOViews.AddNew();
			var lpco = lpcoView.LPCO;
			lpco.CLP_Type = "0003";
			AssertNotNull(lpco.DocumentType);

			var cfiaPGAHeader = Factory.New<CFIAPGAHeader>();
			lpcoView = cfiaPGAHeader.LPCOViews.AddNew();
			lpco = lpcoView.LPCO;
			lpco.CLP_Type = "0003";
			AssertNotNull(lpco.DocumentType);
		}

		public void TestAgencyIDCode()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "0003", "0003 DESC", startDate, endDate);
			var attributeNamePGA = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, "PGA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributePGA = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.GAC);
			var attributeNameValuesAllowed = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypeValuesAllowed, "ValuesAllowed", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributeValuesAllowed01 = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameValuesAllowed.ZXE_Name, "YYY");
			newFactory.Save();

			var header = Factory.New<GACPGAHeader>();
			var lpcoView = header.LPCOViews.AddNew();
			var lpco = lpcoView.LPCO;
			lpco.CLP_Type = "0003";
			AssertEquals("AgencyIDCode", PGACodes.Codes.GAC, lpco.AgencyIDCode);
		}

		public void TestTypeDescription()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "0003", "0003 DESC", startDate, endDate);
			var attributeNamePGA = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, "PGA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributePGA = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.GAC);
			var attributeNameValuesAllowed = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypeValuesAllowed, "ValuesAllowed", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributeValuesAllowed01 = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameValuesAllowed.ZXE_Name, "YYY");
			newFactory.Save();

			RegistrationNumberHelperTest.PrepareGlobalData(Factory);

			var header = Factory.New<GACPGAHeader>();
			var lpcoView = header.LPCOViews.AddNew();
			var lpco = lpcoView.LPCO;
			lpco.CLP_Type = "0003";
			AssertEquals("TypeDescription", "0003 DESC", lpco.TypeDescription);

			var cfiaPGAHeader = Factory.New<CFIAPGAHeader>();
			lpcoView = cfiaPGAHeader.LPCOViews.AddNew();
			lpco = lpcoView.LPCO;
			lpco.CLP_Type = "C01";
			AssertEquals("TypeDescription", "LPCO 001", lpco.TypeDescription);
		}

		public void TestDefaultURNWhenSetDocumentType()
		{
			const string xml1 = @"<DIFDocument xmlns=""http://www.cargowise.com/Schemas/DIFDocument"">
  <PGA>12</PGA>
  <DocumentType>5001</DocumentType>
  <DocumentDescription>desc</DocumentDescription>
  <EDocsDocumentPK>3bf5d443-a5b6-457c-b7a9-d436dca3ea1e</EDocsDocumentPK>
  <Comment>Comment</Comment>
  <DocumentNumber>MaximumLength70</DocumentNumber>
  <EffectiveDate>2013-12-25T12:25:25</EffectiveDate>
  <ExpiryDate>2014-12-25T12:25:25</ExpiryDate>
</DIFDocument>";
			const string xml2 = @"<DIFDocument xmlns=""http://www.cargowise.com/Schemas/DIFDocument"">
  <PGA>12</PGA>
  <DocumentType>5002</DocumentType>
  <DocumentDescription>desc</DocumentDescription>
  <EDocsDocumentPK>3bf5d443-a5b6-457c-b7a9-d436dca3ea1e</EDocsDocumentPK>
  <Comment>Comment</Comment>
  <DocumentNumber>MaximumLength70</DocumentNumber>
  <EffectiveDate>2013-12-25T12:25:25</EffectiveDate>
  <ExpiryDate>2014-12-25T12:25:25</ExpiryDate>
</DIFDocument>";

			var declaration = Factory.New<JobDeclaration>();
			var eDoc = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "Test", Core.Constants.RefDocTypes.CommercialInvoice);
			eDoc.Description = "SOME DESCRIPTION";
			var disWrapper = ObjectFactory.Get<IDISHostWrapper>("CA.IDIFHostWrapper", new object[] { declaration });

			var requiredDocument1 = ((IDISHost)declaration).RequiredDocumentsProvider.RequiredDocuments.AddNew();
			requiredDocument1.EQ_DocDescription = "Dec1";
			var addInfo1 = requiredDocument1.AddInfos.AddNew();
			addInfo1.EX_ReferenceNumber = "REF1";
			addInfo1.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			addInfo1.EX_AddInfo = xml1;

			var requiredDocument2 = ((IDISHost)declaration).RequiredDocumentsProvider.RequiredDocuments.AddNew();
			requiredDocument2.EQ_DocDescription = "Dec2";
			var addInfo2 = requiredDocument2.AddInfos.AddNew();
			addInfo2.EX_ReferenceNumber = "REF2";
			addInfo2.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			addInfo2.EX_AddInfo = xml2;

			var requiredDocument3 = ((IDISHost)declaration).RequiredDocumentsProvider.RequiredDocuments.AddNew();
			requiredDocument3.EQ_DocDescription = "Dec3";
			var addInfo3 = requiredDocument3.AddInfos.AddNew();
			addInfo3.EX_ReferenceNumber = "REF3";
			addInfo3.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			addInfo3.EX_AddInfo = xml2;
			Factory.Save();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = "Y";
			var gacPGAHeader = invoiceLine.GACPGAHeader;
			gacPGAHeader.CA_AllProgramInd = "Y";
			var lpcoView = gacPGAHeader.LPCOViews.AddNew();
			var lpco = lpcoView.LPCO;
			lpco.CLP_Type = "5001";
			AssertEquals("REF1", lpco.CLP_DIFRefNumberOrLocation);

			var lpcoView2 = gacPGAHeader.LPCOViews.AddNew();
			var lpco2 = lpcoView2.LPCO;
			lpco2.CLP_Type = "5002";
			AssertEquals(ZString.Empty, lpco2.CLP_DIFRefNumberOrLocation);
		}

		public void TestDefaultValuesWhenSetDIFReferenceNumber()
		{
			ObjectFactory.Get<Integration.Customs.CA.ICACustomsDataRegistry>().AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");
			var cACompany = Factory.New<GlbCompany>();
			cACompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			cACompany.GC_Code = "CA1";
			var branch1 = cACompany.Branches.AddNew();
			branch1.GB_Code = "CA1";

			OrgHeader orgheader = Factory.New<OrgHeader>();
			orgheader.OH_RL_NKClosestPort = "CALAX";
			orgheader.OH_Code = "OH1";
			var address = orgheader.Addresses.AddNew();
			address.Address1 = "TEST CA Address";

			var permit = Factory.NewWithValidTestData<CusPermitHeader>();
			permit.CPH_OH_PermitHolder = orgheader.PK;
			permit.CPH_Number = "DMNUMBER0001";
			permit.CPH_StartDate = new ZDate(2017, 12, 10);
			var difHost = permit as ICADIFHost;
			var requiredDocument = difHost.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.CommercialInvoice;

			var addInfo = requiredDocument.AddInfos.AddNew();
			addInfo.EX_ReferenceNumber = "REF1";
			addInfo.EX_EQ_RequiredDocument = requiredDocument.PK;
			addInfo.EX_GC_Company = cACompany.PK;
			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			addInfo.EX_AddInfo = "<DIFDocument xmlns=\"http://www.cargowise.com/Schemas/DIFDocument\"><PGA>12</PGA><DocumentType>5002</DocumentType><EDocsDocumentPK>feb71b42-4444-4168-8918-0e0f27d8c375</EDocsDocumentPK><DocumentNumber>DMNUMBER0001</DocumentNumber><EffectiveDate>2017-12-10T00:00:00</EffectiveDate><ExpiryDate>2017-12-12T00:00:00</ExpiryDate></DIFDocument>";

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch1.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.ManufacturerOrgPK = orgheader.PK;
			invoiceLine.CA_GACInd = "Y";
			var gacPGAHeader = invoiceLine.GACPGAHeader;
			gacPGAHeader.CA_AllProgramInd = "Y";

			var lpcoView = gacPGAHeader.LPCOViews.AddNew();
			var lpco = lpcoView.LPCO;
			lpco.CLP_DIFRefNumberOrLocation = "REF1";

			var lpcodefault = gacPGAHeader as ILPCODefaulter;
			Assert(lpcodefault.ShouldDefaultLPCOFields);
			Assert(lpcodefault.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_Type));
			AssertEquals("Document Type", "5002", lpco.CLP_Type);
			Assert(lpcodefault.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_RefNo));
			AssertEquals("Reference Number", "DMNUMBER0001", lpco.CLP_RefNo);
			Assert(lpcodefault.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_StartDate));
			AssertEquals("Start Date", new ZDateTime(2017, 12, 10), lpco.CLP_StartDate);
			Assert(!lpcodefault.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_EndDate));
			AssertEquals("End Date", ZDateTime.Empty, lpco.CLP_EndDate);
			Assert(lpcodefault.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_HolderType));
			AssertEquals("Holder Type", LPCOHolderPartyTypeCodes.Codes.Manufacturer, lpco.CLP_HolderType);

			invoiceLine.ManufacturerOrgPK = ZGuid.Empty;
			lpco.CLP_DIFRefNumberOrLocation = ZString.Empty;
			lpco.CLP_HolderType = ZString.Empty;
			invoice.JZ_OH_Supplier = orgheader.PK;
			lpco.CLP_DIFRefNumberOrLocation = "REF1";
			AssertEquals("Holder Type", LPCOHolderPartyTypeCodes.Codes.Supplier, lpco.CLP_HolderType);

			invoice.JZ_OH_Supplier = ZGuid.Empty;
			lpco.CLP_DIFRefNumberOrLocation = ZString.Empty;
			lpco.CLP_HolderType = ZString.Empty;
			invoice.ExporterDocumentaryAddress.OrganisationPK = orgheader.PK;
			lpco.CLP_DIFRefNumberOrLocation = "REF1";
			AssertEquals("Holder Type", LPCOHolderPartyTypeCodes.Codes.Exporter, lpco.CLP_HolderType);

			invoice.ExporterDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			lpco.CLP_DIFRefNumberOrLocation = ZString.Empty;
			lpco.CLP_HolderType = ZString.Empty;
			declaration.ImporterOfRecordAddress.OrganisationPK = orgheader.PK;
			lpco.CLP_DIFRefNumberOrLocation = "REF1";
			AssertEquals("Holder Type", LPCOHolderPartyTypeCodes.Codes.ImporterOfRecord, lpco.CLP_HolderType);

			declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
			lpco.CLP_DIFRefNumberOrLocation = ZString.Empty;
			lpco.CLP_HolderType = ZString.Empty;
			declaration.JE_OH_Importer = orgheader.PK;
			lpco.CLP_DIFRefNumberOrLocation = "REF1";
			AssertEquals("Holder Type", LPCOHolderPartyTypeCodes.Codes.Importer, lpco.CLP_HolderType);

			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
			lpco.CLP_DIFRefNumberOrLocation = ZString.Empty;
			lpco.CLP_HolderType = ZString.Empty;
			permit.CPH_OH_PermitHolder = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			lpco.CLP_DIFRefNumberOrLocation = "REF1";
			AssertEquals("Holder Type", LPCOHolderPartyTypeCodes.Codes.Broker, lpco.CLP_HolderType);

			invoiceLine.CA_NRCanInd = "Y";
			var nrcanPGAHeader = invoiceLine.NRCanPGAHeader;
			nrcanPGAHeader.CA_RDAProgramInd = "Y";

			var lpcoView2 = nrcanPGAHeader.LPCOViews.AddNew();
			var lpco2 = lpcoView2.LPCO;
			lpco2.CLP_DIFRefNumberOrLocation = "REF1";

			var lpcodefault2 = nrcanPGAHeader as ILPCODefaulter;
			Assert(lpcodefault2.ShouldDefaultLPCOFields);
			Assert(lpcodefault2.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_Type));
			AssertEquals("Document Type", "5002", lpco2.CLP_Type);
			Assert(lpcodefault2.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_RefNo));
			AssertEquals("Reference Number", "DMNUMBER0001", lpco2.CLP_RefNo);
			Assert(lpcodefault2.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_IssueDate));
			AssertEquals("Start Date", new ZDateTime(2017, 12, 10), lpco2.CLP_IssueDate);
			Assert(lpcodefault2.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_EndDate));
			AssertEquals("End Date", new ZDateTime(2017, 12, 12), lpco2.CLP_EndDate);
			Assert(!lpcodefault2.LPCOFieldsDefaultFromURN.Contains(CusCALPCO.Schema.CLP_HolderType));
			AssertEquals("Holder Type", ZString.Empty, lpco2.CLP_HolderType);
		}

		public void TestNameAndContactInformationCanLoadSuccessfullyWhenOpenTheForm()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "TestOrgName";
			importer.OH_Code = "TES";
			CreateContactForAllocation(importer, "Eric Cartman", "xxx@yyy.com", "02512345678", OrgConstants.ContactAllocationType.CAPGA);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.CA_GACInd = "Y";
			var gacPGAHeader = invoiceLine.GACPGAHeader;
			gacPGAHeader.CA_AllProgramInd = "Y";

			var lpcoView = gacPGAHeader.LPCOViews.AddNew();
			var lpco = lpcoView;
			lpco.CLP_Type = "8000";
			lpco.CLP_HolderType = LPCOHolderPartyTypeCodes.Codes.Importer;
			lpco.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Importer;

			AssertEquals("TestOrgName", lpco.CLP_HolderName);
			AssertEquals("Eric Cartman", lpco.CLP_HolderContactName);
			AssertEquals("xxx@yyy.com", lpco.CLP_HolderContactEmail);
			AssertEquals("02512345678", lpco.CLP_HolderContactPhone);

			AssertEquals("TestOrgName", lpco.CLP_ApplicantName);
			AssertEquals("Eric Cartman", lpco.CLP_ApplicantContactName);
			AssertEquals("xxx@yyy.com", lpco.CLP_ApplicantContactEmail);
			AssertEquals("02512345678", lpco.CLP_ApplicantContactPhone);

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var loadLpco = newFactory.Load<CusCALPCO>(lpco.LPCO.PK);
			lpcoView = new LPCOView(loadLpco);
			AssertEquals("TestOrgName", lpcoView.CLP_HolderName);
			AssertEquals("Eric Cartman", lpcoView.CLP_HolderContactName);
			AssertEquals("xxx@yyy.com", lpcoView.CLP_HolderContactEmail);
			AssertEquals("02512345678", lpcoView.CLP_HolderContactPhone);

			AssertEquals("TestOrgName", lpcoView.CLP_ApplicantName);
			AssertEquals("Eric Cartman", lpcoView.CLP_ApplicantContactName);
			AssertEquals("xxx@yyy.com", lpcoView.CLP_ApplicantContactEmail);
			AssertEquals("02512345678", lpcoView.CLP_ApplicantContactPhone);
		}

		public void TestCountryOfIssuanceList()
		{
			gACLPCOVIEW.LPCO.CLP_RN_NKIssuanceCountryCode = "GB";
			AssertCallingListValidationOnCountryPropertyDoesNotThrowAnException(gACLPCOVIEW.LPCO.CLP_RN_NKIssuanceCountryCodeInfo);
		}

		public void TestCountryOfOriginList()
		{
			gACLPCOVIEW.LPCO.CLP_RN_NKOriginCountryCode = "US";
			AssertCallingListValidationOnCountryPropertyDoesNotThrowAnException(gACLPCOVIEW.LPCO.CLP_RN_NKOriginCountryCodeInfo);
		}

		public void TestAuthorizationCountryList()
		{
			gACLPCOVIEW.LPCO.CLP_RN_NKAuthorizationCountry = "AU";
			AssertCallingListValidationOnCountryPropertyDoesNotThrowAnException(gACLPCOVIEW.LPCO.CLP_RN_NKAuthorizationCountryInfo);
		}

		void AssertCallingListValidationOnCountryPropertyDoesNotThrowAnException(ZPropertyInfo countryPropertyInfo)
		{
			AssertNoExceptionThrown(
				$"Calling ListValidation on {countryPropertyInfo.Name}",
				() => ListValidation.ErrorIfInvalidCode(countryPropertyInfo));
		}

		public void TestNoExceptionThrownWhenSave()
		{
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();

			var part = Factory.New<OrgSupplierPart>();
			part.FillWithValidTestData();
			part.OP_PartNum = "1234";
			var part1Relation1 = part.RelatedOrganisations.AddNew();
			part1Relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			part1Relation1.OU_OH = importer1.PK;

			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_OP = part.PK;
			pivot.CI_TariffNum = "1111.11.11";
			pivot.CCA_ECCCIndicator = YesNoList.Codes.Yes;

			var partECCC = pivot.ECCCPGAHeader;
			partECCC.CA_WRMProgramInd = YesNoList.Codes.Yes;
			var partLPCOView = partECCC.LPCOViews.AddNew();
			partLPCOView.CLP_Type = "8000";
			partLPCOView.CLP_RefNo = "TEST";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_OH_Importer = importer1.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;

			var invECCC = invoiceLine.ECCCPGAHeader;
			invECCC.CA_WRMProgramInd = YesNoList.Codes.Yes;
			var invLPCOView = invECCC.LPCOViews.AddNew();
			invLPCOView.CLP_Type = "8001";
			invLPCOView.CLP_RefNo = "TEST2";
			Factory.Save();

			invoiceLine.JI_PartNo = "1234";
			invECCC.LPCOViews.RemoveAndDeleteAll();

			AssertNoExceptionThrown(() =>
			{
				Factory.Save();
			});
		}

		public void TestNoDuplicatedLPCOBeAddedWhenSetProductCodeInInvoiceLine()
		{
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();

			var part = Factory.New<OrgSupplierPart>();
			part.FillWithValidTestData();
			part.OP_PartNum = "1234";
			var part1Relation1 = part.RelatedOrganisations.AddNew();
			part1Relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			part1Relation1.OU_OH = importer1.PK;

			CusClassPartPivot pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_OP = part.PK;
			pivot.CI_TariffNum = "1111.11.11";
			pivot.CCA_ECCCIndicator = YesNoList.Codes.Yes;

			var partECCC = pivot.ECCCPGAHeader;
			partECCC.CA_WRMProgramInd = YesNoList.Codes.Yes;
			partECCC.LPCOViews.RemoveAndDeleteAll();
			var partLPCOView = partECCC.LPCOViews.AddNew();
			partLPCOView.CLP_Type = "8000";
			partLPCOView.CLP_RefNo = "TEST";
			Factory.Save();

			AssertEquals("LPCOViews count should be 1", 1, partECCC.LPCOViews.Count);
			AssertEquals("CLP_Type should be 8000", "8000", partECCC.LPCOViews[0].CLP_Type);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_OH_Importer = importer1.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;

			var invECCC = invoiceLine.ECCCPGAHeader;
			invECCC.CA_WRMProgramInd = YesNoList.Codes.Yes;
			invECCC.LPCOViews.RemoveAndDeleteAll();
			var invLPCOView = invECCC.LPCOViews.AddNew();
			invLPCOView.CLP_Type = "8001";
			invLPCOView.CLP_RefNo = "TEST2";
			Factory.Save();
			AssertEquals("LPCOViews count should be 1", 1, invECCC.LPCOViews.Count);
			AssertEquals("CLP_Type should be 8001", "8001", invECCC.LPCOViews[0].CLP_Type);

			var newFactory = new BusinessObjectFactory();
			invoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			invoiceLine.JI_PartNo = "1234";
			invECCC = invoiceLine.ECCCPGAHeader;
			newFactory.Save();

			AssertEquals("LPCOViews count should be 1", 1, invECCC.LPCOViews.Count);
			AssertEquals("CLP_Type should be 8000 which is from product job", "8000", invECCC.LPCOViews[0].CLP_Type);

			AssertEquals("LPCOViews count should be 1", 1, partECCC.LPCOViews.Count);
			AssertEquals("CLP_Type should be 8000 and no duplicated record", "8000", partECCC.LPCOViews[0].CLP_Type);
		}

		public void TestValueCannotBeSetWhenIsOverriddenIsFalse()
		{
			var lpco = Factory.New<CusCALPCO>();
			lpco.CLP_HolderName = "1";
			lpco.CLP_HolderContactEmail = "1";
			lpco.CLP_HolderContactName = "1";
			lpco.CLP_HolderContactPhone = "1";
			lpco.CLP_IsHolderOverridden = true;
			AssertEquals(ZString.Empty, lpco.CLP_HolderName);
			AssertEquals(ZString.Empty, lpco.CLP_HolderContactEmail);
			AssertEquals(ZString.Empty, lpco.CLP_HolderContactName);
			AssertEquals(ZString.Empty, lpco.CLP_HolderContactPhone);
			lpco.CLP_HolderName = "1";
			lpco.CLP_HolderContactEmail = "1";
			lpco.CLP_HolderContactName = "1";
			lpco.CLP_HolderContactPhone = "1";
			AssertEquals("1", lpco.CLP_HolderName);
			AssertEquals("1", lpco.CLP_HolderContactEmail);
			AssertEquals("1", lpco.CLP_HolderContactName);
			AssertEquals("1", lpco.CLP_HolderContactPhone);

			lpco.CLP_ApplicantName = "1";
			lpco.CLP_ApplicantContactEmail = "1";
			lpco.CLP_ApplicantContactName = "1";
			lpco.CLP_ApplicantContactPhone = "1";
			lpco.CLP_IsApplicantOverridden = true;
			AssertEquals(ZString.Empty, lpco.CLP_ApplicantName);
			AssertEquals(ZString.Empty, lpco.CLP_ApplicantContactEmail);
			AssertEquals(ZString.Empty, lpco.CLP_ApplicantContactName);
			AssertEquals(ZString.Empty, lpco.CLP_ApplicantContactPhone);
			lpco.CLP_ApplicantName = "1";
			lpco.CLP_ApplicantContactEmail = "1";
			lpco.CLP_ApplicantContactName = "1";
			lpco.CLP_ApplicantContactPhone = "1";
			AssertEquals("1", lpco.CLP_ApplicantName);
			AssertEquals("1", lpco.CLP_ApplicantContactEmail);
			AssertEquals("1", lpco.CLP_ApplicantContactName);
			AssertEquals("1", lpco.CLP_ApplicantContactPhone);
		}

		public void TestContactInfoShouldBeClearedWhenIsOverriddenIsSetToFalse()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "TestOrgName";
			importer.OH_Code = "TES";
			CreateContactForAllocation(importer, "Eric Cartman", "xxx@yyy.com", "02512345678", OrgConstants.ContactAllocationType.CAPGA);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_OH_Importer = importer.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;

			var gac = invoiceLine.GACPGAHeader;
			gac.CA_AllProgramInd = YesNoList.Codes.Yes;
			var lpcoView = gac.LPCOViews.AddNew();
			lpcoView.CLP_IsApplicantOverridden = true;
			lpcoView.CLP_ApplicantName = "1";
			lpcoView.CLP_ApplicantContactName = "2";
			lpcoView.CLP_ApplicantContactEmail = "3@1.com";
			lpcoView.CLP_ApplicantContactPhone = "4";
			lpcoView.CLP_IsApplicantOverridden = false;
			lpcoView.CLP_IsHolderOverridden = true;
			lpcoView.CLP_HolderName = "5";
			lpcoView.CLP_HolderContactName = "6";
			lpcoView.CLP_HolderContactEmail = "7@1.com";
			lpcoView.CLP_HolderContactPhone = "8";
			lpcoView.CLP_IsHolderOverridden = false;
			Factory.Save();

			var lpco = (new BusinessObjectFactory()).Load<CusCALPCOForTest>(lpcoView.LPCO.PK);
			AssertEquals(ZString.Empty, lpco.CLP_ApplicantName);
			AssertEquals(ZString.Empty, lpco.CLP_ApplicantContactName);
			AssertEquals(ZString.Empty, lpco.CLP_ApplicantContactEmail);
			AssertEquals(ZString.Empty, lpco.CLP_ApplicantContactPhone);
			AssertEquals(ZString.Empty, lpco.CLP_HolderName);
			AssertEquals(ZString.Empty, lpco.CLP_HolderContactName);
			AssertEquals(ZString.Empty, lpco.CLP_HolderContactEmail);
			AssertEquals(ZString.Empty, lpco.CLP_HolderContactPhone);
		}

		class CusCALPCOForTest : AutoCusCALPCO
		{
			public CusCALPCOForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = "Y";
			this.gACCHeader = invoiceLine.GACPGAHeader;
			this.gACLPCOVIEW = gACCHeader.LPCOViews.AddNew();
		}

		public void TestClearPartyNameWhenClearOrgCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Centilitre;
			invoiceLine.CA_GACInd = "Y";

			var gacPGAHeader = invoiceLine.GACPGAHeader;
			var lpcoView = gacPGAHeader.LPCOViews.AddNew();
			var lpco = lpcoView.LPCO;
			lpco.CLP_HolderType = LPCOHolderPartyTypeCodes.Codes.Other;

			var testOrg = Factory.New<OrgHeader>();
			testOrg.OH_FullName = "TEST ENG COMPANY";
			testOrg.MainAddress.OA_Address1 = "TEST ENG ADDRESS 1";
			testOrg.MainAddress.OA_Address2 = "TEST ENG ADDRESS 2";
			testOrg.MainAddress.OA_PostCode = "12345A";
			testOrg.MainAddress.OA_City = "TEST City";
			testOrg.OH_Language = Core.SharedConstants.Languages.EnglishAmerican;

			lpco.LPCOHolderOrgPK = testOrg.PK;
			lpco.CLP_HolderName = "Lilly";

			lpco.LPCOHolderOrgPK = ZGuid.Empty;

			AssertEquals(ZString.Empty, lpco.CLP_HolderName);
			AssertEquals(ZGuid.Empty, lpco.CLP_OA_Holder);
		}

		public void TestNoExceptionThrownWhenGetDeclarationAndLPCOIsDeleted()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_NRCanInd = "Y";
			var nrCanPGAHeader = invoiceLine.NRCanPGAHeader;
			nrCanPGAHeader.CA_RDAProgramInd = "Y";

			var lpcoView = nrCanPGAHeader.LPCOViews.AddNew();
			var lpco = lpcoView.LPCO;
			lpco.CLP_RefNo = "AAA";
			Factory.Save();

			lpco.Delete();
			AssertNoExceptionThrown(() =>
			{
				var dec1 = lpco.Declaration;
			});
		}

		public void TestColumnValidationHumanReadableNames()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "Description";
			invoiceLine.CA_CFIAInd = "Y";
			var cfiaHeader = invoiceLine.CFIAPGAHeader;
			var lpcoView = cfiaHeader.LPCOViews.AddNew();
			var lpco = lpcoView.LPCO;
			AssertEquals("Document Type", lpco.CLP_TypeInfo.HumanReadableName);
			AssertEquals("Ref No", lpco.CLP_RefNoInfo.HumanReadableName);
			AssertEquals("DIF URN", lpco.CLP_DIFRefNumberOrLocationInfo.HumanReadableName);
		}

		LPCOView gACLPCOVIEW;
		GACPGAHeader gACCHeader;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = "Y";
			var header = invoiceLine.GACPGAHeader;
			var lpcoViews = header.LPCOViews.AddNew();
			return lpcoViews.LPCO;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "Description";
			invoiceLine.CA_CFIAInd = "Y";
			var cfiaHeader = invoiceLine.CFIAPGAHeader;
			var lpcoView = cfiaHeader.LPCOViews.AddNew();
			lpcoView.CLP_IsHolderOverridden = true;
			lpcoView.CLP_IsApplicantOverridden = true;
			return lpcoView.LPCO;
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
			var lpco = lpcoView.LPCO;
			lpco.CLP_Type = "5001";
			Factory.Save();
			lpco.Reload();

			lpco.RunPreSaveValidation();
			AssertHasMessageErrorContaining(lpco.CLP_RefNoInfo, MandatoryValidation.YouHaveNotEntered);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			var invoice2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.CA_HCInd = YesNoList.Codes.Yes;

			var hc2 = invoiceLine2.HCPGAHeader;
			hc2.CA_APIProgramInd = YesNoList.Codes.Yes;
			var lpcoView2 = hc2.LPCOViews.AddNew();
			var lpco2 = lpcoView2.LPCO;
			lpco2.CLP_Type = "5001";
			Factory.Save();
			lpco2.Reload();

			lpco2.RunPreSaveValidation();
			AssertNoMessageErrorContaining(lpco2.CLP_RefNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>() { CusCALPCO.Schema.CLP_IsApplicantOverridden, CusCALPCO.Schema.CLP_IsHolderOverridden };
		}

		#endregion
	}
}
