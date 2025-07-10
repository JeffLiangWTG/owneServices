using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class CommunicationWrapperTest : DataProviderTestCase<CommunicationWrapper>
{
	public void TestSequenceNumeric()
	{
		AssertEquals("SequenceNumeric", 1, wrapper.SequenceNumeric);
	}

	public void TestId()
	{
		AssertEquals("Communication Wrapper - ID", "+3164259513", wrapper.Id);
	}

	public void TestTypeCode()
	{
		AssertEquals("Communication Wrapper - TypeCode", "TE", wrapper.TypeCode);
	}

	public void TestCommunicationEmailAndPhone()
	{
		var staff = WrapperTestHelper.CreateStaff(Factory, "CUS", "Kees Preker", "kees.preker@test.com");
		WrapperTestHelper.AddEmailaddressToStaff(staff, "kees.preker1@test.com", "CUS");
		staff.GS_WorkPhone = "+31612345678";

		var contactWrapper = new ContactWrapper(staff);

		CombineAssertions(() =>
		{
			AssertEquals("Communications email and phone", 2, contactWrapper.Communications.Count);
			AssertEquals("+31612345678", contactWrapper.Communications.ElementAt(0).Id);
			AssertEquals("TE", contactWrapper.Communications.ElementAt(0).TypeCode);
			AssertEquals("kees.preker@test.com", contactWrapper.Communications.ElementAt(1).Id);
			AssertEquals("EM", contactWrapper.Communications.ElementAt(1).TypeCode);
		});
	}

	protected override CommunicationWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();
		var contact = Factory.New<OrgContact>();
		contact.OC_Phone = "+3164259513";
		wrapper = new CommunicationWrapper(contact.OC_Phone, NLConstants.DMSMessageValues.TypeCodeTelephone, 1);
	}
	CommunicationWrapper wrapper;
}
