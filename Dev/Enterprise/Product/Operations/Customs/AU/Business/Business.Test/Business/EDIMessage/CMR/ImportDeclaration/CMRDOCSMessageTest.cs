using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRDOCSMessage))]
	sealed class CMRDOCSMessageTest : CMRImportDeclarationMessageTest
	{
		public void TestGetReport()
		{
			ZString expectedEmailBody =
				@"Status: B00122382/1

Reason:
	TEST 2 IN ACCORDANCE WITH SECTION 71DA OF THE CUSTOMS ACT 1901 YOU ARE REQUIRED TO DELIVER THE FOLLOWING INFORMATION AND COMMERCIAL DOCUMENTS 

Customs Officer:
	Name: SEAN BLACKMORE
	Phone: 02 6229 3571
	Fax: 02 6229 3571
	Email: sean.blackmore@customs.gov.au
";
			CMRDOCSMessage message = Factory.New<CMRDOCSMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.DOCS;
			AssertEquals("Body Of Email", expectedEmailBody, message.GetReport());
		}

		protected override EDIMessage GetEDIMessage(string reference)
		{
			CMRDOCSMessageTestClass message = Factory.New<CMRDOCSMessageTestClass>();
			message.EM_MessageText = CMRImportDeclarationTestData.DOCS.Replace("B00122382", reference);
			return message;
		}

		protected override BusinessObject GetWrappedObject(string reference)
		{
			CMRDOCSMessageTestClass message = (CMRDOCSMessageTestClass)GetEDIMessage(reference);
			return message.GetWrappedObjectTestMethod();
		}
	}
}
