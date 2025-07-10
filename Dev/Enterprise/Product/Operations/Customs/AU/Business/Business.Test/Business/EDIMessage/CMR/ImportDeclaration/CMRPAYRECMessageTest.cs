using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRPAYRECMessage))]
	sealed class CMRPAYRECMessageTest : CMRImportDeclarationMessageTest
	{
		public void TestGetReportWithWeirdReferenceNumber()
		{
			CMRPAYRECMessage message = Factory.New<CMRPAYRECMessage>();
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "BAlklklk/1";
			message.EM_MessageText = CMRImportDeclarationTestData.PAYREC.Replace("B00122382", "BAlklklk/1");

			ZString report = message.GetReport();
			AssertEquals("EFT Run Number", true, report.Contains("EFT Run Number"));
			AssertEquals("ICS Receipt Number", true, report.Contains("ICS Receipt Number"));
			AssertEquals("Total Other Charges", true, report.Contains("Total Other Charges"));
			AssertEquals("Total Paid Amount", true, report.Contains("Total Paid Amount"));
			AssertEquals("Total Payable Admin", true, report.Contains("Total Payable Admin"));
			AssertEquals("Total Payable Duty", true, report.Contains("Total Payable Duty"));
			AssertEquals("Total Payable GST", true, report.Contains("Total Payable GST"));
			AssertEquals("Total Payable LCT", true, report.Contains("Total Payable LCT"));
			AssertEquals("Total Payable WET", true, report.Contains("Total Payable LCT"));
			AssertEquals("Total WoodLevy", true, report.Contains("Total WoodLevy"));
			AssertEquals("AQIS Processing Charge", true, report.Contains("AQIS Processing Charge"));
			AssertEquals("Declaration Processing Charge", true, report.Contains("Declaration Processing Charge"));
		}

		public void TestGetReport()
		{
			CMRPAYRECMessage message = Factory.New<CMRPAYRECMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.PAYREC;

			ZString email = message.GetReport();
			Assert("Email is not empty", !email.IsEmpty);
			Assert("Email contains payment date", email.Contains("Payment Date"));
			Assert("Email contains Total Other Charges", email.Contains("Total Other Charges"));
			Assert("Email contains Total Paid Amount", email.Contains("Total Paid Amount"));
			Assert("Email contains Total Payable Admin", email.Contains("Total Payable Admin"));
			Assert("Email contains Total Payable Duty", email.Contains("Total Payable Duty"));
			Assert("Email contains Total Payable GST", email.Contains("Total Payable GST"));
			Assert("Email contains Total Payable LCT", email.Contains("Total Payable LCT"));
			Assert("Email contains Total Payable WET", email.Contains("Total Payable WET"));
			Assert("Email contains Total WoodLevy", email.Contains("Total WoodLevy"));
			Assert("Email contains AQIS Processing Charge", email.Contains("AQIS Processing Charge"));
			Assert("Email contains Declaration Processing Charge", email.Contains("Declaration Processing Charge"));
			Assert("Email contains Line Actual Duty", email.Contains("Line Actual Duty"));
			Assert("Email contains AQIS Services Amount", email.Contains("AQIS Services Amount"));
		}

		protected override EDIMessage GetEDIMessage(string reference)
		{
			CMRPAYRECMessageTestClass message = Factory.New<CMRPAYRECMessageTestClass>();
			message.EM_MessageText = CMRImportDeclarationTestData.PAYREC.Replace("B00122382", reference);
			return message;
		}

		protected override BusinessObject GetWrappedObject(string reference)
		{
			CMRPAYRECMessageTestClass message = (CMRPAYRECMessageTestClass)GetEDIMessage(reference);
			return message.GetWrappedObjectTestMethod();
		}
	}
}
