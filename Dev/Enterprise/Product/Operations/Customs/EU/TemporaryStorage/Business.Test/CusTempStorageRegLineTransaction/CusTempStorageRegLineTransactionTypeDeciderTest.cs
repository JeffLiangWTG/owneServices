using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineTransactionTypeDecider))]
sealed class CusTempStorageRegLineTransactionTypeDeciderTest : TestCaseWithFactory
{
	public void TestGetTypeForBinding()
	{
		AssertEquals("Type", typeof(CusTempStorageRegLineTransaction), Decider.GetTypeForBinding());
	}

	public void TestGetTypeForNew()
	{
		AssertEquals("Type", typeof(CusTempStorageRegLineTransaction), Decider.GetTypeForNew());
	}

	public void TestGetRegLineType()
	{
		AssertEquals("Type", typeof(CusTempStorageRegLine), Decider.GetRegLineType());
	}

	CusTempStorageRegLineTransactionTypeDecider Decider => decider ??= new CusTempStorageRegLineTransactionTypeDecider();
	CusTempStorageRegLineTransactionTypeDecider decider;
}
