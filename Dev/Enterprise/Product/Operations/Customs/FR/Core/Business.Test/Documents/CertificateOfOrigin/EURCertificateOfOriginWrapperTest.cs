using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Documents.Testing
{
	class EURCertificateOfOriginWrapperTest : TestCaseWithFactory
	{
		public void TestInvoiceLines()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tradeGroupEUSEC = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EUForSafetyAndSecurity, new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.AddCountry(tradeGroupEUSEC, Core.Constants.CountryCodes.Switzerland, new ZDate(1900, 1, 1), new ZDate(2079, 6, 6));
			Factory.Save();

			Assert("Prerequisite: Germany is EU member and as such should be considered SafetyAndSecurity compliant.", Factory.IsCountryConsideredInEuForSafetyAndSecurity(Core.Constants.CountryCodes.Germany));
			Assert("Prerequisite: Switzerland is EUSEC trade group member and as such should be considered SafetyAndSecurity compliant.", Factory.IsCountryConsideredInEuForSafetyAndSecurity(Core.Constants.CountryCodes.Switzerland));

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000000001";
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000000001";
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Switzerland;
			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "0000000001";
			invoiceLine3.JI_CountryOfOrigin = Core.Constants.CountryCodes.Germany;
			var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "0000000001";
			invoiceLine4.JI_CountryOfOrigin = Core.Constants.CountryCodes.EuropeanUnion;

			var wrapper = new EURCertificateOfOriginWrapperForTest(declaration);
			AssertContainsExactElementsInAnyOrder("Invoice lines with country of origin not EUSEC members or EU members (CN for instance) should be skipped.", new EU.Business.Declaration.JobComInvoiceLine[] { invoiceLine2, invoiceLine3, invoiceLine4 }, wrapper.InvoiceLines);
		}

		public void TestCustomsEndorsement()
		{
			var declaration = Factory.New<JobDeclaration>();
			EU.Business.Documents.CertificateOfOrigin.IEURCertificateOfOrigin dataProvider = new EURCertificateOfOriginWrapper(declaration);
			AssertType<FRCustomsEndorsementWrapper>("CustomsEndorsement", dataProvider.CustomsEndorsement);
		}

		public void TestExporterDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			EU.Business.Documents.CertificateOfOrigin.IEURCertificateOfOrigin dataProvider = new EURCertificateOfOriginWrapper(declaration);
			AssertType<FRExporterDeclarationWrapper>("ExporterDeclaration Type", dataProvider.ExporterDeclaration);
		}
		public void TestOriginCountryInFrench_ForTNDZMA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKOrigin = "TNKEB";
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			AssertEquals("OriginCountry", "Tunisie", wrapper.OriginCountry);
		}

		public void TestOriginCountryInFrench_ForOthers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKOrigin = "AUSYD";
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			AssertEquals("OriginCountry", "Australia", wrapper.OriginCountry);
		}

		public void TestDestinationCountryInFrench_ForTNDZMA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKFinalDestination = "TNKEB";
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			AssertEquals("DestinationCountry", "Tunisie", wrapper.DestinationCountry);
		}

		public void TestDestinationCountryInFrench_ForOthers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKFinalDestination = "DEBER";
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			AssertEquals("DestinationCountry", "Germany", wrapper.DestinationCountry);
		}

		public void TestOriginGroupInFrench_ForTNDZMA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKOrigin = "TNKEB";
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			AssertEquals("OriginGroup", ZString.Empty, wrapper.OriginGroup);
		}

		public void TestOriginGroupInFrench_ForOthers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKOrigin = "AUSYD";
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			AssertEquals("OriginGroup", ZString.Empty, wrapper.OriginGroup);
		}

		public void TestDestinationGroupInFrench_ForTNDZMA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKFinalDestination = "TNKEB";
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			AssertEquals("DestinationGroup", "Tunisie", wrapper.DestinationGroup);
		}

		public void TestDestinationGroupInFrench_ForOthers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			AssertEquals("DestinationGroup", "Australia", wrapper.DestinationGroup);
		}

		public void TestImporterAddressInFrench_ForTNDZMA()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "FREDS SUPPLY CO";
			var firstAddress = importer.Addresses.AddNew();
			firstAddress.OA_Address1 = "367 George St";
			firstAddress.OA_City = "Sydney";
			firstAddress.OA_State = "NSW";
			firstAddress.OA_PostCode = "2000";
			firstAddress.OA_RN_NKCountryCode = "AU";

			var secondAddress = importer.Addresses.AddNew();
			secondAddress.OA_Address1 = "367 George St";
			secondAddress.OA_City = "Sydney";
			secondAddress.OA_State = "NSW";
			secondAddress.OA_PostCode = "2000";
			secondAddress.OA_RN_NKCountryCode = "TN";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = secondAddress.PK;

			var wrapper = GetNewWrapperFromDeclaration(declaration);
			AssertEquals("ImporterAddress", "FREDS SUPPLY CO\n367 GEORGE ST\nSYDNEY NSW 2000\nTUNISIE", wrapper.ImporterAddress);
		}

		public void TestImporterAddressInFrench_ForOthers()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "FREDS SUPPLY CO";
			var firstAddress = importer.Addresses.AddNew();
			firstAddress.OA_Address1 = "367 George St";
			firstAddress.OA_City = "Sydney";
			firstAddress.OA_State = "NSW";
			firstAddress.OA_PostCode = "2000";
			firstAddress.OA_RN_NKCountryCode = "TN";

			var secondAddress = importer.Addresses.AddNew();
			secondAddress.OA_Address1 = "367 George St";
			secondAddress.OA_City = "Sydney";
			secondAddress.OA_State = "NSW";
			secondAddress.OA_PostCode = "2000";
			secondAddress.OA_RN_NKCountryCode = "AU";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = secondAddress.PK;

			var wrapper = GetNewWrapperFromDeclaration(declaration);
			AssertEquals("ImporterAddress", "FREDS SUPPLY CO\n367 GEORGE ST\nSYDNEY NSW 2000\nAUSTRALIA", wrapper.ImporterAddress);
		}

		public void TestOriginGroupInFrench_IsUE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKOrigin = "TNKEB";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLines1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLines1.JI_Tariff = "ABC";
			invoiceLines1.JI_CountryOfOrigin = "FR";
			var invoiceLines2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLines2.JI_Tariff = "ABC";
			invoiceLines2.JI_CountryOfOrigin = "IT";
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			AssertEquals("OriginGroup", "UE", wrapper.OriginGroup);
		}

		public void TestOriginGroupInFrench_IsNotUE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKOrigin = "TNKEB";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLines1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLines1.JI_Tariff = "ABC";
			invoiceLines1.JI_CountryOfOrigin = "FR";
			var invoiceLines2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLines2.JI_Tariff = "ABC";
			invoiceLines2.JI_CountryOfOrigin = "FR";
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			AssertEquals("OriginGroup", "FR", wrapper.OriginGroup);
		}

		public void TestOriginGroupInFrench_IsNotUEWithSomeCountryNotAsPartOfEU()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKOrigin = "TNKEB";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLines1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLines1.JI_Tariff = "ABC";
			invoiceLines1.JI_CountryOfOrigin = "FR";
			var invoiceLines2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLines2.JI_Tariff = "ABC";
			invoiceLines2.JI_CountryOfOrigin = "CN";
			var wrapper = GetNewWrapperFromDeclaration(declaration);
			AssertEquals("OriginGroup", "FR", wrapper.OriginGroup);
		}

		EU.Business.Documents.CertificateOfOrigin.IEURCertificateOfOrigin GetNewWrapperFromDeclaration(JobDeclaration declaration) => new EURCertificateOfOriginWrapper(declaration);
	}
	public class EURCertificateOfOriginWrapperForTest : EURCertificateOfOriginWrapper
	{
		public EURCertificateOfOriginWrapperForTest(JobDeclaration declaration) : base(declaration)
		{
		}

		public new IEnumerable<EU.Business.Declaration.JobComInvoiceLine> InvoiceLines => base.InvoiceLines;
	}
}
