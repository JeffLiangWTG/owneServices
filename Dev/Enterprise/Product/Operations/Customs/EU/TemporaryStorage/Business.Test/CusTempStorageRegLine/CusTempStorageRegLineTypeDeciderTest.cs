using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineTypeDecider))]
sealed class CusTempStorageRegLineTypeDeciderTest : TestCaseWithFactory
{
	public void TestGetTypeForBinding()
	{
		AssertEquals("Type", typeof(CusTempStorageRegLine), Decider.GetTypeForBinding());
	}

	public void TestGetTypeForNew()
	{
		AssertEquals("Type", typeof(CusTempStorageRegLine), Decider.GetTypeForNew());
	}

	public void TestGetHeaderType()
	{
		AssertEquals("Type", typeof(CusTempStorageRegHeader), Decider.GetHeaderType());
	}

	CusTempStorageRegLineTypeDecider Decider => decider ??= new CusTempStorageRegLineTypeDecider();
	CusTempStorageRegLineTypeDecider decider;
}
