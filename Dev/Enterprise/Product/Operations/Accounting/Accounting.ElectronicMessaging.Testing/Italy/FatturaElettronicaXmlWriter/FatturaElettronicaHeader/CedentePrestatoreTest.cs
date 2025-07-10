using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.TestHelpers.Xml;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Italy.Testing
{
	public class CedentePrestatoreTest : TestCaseWithFactory
	{
		public void TestCedentePrestatoreAR()
		{
			var expectedXmlResult = @"
<CedentePrestatore>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>00000000002</IdCodice>
    </IdFiscaleIVA>
    <CodiceFiscale>0000000000000003</CodiceFiscale>
    <Anagrafica>
      <Denominazione>Test Company</Denominazione>
    </Anagrafica>
    <RegimeFiscale>RF01</RegimeFiscale>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>A Street Building B</Indirizzo>
    <CAP>00123</CAP>
    <Comune>Test City</Comune>
    <Provincia>TS</Provincia>
    <Nazione>IT</Nazione>
  </Sede>
</CedentePrestatore>";

			var actualXmlResult = new CedentePrestatore().BuildXML(transactionInfoAR).ToString();

			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCedentePrestatoreAP()
		{
			var expectedXmlResult = @"
<CedentePrestatore>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>00000000002</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>My Italy Company</Denominazione>
    </Anagrafica>
    <RegimeFiscale>RF01</RegimeFiscale>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>123 Third Street</Indirizzo>
    <CAP>20758</CAP>
    <Comune>Roma</Comune>
    <Provincia>RM</Provincia>
    <Nazione>IT</Nazione>
  </Sede>
</CedentePrestatore>";

			var actualXmlResult = new CedentePrestatore().BuildXML(transactionInfoAP, recipientOrgCategory: OrgConstants.Category.Business, orgHeader: orgHeader).ToString();

			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCedentePrestatoreWithRegimeFiscaleAR()
		{
			var expectedXmlResult = @"
<CedentePrestatore>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>00000000002</IdCodice>
    </IdFiscaleIVA>
    <CodiceFiscale>0000000000000003</CodiceFiscale>
    <Anagrafica>
      <Denominazione>Test Company</Denominazione>
    </Anagrafica>
    <RegimeFiscale>RF02</RegimeFiscale>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>A Street Building B</Indirizzo>
    <CAP>00123</CAP>
    <Comune>Test City</Comune>
    <Provincia>TS</Provincia>
    <Nazione>IT</Nazione>
  </Sede>
</CedentePrestatore>";

			string actualXmlResult;

			using (AccountingConfigurationRegistry.Instance.ItalyTaxRegimeID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ItalyTaxRegimeIdTypes.RF02.Code))
			{
				actualXmlResult = new CedentePrestatore().BuildXML(transactionInfoAR).ToString();
			}

			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCedentePrestatoreWithRegimeFiscaleAP()
		{
			var expectedXmlResult = @"
<CedentePrestatore>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>00000000002</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>My Italy Company</Denominazione>
    </Anagrafica>
    <RegimeFiscale>RF02</RegimeFiscale>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>123 Third Street</Indirizzo>
    <CAP>20758</CAP>
    <Comune>Roma</Comune>
    <Provincia>RM</Provincia>
    <Nazione>IT</Nazione>
  </Sede>
</CedentePrestatore>";

			string actualXmlResult;

			using (AccountingConfigurationRegistry.Instance.ItalyTaxRegimeID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ItalyTaxRegimeIdTypes.RF02.Code))
			{
				actualXmlResult = new CedentePrestatore().BuildXML(transactionInfoAP, recipientOrgCategory: OrgConstants.Category.Business, orgHeader: orgHeader).ToString();
			}

			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}
		public void TestCedentePrestatoreWithEmptyDataSource()
		{
			transactionInfoAR = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);

			var expectedXmlResult = @"
<CedentePrestatore>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice />
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione />
    </Anagrafica>
    <RegimeFiscale>RF01</RegimeFiscale>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo />
    <CAP>00000</CAP>
    <Comune />
    <Nazione>IT</Nazione>
  </Sede>
</CedentePrestatore>";
			var actualXmlResult = new CedentePrestatore().BuildXML(transactionInfoAR).ToString();

			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestCedentePrestatoreIVAisEqualToCodiceFiscaleAR()
		{
			int indexToRemove = transactionInfoAR.BranchAddress.RegistrationNumberCollection.FindIndex(reg => reg.CountryOfIssue.Code.Equals(CountryCodes.Italy) && reg.Type.Code.Equals(ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale));
			transactionInfoAR.BranchAddress.RegistrationNumberCollection.RemoveAt(indexToRemove);

			var codiceFiscaleAsIVA = new UniversalDataBuss.DataObjects.Universal.RegistrationNumber();
			codiceFiscaleAsIVA.CountryOfIssue = new Country() { Code = CountryCodes.Italy };
			codiceFiscaleAsIVA.Type = new RegistrationNumberType();
			codiceFiscaleAsIVA.Type.Code = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
			codiceFiscaleAsIVA.Value = "00000000002";

			transactionInfoAR.BranchAddress.RegistrationNumberCollection.Add(codiceFiscaleAsIVA);

			var expectedXmlResult = @"
<CedentePrestatore>
  <DatiAnagrafici>
    <IdFiscaleIVA>
      <IdPaese>IT</IdPaese>
      <IdCodice>00000000002</IdCodice>
    </IdFiscaleIVA>
    <Anagrafica>
      <Denominazione>Test Company</Denominazione>
    </Anagrafica>
    <RegimeFiscale>RF01</RegimeFiscale>
  </DatiAnagrafici>
  <Sede>
    <Indirizzo>A Street Building B</Indirizzo>
    <CAP>00123</CAP>
    <Comune>Test City</Comune>
    <Provincia>TS</Provincia>
    <Nazione>IT</Nazione>
  </Sede>
</CedentePrestatore>";
			var actualXmlResult = new CedentePrestatore().BuildXML(transactionInfoAR).ToString();

			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		protected override void SetUp()
		{
			base.SetUp();

			testObjectCreator = new TestObjectCreator(Factory);

			var orgProxy = testObjectCreator.CreateOrgHeader("ITPROXY", true, true);
			company = testObjectCreator.CreateNewCompany("TST", orgProxy: orgProxy);
			company.GC_BusinessRegNo = "00000";
			company.GC_RN_NKCountryCode = CountryCodes.Italy;
			company.OrgProxy.OH_Category = OrgConstants.Category.Government;

			Factory.Save();

			transactionInfoAR = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfoAR.Ledger = LedgerTypes.AccountsReceivable;
			transactionInfoAR.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfoAR.BranchAddress.CompanyName = "Test Company";
			transactionInfoAR.BranchAddress.Address1 = "A Street";
			transactionInfoAR.BranchAddress.Address2 = "Building B";
			transactionInfoAR.BranchAddress.Postcode = "0123";
			transactionInfoAR.BranchAddress.City = "Test City";
			transactionInfoAR.BranchAddress.State = "TS";
			transactionInfoAR.BranchAddress.Country = new Country() { Code = CountryCodes.Italy };
			transactionInfoAR.BranchAddress.OrganizationCode = orgProxy.OH_Code;

			transactionInfoAR.BranchAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());
			var regItem1 = new UniversalDataBuss.DataObjects.Universal.RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Italy };
			regItem1.Type = new RegistrationNumberType();
			regItem1.Type.Code = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
			regItem1.Value = "0000000000000003";
			var regItem2 = new UniversalDataBuss.DataObjects.Universal.RegistrationNumber();
			regItem2.CountryOfIssue = new Country() { Code = CountryCodes.Italy };
			regItem2.Type = new RegistrationNumberType();
			regItem2.Type.Code = OrgCusCode.CodeTypes.IVA;
			regItem2.Value = "00000000002";
			var regItem3 = new UniversalDataBuss.DataObjects.Universal.RegistrationNumber();
			regItem3.CountryOfIssue = new Country() { Code = CountryCodes.Japan };
			regItem3.Type = new RegistrationNumberType();
			regItem3.Type.Code = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
			regItem3.Value = "00001";
			var regItem4 = new UniversalDataBuss.DataObjects.Universal.RegistrationNumber();
			regItem4.CountryOfIssue = new Country() { Code = CountryCodes.Japan };
			regItem4.Type = new RegistrationNumberType();
			regItem4.Type.Code = OrgCusCode.CodeTypes.IVA;
			regItem4.Value = "00004";
			transactionInfoAR.BranchAddress.RegistrationNumberCollection.AddRange(new [] { regItem3, regItem4, regItem1, regItem2 });

			transactionInfoAP = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfoAP.Ledger = LedgerTypes.AccountsPayable;
			transactionInfoAP.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfoAP.OrganizationAddress.Address1 = "123";
			transactionInfoAP.OrganizationAddress.Address2 = "Third Street";
			transactionInfoAP.OrganizationAddress.City = "Roma";
			transactionInfoAP.OrganizationAddress.Postcode = "20758";
			transactionInfoAP.OrganizationAddress.State = "RM";
			transactionInfoAP.OrganizationAddress.Country = new Country() { Code = Core.Constants.CountryCodes.Italy };
			transactionInfoAP.OrganizationAddress.CompanyName = "My Italy Company";
			transactionInfoAP.OrganizationAddress.SetRegistrationNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>());

			var registrationNumber = new UniversalDataBuss.DataObjects.Universal.RegistrationNumber();
			registrationNumber.Type = new RegistrationNumberType() { Code = OrgCusCode.CodeTypes.IVA };
			registrationNumber.Value = "00000000002";
			registrationNumber.CountryOfIssue = new Country() { Code = Core.Constants.CountryCodes.Italy };

			transactionInfoAP.OrganizationAddress.RegistrationNumberCollection.Add(registrationNumber);

			var orgCountryCode = transactionInfoAP.OrganizationAddress.Country.Code.Value;
			var customsNumbers = transactionInfoAP.OrganizationAddress.RegistrationNumberCollection;
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "TST";
			foreach (var cn in customsNumbers ?? Enumerable.Empty<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>())
			{
				orgHeader.CustomsCodes.AddNew(cn.Type.Code.Value, cn.Value.Value, cn.CountryOfIssue.Code.Value);
			}
			orgHeader.OH_RL_NKClosestPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, orgCountryCode)).Code;
			orgHeader.OH_IsGlobalAccount = true;
		}

		TestObjectCreator testObjectCreator;
		TransactionInfo transactionInfoAR;
		TransactionInfo transactionInfoAP;
		OrgHeader orgHeader;
		GlbCompany company;
	}
}
