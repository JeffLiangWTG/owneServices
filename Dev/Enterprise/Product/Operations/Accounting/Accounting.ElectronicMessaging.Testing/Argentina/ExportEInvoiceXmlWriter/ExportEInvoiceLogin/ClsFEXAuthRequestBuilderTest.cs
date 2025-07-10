using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using WTG.TestHelpers.Xml;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.ArgentinaOrgCusCodeInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina.Testing
{
	class ClsFEXAuthRequestBuilderTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			AssertType<ClsFEXAuthRequestBuilder>(new ExportEInvoiceXmlBuilder().ClsFEXAuthRequestBuilder_ExposedForTestOnly);
		}

		public void TestBuildClsFEXAuthRequest_WithoutBranchAddress()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var expectedXmlResult = $@"<Auth xmlns=""http://ar.gov.afip.dif.fexv1/"">
  <Token></Token>
  <Sign></Sign>
  <Cuit></Cuit>
</Auth>";
			var builder = (IClsFEXAuthRequestBuilder)new ClsFEXAuthRequestBuilder();
			var actualXmlResult = builder.BuildXML(transactionInfo, "http://ar.gov.afip.dif.fexv1/", string.Empty).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestBuildClsFEXAuthRequest_OneMatchingRecordRegistrationNumber()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.BranchAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType() { Code = OrgCusCodes.CUIT };
			regItem1.Value = "30123456780";
			transactionInfo.BranchAddress.RegistrationNumberCollection.AddRange(new[] { regItem1 });
			var expectedXmlResult = $@"<Auth xmlns=""http://ar.gov.afip.dif.fexv1/"">
  <Token></Token>
  <Sign></Sign>
  <Cuit>30123456780</Cuit>
</Auth>";
			var builder = (IClsFEXAuthRequestBuilder)new ClsFEXAuthRequestBuilder();
			var actualXmlResult = builder.BuildXML(transactionInfo, "http://ar.gov.afip.dif.fexv1/", "30123456780").ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestBuildFEAuthRequest_CleanCuit()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.BranchAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());

			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType() { Code = OrgCusCodes.CUIT };
			regItem1.Value = "30-1234 5TXZ678-3";
			transactionInfo.BranchAddress.RegistrationNumberCollection.AddRange(new[] { regItem1 });

			var expectedXmlResult = $@"<Auth xmlns=""http://ar.gov.afip.dif.fexv1/"">
  <Token></Token>
  <Sign></Sign>
  <Cuit>30123456783</Cuit>
</Auth>";
			var builder = (IClsFEXAuthRequestBuilder)new ClsFEXAuthRequestBuilder();
			var actualXmlResult = builder.BuildXML(transactionInfo, "http://ar.gov.afip.dif.fexv1/", "30-1234 5TXZ678-3").ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}
	}
}
