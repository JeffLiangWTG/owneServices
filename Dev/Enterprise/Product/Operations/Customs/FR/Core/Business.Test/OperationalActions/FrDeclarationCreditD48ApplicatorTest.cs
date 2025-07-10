using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.OperationalActions.Testing
{
	[TestedType(typeof(FrDeclarationCreditD48Applicator))]
	public class FrDeclarationCreditD48ApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestDefaults()
		{
			var applicator = new FrDeclarationCreditD48Applicator(Factory);
			Assert(applicator.D48DocumentCode.IsEmpty);
			Assert(applicator.ReferenceNumber.IsEmpty);
		}
	}
}
