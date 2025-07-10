using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Testing;

[TestsSubclassesOf(typeof(ISumARegisterFormLayoutProvider))]
public abstract class SumARegisterFormLayoutProviderAbstractTest<TLayoutProvider> : TestCaseWithFactory
	where TLayoutProvider : ISumARegisterFormLayoutProvider, new ()
{
	public void TestGetDetailsHeaderLayout() =>
		AssertEquals("GetDetailsHeaderLayout", ExpectedDetailsHeaderLayoutType, Provider.GetDetailsHeaderLayout()?.GetType());

	public void TestGetLinesDetailsLayout() =>
		AssertEquals("GetLinesDetailsLayout", ExpectedLinesDetailsLayoutType, Provider.GetLinesDetailsLayout()?.GetType());

	protected ISumARegisterFormLayoutProvider Provider => GetSumARegisterFormLayoutProviderForTesting();
	protected ISumARegisterFormLayoutProvider GetSumARegisterFormLayoutProviderForTesting() => new TLayoutProvider();

	protected abstract Type ExpectedDetailsHeaderLayoutType { get; }
	protected abstract Type ExpectedLinesDetailsLayoutType { get; }
}
