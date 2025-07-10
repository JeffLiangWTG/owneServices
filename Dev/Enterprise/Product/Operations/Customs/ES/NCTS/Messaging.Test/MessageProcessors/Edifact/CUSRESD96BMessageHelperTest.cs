using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageProcessors.Testing
{
	[TestedType(typeof(CUSRESD96BMessageHelper))]
	public class CUSRESD96BMessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		const string CUSRES_ResponseMessage = @"UNH+20185637975246+CUSRES:D:96B:UN:AVO006'BGM+AVO+17/00179@6+11'DTM+148:2011201856:201'GIS+4:117:148'GIS+2:120:148'NAD+DT+ESA78587268:167:148'RFF+ABT:20ES009998500102'RFF+AFB:99980000521'UNT+8+20185637975246'UNZ+1+20185637975246'";
		const string CUSRES_ErrorMessage = @"UNH+20183958973985+CUSRES:D:96B:UN:AVI006'BGM+963+YYY00022@2+11'GIS+2:117:148'FTX+AAO+++55110:CABECERA:Other Text:El estado del Tránsito impide la operación.:More Text'FTX+AAO+++55000:Other Error::Other Description.'UNT+5+20183958973985'UNZ+1+20183958973985'";

		#region BGM
		public void TestDocumentMessageName()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Response", "AVO", testHelperResponse.DocumentMessageName);
				AssertEquals("Error", "963", testHelperError.DocumentMessageName);
			});
		}
		#endregion

		#region DTM
		public void TestAdmissionDate()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Response", new ZDateTime(2020, 11, 20, 18, 56, 00), testHelperResponse.AdmissionDate);
				AssertEquals("Error", ZDateTime.Invalid, testHelperError.AdmissionDate);
			});
		}
		#endregion

		#region GIS
		public void TestMessageFunction()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Response", "4", testHelperResponse.MessageFunction);
				AssertEquals("Error", "2", testHelperError.MessageFunction);
			});
		}

		public void TestPreviousSummaryDiscrepancy()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Response", "2", testHelperResponse.PreviousSummaryDiscrepancy);
				AssertEquals("Error", ZString.Empty, testHelperError.PreviousSummaryDiscrepancy);
			});
		}
		#endregion

		#region RFF
		public void TestTransitReferenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Response", "20ES009998500102", testHelperResponse.TransitReferenceNumber);
				AssertEquals("Error", ZString.Empty, testHelperError.TransitReferenceNumber);
			});
		}

		public void TestSummaryReferenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Response", "99980000521", testHelperResponse.SummaryReferenceNumber);
				AssertEquals("Error", ZString.Empty, testHelperError.SummaryReferenceNumber);
			});
		}
		#endregion

		#region FTX
		public void TestFreeTextErrors()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Response", false, testHelperResponse.FreeTextErrors.Any());
				AssertEquals("FreeTextErrors[0].Code", "55110", testHelperError.FreeTextErrors[0].Code);
				AssertEquals("FreeTextErrors[0].Location", "CABECERA.Other Text", testHelperError.FreeTextErrors[0].Location);
				AssertEquals("FreeTextErrors[0].Description", "El estado del Tránsito impide la operación..More Text", testHelperError.FreeTextErrors[0].Description);
				AssertEquals("FreeTextErrors[1].Code", "55000", testHelperError.FreeTextErrors[1].Code);
				AssertEquals("FreeTextErrors[1].Location", "Other Error", testHelperError.FreeTextErrors[1].Location);
				AssertEquals("FreeTextErrors[1].Description", "Other Description.", testHelperError.FreeTextErrors[1].Description);
			});
		}
		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			var testMessage = Factory.NewWithValidTestData<EDIMessage>();
			testMessage.EM_MessageText = CUSRES_ResponseMessage;
			return CUSRESD96BMessageHelper.New(testMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var cusresResponse = Factory.New<EDIMessage>();
			cusresResponse.EM_MessageText = CUSRES_ResponseMessage;
			testHelperResponse = CUSRESD96BMessageHelper.New(cusresResponse);

			var cusresError = Factory.New<EDIMessage>();
			cusresError.EM_MessageText = CUSRES_ErrorMessage;
			testHelperError = CUSRESD96BMessageHelper.New(cusresError);
		}

		CUSRESD96BMessageHelper testHelperResponse;
		CUSRESD96BMessageHelper testHelperError;
	}
}
