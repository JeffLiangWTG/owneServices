using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

class StaffContactPersonDataProviderTest : TestCaseWithFactory
{
	public void TestNew()
	{
		AssertNull(StaffContactPersonDataProvider.New(null));
	}

	public void TestName()
	{
		const string fullName = "abc";
		User.GS_FullName = fullName;

		AssertEquals(fullName, DataProvider.Name);
	}

	public void TestPhoneNumber() => CombineAssertions(() =>
	{
		User.GS_WorkPhone = ZString.Empty;
		AssertNull("Empty", DataProvider.PhoneNumber);
		User.GS_WorkPhone = "123";
		AssertEquals("Not empty", "123", DataProvider.PhoneNumber);
	});

	public void TestEMailAddress()
	{
		const string email = "a@b.cd";
		User.GS_EmailAddress = email;

		AssertEquals(email, DataProvider.EmailAddress);
	}

	GlbStaff User => user ?? (user = Factory.New<GlbStaff>());
	GlbStaff user;

	StaffContactPersonDataProvider DataProvider => dataProvider ?? (dataProvider = StaffContactPersonDataProvider.New(User));
	StaffContactPersonDataProvider dataProvider;
}
