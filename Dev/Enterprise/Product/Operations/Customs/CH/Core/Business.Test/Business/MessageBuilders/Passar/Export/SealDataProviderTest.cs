using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class SealDataProviderTest : TestCaseWithFactory
{
	public void TestNewCollection()
	{
		AssertEquals(0, SealDataProvider.NewCollection(ZString.Empty).Count());

		AssertEquals(1, SealDataProvider.NewCollection("123").Count());
	}

	public void TestProperties()
	{
		var seal = SealDataProvider.NewCollection("123").Single();

		CombineAssertions(() =>
		{
			AssertEquals("Sequence", 1, seal.SequenceNumber);
			AssertEquals("Identifier", "123", seal.Identifier);
		});
	}
}
