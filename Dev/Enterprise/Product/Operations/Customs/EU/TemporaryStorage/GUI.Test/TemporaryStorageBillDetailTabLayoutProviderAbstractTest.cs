using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestsSubclassesOf(typeof(ITemporaryStorageBillDetailTabLayoutProvider))]
	public abstract class TemporaryStorageBillDetailTabLayoutProviderAbstractTest<T> : TestCaseWithFactory where T : ITemporaryStorageBillDetailTabLayoutProvider, new()
	{
		public void TestIsSupportingDocumentsTabVisible()
		{
			var tabVisibiltyDecider = new T();
			AssertEquals(ExpectedIsSupportingDocumentsTabVisible, tabVisibiltyDecider.IsSupportingDocumentsTabVisible);
		}

		protected abstract bool ExpectedIsSupportingDocumentsTabVisible { get; }

		public void TestIsAdditionalInformationTabVisible()
		{
			var tabVisibiltyDecider = new T();
			AssertEquals(ExpectedIsAdditionalInformationTabVisible, tabVisibiltyDecider.IsAdditionalInformationTabVisible);
		}

		protected abstract bool ExpectedIsAdditionalInformationTabVisible { get; }
	}
}
