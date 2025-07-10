using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(TSCustomsNumberViewStmNumsWrapperCollection))]
sealed class TSCustomsNumberViewStmNumsWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TSCustomsNumberViewStmNumsWrapperCollection>
{
	public void TestLoadData()
	{
		CombineAssertions(() =>
		{
			var collection = new CustomsNumberViewStmNumsCollection(Provider);
			var stmNums1 = collection.AddNew();
			var stmNums2 = collection.AddNew();
			var wrapperCollection = new TSCustomsNumberViewStmNumsWrapperCollection(collection);
			AssertEquals("wrapperCollection.AllowNew", expected: false, wrapperCollection.AllowNew);
			AssertEquals("wrapperCollection.AllowRemove", expected: false, wrapperCollection.AllowRemove);
			AssertEquals("wrapperCollection.Count", 2, wrapperCollection.Count);

			var wrapper1 = (TSCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums1.PK);
			AssertEquals("wrapper1.StmNums", stmNums1, wrapper1.StmNums);
			var wrapper2 = (TSCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums2.PK);
			AssertEquals("wrapper2.StmNums", stmNums2, wrapper2.StmNums);

			var stmNums3 = collection.AddNew();
			AssertEquals("wrapperCollection.Count", 3, wrapperCollection.Count);
			var wrapper3 = (TSCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums3.PK);
			AssertEquals("wrapper3.StmNums", stmNums3, wrapper3.StmNums);

			collection.Delete(stmNums1);
			AssertEquals("wrapperCollection.Count", 2, wrapperCollection.Count);
			wrapper2 = (TSCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums2.PK);
			AssertEquals("wrapper2.StmNums", stmNums2, wrapper2.StmNums);
			wrapper3 = (TSCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums3.PK);
			AssertEquals("wrapper3.StmNums", stmNums3, wrapper3.StmNums);

			var stmNums4 = collection.AddNew();
			stmNums4.SN_Owner = Premises.PK;
			stmNums4.SN_Type = stmNums2.SN_Type;
			stmNums4.SN_FountainName = stmNums2.SN_FountainName;
			AssertEquals("wrapperCollection.Count", 3, wrapperCollection.Count);
			wrapper2 = (TSCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums2.PK);
			AssertEquals("wrapper2.StmNums", stmNums2, wrapper2.StmNums);
			wrapper3 = (TSCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums3.PK);
			AssertEquals("wrapper3.StmNums", stmNums3, wrapper3.StmNums);
			var wrapper4 = (TSCustomsNumberViewStmNumsWrapper)wrapperCollection.FindByPK(stmNums4.PK);
			AssertEquals("wrapper4.StmNums", stmNums4, wrapper4.StmNums);

			_ = AssertExceptionThrown<NotImplementedException>(() => wrapperCollection.AddNew());
		});
	}

	TSCustomsNumberViewStmNumsBusinessProvider Provider => provider ??= Premises.NumberProvider;
	TSCustomsNumberViewStmNumsBusinessProvider provider;

	CusTempStorageRegPremises Premises => premises ??= Factory.New<CusTempStorageRegPremises>();
	CusTempStorageRegPremises premises;

	protected override TSCustomsNumberViewStmNumsWrapperCollection GetCollectionToTest()
		=> Provider.CustomsNumberWrappers;

	protected override BusinessObject GetNewElementToAddToTheCollection()
		=> Provider.GetOrCreateWrapper(Provider.CustomsNumbers.AddNew());
}
