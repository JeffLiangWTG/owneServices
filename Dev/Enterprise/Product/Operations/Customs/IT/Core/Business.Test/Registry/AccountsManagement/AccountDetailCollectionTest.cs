using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Registry.Testing;

[TestedType(typeof(AccountDetailCollection))]
sealed class AccountDetailCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AccountDetailCollection>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentException>("account is required", () => new AccountDetailCollection(null, Factory));
		AssertExceptionThrown<ArgumentException>("factory is required", () => new AccountDetailCollection(accountCollection.AddNew(), null));
	}

	protected override void SetUp()
	{
		base.SetUp();

		accountCollection = new AccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
	}
	AccountCollection accountCollection;

	protected override AccountDetailCollection GetCollectionToTest()
	{
		return accountCollection.AddNew().AccountDetails;
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		return accountCollection.AddNew().AccountDetails.AddNew();
	}
}
