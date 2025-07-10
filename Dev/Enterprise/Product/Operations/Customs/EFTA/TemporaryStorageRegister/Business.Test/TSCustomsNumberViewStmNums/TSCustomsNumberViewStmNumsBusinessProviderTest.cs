using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(TSCustomsNumberViewStmNumsBusinessProvider))]
sealed class TSCustomsNumberViewStmNumsBusinessProviderTest : NonPersistentBusinessObjectTestCase
{
	public void TestActiveWrapper()
	{
		CombineAssertions(() =>
		{
			var premises = Factory.New<CusTempStorageRegPremises>();
			var provider = premises.NumberProvider;

			AssertNull(provider.ActiveWrapper);

			_ = provider.CustomsNumbers.AddNew();
			var wrapper1 = provider.CustomsNumberWrappers[0];
			wrapper1.IsActive = false;

			AssertNull(provider.ActiveWrapper);

			wrapper1.IsActive = true;
			AssertNotNull(provider.ActiveWrapper);
			AssertSame(provider.ActiveWrapper, wrapper1);

			_ = provider.CustomsNumbers.AddNew();
			var wrapper2 = provider.CustomsNumberWrappers[1];
			wrapper1.IsActive = false;
			wrapper2.IsActive = true;
			AssertNotNull(provider.ActiveWrapper);
			AssertNotSame(provider.ActiveWrapper, wrapper1);
			AssertSame(provider.ActiveWrapper, wrapper2);

			wrapper2.IsActive = false;
			AssertNull(provider.ActiveWrapper);
		});
	}

	public void TestGetSetting()
	{
		var premises = Factory.New<CusTempStorageRegPremises>();
		var provider = premises.NumberProvider;
		var setting = provider.GetSetting("TS");
		AssertSame("Setting is cached", setting, provider.GetSetting("TS"));
	}

	public void TestCustomsNumberWrappers()
	{
		CombineAssertions(() =>
		{
			var provider = (TSCustomsNumberViewStmNumsBusinessProvider)GetNewBusinessObject();
			AssertType<TSCustomsNumberViewStmNumsWrapperCollection>(provider.CustomsNumberWrappers);
			AssertEquals(0, provider.CustomsNumberWrappers.Count);
			_ = provider.CustomsNumbers.AddNew();
			AssertEquals(1, provider.CustomsNumberWrappers.Count);
			var wrapper = provider.CustomsNumberWrappers[0];
			AssertType<TSCustomsNumberViewStmNumsWrapper>(wrapper);
		});
	}

	public void TestGetOrCreateWrapper()
	{
		CombineAssertions(() =>
		{
			var provider = (TSCustomsNumberViewStmNumsBusinessProvider)GetNewBusinessObject();
			AssertNull(provider.GetOrCreateWrapper(null));
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			var wrapper = provider.GetOrCreateWrapper(stmNum);
			AssertType<TSCustomsNumberViewStmNumsWrapper>(wrapper);
			AssertSame(wrapper, provider.GetOrCreateWrapper(stmNum));
		});
	}

	public void TestGetNewLookups()
	{
		CombineAssertions(() =>
		{
			var provider = (TSCustomsNumberViewStmNumsBusinessProvider)GetNewBusinessObject();
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			var lookups = stmNum.Lookups;
			AssertEquals(typeof(CustomsNumberViewStmNumsLookups), lookups.GetType());
			stmNum.Provider = provider;
			lookups = stmNum.Lookups;
			AssertEquals(typeof(TSCustomsNumberViewStmNumsLookups), lookups.GetType());
		});
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var premises = Factory.New<CusTempStorageRegPremises>();
		return premises.NumberProvider;
	}
}
