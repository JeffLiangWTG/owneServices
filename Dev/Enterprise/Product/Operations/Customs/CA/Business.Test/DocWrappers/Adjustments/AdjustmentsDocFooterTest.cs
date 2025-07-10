using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(AdjustmentsDocFooter))]
	abstract class AdjustmentsDocFooterTest : NonPersistentBusinessObjectTestCase
	{
		public abstract void TestTotalDuty();
		public abstract void TestTotalDutyForNoAsAccount();

		public abstract void TestAmountDue();

		public abstract void TestAdjustmentsDocumentFooterMembers();

		public abstract void TestExplanation_ShouldShowInFull();
	}
}
