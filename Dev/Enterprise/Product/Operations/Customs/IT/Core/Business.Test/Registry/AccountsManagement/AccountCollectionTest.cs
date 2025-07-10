using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Registry.Testing;

[TestedType(typeof(AccountCollection))]
sealed class AccountCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<AccountCollection>
{
	protected override bool RequiresFactory
	{
		get { return true; }
	}

	protected override bool RequiresFallbackLevel
	{
		get { return true; }
	}

	protected override AccountCollection GetCollectionToTest()
	{
		return new AccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		return new Account(GetCollectionToTest());
	}
}
