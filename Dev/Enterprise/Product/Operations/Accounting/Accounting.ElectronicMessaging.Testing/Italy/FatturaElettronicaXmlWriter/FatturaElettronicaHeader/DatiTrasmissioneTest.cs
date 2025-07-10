using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.TestHelpers.Xml;
using static Enterprise.Core.Constants;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Accounting.ElectronicMessaging.Italy.Testing
{
	public class DatiTrasmissioneTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestBuildXMLWithSameRegistrationCodes()
		{
			var factory = new BusinessObjectFactory();
			var address = factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "Test Address";

			var address1 = factory.NewWithValidTestData<OrgAddress>();
			address1.OA_Code = "Test Address1";

			var cusCode = factory.NewWithValidTestData<OrgCusCode>();
			cusCode.OK_CodeType = ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail;
			cusCode.OK_RN_NKCodeCountry = CountryCodes.Italy;
			cusCode.OK_OA_PremisesAddress = address.PK;
			cusCode.OK_CustomsRegNo = "test@wisetechglobal.com";

			var cusCode1 = factory.NewWithValidTestData<OrgCusCode>();
			cusCode1.OK_CodeType = ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail;
			cusCode1.OK_RN_NKCodeCountry = CountryCodes.Italy;
			cusCode1.OK_OA_PremisesAddress = address1.PK;
			cusCode1.OK_CustomsRegNo = "testueser@wisetechglobal.com";

			var cusCode2 = factory.NewWithValidTestData<OrgCusCode>();
			cusCode2.OK_CodeType = ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail;
			cusCode2.OK_RN_NKCodeCountry = CountryCodes.Italy;
			cusCode2.OK_CustomsRegNo = "empty@wisetechglobal.com";

			factory.Save();

			var regNumber = CreateOrgRegistrationNumber(ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail, CountryCodes.Italy, OrgConstants.Category.Business);
			regNumber.Value = "test@wisetechglobal.com";
			var regNumber1 = CreateOrgRegistrationNumber(ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail, CountryCodes.Italy, OrgConstants.Category.Business);
			var regNumber2 = CreateOrgRegistrationNumber(ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail, CountryCodes.Italy, OrgConstants.Category.Business);
			regNumber2.Value = "empty@wisetechglobal.com";

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.AddressShortCode = "Test Address";
			transaction.OrganizationAddress.Country = new Country() { Code = CountryCodes.Italy };
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(regNumber);
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(regNumber1);
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(regNumber2);

			var expectedXmlResult = $@"
<DatiTrasmissione>
  <IdTrasmittente>
    <IdPaese>IT</IdPaese>
    <IdCodice>13149600150</IdCodice>
  </IdTrasmittente>
  <ProgressivoInvio>PLACEHOLDER</ProgressivoInvio>
  <FormatoTrasmissione>FPR12</FormatoTrasmissione>
  <CodiceDestinatario>0000000</CodiceDestinatario>
  <PECDestinatario>test@wisetechglobal.com</PECDestinatario>
</DatiTrasmissione>";

			var actualXmlResult = new DatiTrasmissione().BuildXML(transaction, OrgConstants.Category.Business).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);

			cusCode.Delete();
			factory.Save();
			transaction.OrganizationAddress.RegistrationNumberCollection.Remove(regNumber);

			expectedXmlResult = $@"
<DatiTrasmissione>
  <IdTrasmittente>
    <IdPaese>IT</IdPaese>
    <IdCodice>13149600150</IdCodice>
  </IdTrasmittente>
  <ProgressivoInvio>PLACEHOLDER</ProgressivoInvio>
  <FormatoTrasmissione>FPR12</FormatoTrasmissione>
  <CodiceDestinatario>0000000</CodiceDestinatario>
  <PECDestinatario>empty@wisetechglobal.com</PECDestinatario>
</DatiTrasmissione>";

			actualXmlResult = new DatiTrasmissione().BuildXML(transaction, OrgConstants.Category.Business).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);

			transaction.OrganizationAddress.AddressShortCode = null;

			expectedXmlResult = $@"
