using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Registry;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CustomsMessageFilenameProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentException>("account is mandatory", () => new CustomsMessageFilenameProvider(account: null, messageType: null, factory: null));
		AssertExceptionThrown<ArgumentException>("messageType is mandatory", () => new CustomsMessageFilenameProvider(account: accountForNode1234, messageType: null, factory: null));
		AssertExceptionThrown<ArgumentException>("messageType is mandatory (not empty)", () => new CustomsMessageFilenameProvider(account: accountForNode1234, messageType: "", factory: null));
		AssertExceptionThrown<ArgumentException>("factory is mandatory", () => new CustomsMessageFilenameProvider(account: accountForNode1234, messageType: "R", factory: null));
		AssertNoExceptionThrown(() => new CustomsMessageFilenameProvider(account: accountForNode1234, messageType: "R", factory: Factory));
	}

	[TestDate(2020, 06, 29)]
	public void TestGenerateFilename()
	{
		ICustomsMessageFilenameProvider providerForNode1234 = new CustomsMessageFilenameProvider(accountForNode1234, messageType: "R", Factory);
		AssertEquals("12340629.R00", providerForNode1234.GenerateFilename());
		AssertEquals("12340629.R01", providerForNode1234.GenerateFilename());

		ICustomsMessageFilenameProvider providerForNode5678 = new CustomsMessageFilenameProvider(accountForNode5678, messageType: "T", Factory);
		AssertEquals("56780629.T00", providerForNode5678.GenerateFilename());
		AssertEquals("56780629.T01", providerForNode5678.GenerateFilename());

		TestDateAttribute.AddDays(1);
		AssertEquals("12340630.R00", providerForNode1234.GenerateFilename());
		AssertEquals("12340630.R01", providerForNode1234.GenerateFilename());
	}

	protected override void SetUp()
	{
		base.SetUp();
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.New<OrgHeader>().OH_Code = "DEC2";
		Factory.Save();
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

		var accountCollection = new AccountCollectionTestBuilder(company.PK.ToGuid())
			.AppendAccount("11111111111-001", "1234").AppendAccountDetail("1234-DEC1", "DEC1")
			.AppendAccount("22222222222-001", "5678").AppendAccountDetail("5678-DEC2", "DEC2")
			.Build();

		accountForNode1234 = accountCollection[0];
		accountForNode5678 = accountCollection[1];
	}

	Account accountForNode1234;
	Account accountForNode5678;
}
