using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using WTG.TestHelpers.Xml;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina.Testing
{
	class FECabRequestBuilderTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			AssertType<FECabRequestBuilder>(new LocalEInvoiceXmlBuilder().FeCabRequestBuilder_ExposedForTestOnly);
		}

		public void TestBuildFECabRequest_NullTransactionInfo()
		{
			var builder = (IFECabRequestBuilder)new FECabRequestBuilder();
			AssertExceptionThrown<ArgumentNullException>("Transaction cant be null", () => builder.BuildFECabRequestInfo(null, "", null));
		}

		#region CbteTipo

		public void TestBuildFECabRequest_ElegibleComplianceSubType_CantRegAlwaysIs1()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.ComplianceSubType = "TXA";
			var expectedXmlResult = $@"<FeCabReq>
  <CantReg>1</CantReg>
  <PtoVta></PtoVta>
  <CbteTipo>1</CbteTipo>
</FeCabReq>";
			var builder = (IFECabRequestBuilder)new FECabRequestBuilder();
			var actualXmlResult = builder.BuildFECabRequestInfo(transactionInfo, string.Empty, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestBuildFECabRequest_EmptyComplianceSubType_CantRegAlwaysIs1()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.ComplianceSubType = ZString.Empty;
			var expectedXmlResult = $@"<FeCabReq>
  <CantReg>1</CantReg>
  <PtoVta></PtoVta>
  <CbteTipo></CbteTipo>
</FeCabReq>";
			var builder = (IFECabRequestBuilder)new FECabRequestBuilder();
			var actualXmlResult = builder.BuildFECabRequestInfo(transactionInfo, string.Empty, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestBuildFECabRequest_NotElegibleComplianceSubType_CantRegAlwaysIs1()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.ComplianceSubType = "XXX";
			var expectedXmlResult = $@"<FeCabReq>
  <CantReg>1</CantReg>
  <PtoVta></PtoVta>
  <CbteTipo></CbteTipo>
</FeCabReq>";
			var builder = (IFECabRequestBuilder)new FECabRequestBuilder();
			var actualXmlResult = builder.BuildFECabRequestInfo(transactionInfo, string.Empty, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestBuildFECabRequest_MissingComplianceSubType_CantRegAlwaysIs1()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var expectedXmlResult = $@"<FeCabReq>
  <CantReg>1</CantReg>
  <PtoVta></PtoVta>
  <CbteTipo></CbteTipo>
</FeCabReq>";
			var builder = (IFECabRequestBuilder)new FECabRequestBuilder();
			var actualXmlResult = builder.BuildFECabRequestInfo(transactionInfo, string.Empty, null).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		#endregion

		#region PtoVta

		public void TestPtoVtaTag_WhenMissingTransactionReference()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var expectedXmlResult = $@"
<PtoVta></PtoVta>";

			var builder = (IFECabRequestBuilder)new FECabRequestBuilder();
			var actualXmlResult = builder.BuildFECabRequestInfo(transactionInfo, string.Empty, null).ToString().Replace(" ", "");

			AssertContains(expectedXmlResult, actualXmlResult);
		}

		public void TestPtoVtaTag_WhenTransactionReferenceIsEmpty()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.TransactionReference = " ";

			var expectedXmlResult = $@"
<PtoVta></PtoVta>";

			var builder = (IFECabRequestBuilder)new FECabRequestBuilder();
			var actualXmlResult = builder.BuildFECabRequestInfo(transactionInfo, string.Empty, null).ToString().Replace(" ", "");

			AssertContains(expectedXmlResult, actualXmlResult);
		}

		public void TestPtoVtaTag_FromAccComplianceSequence()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.ComplianceSubType = "TXA";
			transactionInfo.TransactionReference = "001300654089";

			var accComplianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			accComplianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			accComplianceSequence.XD_SequenceClass = "TXA";
			accComplianceSequence.XD_Prefix = "0013";
			accComplianceSequence.XD_MaximumNumberDigits = 8;

			var fECabRequestBuilder = new FECabRequestBuilder();
			var ifECabRequestBuilder = (IFECabRequestBuilder)fECabRequestBuilder;

			var expectedXmlResult = $@"
<PtoVta>0013</PtoVta>";

			var actualXmlResult = ifECabRequestBuilder.BuildFECabRequestInfo(transactionInfo, string.Empty, accComplianceSequence).ToString().Replace(" ", "");
			AssertContains(expectedXmlResult, actualXmlResult);
		}

		public void TestPtoVtaTag_AccComplianceSequenceUnMatch()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.ComplianceSubType = "TXA";
			transactionInfo.TransactionReference = "001300654089";

			var accComplianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			accComplianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			accComplianceSequence.XD_SequenceClass = "TXB";
			accComplianceSequence.XD_Prefix = "0014";
			accComplianceSequence.XD_MaximumNumberDigits = 8;

			var expectedXmlResult = $@"<PtoVta>0014</PtoVta>";

			var fECabRequestBuilder = new FECabRequestBuilder();
			var ifECabRequestBuilder = (IFECabRequestBuilder)fECabRequestBuilder;
			var actualXmlResult = ifECabRequestBuilder.BuildFECabRequestInfo(transactionInfo, string.Empty, accComplianceSequence).ToString().Replace(" ", "");
			AssertContains(expectedXmlResult, actualXmlResult);
		}

		public void TestPtoVtaTag_WhenAccComplianceSequencePrefixIsMissing()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.ComplianceSubType = "TXA";
			transactionInfo.TransactionReference = "000130654089";

			var accComplianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			accComplianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			accComplianceSequence.XD_SequenceClass = "TXA";
			accComplianceSequence.XD_Prefix = null;
			accComplianceSequence.XD_MaximumNumberDigits = 8;

			var fECabRequestBuilder = new FECabRequestBuilder();
			var ifECabRequestBuilder = (IFECabRequestBuilder)fECabRequestBuilder;

			var expectedXmlResult = $@"<PtoVta></PtoVta>";

			var actualXmlResult = ifECabRequestBuilder.BuildFECabRequestInfo(transactionInfo, string.Empty, accComplianceSequence).ToString().Replace(" ", "");
			AssertContains(expectedXmlResult, actualXmlResult);
		}

		public void TestPtoVtaTag_ManualPrefix_ContainsOnlyNumbers()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.ComplianceSubType = "TXA";
			transactionInfo.TransactionReference = "00013";

			var fECabRequestBuilder = new FECabRequestBuilder();
			var ifECabRequestBuilder = (IFECabRequestBuilder)fECabRequestBuilder;

			var expectedXmlResult = $@"
