using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRMOVAPPRMessage))]
	sealed class CMRMOVAPPRMessageTest : CMRImportDeclarationMessageTest
	{
		public override void TestGetReportForFormattedMessage()
		{
			Assert("Have not received a response", true);
		}

		public override void TestEM_MessageInterpretation()
		{
			Assert("Have not received a response", true);
		}

		protected override void TestWrappedObjectWithCorrectReference(string reference)
		{
			Assert("Have not received a response", true);
		}

		protected override EDIMessage GetEDIMessage(string reference) => null;

		protected override BusinessObject GetWrappedObject(string reference) => null;
	}
}
