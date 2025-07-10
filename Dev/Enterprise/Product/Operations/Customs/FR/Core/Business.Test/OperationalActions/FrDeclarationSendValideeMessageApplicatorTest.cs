using Enterprise.Environment;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.OperationalActions.Testing
{
	[TestedType(typeof(FrDeclarationSendValideeMessageApplicator))]
	public class FrDeclarationSendValideeMessageApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestSendMessagesEvenWithMessageErrors()
		{
			var applicator = new FrDeclarationSendValideeMessageApplicator();

			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
			AssertEquals(false, applicator.SendMessagesEvenWithMessageErrorsInfo.ReadOnly);

			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
			AssertEquals(true, applicator.SendMessagesEvenWithMessageErrorsInfo.ReadOnly);
		}
	}
}
