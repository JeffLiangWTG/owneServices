using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class PartyWithStaffWrapperTest : DataProviderTestCase<PartyWithStaffWrapper>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		var party = Factory.New<OrgHeader>();
		var staff = Factory.New<GlbStaff>();
		AssertNull("party: null, staff: null", PartyWithStaffWrapper.New(null, null));
		AssertNull("party: filled, staff: null", PartyWithStaffWrapper.New(party, null));
		AssertNull("party: null, staff: filled", PartyWithStaffWrapper.New(null, staff));
		AssertNotNull("party: filled, staff: filled", PartyWithStaffWrapper.New(party, staff));
	});

	public void TestInheritance()
	{
		AssertEquals(true, typeof(PartyWithStaffWrapper).IsSubclassOf(typeof(PartyWrapper)));
	}

	public void TestContact()
	{
		glbStaff.GS_FullName = "AA";
		CombineAssertions(() =>
		{
			AssertType<ContactWrapper>("Type", wrapper.Contact);
			AssertEquals("Name", "AA", wrapper.Contact.Name);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		orgHeader = Factory.New<OrgHeader>();
		glbStaff = Factory.New<GlbStaff>();
		wrapper = PartyWithStaffWrapper.New(orgHeader, glbStaff);
	}
	OrgHeader orgHeader;
	GlbStaff glbStaff;
	PartyWithStaffWrapper wrapper;

	protected override PartyWithStaffWrapper GetProvider() => wrapper;
}
