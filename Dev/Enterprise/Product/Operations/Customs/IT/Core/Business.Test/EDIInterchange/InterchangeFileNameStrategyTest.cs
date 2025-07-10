using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Registry;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class InterchangeFileNameStrategyTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("account is required", () => new InterchangeFileNameStrategy(null, "", null));
		AssertExceptionThrown<ArgumentException>("messageType is required", () => new InterchangeFileNameStrategy(account, "", null));
		AssertExceptionThrown<ArgumentException>("factory is required", () => new InterchangeFileNameStrategy(account, "R", null));
		AssertNoExceptionThrown(() => new InterchangeFileNameStrategy(account, "R", Factory));
	}

	[TestDate(2020, 09, 14)]
	public void TestGetFileName()
	{
		IInterchangeFileNameStrategy strategy = new InterchangeFileNameStrategy(account, "R", Factory);
		AssertEquals("12340914.R00", strategy.GetFileName());
		AssertEquals("12340914.R01", strategy.GetFileName());
		AssertEquals("12340914.R02", strategy.GetFileName());
	}

	protected override void SetUp()
	{
		base.SetUp();
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		var accountCollection = new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();
		account = accountCollection[0];
	}

	Account account;
}
