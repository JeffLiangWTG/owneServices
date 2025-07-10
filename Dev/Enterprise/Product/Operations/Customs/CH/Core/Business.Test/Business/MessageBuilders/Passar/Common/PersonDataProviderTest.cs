using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

class PersonDataProviderTest : TestCaseWithFactory
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull(PersonDataProvider.New(null, null));
			AssertType<PersonDataProvider>("type", PersonDataProvider.New(Organisation, null));
			AssertType<PersonDataProvider>("type", PersonDataProvider.New(null, User));
		});
	}

	public void TestIdentificationNumber_ShouldBeCustomsRegNo_WhenCustomsCodeWithTypeBID()
	{
		const string customsRegNo = "123";
		Organisation.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, customsRegNo);

		AssertEquals(customsRegNo, DataProvider.IdentificationNumber);
	}

	public void TestIdentificationNumber_ShouldBeCached()
	{
		var orgCusCode = Organisation.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.BID, "123");

		var initiallyAccessedValue = DataProvider.IdentificationNumber;
		orgCusCode.OK_CustomsRegNo = "456";

		AssertEquals(initiallyAccessedValue, DataProvider.IdentificationNumber);
	}

	public void TestIdentificationNumber_ShouldBeNull_WhenNoCustomsCodeWithTypeBID()
	{
		const string customsRegNo = "123";
		Organisation.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.CAD, customsRegNo);

		AssertNull(DataProvider.IdentificationNumber);
	}

	public void TestAeoReferenceNumber()
	{
		const string aeoReferenceNumber = "456";
		Organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, aeoReferenceNumber);

		AssertEquals(aeoReferenceNumber, DataProvider.AeoReferenceNumber);
	}

	public void TestAeoReferenceNumber_ShouldBeCached()
	{
		var aeoReferenceNumber = Organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "456");

		var initiallyAccessedValue = DataProvider.AeoReferenceNumber;
		aeoReferenceNumber.OK_CustomsRegNo = "456";

		AssertEquals(initiallyAccessedValue, DataProvider.AeoReferenceNumber);
	}

	public void TestAeoReferenceNumber_ShouldBeNull_WhenNoCustomsCodeWithTypeAEO()
	{
		Organisation.CustomsCodes.RemoveAll();

		AssertNull(DataProvider.AeoReferenceNumber);
	}

	public void TestContactPerson()
	{
		CombineAssertions(() =>
		{
			AssertType<StaffContactPersonDataProvider>("type", DataProvider.ContactPerson);
			AssertSame("cached", DataProvider.ContactPerson, DataProvider.ContactPerson);
		});
	}

	OrgHeader Organisation => organisation ?? (organisation = Factory.New<OrgHeader>());
	OrgHeader organisation;

	GlbStaff User => user ?? (user = Factory.New<GlbStaff>());
	GlbStaff user;

	PersonDataProvider DataProvider => dataProvider ?? (dataProvider = PersonDataProvider.New(Organisation, User));
	PersonDataProvider dataProvider;
}
