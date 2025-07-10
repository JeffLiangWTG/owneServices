using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageProcessors.Testing
{
	[TestedType(typeof(CUSRESV921ESMessageHelper))]
	public class CUSRESV921ESMessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		const string CUSRES_ResponseMessage = @"UNH+07140311062255+CUSRES:1:921:UN:ECS001'BGM+962+@EPRUEBA_CW01+11'NAD+EX+A12345678:167:148'NAD+2+ESA12345678:167:148'DTM+148:1906071403:201'DTM+268:20190905:102'GIS+4:117:148'GIS+24:116::A3'GIS+24:119:A2:1'RFF+ABT:19ES00999910003845'AUT+HDGM4EAWZSTJJ9XQ+LEVA'DTM+204:1906071403:201'UNT+12+07140311062255'";
		const string CUSRES_ErrorMessage = @"UNH+07140311062256+CUSRES:1:921:UN:ECS001'BGM+963+@ERROR_MESSAGE+11'DTM+148:1910101403:201'GIS+2:117:148'FTX+AAO+++00338:PARTIDA(001).CASILLA 44 (001).Tipo de documento:Other Text:Código  documento incorrecto:More Text'FTX+AAO+++55000:Other Error::Other Description.'RFF+ABT:19ES00999910003846'UNT+12+07140311062256'UNZ+1+07140311062256'";
		const string CUSRES_ResponseComplementaryMessage = @"UNH+07140311062257+CUSRES:1:921:UN:ECS001'BGM+964+@EPRUEBA_CW02+11'NAD+1+ES12345678A:167:148'DTM+148:1906072020:201'GIS+5:117:148'GIS+4:118:148'GIS+24:119:A1:2'RFF+ABT:19ES00999910003847'AUT+HDGM4EAWZSTJJ9XQ+LEVA'DTM+204:1906071403:201'AUT+T8G22119A22D0239+T2LF'DTM+204:1906071403:201'UNT+12+07140311062257'UNZ+1+07140311062257'";

		#region BGM
		public void TestDocumentMessageName()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Response", "962", testHelperResponse.DocumentMessageName);
				AssertEquals("Error", "963", testHelperError.DocumentMessageName);
				AssertEquals("ResponseComplementary", "964", testHelperResponseComplementary.DocumentMessageName);
			});
		}

		public void TestUniqueReferenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Response", "@EPRUEBA_CW01", ExportResponse.UniqueReferenceNumber);
				AssertEquals("Error", "@ERROR_MESSAGE", ExportResponseError.UniqueReferenceNumber);
				AssertEquals("ResponseComplementary", "@EPRUEBA_CW02", ExportResponseComplementary.UniqueReferenceNumber);
			});
		}
		#endregion

		#region DTM
		public void TestAdmissionDate()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Response", new ZDateTime(2019, 06, 07, 14, 03, 00), testHelperResponse.AdmissionDate);
				AssertEquals("Error", new ZDateTime(2019, 10, 10, 14, 03, 00), testHelperError.AdmissionDate);
				AssertEquals("ResponseComplementary", new ZDateTime(2019, 06, 07, 20, 20, 00), testHelperResponseComplementary.AdmissionDate);
			});
		}

		public void TestTransitMaxDate()
		{
			AssertEquals(new ZDateTime(2019, 09, 05), testHelperResponse.TransitMaxDate);
		}

		public void TestCSVReleaseCreationDate()
		{
			AssertEquals(new ZDateTime(2019, 06, 07, 14, 03, 00), testHelperResponse.CSVReleaseCreationDate);
		}
		#endregion

		#region GIS
		public void TestMessageFunction()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Response", "4", testHelperResponse.MessageFunction);
				AssertEquals("Error", "2", testHelperError.MessageFunction);
				AssertEquals("ResponseComplementary", "5", testHelperResponseComplementary.MessageFunction);
			});
		}

		public void TestMessageFunctionCAN()
		{
			AssertEquals("4", ExportResponseComplementary.MessageFunctionCAN);
		}

		public void TestCustomsClearanceStatus()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Response", "A2", ExportResponse.CustomsClearanceStatus);
				AssertEquals("ResponseComplementary", "A1", ExportResponseComplementary.CustomsClearanceStatus);
			});
		}

		public void TestPrintActionRequired()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Response", "1", testHelperResponse.PrintActionRequired);
				AssertEquals("ResponseComplementary", "2", testHelperResponseComplementary.PrintActionRequired);
			});
		}
		#endregion

		#region RFF
		public void TestRegistrationNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Response", "19ES00999910003845", testHelperResponse.RegistrationNumber);
				AssertEquals("Error", "19ES00999910003846", testHelperError.RegistrationNumber);
				AssertEquals("ResponseComplementary", "19ES00999910003847", testHelperResponseComplementary.RegistrationNumber);
			});
		}
		#endregion

		#region FTX
		public void TestFreeTextErrors()
		{
			CombineAssertions(() =>
			{
				AssertEquals("FreeTextErrors[0].Code", "00338", testHelperError.FreeTextErrors[0].Code);
				AssertEquals("FreeTextErrors[0].Location", "PARTIDA(001).CASILLA 44 (001).Tipo de documento.Other Text", testHelperError.FreeTextErrors[0].Location);
				AssertEquals("FreeTextErrors[0].Description", "Código  documento incorrecto.More Text", testHelperError.FreeTextErrors[0].Description);
				AssertEquals("FreeTextErrors[1].Code", "55000", testHelperError.FreeTextErrors[1].Code);
				AssertEquals("FreeTextErrors[1].Location", "Other Error", testHelperError.FreeTextErrors[1].Location);
				AssertEquals("FreeTextErrors[1].Description", "Other Description.", testHelperError.FreeTextErrors[1].Description);
			});
		}
		#endregion

		#region AUT
		public void TestCSVReleaseCode()
		{
			AssertEquals("HDGM4EAWZSTJJ9XQ", testHelperResponse.CSVReleaseCode);
		}

		public void TestCSVT2LFCode()
		{
			AssertEquals("T8G22119A22D0239", ExportResponseComplementary.CSVT2LFCode);
		}
		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			var testMessage = Factory.NewWithValidTestData<EDIMessage>();
			testMessage.EM_MessageText = CUSRES_ResponseMessage;
			return CUSRESV921ESMessageHelper.New(testMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var cusresResponse = Factory.New<EDIMessage>();
			cusresResponse.EM_MessageText = CUSRES_ResponseMessage;
			testHelperResponse = CUSRESV921ESMessageHelper.New(cusresResponse);

			var cusresError = Factory.New<EDIMessage>();
			cusresError.EM_MessageText = CUSRES_ErrorMessage;
			testHelperError = CUSRESV921ESMessageHelper.New(cusresError);

			var cusresResponseComplementary = Factory.New<EDIMessage>();
			cusresResponseComplementary.EM_MessageText = CUSRES_ResponseComplementaryMessage;
			testHelperResponseComplementary = CUSRESV921ESMessageHelper.New(cusresResponseComplementary);
		}

		ICUSRESV921ESMessageProvider testHelperResponse;
		ICUSRESV921ESMessageProvider testHelperError;
		ICUSRESV921ESMessageProvider testHelperResponseComplementary;

		IExportResponseMessageProvider ExportResponse => (IExportResponseMessageProvider)testHelperResponse;
		IExportResponseMessageProvider ExportResponseError => (IExportResponseMessageProvider)testHelperError;
		IExportResponseMessageProvider ExportResponseComplementary => (IExportResponseMessageProvider)testHelperResponseComplementary;
	}
}
