using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.DE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CFCPEDHeaderProvider))]
	sealed class CFCPEDHeaderProviderTest : MonthlyClosingDecHeaderProviderAbstractTest<CFCPEDHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new CFCPEDHeaderProvider(null, string.Empty));
		}

		public void TestInputTaxDeductionFlag()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declaration.CRD_OA_DeclarantAddress = declarant.MainAddress.PK;
			var orgImpAddInfo = (DEOrgImpAddInfo)declarant.CountryData.ImpAddInfo;
			orgImpAddInfo.ZO_VATClaimBack = YesNoList.Codes.Yes;

			AssertEquals(true, Provider.InputTaxDeductionFlag);
		}

		public void TestInputTaxDeductionFlag_CompanyIsNotDE()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declaration.CRD_OA_DeclarantAddress = declarant.MainAddress.PK;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				AssertNoExceptionThrown(() => _ = Provider.InputTaxDeductionFlag);
			}
		}

		public void TestTaxOffice_SEL()
		{
			SetDeclarantAndRepresentative(RepresentationTypeList.Codes._1Self);
			AssertEquals("DE000666", Provider.TaxOffice);
		}

		public void TestTaxOffice_DIR()
		{
			SetDeclarantAndRepresentative(RepresentationTypeList.Codes._2Direct);
			AssertEquals("DE000777", Provider.TaxOffice);
		}

		public void TestTaxOffice_IND()
		{
			SetDeclarantAndRepresentative(RepresentationTypeList.Codes._3Indirect);
			AssertEquals("DE000666", Provider.TaxOffice);
		}

		public void TestTaxOffice_None()
		{
			AssertNull(Provider.TaxOffice);
		}

		public void TestMandateReference()
		{
			var header = Factory.New<CusAuthorisationHeader>();
			header.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			var rule = header.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.MandateReference;
			rule.CPR_ValueFrom = "MANDATEREFERENCE";
			declaration.CRD_CPH_ReconClearanceAuthorisation = header.PK;

			AssertEquals("MANDATEREFERENCE", Provider.MandateReference);
		}

		public void TestBodies()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var entryLine1 = Factory.New<CusEntryLine>();
			entryLine1.CL_CH = entryHeader.PK;
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = Factory.New<CusEntryLine>();
			entryLine2.CL_CH = entryHeader.PK;
			entryLine2.CL_LineNumber = 2;
			var invoice1 = Factory.New<JobComInvoiceHeader>();
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			var invoice2 = Factory.New<JobComInvoiceHeader>();
			var line2 = invoice2.JobComInvoiceLines.AddNew();
			var reconEntry1 = declaration.CusReconEntries.AddNew();
			reconEntry1.CRE_CH_OriginalEntry = entryHeader.PK;
			var reconEntryLine1 = reconEntry1.CusReconEntryLines.AddNew();
			reconEntryLine1.CRL_OriginalEntryLineNumber = 1;
			var reconEntry2 = declaration.CusReconEntries.AddNew();
			reconEntry2.CRE_CH_OriginalEntry = entryHeader.PK;
			var reconEntryLine2 = reconEntry2.CusReconEntryLines.AddNew();
			reconEntryLine2.CRL_OriginalEntryLineNumber = 2;
			AssertEquals(2, Provider.Bodies.Count);
		}

		public void TestBodies_NoLines()
		{
			AssertEquals(0, Provider.Bodies.Count);
		}

		protected override void SetUp()
		{
			declaration = Factory.NewWithValidTestData<CusReconDeclaration>();
		}

		protected override CFCPEDHeaderProvider GetProvider() => new CFCPEDHeaderProvider(declaration, string.Empty);

		void SetDeclarantAndRepresentative(string declarantType)
		{
			declaration.CRD_DeclarantType = declarantType;
			var declarantAddress = GetOrgWithEORNumberAndEORIBranch("EOR1", "EBS1");
			declarantAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Greece;
			declarantAddress.Header.CustomsCodes.AddNew(OrgCusCode.GreeceCodeTypes.AFM, "08154711", Constants.CountryCodes.Greece);
			declarantAddress.Header.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice, "DE000666", Constants.CountryCodes.Germany);
			AddDefermentAccountToOrg(declarantAddress.Header, "123456", "EOR1_123456_BIN");
			AddDefermentAccountToOrg(declarantAddress.Header, "123457", "EOR1_123457_BIN");
			declaration.CRD_OA_DeclarantAddress = declarantAddress.PK;
			var representativeAddress = GetOrgWithEORNumberAndEORIBranch("EOR2", "EBS2");
			representativeAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Greece;
			representativeAddress.Header.CustomsCodes.AddNew(OrgCusCode.GreeceCodeTypes.AFM, "08154712", Constants.CountryCodes.Greece);
			representativeAddress.Header.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice, "DE000777", Constants.CountryCodes.Germany);
			AddDefermentAccountToOrg(representativeAddress.Header, "123456", "EOR2_123456_BIN");
			AddDefermentAccountToOrg(representativeAddress.Header, "123457", "EOR2_123457_BIN");
			declaration.CRD_OA_RepresentativeAddress = representativeAddress.PK;
		}

		void AddDefermentAccountToOrg(OrgHeader org, string accountNumber, string bin)
		{
			var account = org.AddDefermentAccountNumber("E", accountNumber);
			account.CZ_Type = "10";
			account.CZ_Issuer = "F";
			account.DecryptedPassword = bin;
		}

		new ICFCPEDHeader Provider => base.Provider;
	}
}
