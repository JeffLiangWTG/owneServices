using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRREFACCMessage))]
	sealed class CMRREFACCMessageTest : CMRImportDeclarationMessageTest
	{
		public void TestGetStatus()
		{
			CMRREFACCMessage message = Factory.New<CMRREFACCMessage>();
			AssertEquals("Status should be Clear for message subtype not to be set as 'rejected' as there is no FTX segment for status for the message", CMRMessage.CMRMessageStatusDescription.CLEAR, message.GetStatus());
		}

		public void TestGetReport()
		{
			CMRREFACCMessage message = Factory.New<CMRREFACCMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.REFACC;

			ZString report = message.GetReport();
			AssertEquals("Entry Number: ", true, report.Contains("Entry Number: AAAA6RHTE"));
			AssertEquals("EFT Run Number: ", true, report.Contains("EFT Run Number: BBBB4WPGJ"));
			AssertEquals("Claim Number: ", true, report.Contains("Claim Number: OWNER REFH"));
		}

		protected override EDIMessage GetEDIMessage(string reference)
		{
			CMRREFACCMessageTestClass message = Factory.New<CMRREFACCMessageTestClass>();
			message.EM_MessageText = CMRImportDeclarationTestData.REFACC.Replace("B00122382", reference);
			return message;
		}

		protected override BusinessObject GetWrappedObject(string reference)
		{
			CMRREFACCMessageTestClass message = (CMRREFACCMessageTestClass)GetEDIMessage(reference);
			return message.GetWrappedObjectTestMethod();
		}
	}
}
