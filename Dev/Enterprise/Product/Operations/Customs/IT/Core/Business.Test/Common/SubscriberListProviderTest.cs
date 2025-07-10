using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SubscriberListProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("factory is required", () => new SubscriberListProvider(null, null));
		AssertExceptionThrown<ArgumentNullException>("customsProfileDataProvider is required", () => new SubscriberListProvider(Factory, null));
		AssertNoExceptionThrown(() => new SubscriberListProvider(Factory, GetDataProvider(ZString.Empty)));
	}

	public void TestGetSubscribersReturnsNoResultQuery()
	{
		var staffCollection = new SubscriberListProvider(Factory, GetDataProvider(ZString.Empty)).GetSubscribers();
		Assert("IsNoResultQuery", staffCollection.CompleteFilter.IsNoResultQuery);
		AssertEquals("Count", 0, staffCollection.Count);
	}

	public void TestGetSubscribersReturnsElements()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.AppendAccount("11111111111-002", "5678")
			.AppendAccountDetail("5678-DEC1", "DEC1")
			.Build();

		var staff1 = Factory.NewWithValidTestData<GlbStaff>();
		staff1.GS_Code = "ST1";
		staff1.GS_FullName = "STAFF1 FULL NAME";
		var staff1Wrapper = GlbStaffWrapper.Get(staff1);
		staff1Wrapper.PasswordCollection.AddNew().GP_UserID = "1234";

		var staff2 = Factory.NewWithValidTestData<GlbStaff>();
		staff2.GS_Code = "ST2";
		staff2.GS_FullName = "STAFF2 FULL NAME";
		var staff2Wrapper = GlbStaffWrapper.Get(staff2);
		staff2Wrapper.PasswordCollection.AddNew().GP_UserID = "1234";
		staff2Wrapper.PasswordCollection.AddNew().GP_UserID = "5678";
		Factory.Save();

		var staffCollection = new SubscriberListProvider(Factory, GetDataProvider("9999")).GetSubscribers();
		AssertEquals("CustomsProfile = '9999'", 0, staffCollection.Count);

		staffCollection = new SubscriberListProvider(Factory, GetDataProvider("1234-DEC1")).GetSubscribers();
		AssertArrayEqualsByElements("CustomsProfile = '1234'", new GlbStaff[] { staff1, staff2 }, staffCollection.ToArray());

		staffCollection = new SubscriberListProvider(Factory, GetDataProvider("5678-DEC1")).GetSubscribers();
		AssertArrayEqualsByElements("CustomsProfile = '5678'", new GlbStaff[] { staff2 }, staffCollection.ToArray());
	}

	public void TestGetSubscribersReportsDeveloperException()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();

		const int staffCollectionCountThreshold = 50;
		for (int i = 0; i < staffCollectionCountThreshold; i++)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = $"{i}";
			var staffWrapper = GlbStaffWrapper.Get(staff);
			staffWrapper.PasswordCollection.AddNew().GP_UserID = "1234";
		}
		Factory.Save();

		var staffCollection = new SubscriberListProvider(Factory, GetDataProvider("1234-DEC1")).GetSubscribers();
		ErrorReporter.Clear();
		AssertEquals(50, staffCollection.Count);
		Assert(!ErrorReporter.HasBeenReported("IT.SubscriberListLookups|SubscribersFor_1234"));

		var staff51 = Factory.NewWithValidTestData<GlbStaff>();
		staff51.GS_Code = $"51";
		var staff51Wrapper = GlbStaffWrapper.Get(staff51);
		staff51Wrapper.PasswordCollection.AddNew().GP_UserID = "1234";
		Factory.Save();

		Factory.ClearCachedValue<GlbStaffCollection>("IT.SubscriberListLookups|SubscribersFor_1234");
		ErrorReporter.Clear();
		staffCollection = new SubscriberListProvider(Factory, GetDataProvider("1234-DEC1")).GetSubscribers();
		AssertEquals(51, staffCollection.Count);
		Assert(ErrorReporter.HasBeenReported("IT.SubscriberListLookups|SubscribersFor_1234"));
		ErrorReporter.Clear();
	}

	ICustomsProfileDataProvider GetDataProvider(ZString customsProfile)
	{
		var mock = new Mock<ICustomsProfileDataProvider>();
		mock.Setup(m => m.CustomsProfile).Returns(customsProfile);
		return mock.Object;
	}
}
