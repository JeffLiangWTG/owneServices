using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRPAYSTDRMessage))]
	sealed class CMRPAYSTDRMessageTest : CMRImportDeclarationMessageTest
	{
		public void TestProcessPAYSTDR()
		{
			CMRPAYSTDRMessage message = Factory.New<CMRPAYSTDRMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.PAYSTDR;
			AssertEquals("Report Not Empty", false, message.GetReport().IsEmpty);
			AssertEquals("Report body has the error text", true, message.GetReport().Contains("The length of IMPORTERID in BODY is 5 characters which is less than the minimum field length of 11 characters"));
		}

		public void TestSetDefaultValues()
		{
			CMRPAYSTDRMessage message = Factory.New<CMRPAYSTDRMessage>();
			AssertEquals("Message type is set", CMRMessage.CMRMessageTypes.PAYSTD, message.EM_MessageType);
		}

		protected override EDIMessage GetEDIMessage(string reference)
		{
			CMRPAYSTDRMessageTestClass message = Factory.New<CMRPAYSTDRMessageTestClass>();
			message.EM_MessageText = CMRImportDeclarationTestData.PAYSTDR.Replace("B00122382", reference);
			return message;
		}

		protected override BusinessObject GetWrappedObject(string reference)
		{
			CMRPAYSTDRMessageTestClass message = (CMRPAYSTDRMessageTestClass)GetEDIMessage(reference);
			return message.GetWrappedObjectTestMethod();
		}
	}
}
