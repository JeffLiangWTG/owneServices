using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCALPCOLookupsTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var lpco = Factory.New<CusCALPCO>();
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), lpco.Lookups.DocumentTypeCodes.GetType());

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "0003", "0003 DESC", startDate, endDate);
			var attributeNamePGA = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, "PGA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributePGA = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.GAC);
			var attributeNameValuesAllowed = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypeValuesAllowed, "ValuesAllowed", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributeValuesAllowed01 = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameValuesAllowed.ZXE_Name, "YYY");
			var attributeValuesAllowed02 = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameValuesAllowed.ZXE_Name, "ZZZ");
			newFactory.Save();

			lpco.CLP_Type = "0003";
			AssertEquals(0, lpco.Lookups.RefNumbers.Count);

			var gacHeader = Factory.New<GACPGAHeader>();
			lpco = gacHeader.LPCOViews.AddNew().LPCO;
			lpco.CLP_Type = "0003";
			AssertEquals(2, lpco.Lookups.RefNumbers.Count);
		}

		public void TestCFIADocumentTypeCodes()
		{
			var cfia = Factory.New<CFIAPGAHeader>();
			var lpcoView = cfia.LPCOViews.AddNew();
			var lpco = lpcoView.LPCO;
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), lpco.Lookups.DocumentTypeCodes.GetType());
		}

		public void TestDocumentPGATypeCodes()
		{
			Factory.RefreshEnabled = true;

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "0055", "0055 DESC", startDate, endDate);
			var attributeNamePGA = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, "PGA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributeGAC = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.GAC);
			var attributeHC = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.HC);
			var attributeNameValuesAllowed = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypeValuesAllowed, "ValuesAllowed", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributeValuesAllowed01 = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameValuesAllowed.ZXE_Name, "YYY");

			var code2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIALPCOType, "0004", "0004 DESC", startDate, endDate);
			newFactory.Save();

			var gacHeader = Factory.New<GACPGAHeader>();
			var lpcoView = gacHeader.LPCOViews.AddNew();
			var lpco = lpcoView.LPCO;
			AssertEquals(1, lpco.Lookups.DocumentTypeCodes.Count);

			var hcHeader = Factory.New<HCPGAHeader>();
			var lpco2 = hcHeader.LPCOViews.AddNew().LPCO;
			AssertEquals(1, lpco2.Lookups.DocumentTypeCodes.Count);

			var lpco3 = Factory.New<CusCALPCO>();
			AssertEquals(2, lpco3.Lookups.DocumentTypeCodes.Count);
		}

		public void TestRefNumbers_CFIASafeFoodLicenses()
		{
			var importerbuyer = Factory.New<OrgHeader>();
			importerbuyer.OH_Code = "TE#1";
			var impAddInfo = OrgImpAddInfo.Get(importerbuyer);
			var sfLicense1 = impAddInfo.SafeFoodLicenses.AddNew();
			sfLicense1.CY_Code = "BANANA";
			sfLicense1.CY_Data = "BANANADESC";
			var sfLicense2 = impAddInfo.SafeFoodLicenses.AddNew();
			sfLicense2.CY_Code = "APPLE";
			sfLicense2.CY_Data = "APPLEDESC";

			var importerNoLicense = Factory.New<OrgHeader>();
			importerNoLicense.OH_Code = "TE#2";

			var importer3 = Factory.New<OrgHeader>();
			importer3.OH_Code = "TE#3";
			var impAddInfo2 = OrgImpAddInfo.Get(importer3);
			var sfLicense21 = impAddInfo2.SafeFoodLicenses.AddNew();
			sfLicense21.CY_Code = "CORN";
			sfLicense21.CY_Data = "CORNDESC";
			var sfLicense22 = impAddInfo2.SafeFoodLicenses.AddNew();
			sfLicense22.CY_Code = "BAGEL";
			sfLicense22.CY_Data = "BAGELDESC";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importerbuyer.PK;

			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

			invoiceLine.CA_CFIAInd = "Y";
			var cfiaPGAHeader = invoiceLine.CFIAPGAHeader;

			var lpco = cfiaPGAHeader.LPCOViews.AddNew().LPCO;
			lpco.CLP_Type = RegistrationNumberHelper.SafeFoodForCanadiansLicence;
			AssertEquals("2 licenses", 2, lpco.Lookups.RefNumbers.Count);
			AssertEquals("BANANA, APPLE", lpco.Lookups.RefNumbers.CodesAsString);

			declaration.JE_OH_Importer = importerNoLicense.PK;
			AssertEquals("no licenses", 0, lpco.Lookups.RefNumbers.Count);

			declaration.JE_OH_Importer = importer3.PK;
			AssertEquals("2 licenses", 2, lpco.Lookups.RefNumbers.Count);
			AssertEquals("CORN, BAGEL", lpco.Lookups.RefNumbers.CodesAsString);
		}

		public void TestApplicantTypeCodes()
		{
			var declaration = Factory.New<JobDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.CA_GACInd = "Y";
			var gacPGAHeader = invoiceLine.GACPGAHeader;
			gacPGAHeader.CA_AllProgramInd = "Y";

			var lpcoView = gacPGAHeader.LPCOViews.AddNew();
			var lpco = lpcoView.LPCO;
			AssertEquals(typeof(LPCOHolderPartyTypeCodes), lpco.Lookups.LPCOApplicantCodes.GetType());

			var declarationLPCO = declaration.LPCOs.AddNew();
			AssertEquals("IMP, SUP, IOR, OTH", declarationLPCO.Lookups.LPCOApplicantCodes.CodesAsString);
		}

		public void TestUQList()
		{
			var gacPGAHeader = Factory.New<GACPGAHeader>();
			var lpcoView = gacPGAHeader.LPCOViews.AddNew();
			var lpco = lpcoView.LPCO;
			var list = lpco.Lookups.UQList;

			AssertEquals(typeof(CodeDescriptionPairList), list.GetType());

			var expectedList = new CodeDescriptionPairList();
			expectedList.AddRange(Factory.GetCachedValue<IIDUnitOfCountCodeList>());
			expectedList.AddRangeOverwriteIfExists(Factory.GetCachedValue<IIDUnitOfMeasureCodeList>());

			AssertContainsExactElementsInAnyOrder(expectedList, list);
		}

		public void TestDIFDocumentIDList()
		{
			var lpco = Factory.New<CusCALPCO>();
			AssertEquals(typeof(JobRequiredDocumentAddInfoCollection), lpco.Lookups.DIFDocumentIDList.GetType());
		}

		public void TestHolderPartyTypeCodes()
		{
			var declaration = Factory.New<JobDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.CA_GACInd = "Y";
			var gacPGAHeader = invoiceLine.GACPGAHeader;
			gacPGAHeader.CA_AllProgramInd = "Y";

			var lpcoView = gacPGAHeader.LPCOViews.AddNew();
			var lpco = lpcoView.LPCO;
			AssertEquals(typeof(LPCOHolderPartyTypeCodes), lpco.Lookups.HolderPartyTypeCodes.GetType());

			var declarationLPCO = declaration.LPCOs.AddNew();
			AssertEquals("IMP, SUP, IOR, OTH", declarationLPCO.Lookups.HolderPartyTypeCodes.CodesAsString);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			cNSCHeader = Factory.New<CNSCPGAHeader>();
			cNSCHeader.CA_AllProgramInd = "Y";
			cNSCHeader.LPCOViews.AddNew();
		}
		CNSCPGAHeader cNSCHeader;

		#endregion
	}
}
