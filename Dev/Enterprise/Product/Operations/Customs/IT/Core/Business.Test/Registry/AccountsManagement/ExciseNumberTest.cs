using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Registry.Testing;

[TestedType(typeof(ExciseNumber))]
sealed class ExciseNumberTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		var account = new AccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory).AddNew();

		AssertExceptionThrown<ArgumentNullException>("account is required", () => new ExciseNumber(null, null));
		AssertExceptionThrown<ArgumentNullException>("factory is required", () => new ExciseNumber(account, null));
		AssertNoExceptionThrown(() => new ExciseNumber(account, Factory));
	}

	public void TestNumberMaxLength()
	{
		var exciseNumber = GetNewExciseNumber();
		AssertEquals(13, exciseNumber.NumberInfo.MaxLength);
	}

	public void TestSetNumberTriggersValidation()
	{
		var exciseNumber = GetNewExciseNumber();
		exciseNumber.Number = "XXX";
		AssertHasErrorContaining("Full test in validation test class", exciseNumber.NumberInfo, "Excise Number is in an invalid format.");
	}

	public void TestPreSaveValidation()
	{
		var exciseNumber = GetNewExciseNumber();
		exciseNumber.RunPreSaveValidation();
		AssertHasErrorContaining("Full test in validation test class", exciseNumber.NumberInfo, "Please enter a value");
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewExciseNumber();

	ExciseNumber GetNewExciseNumber()
	{
		var accountCollection = new AccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
		return accountCollection.AddNew().ExciseNumbers.AddNew();
	}
}
