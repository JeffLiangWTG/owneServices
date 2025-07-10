using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineTransactionLookups))]
sealed class CusTempStorageRegLineTransactionLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestTransactionTypeList()
	{
		var lookups = new CusTempStorageRegLineTransactionLookups(Factory.New<CusTempStorageRegLineTransaction>());
		CombineAssertions(() =>
		{
			AssertType<CusTempStorageRegLineTransactionTypeList>("Type", lookups.TransactionTypeList);
			AssertSame("Cached", lookups.TransactionTypeList, lookups.TransactionTypeList);
		});
	}

	public void TestTransactionStatusList()
	{
		var lookups = new CusTempStorageRegLineTransactionLookups(Factory.New<CusTempStorageRegLineTransaction>());
		CombineAssertions(() =>
		{
			AssertType<CusTempStorageRegLineTransactionStatusList>("Type", lookups.TransactionStatusList);
			AssertSame("Cached", lookups.TransactionStatusList, lookups.TransactionStatusList);
		});
	}

	public void TestTransactionInternalReferenceTypeList()
	{
		var lookups = new CusTempStorageRegLineTransactionLookups(Factory.New<CusTempStorageRegLineTransaction>());
		CombineAssertions(() =>
		{
			AssertType<CusTempStorageRegLineTransactionInternalReferenceTypeList>("Type", lookups.InternalReferenceTypeList);
			AssertSame("Cached", lookups.InternalReferenceTypeList, lookups.InternalReferenceTypeList);
		});
	}

	public void TestReferenceTypeList()
	{
		var lookups = new CusTempStorageRegLineTransactionLookups(Factory.New<CusTempStorageRegLineTransaction>());
		CombineAssertions(() =>
		{
			AssertType<CusTempStorageRegLineTransactionReferenceTypeList>("Type", lookups.ReferenceTypeList);
			AssertSame("Cached", lookups.ReferenceTypeList, lookups.ReferenceTypeList);
		});
	}
}
