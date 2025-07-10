using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing;

sealed class InvoiceLinePackageValidationDeciderTest : TestCaseWithFactory
{
		[ExpectNoExceptions]
		public void TestIsRuleR0219Active() => NUnit.Framework.Assert.That(validationDecider.IsRuleR0219Active, NUnit.Framework.Is.EqualTo(true));

		[ExpectNoExceptions]
		public void TestIsRuleR0220Active() => NUnit.Framework.Assert.That(validationDecider.IsRuleR0220Active, NUnit.Framework.Is.EqualTo(true));

		[ExpectNoExceptions]
		public void TestIsRuleR0364Active() => NUnit.Framework.Assert.That(validationDecider.IsRuleR0364Active, NUnit.Framework.Is.EqualTo(true));

		protected override void SetUp()
		{
			base.SetUp();
			validationDecider = new InvoiceLinePackageValidationDecider();
		}

		IInvoiceLinePackageValidationDecider validationDecider;
}
