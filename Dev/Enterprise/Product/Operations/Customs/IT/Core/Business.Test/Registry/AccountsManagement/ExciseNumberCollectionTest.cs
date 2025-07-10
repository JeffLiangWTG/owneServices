using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Registry.Testing;

[TestedType(typeof(ExciseNumberCollection))]
sealed class ExciseNumberCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExciseNumberCollection>
{
	public void TestConstructor()
	{
		var account = new AccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory).AddNew();

		AssertExceptionThrown<ArgumentNullException>("account is required", () => new ExciseNumberCollection(null, null));
		AssertExceptionThrown<ArgumentNullException>("factory is required", () => new ExciseNumberCollection(account, null));
		AssertNoExceptionThrown(() => new ExciseNumberCollection(account, Factory));
	}

	public void TestContainsExciseNumber()
	{
		var exciseNumberCollection = GetNewCollection();
		exciseNumberCollection.AddNew().Number = "XXX";
		exciseNumberCollection.AddNew().Number = "YYY";
		Assert("Contains XXX", exciseNumberCollection.ContainsExciseNumber("XXX"));
		Assert("Contains YYY", exciseNumberCollection.ContainsExciseNumber("YYY"));
		Assert("Not contain ZZZ", !exciseNumberCollection.ContainsExciseNumber("ZZZ"));
	}

	protected override ExciseNumberCollection GetCollectionToTest() => GetNewCollection();

	protected override BusinessObject GetNewElementToAddToTheCollection() => GetNewCollection().AddNew();

	ExciseNumberCollection GetNewCollection()
	{
		var accountCollection = new AccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
		return accountCollection.AddNew().ExciseNumbers;
	}
}
