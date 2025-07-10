using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRPAYINVMessage))]
	sealed class CMRPAYINVMessageTest : CMRImportDeclarationMessageTest
	{
		public void TestGetReportMessage()
		{
			CMRPAYINVMessage message = Factory.New<CMRPAYINVMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.PAYINV;
			AssertEquals("Body Of Email", false, message.GetReport().IsEmpty);
			AssertEquals("Email body has the error text", true, message.GetReport().Contains("IMPORT DEC ID MUST BE VALID"));
		}

		protected override EDIMessage GetEDIMessage(string reference)
		{
			CMRPAYINVMessageTestClass message = Factory.New<CMRPAYINVMessageTestClass>();
			message.EM_MessageText = CMRImportDeclarationTestData.PAYINV.Replace("B00122382", reference);
			return message;
		}

		protected override BusinessObject GetWrappedObject(string reference)
		{
			CMRPAYINVMessageTestClass message = (CMRPAYINVMessageTestClass)GetEDIMessage(reference);
			return message.GetWrappedObjectTestMethod();
		}
	}
}
