using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.TestHelpers.Xml;

namespace Enterprise.Accounting.ElectronicMessaging.Italy.Testing
{
	public class CessionarioCommittenteTest : TestCaseWithFactory
	{
		public void TestCessionarioCommittente_RecipientCountryIsItalyAndRecipientOrgCategoryIsNAT()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "123";
			transaction.OrganizationAddress.Address2 = "Third Street";
			transaction.OrganizationAddress.City = "Roma";
			transaction.OrganizationAddress.Postcode = "20758";
			transaction.OrganizationAddress.State = "RM";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.Italy };
			transaction.OrganizationAddress.CompanyName = "Mario Luca Rossi";
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberCodiceFiscale());
			var orgHeader = CreateOrganisationFromUniversalAddress(transaction.OrganizationAddress.Country.Code.Value, transaction.OrganizationAddress.RegistrationNumberCollection, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <CodiceFiscale>0000000000000003</CodiceFiscale>
    <Anagrafica>
      <Nome>Mario</Nome>
      <Cognome>Luca Rossi</Cognome>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>123 Third Street</Indirizzo>
    <CAP>20758</CAP>
    <Comune>Roma</Comune>
    <Provincia>RM</Provincia>
    <Nazione>IT</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.NaturalPersonIndividual, orgHeader).ToString();
		
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientCountryIsItalyAndRecipientOrgCategoryIsNAT_OnlyOneNameIsAvailable()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "123";
			transaction.OrganizationAddress.Address2 = "Third Street";
			transaction.OrganizationAddress.City = "Roma";
			transaction.OrganizationAddress.Postcode = "20758";
			transaction.OrganizationAddress.State = "RM";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.Italy };
			transaction.OrganizationAddress.CompanyName = "Mario";
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberCodiceFiscale());
			var orgHeader = CreateOrganisationFromUniversalAddress(transaction.OrganizationAddress.Country.Code.Value, transaction.OrganizationAddress.RegistrationNumberCollection, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <CodiceFiscale>0000000000000003</CodiceFiscale>
    <Anagrafica>
      <Nome>Mario</Nome>
      <Cognome>Mario</Cognome>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>123 Third Street</Indirizzo>
    <CAP>20758</CAP>
    <Comune>Roma</Comune>
    <Provincia>RM</Provincia>
    <Nazione>IT</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.NaturalPersonIndividual, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientCountryIsItalyAndRecipientOrgCategoryIsNotNAT_IVACodeAvailableAR()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "123";
			transaction.OrganizationAddress.Address2 = "Third Street";
			transaction.OrganizationAddress.City = "Roma";
			transaction.OrganizationAddress.Postcode = "20758";
			transaction.OrganizationAddress.State = "RM";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.Italy };
			transaction.OrganizationAddress.CompanyName = "My Italy Company";
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberIVA());
			var orgHeader = CreateOrganisationFromUniversalAddress(transaction.OrganizationAddress.Country.Code.Value, transaction.OrganizationAddress.RegistrationNumberCollection, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>00000000002</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>My Italy Company</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>123 Third Street</Indirizzo>
    <CAP>20758</CAP>
    <Comune>Roma</Comune>
    <Provincia>RM</Provincia>
    <Nazione>IT</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientCountryIsItalyAndRecipientOrgCategoryIsNotNAT_IVACodeAvailableAP()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var orgProxy = testObjectCreator.CreateOrgHeader("ITPROXY", true, true);
			var company = testObjectCreator.CreateNewCompany("TST", orgProxy: orgProxy);
			company.GC_BusinessRegNo = "00000";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			company.OrgProxy.OH_Category = OrgConstants.Category.Government;

			Factory.Save();

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.Ledger = LedgerTypes.AccountsPayable;
			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress.CompanyName = "Test Company";
			transaction.BranchAddress.Address1 = "A Street";
			transaction.BranchAddress.Address2 = "Building B";
			transaction.BranchAddress.Postcode = "12345";
			transaction.BranchAddress.City = "Test City";
			transaction.BranchAddress.State = "TS";
			transaction.BranchAddress.Country = new Country() { Code = Core.Constants.CountryCodes.Italy };
			transaction.BranchAddress.OrganizationCode = orgProxy.OH_Code;

			transaction.BranchAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.BranchAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberIVA());

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>00000000002</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>Test Company</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>A Street Building B</Indirizzo>
    <CAP>12345</CAP>
    <Comune>Test City</Comune>
    <Provincia>TS</Provincia>
    <Nazione>IT</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgProxy).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientCountryIsItalyAndRecipientOrgCategoryIsNAT_IVAAndCodiceFiscaleAvailableAR()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "123";
			transaction.OrganizationAddress.Address2 = "Third Street";
			transaction.OrganizationAddress.City = "Roma";
			transaction.OrganizationAddress.Postcode = "20758";
			transaction.OrganizationAddress.State = "RM";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.Italy };
			transaction.OrganizationAddress.CompanyName = "Mario Luca Rossi";
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberIVA());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberCodiceFiscale());
			var orgHeader = CreateOrganisationFromUniversalAddress(transaction.OrganizationAddress.Country.Code.Value, transaction.OrganizationAddress.RegistrationNumberCollection, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <CodiceFiscale>0000000000000003</CodiceFiscale>
    <Anagrafica>
      <Nome>Mario</Nome>
      <Cognome>Luca Rossi</Cognome>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>123 Third Street</Indirizzo>
    <CAP>20758</CAP>
    <Comune>Roma</Comune>
    <Provincia>RM</Provincia>
    <Nazione>IT</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.NaturalPersonIndividual, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientCountryIsItalyAndRecipientOrgCategoryIsNAT_IVAAndCodiceFiscaleAvailableAP()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var orgProxy = testObjectCreator.CreateOrgHeader("ITPROXY", true, true);
			var company = testObjectCreator.CreateNewCompany("TST", orgProxy: orgProxy);
			company.GC_BusinessRegNo = "00000";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			company.OrgProxy.OH_Category = OrgConstants.Category.Government;

			Factory.Save();

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.Ledger = LedgerTypes.AccountsPayable;
			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress.CompanyName = "Test Company";
			transaction.BranchAddress.Address1 = "A Street";
			transaction.BranchAddress.Address2 = "Building B";
			transaction.BranchAddress.Postcode = "12345";
			transaction.BranchAddress.City = "Test City";
			transaction.BranchAddress.State = "TS";
			transaction.BranchAddress.Country = new Country() { Code = Core.Constants.CountryCodes.Italy };
			transaction.BranchAddress.OrganizationCode = orgProxy.OH_Code;

			transaction.BranchAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.BranchAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberIVA());
			transaction.BranchAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberCodiceFiscale());

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>00000000002</IdCodice>
    </IdFiscaleIVA>
    <CodiceFiscale>0000000000000003</CodiceFiscale>
    <Anagrafica>
      <Denominazione>Test Company</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>A Street Building B</Indirizzo>
    <CAP>12345</CAP>
    <Comune>Test City</Comune>
    <Provincia>TS</Provincia>
    <Nazione>IT</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgProxy).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientCountryIsItalyAndRecipientOrgCategoryIsNotNAT_IVAAndCodiceFiscaleAvailable_DifferentValues()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "123";
			transaction.OrganizationAddress.Address2 = "Third Street";
			transaction.OrganizationAddress.City = "Roma";
			transaction.OrganizationAddress.Postcode = "20758";
			transaction.OrganizationAddress.State = "RM";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.Italy };
			transaction.OrganizationAddress.CompanyName = "My Italy Company";
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberIVA());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberCodiceFiscale());
			var orgHeader = CreateOrganisationFromUniversalAddress(transaction.OrganizationAddress.Country.Code.Value, transaction.OrganizationAddress.RegistrationNumberCollection, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>00000000002</IdCodice>
    </IdFiscaleIVA>
    <CodiceFiscale>0000000000000003</CodiceFiscale>
    <Anagrafica>
      <Denominazione>My Italy Company</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>123 Third Street</Indirizzo>
    <CAP>20758</CAP>
    <Comune>Roma</Comune>
    <Provincia>RM</Provincia>
    <Nazione>IT</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientCountryIsItalyAndRecipientOrgCategoryIsNotNAT_IVAAndCodiceFiscaleAvailable_SameValuesAR()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "123";
			transaction.OrganizationAddress.Address2 = "Third Street";
			transaction.OrganizationAddress.City = "Roma";
			transaction.OrganizationAddress.Postcode = "20758";
			transaction.OrganizationAddress.State = "RM";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.Italy };
			transaction.OrganizationAddress.CompanyName = "My Italy Company";
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberIVA());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberCodiceFiscale_SameAsIVA());
			var orgHeader = CreateOrganisationFromUniversalAddress(transaction.OrganizationAddress.Country.Code.Value, transaction.OrganizationAddress.RegistrationNumberCollection, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>00000000002</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>My Italy Company</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>123 Third Street</Indirizzo>
    <CAP>20758</CAP>
    <Comune>Roma</Comune>
    <Provincia>RM</Provincia>
    <Nazione>IT</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientCountryIsItalyAndRecipientOrgCategoryIsNotNAT_IVAAndCodiceFiscaleAvailable_SameValuesAP()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var orgProxy = testObjectCreator.CreateOrgHeader("ITPROXY", true, true);
			var company = testObjectCreator.CreateNewCompany("TST", orgProxy: orgProxy);
			company.GC_BusinessRegNo = "00000";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			company.OrgProxy.OH_Category = OrgConstants.Category.Government;

			Factory.Save();

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.Ledger = LedgerTypes.AccountsPayable;
			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress.CompanyName = "Test Company";
			transaction.BranchAddress.Address1 = "A Street";
			transaction.BranchAddress.Address2 = "Building B";
			transaction.BranchAddress.Postcode = "12345";
			transaction.BranchAddress.City = "Test City";
			transaction.BranchAddress.State = "TS";
			transaction.BranchAddress.Country = new Country() { Code = Core.Constants.CountryCodes.Italy };
			transaction.BranchAddress.OrganizationCode = orgProxy.OH_Code;

			transaction.BranchAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.BranchAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberIVA());
			transaction.BranchAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberCodiceFiscale_SameAsIVA());

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>00000000002</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>Test Company</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>A Street Building B</Indirizzo>
    <CAP>12345</CAP>
    <Comune>Test City</Comune>
    <Provincia>TS</Provincia>
    <Nazione>IT</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgProxy).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientCountryIsItalyAndHasItalianIVAAndGBRAndGCR()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "123";
			transaction.OrganizationAddress.Address2 = "Third Street";
			transaction.OrganizationAddress.City = "Roma";
			transaction.OrganizationAddress.Postcode = "20758";
			transaction.OrganizationAddress.State = "RM";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.Italy };
			transaction.OrganizationAddress.CompanyName = "My Italy Company";
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberIVA());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberGovBusinessCode_GBR(Core.Constants.CountryCodes.Italy));
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberCorporateCode_GCR(Core.Constants.CountryCodes.Italy));
			var orgHeader = CreateOrganisationFromUniversalAddress(transaction.OrganizationAddress.Country.Code.Value, transaction.OrganizationAddress.RegistrationNumberCollection, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>00000000002</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>My Italy Company</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>123 Third Street</Indirizzo>
    <CAP>20758</CAP>
    <Comune>Roma</Comune>
    <Provincia>RM</Provincia>
    <Nazione>IT</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientCountryIsItalyAndHasItalianGBRAndGCR()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "123";
			transaction.OrganizationAddress.Address2 = "Third Street";
			transaction.OrganizationAddress.City = "Roma";
			transaction.OrganizationAddress.Postcode = "20758";
			transaction.OrganizationAddress.State = "RM";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.Italy };
			transaction.OrganizationAddress.CompanyName = "My Italy Company";
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberGovBusinessCode_GBR(Core.Constants.CountryCodes.Italy));
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberCorporateCode_GCR(Core.Constants.CountryCodes.Italy));
			var orgHeader = CreateOrganisationFromUniversalAddress(transaction.OrganizationAddress.Country.Code.Value, transaction.OrganizationAddress.RegistrationNumberCollection, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>0000000000000004</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>My Italy Company</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>123 Third Street</Indirizzo>
    <CAP>20758</CAP>
    <Comune>Roma</Comune>
    <Provincia>RM</Provincia>
    <Nazione>IT</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientCountryIsItalyAndHasItalianGCR()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "123";
			transaction.OrganizationAddress.Address2 = "Third Street";
			transaction.OrganizationAddress.City = "Roma";
			transaction.OrganizationAddress.Postcode = "20758";
			transaction.OrganizationAddress.State = "RM";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.Italy };
			transaction.OrganizationAddress.CompanyName = "My Italy Company";
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberCorporateCode_GCR(Core.Constants.CountryCodes.Italy));
			var orgHeader = CreateOrganisationFromUniversalAddress(transaction.OrganizationAddress.Country.Code.Value, transaction.OrganizationAddress.RegistrationNumberCollection, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>0000000000000005</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>My Italy Company</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>123 Third Street</Indirizzo>
    <CAP>20758</CAP>
    <Comune>Roma</Comune>
    <Provincia>RM</Provincia>
    <Nazione>IT</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientCountryIsItalyAndRecipientOrgCategoryIsNotNAT_IVACodeNotAvailable_CodiceFiscaleAvailable()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "123";
			transaction.OrganizationAddress.Address2 = "Third Street";
			transaction.OrganizationAddress.City = "Roma";
			transaction.OrganizationAddress.Postcode = "20758";
			transaction.OrganizationAddress.State = "RM";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.Italy };
			transaction.OrganizationAddress.CompanyName = "My Italy Company";
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberCodiceFiscale());
			var orgHeader = CreateOrganisationFromUniversalAddress(transaction.OrganizationAddress.Country.Code.Value, transaction.OrganizationAddress.RegistrationNumberCollection, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>MyItalyCompany</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>My Italy Company</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>123 Third Street</Indirizzo>
    <CAP>20758</CAP>
    <Comune>Roma</Comune>
    <Provincia>RM</Provincia>
    <Nazione>IT</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientCountryIsItalyAndRecipientOrgCategoryIsNotNAT_IVACodeAndCODCodeNotAvailable()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "123";
			transaction.OrganizationAddress.Address2 = "Third Street";
			transaction.OrganizationAddress.City = "Roma";
			transaction.OrganizationAddress.Postcode = "20758";
			transaction.OrganizationAddress.State = "RM";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.Italy };
			transaction.OrganizationAddress.CompanyName = "My Italy Company";
			var orgHeader = CreateOrganisationFromUniversalAddress(transaction.OrganizationAddress.Country.Code.Value, null, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>MyItalyCompany</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>My Italy Company</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>123 Third Street</Indirizzo>
    <CAP>20758</CAP>
    <Comune>Roma</Comune>
    <Provincia>RM</Provincia>
    <Nazione>IT</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientCountryIsItaly_RegistrationCodesFromUSAndITAvailableForGlobalITAccount()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "123";
			transaction.OrganizationAddress.Address2 = "Third Street";
			transaction.OrganizationAddress.City = "Roma";
			transaction.OrganizationAddress.Postcode = "20758";
			transaction.OrganizationAddress.State = "RM";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.Italy };
			transaction.OrganizationAddress.CompanyName = "My Italy Company";
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberGovBusinessCode_GBR(Core.Constants.CountryCodes.Italy));
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberCorporateCode_GCR(Core.Constants.CountryCodes.UnitedStates));
			var orgHeader = CreateOrganisationFromUniversalAddress(Core.Constants.CountryCodes.Italy, transaction.OrganizationAddress.RegistrationNumberCollection, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>0000000000000004</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>My Italy Company</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>123 Third Street</Indirizzo>
    <CAP>20758</CAP>
    <Comune>Roma</Comune>
    <Provincia>RM</Provincia>
    <Nazione>IT</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientCountryIsNotItaly_HasUSAndITRegistrationsForGlobalOrg()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "300";
			transaction.OrganizationAddress.Address2 = "Main Street";
			transaction.OrganizationAddress.City = "Seattle";
			transaction.OrganizationAddress.Postcode = "98104";
			transaction.OrganizationAddress.State = "WA";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.UnitedStates };
			transaction.OrganizationAddress.CompanyName = "James Brown";
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberGovBusinessCode_GBR(Core.Constants.CountryCodes.UnitedStates));
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberCorporateCode_GCR(Core.Constants.CountryCodes.Italy));
			var orgHeader = CreateOrganisationFromUniversalAddress(Core.Constants.CountryCodes.Italy, transaction.OrganizationAddress.RegistrationNumberCollection, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>US</IdPaese>
      <IdCodice>0000000000000004</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Nome>James</Nome>
      <Cognome>Brown</Cognome>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>300 Main Street</Indirizzo>
    <CAP>98104</CAP>
    <Comune>Seattle</Comune>
    <Nazione>US</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.NaturalPersonIndividual, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientCountryIsNotItaly_HasUSAndITRegistrationsForNonGlobalOrg()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "300";
			transaction.OrganizationAddress.Address2 = "Main Street";
			transaction.OrganizationAddress.City = "Seattle";
			transaction.OrganizationAddress.Postcode = "98104";
			transaction.OrganizationAddress.State = "WA";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.UnitedStates };
			transaction.OrganizationAddress.CompanyName = "James Brown";
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberGovBusinessCode_GBR(Core.Constants.CountryCodes.UnitedStates));
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberCorporateCode_GCR(Core.Constants.CountryCodes.Italy));
			var orgHeader = CreateOrganisationFromUniversalAddress(Core.Constants.CountryCodes.Italy, transaction.OrganizationAddress.RegistrationNumberCollection, isGlobalAccount: false);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>0000000000000005</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Nome>James</Nome>
      <Cognome>Brown</Cognome>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>300 Main Street</Indirizzo>
    <CAP>98104</CAP>
    <Comune>Seattle</Comune>
    <Nazione>US</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.NaturalPersonIndividual, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientCountryIsNotItaly_RegistrationTypeSupportedForOrgCountry()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "300";
			transaction.OrganizationAddress.Address2 = "Main Street";
			transaction.OrganizationAddress.City = "Seattle";
			transaction.OrganizationAddress.Postcode = "98104";
			transaction.OrganizationAddress.State = "WA";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.UnitedStates };
			transaction.OrganizationAddress.CompanyName = "My USA Company";
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberGovBusinessCode_GBR(Core.Constants.CountryCodes.UnitedStates));
			var orgHeader = CreateOrganisationFromUniversalAddress(transaction.OrganizationAddress.Country.Code.Value, transaction.OrganizationAddress.RegistrationNumberCollection, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>US</IdPaese>
      <IdCodice>0000000000000004</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>My USA Company</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>300 Main Street</Indirizzo>
    <CAP>98104</CAP>
    <Comune>Seattle</Comune>
    <Nazione>US</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientCountryIsNotItaly_RegistrationTypeUnsupportedForOrgCountry()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "300";
			transaction.OrganizationAddress.Address2 = "Main Street";
			transaction.OrganizationAddress.City = "Seattle";
			transaction.OrganizationAddress.Postcode = "98104";
			transaction.OrganizationAddress.State = "WA";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.UnitedStates };
			transaction.OrganizationAddress.CompanyName = "My USA Company";
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberIVA(Core.Constants.CountryCodes.UnitedStates));
			var orgHeader = CreateOrganisationFromUniversalAddress(transaction.OrganizationAddress.Country.Code.Value, transaction.OrganizationAddress.RegistrationNumberCollection, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>US</IdPaese>
      <IdCodice>MyUSACompany</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>My USA Company</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>300 Main Street</Indirizzo>
    <CAP>98104</CAP>
    <Comune>Seattle</Comune>
    <Nazione>US</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientCountryIsNotItaly_IVACodeNotAvailable_NoOtherRegistrationNumbersAvailable()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "300";
			transaction.OrganizationAddress.Address2 = "Main Street";
			transaction.OrganizationAddress.City = "Seattle";
			transaction.OrganizationAddress.Postcode = "98104";
			transaction.OrganizationAddress.State = "WA";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.UnitedStates };
			transaction.OrganizationAddress.CompanyName = "This is my really really long USA Company Name";
			var orgHeader = CreateOrganisationFromUniversalAddress(transaction.OrganizationAddress.Country.Code.Value, null, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>US</IdPaese>
      <IdCodice>ThisismyreallyreallylongUSAC</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>This is my really really long USA Company Name</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>300 Main Street</Indirizzo>
    <CAP>98104</CAP>
    <Comune>Seattle</Comune>
    <Nazione>US</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientCountryIsNotItaly_AnotherRegistrationNumberAvailableFromDifferentCountry()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "300";
			transaction.OrganizationAddress.Address2 = "Main Street";
			transaction.OrganizationAddress.City = "Seattle";
			transaction.OrganizationAddress.Postcode = "98104";
			transaction.OrganizationAddress.State = "WA";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.UnitedStates };
			transaction.OrganizationAddress.CompanyName = "My USA Company";

			var abnRegCode = CreateOrgRegistrationNumber(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, Core.Constants.CountryCodes.Australia, "53004085616");
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(abnRegCode);
			var orgHeader = CreateOrganisationFromUniversalAddress(transaction.OrganizationAddress.Country.Code.Value, transaction.OrganizationAddress.RegistrationNumberCollection, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>US</IdPaese>
      <IdCodice>MyUSACompany</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>My USA Company</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>300 Main Street</Indirizzo>
    <CAP>98104</CAP>
    <Comune>Seattle</Comune>
    <Nazione>US</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientCountryIsNotItaly_VariousRegistrationsAvailableFromDifferentCountries()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "300";
			transaction.OrganizationAddress.Address2 = "Main Street";
			transaction.OrganizationAddress.City = "Seattle";
			transaction.OrganizationAddress.Postcode = "98104";
			transaction.OrganizationAddress.State = "WA";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.UnitedStates };
			transaction.OrganizationAddress.CompanyName = "My USA Company";

			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumber(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, Core.Constants.CountryCodes.Australia, "53004085616"));
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberCorporateCode_GCR(Core.Constants.CountryCodes.Italy));
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberCorporateCode_GCR(Core.Constants.CountryCodes.UnitedStates));
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberGovBusinessCode_GBR(Core.Constants.CountryCodes.UnitedStates));
			var orgHeader = CreateOrganisationFromUniversalAddress(transaction.OrganizationAddress.Country.Code.Value, transaction.OrganizationAddress.RegistrationNumberCollection, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>US</IdPaese>
      <IdCodice>0000000000000004</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>My USA Company</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>300 Main Street</Indirizzo>
    <CAP>98104</CAP>
    <Comune>Seattle</Comune>
    <Nazione>US</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_NoRecipentCountryAndNoRegistrationNumbersAR()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "300";
			transaction.OrganizationAddress.Address2 = "Main Street";
			transaction.OrganizationAddress.City = "Seattle";
			transaction.OrganizationAddress.Postcode = "98104";
			transaction.OrganizationAddress.State = "WA";
			transaction.OrganizationAddress.CompanyName = "My USA Company";

			var orgHeader = CreateOrganisationFromUniversalAddress(Core.Constants.CountryCodes.UnitedStates, null, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>US</IdPaese>
      <IdCodice>MyUSACompany</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>My USA Company</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>300 Main Street</Indirizzo>
    <CAP>98104</CAP>
    <Comune>Seattle</Comune>
    <Nazione></Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_NoRecipentCountryAndNoRegistrationNumbersAP()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var orgProxy = testObjectCreator.CreateOrgHeader("ITPROXY", true, true);
			var company = testObjectCreator.CreateNewCompany("TST", orgProxy: orgProxy);
			company.GC_BusinessRegNo = "00000";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			company.OrgProxy.OH_Category = OrgConstants.Category.Government;

			Factory.Save();

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.Ledger = LedgerTypes.AccountsPayable;
			transaction.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.BranchAddress.CompanyName = "Test Company";
			transaction.BranchAddress.Address1 = "A Street";
			transaction.BranchAddress.Address2 = "Building B";
			transaction.BranchAddress.Postcode = "12345";
			transaction.BranchAddress.City = "Test City";
			transaction.BranchAddress.State = "TS";
			transaction.BranchAddress.Country = new Country() { Code = Core.Constants.CountryCodes.Italy };
			transaction.BranchAddress.OrganizationCode = orgProxy.OH_Code;

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice />
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>Test Company</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>A Street Building B</Indirizzo>
    <CAP>12345</CAP>
    <Comune>Test City</Comune>
    <Provincia>TS</Provincia>
    <Nazione>IT</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgProxy).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientZipCodeIsLongerThan5Characters()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "63";
			transaction.OrganizationAddress.Address2 = "Renmin Lu";
			transaction.OrganizationAddress.City = "Qingdao Shi";
			transaction.OrganizationAddress.Postcode = "266033";
			transaction.OrganizationAddress.State = "SD";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.China };
			transaction.OrganizationAddress.CompanyName = "My China Company";
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberVAT(Core.Constants.CountryCodes.China));
			var orgHeader = CreateOrganisationFromUniversalAddress(transaction.OrganizationAddress.Country.Code.Value, transaction.OrganizationAddress.RegistrationNumberCollection, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>CN</IdPaese>
      <IdCodice>00000000006</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>My China Company</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>63 Renmin Lu</Indirizzo>
    <CAP>26603</CAP>
    <Comune>Qingdao Shi</Comune>
    <Nazione>CN</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientZipCodeIsLessThan5Characters()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "Strickstrasse";
			transaction.OrganizationAddress.Address2 = "28";
			transaction.OrganizationAddress.City = "Zuerich";
			transaction.OrganizationAddress.Postcode = "8022";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.Switzerland };
			transaction.OrganizationAddress.CompanyName = "My Switzerland Company";
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberVAT(Core.Constants.CountryCodes.Switzerland));
			var orgHeader = CreateOrganisationFromUniversalAddress(transaction.OrganizationAddress.Country.Code.Value, transaction.OrganizationAddress.RegistrationNumberCollection, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>CH</IdPaese>
      <IdCodice>00000000006</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>My Switzerland Company</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>Strickstrasse 28</Indirizzo>
    <CAP>08022</CAP>
    <Comune>Zuerich</Comune>
    <Nazione>CH</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCessionarioCommittente_RecipientZipCodeHasAlphabeticalCharacters()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Address1 = "Ardenham Court";
			transaction.OrganizationAddress.Address2 = "Oxford Road";
			transaction.OrganizationAddress.City = "Aylesbury";
			transaction.OrganizationAddress.State = "BM";
			transaction.OrganizationAddress.Postcode = "HP19 3EQ";
			transaction.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.UnitedKingdom };
			transaction.OrganizationAddress.CompanyName = "My United Kingdom Company";
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(CreateOrgRegistrationNumberVAT(Core.Constants.CountryCodes.UnitedKingdom));
			var orgHeader = CreateOrganisationFromUniversalAddress(transaction.OrganizationAddress.Country.Code.Value, transaction.OrganizationAddress.RegistrationNumberCollection, isGlobalAccount: true);

			var expectedXmlResult = $@"
