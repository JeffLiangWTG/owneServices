using System;
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
	public class FEAuthRequestBuilderTest : TestCaseWithFactory
	{
		public void TestBuildFEAuthRequest_NullTransactionInfo()
		{
			var builder = (IFEAuthRequestBuilder)new FEAuthRequestBuilder();
			AssertExceptionThrown<ArgumentNullException>("Transaction cant be null", () => builder.BuildFEAuthRequestInfo(null, "", null));
		}

		public void TestBuildFEAuthRequest_WithoutBranchAddress()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var expectedXmlResult = $@"<Auth>
  <Token></Token>
  <Sign></Sign>
  <Cuit></Cuit>
</Auth>";
			var builder = (IFEAuthRequestBuilder)new FEAuthRequestBuilder();
			var actualXmlResult = builder.BuildFEAuthRequestInfo(transactionInfo, String.Empty, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestBuildFEAuthRequest_WithoutAnyRegistrationNumber()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			var expectedXmlResult = $@"<Auth>
  <Token></Token>
  <Sign></Sign>
  <Cuit></Cuit>
</Auth>";
			var builder = (IFEAuthRequestBuilder)new FEAuthRequestBuilder();
			var actualXmlResult = builder.BuildFEAuthRequestInfo(transactionInfo, String.Empty, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestBuildFEAuthRequest_NoMatchingRecordsRegistrationNumber()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.BranchAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType() { Code = OrgCusCodes.DNI };
			regItem1.Value = "29999999";

			var regItem2 = new RegistrationNumber();
			regItem2.CountryOfIssue = new Country() { Code = CountryCodes.Burundi };
			regItem2.Type = new RegistrationNumberType() { Code = OrgCusCodes.CUF };
			regItem2.Value = "50000001047";

			#region Without CountryOfIssue

			var regItem3 = new RegistrationNumber();
			regItem3.Type = new RegistrationNumberType() { Code = OrgCusCodes.CUIT };
			regItem3.Value = "33708031599";

			#endregion

			#region Without Type

			var regItem4 = new RegistrationNumber();
			regItem4.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem4.Value = "7900541";

			#endregion

			#region Without Value

			var regItem5 = new RegistrationNumber();
			regItem5.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem5.Type = new RegistrationNumberType() { Code = OrgCusCodes.DNI };

			#endregion

			transactionInfo.BranchAddress.RegistrationNumberCollection.AddRange(new[] { regItem1, regItem2, regItem3, regItem4, regItem5 });
			var expectedXmlResult = $@"<Auth>
  <Token></Token>
  <Sign></Sign>
  <Cuit></Cuit>
</Auth>";
			var builder = (IFEAuthRequestBuilder)new FEAuthRequestBuilder();
			var actualXmlResult = builder.BuildFEAuthRequestInfo(transactionInfo, String.Empty, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestBuildFEAuthRequest_OneMatchingRecordRegistrationNumber()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.BranchAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType() { Code = OrgCusCodes.CUIT };
			regItem1.Value = "30123456780";
			transactionInfo.BranchAddress.RegistrationNumberCollection.AddRange(new[] { regItem1 });
			var expectedXmlResult = $@"<Auth>
  <Token></Token>
  <Sign></Sign>
  <Cuit>30123456780</Cuit>
</Auth>";
			var builder = (IFEAuthRequestBuilder)new FEAuthRequestBuilder();
			var actualXmlResult = builder.BuildFEAuthRequestInfo(transactionInfo, String.Empty, "30123456780").ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestBuildFEAuthRequest_MoreOneMatchingRecordRegistrationNumber()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.BranchAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.BranchAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>());
			var regItem1 = new RegistrationNumber();
			regItem1.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem1.Type = new RegistrationNumberType() { Code = OrgCusCodes.DNI };
			regItem1.Value = "29999999";

			var regItem2 = new RegistrationNumber();
			regItem2.CountryOfIssue = new Country() { Code = CountryCodes.Burundi };
			regItem2.Type = new RegistrationNumberType() { Code = OrgCusCodes.CUF };
			regItem2.Value = "50000001047";

			var regItem3 = new RegistrationNumber();
			regItem3.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem3.Type = new RegistrationNumberType() { Code = OrgCusCodes.CUIT };
			regItem3.Value = "30123456780";

			var regItem4 = new RegistrationNumber();
			regItem4.CountryOfIssue = new Country() { Code = CountryCodes.Argentina };
			regItem4.Type = new RegistrationNumberType() { Code = OrgCusCodes.CUIT };
			regItem4.Value = "30716762145";

			var regItem5 = new RegistrationNumber();
			regItem5.Type = new RegistrationNumberType() { Code = OrgCusCodes.CUIT };
			regItem5.Value = "33708031599";

			transactionInfo.BranchAddress.RegistrationNumberCollection.AddRange(new[] { regItem1, regItem2, regItem3, regItem4, regItem5 });

			var expectedXmlResult = $@"<Auth>
  <Token></Token>
  <Sign></Sign>
  <Cuit>30123456780</Cuit>
</Auth>";
			var builder = (IFEAuthRequestBuilder)new FEAuthRequestBuilder();
			var actualXmlResult = builder.BuildFEAuthRequestInfo(transactionInfo, String.Empty, "30123456780").ToString();
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

			var expectedXmlResult = $@"<Auth>
  <Token></Token>
  <Sign></Sign>
  <Cuit>30123456783</Cuit>
</Auth>";
			var builder = (IFEAuthRequestBuilder)new FEAuthRequestBuilder();
			var actualXmlResult = builder.BuildFEAuthRequestInfo(transactionInfo, String.Empty, "30-1234 5TXZ678-3").ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}
	}
}
