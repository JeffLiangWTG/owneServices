using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT.Testing
{
	public class Header1_30Test : TestCaseWithFactory
	{
		[TestDate(2022, 9, 20)]
		public void TestHeader1_30()
		{
			var createStaff = Creator.CreateStaff("ZZZ");
			createStaff.GS_FullName = "Test Full Name";
			createStaff.GS_WorkPhone = "18778898160";
			createStaff.GS_EmailAddress = "Jeson.Bai@WiseTechGlobal.com";

			var norwayCompany = Factory.NewWithValidTestData<GlbCompany>();
			norwayCompany.GC_Code = "NOT";
			norwayCompany.GC_Name = "Test NO company";
			norwayCompany.GC_Address1 = "Address 1";
			norwayCompany.GC_Address2 = "Address 2";
			norwayCompany.GC_RN_NKCountryCode = "NO";
			norwayCompany.GC_PostCode = "7654";
			norwayCompany.GC_OH_OrgProxy = Creator.ActiveOrg.PK;
			norwayCompany.GC_City = "Test City";
			norwayCompany.GC_RX_NKLocalCurrency = "USD";

			Factory.Save();

			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			report.ACR_GC_Company = norwayCompany.PK;
			report.ACR_SystemCreateUser = createStaff.GS_Code;
			report.ACR_DateFrom = new ZDate(2022, 9, 20);
			report.ACR_DateTo = new ZDate(2022, 9, 21);

			var govRegistrationCode = report.Company.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GovBusinessCode, "GBR123");
			govRegistrationCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Norway;

			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_IsDefaultReceiptBankAccount = true;
			bankAccount.AB_RX_NKAccountCurrency = report.Company.LocalCurrency.Code;
			bankAccount.AB_GC = report.Company.PK;
			bankAccount.AB_GB = ZGuid.Empty;
			bankAccount.AB_AccountNum = "Acount123";

			Creator.CreateTestPeriods(new ZDateTime(2022, 9, 20));

			var header = new Header1_30();

			Factory.Save();
			var additionalDataCollector = new SAFTAdditionalDataCollector(report, ComplianceReportDataCollectionMode.SAFT1_30, this.Creator.ABIGAS.PK, this.Creator.ABIGAS.OH_Code);
			var result = header.BuildXml(report, additionalData: additionalDataCollector);

			AssertHeader("NA", result.ToString());

			var taxRegistrationCode = report.Company.OrgProxy.CustomsCodes.AddNew(OrgCusCode.NorwayCodeTypes.MVA, "MVA123");
			taxRegistrationCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Norway;

			header = new Header1_30();
			result = header.BuildXml(report, additionalData: additionalDataCollector);

			AssertHeader("MVA123", result.ToString());
		}

		void AssertHeader(string taxRegistrationNumber, string result)
		{
			AssertEquals($@"<Header>
  <AuditFileVersion>1.30</AuditFileVersion>
  <AuditFileCountry>NO</AuditFileCountry>
  <AuditFileDateCreated>2022-09-20</AuditFileDateCreated>
  <SoftwareCompanyName>Wisetech Global Limited</SoftwareCompanyName>
  <SoftwareID>CargoWise</SoftwareID>
  <SoftwareVersion>{ReleaseInfo.Instance.VersionNumber}</SoftwareVersion>
  <Company>
    <RegistrationNumber>GBR123</RegistrationNumber>
    <Name>Test NO company</Name>
    <Address>
      <StreetName>Address 1</StreetName>
      <AdditionalAddressDetail>Address 2</AdditionalAddressDetail>
      <City>Test City</City>
      <PostalCode>7654</PostalCode>
      <Country>NO</Country>
      <AddressType>PostalAddress</AddressType>
    </Address>
    <Contact>
      <ContactPerson>
        <FirstName>NotUsed</FirstName>
        <LastName>Test Full Name</LastName>
      </ContactPerson>
      <Telephone>18778898160</Telephone>
      <Email>Jeson.Bai@WiseTechGlobal.com</Email>
    </Contact>
    <TaxRegistration>
      <TaxRegistrationNumber>{taxRegistrationNumber}</TaxRegistrationNumber>
      <TaxAuthority>Skatteetaten</TaxAuthority>
    </TaxRegistration>
    <BankAccount>
      <BankAccountNumber>Acount123</BankAccountNumber>
    </BankAccount>
  </Company>
  <DefaultCurrencyCode>USD</DefaultCurrencyCode>
  <SelectionCriteria>
    <SelectionStartDate>2022-09-20</SelectionStartDate>
    <SelectionEndDate>2022-09-21</SelectionEndDate>
  </SelectionCriteria>
  <TaxAccountingBasis>A</TaxAccountingBasis>
</Header>", result);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var mockFeatureManager = new Mock<IFeatureControlManager>();
			var mockFeatureData = new Mock<IFeatureData>();
			mockFeatureManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingSAFT13Report, CancellationToken.None)).Returns(Task.FromResult(mockFeatureData.Object));
			ObjectFactory.Substitute(mockFeatureManager.Object);
		}

		protected override void TearDown()
		{
			base.TearDown();
			ObjectFactory.DisposeSubstitutions();
		}

		TestObjectCreator Creator
		{
			get { return fcreator ?? (fcreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fcreator;
	}
}
