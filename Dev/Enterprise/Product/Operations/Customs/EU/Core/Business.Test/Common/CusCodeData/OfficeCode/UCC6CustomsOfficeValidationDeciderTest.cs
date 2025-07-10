using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class UCC6CustomsOfficeValidationDeciderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestIsRuleR0676Active() => NUnit.Framework.Assert.That(validationDecider.IsRuleR0676Active, NUnit.Framework.Is.EqualTo(false));

		protected override void SetUp()
		{
			base.SetUp();
			validationDecider = new UCC6CustomsOfficeValidationDecider();
		}

		ICustomsOfficeValidationDecider validationDecider;
	}
}
