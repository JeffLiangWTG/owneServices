using System;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(SplitProvider))]
	sealed class SplitProviderBaseOnlyTest : SplitProviderProviderAbstractTest<SplitProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new SplitProvider(null));
		}

		public void TestEMCSJobDeclaration()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(nameof(SplitProvider.EMCSJobDeclaration), SplitProvider.EMCSJobDeclaration);
				AssertEquals(nameof(SplitProvider.EMCSJobDeclaration), emcsDeclaration, SplitProvider.EMCSJobDeclaration);
			});
		}

		protected override SplitProvider GetSplitProvider() => new SplitProvider(emcsInvoiceHeader);
	}
}
