using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Forwarding.Business.Testing
{
	abstract class RelatedCancellableDataSupporterAbstractTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCanCancel()
		{
			var supporter = GetSupporter();
			var parent = GetParent();
			AssertCanCancel(supporter, parent);
		}

		[ExpectNoExceptions]
		public void TestSetIsCancelled()
		{
			var supporter = GetSupporter();
			var parent = GetParent();
			AssertSetIsCancelled(supporter, parent);
		}

		protected abstract void AssertCanCancel(BaseRelatedCancellableDataSupporter supporter, IBusiness parent);
		protected abstract void AssertSetIsCancelled(BaseRelatedCancellableDataSupporter supporter, IBusiness parent);
		protected abstract BaseRelatedCancellableDataSupporter GetSupporter();
		protected abstract IBusiness GetParent();
	}
}
