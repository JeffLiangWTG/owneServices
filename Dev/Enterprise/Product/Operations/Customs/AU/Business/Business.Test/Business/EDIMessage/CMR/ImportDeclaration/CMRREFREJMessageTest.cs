using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRREFREJMessage))]
	sealed class CMRREFREJMessageTest : CMRImportDeclarationMessageTest
	{
		protected override EDIMessage GetEDIMessage(string reference)
		{
			CMRREFREJMessageTestClass message = Factory.New<CMRREFREJMessageTestClass>();
			message.EM_MessageText = CMRImportDeclarationTestData.REFREJ.Replace("B00122382", reference);
			return message;
		}

		protected override BusinessObject GetWrappedObject(string reference)
		{
			CMRREFREJMessageTestClass message = (CMRREFREJMessageTestClass)GetEDIMessage(reference);
			return message.GetWrappedObjectTestMethod();
		}
	}
}