<DatiTrasmissione>
  <IdTrasmittente>
    <IdPaese>IT</IdPaese>
    <IdCodice>13149600150</IdCodice>
  </IdTrasmittente>
  <ProgressivoInvio>PLACEHOLDER</ProgressivoInvio>
  <FormatoTrasmissione>FPR12</FormatoTrasmissione>
  <CodiceDestinatario>0000000</CodiceDestinatario>
  <PECDestinatario>empty@wisetechglobal.com</PECDestinatario>
</DatiTrasmissione>";

			actualXmlResult = new DatiTrasmissione().BuildXML(transaction, OrgConstants.Category.Business).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiTrasmissione_ForPayablesTransactions()
		{
			var cuuRegNumber = CreateOrgRegistrationNumber(ItalyOrgCusCodeInfo.OrgCusCodes.CUU, CountryCodes.Italy, OrgConstants.Category.Business);

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.Ledger = LedgerTypes.AccountsPayable;
			transaction.TransactionType = TransactionType.INV;
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(cuuRegNumber);
			transaction.OrganizationAddress.Country = new Country() { Code = CountryCodes.Italy };

			var expectedXmlResult = $@"
<DatiTrasmissione>
  <IdTrasmittente>
    <IdPaese>IT</IdPaese>
    <IdCodice>13149600150</IdCodice>
  </IdTrasmittente>
  <ProgressivoInvio>PLACEHOLDER</ProgressivoInvio>
  <FormatoTrasmissione>FPR12</FormatoTrasmissione>
  <CodiceDestinatario>0000000</CodiceDestinatario>
</DatiTrasmissione>";

			var actualXmlResult = new DatiTrasmissione().BuildXML(transaction, OrgConstants.Category.Business).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiTrasmissione_RecipientOrgCategoryIsGOVAndRecipieantCountryIsItaly()
		{
			var cuuRegNumber = CreateOrgRegistrationNumber(ItalyOrgCusCodeInfo.OrgCusCodes.CUU, CountryCodes.Italy, OrgConstants.Category.Government);

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(cuuRegNumber);
			transaction.OrganizationAddress.Country = new Country() { Code = CountryCodes.Italy };

			var expectedXmlResult = $@"
<DatiTrasmissione>
  <IdTrasmittente>
    <IdPaese>IT</IdPaese>
    <IdCodice>13149600150</IdCodice>
  </IdTrasmittente>
  <ProgressivoInvio>PLACEHOLDER</ProgressivoInvio>
  <FormatoTrasmissione>FPA12</FormatoTrasmissione>
  <CodiceDestinatario>123456</CodiceDestinatario>
</DatiTrasmissione>";

			var actualXmlResult = new DatiTrasmissione().BuildXML(transaction, OrgConstants.Category.Government).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiTrasmissione_RecipientOrgCategoryIsGOVAndRecipieantCountryIsItaly_CUURegistrationNumberNotAvailable()
		{
			var recipientOrgCategory = OrgConstants.Category.Government;
			var pecDestinarioEmailAddress = CreateOrgRegistrationNumber(ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail, CountryCodes.Italy, recipientOrgCategory);

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Country = new Country() { Code = CountryCodes.Italy };
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(pecDestinarioEmailAddress);

			var expectedXmlResult = $@"
<DatiTrasmissione>
  <IdTrasmittente>
    <IdPaese>IT</IdPaese>
    <IdCodice>13149600150</IdCodice>
  </IdTrasmittente>
  <ProgressivoInvio>PLACEHOLDER</ProgressivoInvio>
  <FormatoTrasmissione>FPA12</FormatoTrasmissione>
  <CodiceDestinatario>0000000</CodiceDestinatario>
  <PECDestinatario>testueser@wisetechglobal.com</PECDestinatario>
</DatiTrasmissione>";

			var actualXmlResult = new DatiTrasmissione().BuildXML(transaction, recipientOrgCategory).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiTrasmissione_RecipientOrgCategoryIsGOVAndRecipieantCountryIsItaly_CUURegistrationNumberIssuedByNonItalyCountry()
		{
			var recipientOrgCategory = OrgConstants.Category.Government;
			var cuuRegNumber = CreateOrgRegistrationNumber(ItalyOrgCusCodeInfo.OrgCusCodes.CUU, CountryCodes.Australia, recipientOrgCategory);
			var pecDestinarioEmailAddress = CreateOrgRegistrationNumber(ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail, CountryCodes.Italy, recipientOrgCategory);

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(cuuRegNumber);
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(pecDestinarioEmailAddress);
			transaction.OrganizationAddress.Country = new Country() { Code = CountryCodes.Italy };

			var expectedXmlResult = $@"
<DatiTrasmissione>
  <IdTrasmittente>
    <IdPaese>IT</IdPaese>
    <IdCodice>13149600150</IdCodice>
  </IdTrasmittente>
  <ProgressivoInvio>PLACEHOLDER</ProgressivoInvio>
  <FormatoTrasmissione>FPA12</FormatoTrasmissione>
  <CodiceDestinatario>0000000</CodiceDestinatario>
  <PECDestinatario>testueser@wisetechglobal.com</PECDestinatario>
</DatiTrasmissione>";

			var actualXmlResult = new DatiTrasmissione().BuildXML(transaction, recipientOrgCategory).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiTrasmissione_RecipientOrgCategoryIsBUSAndRecipieantCountryIsItaly()
		{
			var cuuRegNumber = CreateOrgRegistrationNumber(ItalyOrgCusCodeInfo.OrgCusCodes.CUU, CountryCodes.Italy, OrgConstants.Category.Business);

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(cuuRegNumber);
			transaction.OrganizationAddress.Country = new Country() { Code = CountryCodes.Italy };

			var pecDestinarioEmailAddress = CreateOrgRegistrationNumber(ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail, CountryCodes.Italy, OrgConstants.Category.Business);
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(pecDestinarioEmailAddress);

			var expectedXmlResult = $@"
<DatiTrasmissione>
  <IdTrasmittente>
    <IdPaese>IT</IdPaese>
    <IdCodice>13149600150</IdCodice>
  </IdTrasmittente>
  <ProgressivoInvio>PLACEHOLDER</ProgressivoInvio>
  <FormatoTrasmissione>FPR12</FormatoTrasmissione>
  <CodiceDestinatario>1234567</CodiceDestinatario>
</DatiTrasmissione>";

			var actualXmlResult = new DatiTrasmissione().BuildXML(transaction, OrgConstants.Category.Business).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiTrasmissione_RecipientOrgCategoryIsBUSAndRecipieantCountryIsItaly_CUURegistrationNumberNotAvailable()
		{
			var recipientOrgCategory = OrgConstants.Category.Business;
			var pecDestinarioEmailAddress = CreateOrgRegistrationNumber(ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail, CountryCodes.Italy, recipientOrgCategory);

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Country = new Country() { Code = CountryCodes.Italy };
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(pecDestinarioEmailAddress);

			var expectedXmlResult = $@"
<DatiTrasmissione>
  <IdTrasmittente>
    <IdPaese>IT</IdPaese>
    <IdCodice>13149600150</IdCodice>
  </IdTrasmittente>
  <ProgressivoInvio>PLACEHOLDER</ProgressivoInvio>
  <FormatoTrasmissione>FPR12</FormatoTrasmissione>
  <CodiceDestinatario>0000000</CodiceDestinatario>
  <PECDestinatario>testueser@wisetechglobal.com</PECDestinatario>
</DatiTrasmissione>";

			var actualXmlResult = new DatiTrasmissione().BuildXML(transaction, recipientOrgCategory).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiTrasmissione_RecipientOrgCategoryIsBUSAndRecipieantCountryIsItaly_CUURegistrationNumberIssuedByNonItalyCountry()
		{
			var recipientOrgCategory = OrgConstants.Category.Business;
			var cuuRegNumber = CreateOrgRegistrationNumber(ItalyOrgCusCodeInfo.OrgCusCodes.CUU, CountryCodes.Australia, recipientOrgCategory);
			var pecDestinarioEmailAddress = CreateOrgRegistrationNumber(ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail, CountryCodes.Italy, recipientOrgCategory);

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(cuuRegNumber);
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(pecDestinarioEmailAddress);
			transaction.OrganizationAddress.Country = new Country() { Code = CountryCodes.Italy };

			var expectedXmlResult = $@"
<DatiTrasmissione>
  <IdTrasmittente>
    <IdPaese>IT</IdPaese>
    <IdCodice>13149600150</IdCodice>
  </IdTrasmittente>
  <ProgressivoInvio>PLACEHOLDER</ProgressivoInvio>
  <FormatoTrasmissione>FPR12</FormatoTrasmissione>
  <CodiceDestinatario>0000000</CodiceDestinatario>
  <PECDestinatario>testueser@wisetechglobal.com</PECDestinatario>
</DatiTrasmissione>";

			var actualXmlResult = new DatiTrasmissione().BuildXML(transaction, recipientOrgCategory).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiTrasmissione_RecipientOrgCategoryIsBUSAndRecipieantCountryIsNotItaly()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			var transOrgAddr = transaction.OrganizationAddress;
			transOrgAddr.Country = new Country() { Code = CountryCodes.Australia };

			var expectedXmlResult = @"
<DatiTrasmissione>
  <IdTrasmittente>
    <IdPaese>IT</IdPaese>
    <IdCodice>13149600150</IdCodice>
  </IdTrasmittente>
  <ProgressivoInvio>PLACEHOLDER</ProgressivoInvio>
  <FormatoTrasmissione>FPR12</FormatoTrasmissione>
  <CodiceDestinatario>{0}</CodiceDestinatario>
</DatiTrasmissione>";

			var builder = new DatiTrasmissione();
			var actualXmlResult = builder.BuildXML(transaction, OrgConstants.Category.Business).ToString();
			XmlComparison.CompareAndAssertXml(string.Format(expectedXmlResult, "XXXXXXX"), actualXmlResult);

			transOrgAddr.Country = new Country() { Code = CountryCodes.SanMarino };
			actualXmlResult = builder.BuildXML(transaction, OrgConstants.Category.Business).ToString();
			XmlComparison.CompareAndAssertXml(string.Format(expectedXmlResult, "2R4GTO8"), actualXmlResult);
		}

		public void TestDatiTrasmissione_RecipientOrgCategoryIsNATAndRecipieantCountryIsItaly()
		{
			var recipientOrgCategory = OrgConstants.Category.NaturalPersonIndividual;
			var pecDestinarioEmailAddress = CreateOrgRegistrationNumber(ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail, CountryCodes.Italy, recipientOrgCategory);

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Country = new Country() { Code = CountryCodes.Italy };
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(pecDestinarioEmailAddress);

			var expectedXmlResult = $@"
<DatiTrasmissione>
  <IdTrasmittente>
    <IdPaese>IT</IdPaese>
    <IdCodice>13149600150</IdCodice>
  </IdTrasmittente>
  <ProgressivoInvio>PLACEHOLDER</ProgressivoInvio>
  <FormatoTrasmissione>FPR12</FormatoTrasmissione>
  <CodiceDestinatario>0000000</CodiceDestinatario>
  <PECDestinatario>testueser@wisetechglobal.com</PECDestinatario>
</DatiTrasmissione>";

			var actualXmlResult = new DatiTrasmissione().BuildXML(transaction, recipientOrgCategory).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);

			transaction.OrganizationAddress.RegistrationNumberCollection.Remove(pecDestinarioEmailAddress);
			expectedXmlResult = $@"
<DatiTrasmissione>
  <IdTrasmittente>
    <IdPaese>IT</IdPaese>
    <IdCodice>13149600150</IdCodice>
  </IdTrasmittente>
  <ProgressivoInvio>PLACEHOLDER</ProgressivoInvio>
  <FormatoTrasmissione>FPR12</FormatoTrasmissione>
  <CodiceDestinatario>0000000</CodiceDestinatario>
</DatiTrasmissione>";

			actualXmlResult = new DatiTrasmissione().BuildXML(transaction, recipientOrgCategory).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiTrasmissione_RecipientOrgCategoryIsNATAndRecipieantCountryIsNotItaly()
		{
			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Country = new Country() { Code = CountryCodes.Australia };

			var expectedXmlResult = $@"
<DatiTrasmissione>
  <IdTrasmittente>
    <IdPaese>IT</IdPaese>
    <IdCodice>13149600150</IdCodice>
  </IdTrasmittente>
  <ProgressivoInvio>PLACEHOLDER</ProgressivoInvio>
  <FormatoTrasmissione>FPR12</FormatoTrasmissione>
  <CodiceDestinatario>XXXXXXX</CodiceDestinatario>
</DatiTrasmissione>";

			var actualXmlResult = new DatiTrasmissione().BuildXML(transaction, OrgConstants.Category.NaturalPersonIndividual).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiTrasmissione_RecipientOrgCategoryIsEmpty()
		{
			var recipientOrgCategory = ZString.Empty;
			var pecDestinarioEmailAddress = CreateOrgRegistrationNumber(ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail, CountryCodes.Italy, recipientOrgCategory);

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.Country = new Country() { Code = CountryCodes.Italy };
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(pecDestinarioEmailAddress);

			var expectedXmlResult = $@"
<DatiTrasmissione>
  <IdTrasmittente>
    <IdPaese>IT</IdPaese>
    <IdCodice>13149600150</IdCodice>
  </IdTrasmittente>
  <ProgressivoInvio>PLACEHOLDER</ProgressivoInvio>
  <FormatoTrasmissione>FPR12</FormatoTrasmissione>
  <CodiceDestinatario>0000000</CodiceDestinatario>
  <PECDestinatario>testueser@wisetechglobal.com</PECDestinatario>
</DatiTrasmissione>";

			var actualXmlResult = new DatiTrasmissione().BuildXML(transaction, recipientOrgCategory).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestDatiTrasmissione_RecipientOrgCategoryIsGOVAndRecipieantCountryIsNotItaly()
		{
			var cuuRegNumber = CreateOrgRegistrationNumber(ItalyOrgCusCodeInfo.OrgCusCodes.CUU, CountryCodes.Italy, OrgConstants.Category.Government);

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.OrganizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			transaction.OrganizationAddress.RegistrationNumberCollection.Add(cuuRegNumber);
			transaction.OrganizationAddress.Country = new Country() { Code = CountryCodes.Australia };

			var expectedXmlResult = $@"
<DatiTrasmissione>
  <IdTrasmittente>
    <IdPaese>IT</IdPaese>
    <IdCodice>13149600150</IdCodice>
  </IdTrasmittente>
  <ProgressivoInvio>PLACEHOLDER</ProgressivoInvio>
  <FormatoTrasmissione>FPR12</FormatoTrasmissione>
  <CodiceDestinatario>XXXXXXX</CodiceDestinatario>
</DatiTrasmissione>";

			var actualXmlResult = new DatiTrasmissione().BuildXML(transaction, OrgConstants.Category.Government).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		RegistrationNumber CreateOrgRegistrationNumber(string registrationType, string countryCode, string receipientOrgCategory)
		{
			var registrationNumberType = new RegistrationNumberType();
			registrationNumberType.Code = registrationType;
			var registrationNumber = new RegistrationNumber();
			registrationNumber.Type = registrationNumberType;
			if (registrationType == ItalyOrgCusCodeInfo.OrgCusCodes.CertifiedEmail)
			{
				registrationNumber.Value = "testueser@wisetechglobal.com";
			}
			else
			{
				registrationNumber.Value = (countryCode == CountryCodes.Italy) ?
									(receipientOrgCategory == OrgConstants.Category.Government) ? "123456" : "1234567" :
									"XXXXXXX";
			}
			registrationNumber.CountryOfIssue = new Country() { Code = countryCode };
			return registrationNumber;
		}
	}
}
