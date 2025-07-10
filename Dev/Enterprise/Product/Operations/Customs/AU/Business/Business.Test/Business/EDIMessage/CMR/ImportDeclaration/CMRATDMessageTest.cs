using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRATDMessage))]
	sealed class CMRATDMessageTest : CMRImportDeclarationMessageTest
	{
		public void TestGetReport()
		{
			ZString expectedEmailBody =
				@"Status: FINALISED
Status Description: FINALISED

Authority to Deal Date Issued: 04-Feb-05
Payment Finalised Date: 04-Feb-05
Authority to Deal Security Code: AAAANNPTY
";
			CMRATDMessage message = Factory.New<CMRATDMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.ATD;
			AssertEquals("Body Of Email", expectedEmailBody, message.GetReport());
		}

		public void TestGetReportWithAuthorityToDealActionReason()
		{
			ZString expectedEmailBody =
				@"Status: FINALISED
Status Description: FINALISED

Authority to Deal Date Issued: 04-Feb-05
Payment Finalised Date: 04-Feb-05
Authority to Deal Security Code: AAAANNPTY

Authority to Deal Action Reason: Authority to Deal Action Reason
";
			CMRATDMessage message = Factory.New<CMRATDMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.ATDWithActionReason;
			AssertEquals("Body Of Email", expectedEmailBody, message.GetReport());
		}

		protected override EDIMessage GetEDIMessage(string reference)
		{
			CMRATDMessageTestClass message = Factory.New<CMRATDMessageTestClass>();
			message.EM_MessageText = CMRImportDeclarationTestData.ATD.Replace("B00122382", reference);
			return message;
		}

		protected override BusinessObject GetWrappedObject(string reference)
		{
			CMRATDMessageTestClass message = (CMRATDMessageTestClass)GetEDIMessage(reference);
			return message.GetWrappedObjectTestMethod();
		}
	}
}