<PtoVta></PtoVta>";

			var actualXmlResult = ifECabRequestBuilder.BuildFECabRequestInfo(transactionInfo, string.Empty, null).ToString().Replace(" ", "");
			AssertContains(expectedXmlResult, actualXmlResult);
		}

		public void TestPtoVtaTag_ComplianceSequence_Prefix_ContainsNumbersAndCharacters()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.ComplianceSubType = "TXA";
			transactionInfo.TransactionReference = "FCA0000004-000002526";

			var accComplianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			accComplianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			accComplianceSequence.XD_SequenceClass = "TXA";
			accComplianceSequence.XD_Prefix = "0000004";
			accComplianceSequence.XD_MaximumNumberDigits = 9;

			var fECabRequestBuilder = new FECabRequestBuilder();
			var ifECabRequestBuilder = (IFECabRequestBuilder)fECabRequestBuilder;

			var expectedXmlResult = $@"
<PtoVta>0000004</PtoVta>";

			var actualXmlResult = ifECabRequestBuilder.BuildFECabRequestInfo(transactionInfo, string.Empty, accComplianceSequence).ToString().Replace(" ", "");
			AssertContains("<PtoVta> must contains only numbers",expectedXmlResult, actualXmlResult);
		}

		public void TestPtoVtaTag_Manual_Prefix_ContainsNumbersAndCharacters()
		{
			TransactionInfo transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transactionInfo.ComplianceSubType = "TXA";
			transactionInfo.TransactionReference = "FCA00004-000002526";

			var fECabRequestBuilder = new FECabRequestBuilder();
			var ifECabRequestBuilder = (IFECabRequestBuilder)fECabRequestBuilder;

			var expectedXmlResult = $@"
<PtoVta></PtoVta>";

			var actualXmlResult = ifECabRequestBuilder.BuildFECabRequestInfo(transactionInfo, string.Empty, null).ToString().Replace(" ", "");
			AssertContains("<PtoVta> must contains only numbers", expectedXmlResult, actualXmlResult);
		}

		#endregion
	}
}
