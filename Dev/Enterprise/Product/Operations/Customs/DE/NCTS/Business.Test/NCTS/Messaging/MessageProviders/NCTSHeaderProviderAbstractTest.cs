using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(NCTSHeaderProvider))]
	abstract class NCTSHeaderProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T>
		where T : NCTSHeaderProvider
	{
		protected T HeaderProvider => headerProvider ?? (headerProvider = GetHeaderProvider());
		T headerProvider;

		protected abstract T GetHeaderProvider();

		protected override T GetProvider() => HeaderProvider;
	}
}