<CessionarioCommittente>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>GB</IdPaese>
      <IdCodice>00000000006</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>My United Kingdom Company</Denominazione>
    </Anagrafica>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>Ardenham Court Oxford Road</Indirizzo>
    <CAP>00000</CAP>
    <Comune>Aylesbury</Comune>
    <Nazione>GB</Nazione>
  </Sede>
</CessionarioCommittente>";

			var actualXmlResult = new CessionarioCommittente().BuildXML(transaction, OrgConstants.Category.Business, orgHeader).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public static UniversalDataBuss.DataObjects.Universal.RegistrationNumber CreateOrgRegistrationNumberIVA(string countryCode = Core.Constants.CountryCodes.Italy)
		{
			return CreateOrgRegistrationNumber(OrgCusCode.CodeTypes.IVA, countryCode, "00000000002");
		}
		UniversalDataBuss.DataObjects.Universal.RegistrationNumber CreateOrgRegistrationNumberVAT(string countryCode)
		{
			return CreateOrgRegistrationNumber(OrgCusCode.CodeTypes.VATCode, countryCode, "00000000006");
		}

		public static UniversalDataBuss.DataObjects.Universal.RegistrationNumber CreateOrgRegistrationNumberCodiceFiscale(string countryCode = Core.Constants.CountryCodes.Italy)
		{
			return CreateOrgRegistrationNumber(ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale, countryCode, "0000000000000003");
		}

		UniversalDataBuss.DataObjects.Universal.RegistrationNumber CreateOrgRegistrationNumberCodiceFiscale_SameAsIVA(string countryCode = Core.Constants.CountryCodes.Italy)
		{
			return CreateOrgRegistrationNumber(ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale, countryCode, "00000000002");
		}

		UniversalDataBuss.DataObjects.Universal.RegistrationNumber CreateOrgRegistrationNumberGovBusinessCode_GBR(string countryCode)
		{
			return CreateOrgRegistrationNumber(OrgCusCode.CodeTypes.GovBusinessCode, countryCode, "0000000000000004");
		}
		UniversalDataBuss.DataObjects.Universal.RegistrationNumber CreateOrgRegistrationNumberCorporateCode_GCR(string countryCode)
		{
			return CreateOrgRegistrationNumber(OrgCusCode.CodeTypes.CorporationCode, countryCode, "0000000000000005");
		}

		public static UniversalDataBuss.DataObjects.Universal.RegistrationNumber CreateOrgRegistrationNumber(string registrationType, string countryCode, string regValue)
		{
			var registrationNumber = new UniversalDataBuss.DataObjects.Universal.RegistrationNumber();
			registrationNumber.Type = new RegistrationNumberType() { Code = registrationType };
			registrationNumber.Value = regValue;
			registrationNumber.CountryOfIssue = new Country() { Code = countryCode };
			return registrationNumber;
		}

		OrgHeader CreateOrganisationFromUniversalAddress(ZString orgCountryCode, IEnumerable<UniversalDataBuss.DataObjects.Universal.RegistrationNumber> customsNumbers, bool isGlobalAccount)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "TST";
			foreach (var cn in customsNumbers ?? Enumerable.Empty<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>())
			{
				orgHeader.CustomsCodes.AddNew(cn.Type.Code.Value, cn.Value.Value, cn.CountryOfIssue.Code.Value);
			}
			orgHeader.OH_RL_NKClosestPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, orgCountryCode)).Code;
			orgHeader.OH_IsGlobalAccount = isGlobalAccount;
			return orgHeader;
		}
	}
}
