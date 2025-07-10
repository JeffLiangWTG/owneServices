using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(UCC6ImportCusFiscalReferenceValidationDecider))]
	sealed class UCC6ImportCusFiscalReferenceValidationSupporterTest : TestCaseWithFactory
	{
		public void TestIsFiscalReferenceAvailabilityValidationActive()
		{
			var validationDecider = new UCC6ImportCusFiscalReferenceValidationDecider();
			AssertEquals(true, validationDecider.IsFiscalReferenceAvailabilityValidationActive);
		}

		public void TestIsRuleR0010Active()
		{
			var validationDecider = new UCC6ImportCusFiscalReferenceValidationDecider();
			AssertEquals(true, validationDecider.IsRuleR0010Active);
		}
	}
}
