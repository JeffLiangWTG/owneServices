using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	abstract class AdjustmentsDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public abstract void TestDocumentPagesOrder();

		public abstract void TestSimpleLineChangeScenario();

		public abstract void TestDeleteLineScenario();

		public abstract void TestSplitALineOnSameSubHeader();

		public abstract void TestLineWithMultipleDuties();

		public abstract void TestAdjustmentsDocumentWrapperMembers();

		public abstract void TestThrowExceptionWhenMessageTypeIsInvalid();

		protected abstract AdjustmentsDocumentWrapper CreateNewWrapper(JobDeclaration declaration);
	}
}
