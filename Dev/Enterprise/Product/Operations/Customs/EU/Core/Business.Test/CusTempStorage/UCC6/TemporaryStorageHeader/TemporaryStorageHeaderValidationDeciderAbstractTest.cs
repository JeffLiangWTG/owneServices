using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestsSubclassesOf(typeof(ITemporaryStorageHeaderValidationDecider))]
	public abstract class TemporaryStorageHeaderValidationDeciderAbstractTest<T> : TestCaseWithFactory
		where T : class, ITemporaryStorageHeaderValidationDecider, new()
	{
		public void TestIsRule058Active()
		{
			var validationDecider = new T();
			AssertEquals(ExpectedIsRule058Active, validationDecider.IsRule058Active);
		}

		protected abstract bool ExpectedIsRule058Active { get; }

		public void TestIsPreLodgedStatusCheckActive()
		{
			var validationDecider = new T();
			AssertEquals(ExpectedIsPreLodgedStatusCheckActive, validationDecider.IsPreLodgedStatusCheckActive);
		}

		protected abstract bool ExpectedIsPreLodgedStatusCheckActive { get; }

		public void TestIsCheckCRNAndMRNForTSAActive()
		{
			var validationDecider = new T();
			AssertEquals(ExpectedIsCheckCRNAndMRNForTSAActive, validationDecider.IsCheckCRNAndMRNForTSAActive);
		}

		protected abstract bool ExpectedIsCheckCRNAndMRNForTSAActive { get; }

		public void TestIsMessageTypeCheckActive()
		{
			var validationDecider = new T();
			AssertEquals(ExpectedIsMessageTypeCheckActive, validationDecider.IsMessageTypeCheckActive);
		}

		protected abstract bool ExpectedIsMessageTypeCheckActive { get; }

		public void TestIsAuthorizationUsageForTSActive()
		{
			var validationDecider = new T();
			AssertEquals(ExpectedIsAuthorizationUsageCheckActive, validationDecider.IsAuthorizationUsageCheckActive);
		}

		protected abstract bool ExpectedIsAuthorizationUsageCheckActive { get; }
	}
}
