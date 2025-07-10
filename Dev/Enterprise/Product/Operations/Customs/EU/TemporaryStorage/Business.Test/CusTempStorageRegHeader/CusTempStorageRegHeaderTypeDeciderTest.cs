using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageRegHeaderTypeDecider))]
sealed class CusTempStorageRegHeaderTypeDeciderTest : TestCaseWithFactory
{
	public void TestGetTypeForBinding()
	{
		AssertEquals("Type", typeof(CusTempStorageRegHeader), Decider.GetTypeForBinding());
	}

	public void TestGetTypeForNew()
	{
		AssertEquals("Type", typeof(CusTempStorageRegHeader), Decider.GetTypeForNew());
	}

	CusTempStorageRegHeaderTypeDecider Decider => decider ??= new CusTempStorageRegHeaderTypeDecider();
	CusTempStorageRegHeaderTypeDecider decider;
}
