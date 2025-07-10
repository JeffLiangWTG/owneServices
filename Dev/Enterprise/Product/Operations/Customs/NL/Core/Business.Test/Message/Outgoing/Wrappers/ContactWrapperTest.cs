using System;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class ContactWrapperTest : DataProviderTestCase<ContactWrapper>
{
	public void TestConstructor_GlbStaff()
	{
		GlbStaff staff = null;
		AssertExceptionThrown<ArgumentNullException>(() => new ContactWrapper(staff));
	}

	public void TestConstructor_OrgContact()
	{
		OrgContact orgContact = null;
		AssertExceptionThrown<ArgumentNullException>(() => new ContactWrapper(orgContact));
	}

	public void TestName_GlbStaff()
	{
		var wrapper = GetProvider();
		AssertEquals("AA", wrapper.Name);
	}

	public void TestName_OrgContact()
	{
		var wrapper = CreateContactWrapperForOrgContact();
		AssertEquals("Exporter Contact Name", wrapper.Name);
	}

	public void TestCommunication_GlbStaff()
	{
		var wrapper = GetProvider();
		CombineAssertions(() =>
		{
			AssertEquals("Count", 2, wrapper.Communications.Count);
			AssertCommunication("1", wrapper.Communications.Single(x => x.SequenceNumeric == 1), "+0123456789", NLConstants.DMSMessageValues.TypeCodeTelephone);
			AssertCommunication("2", wrapper.Communications.Single(x => x.SequenceNumeric == 2), "123@WTG.com", NLConstants.DMSMessageValues.TypeCodeMail);
		});
	}

	public void TestCommunications_OrgContact()
	{
		var wrapper = CreateContactWrapperForOrgContact();
		CombineAssertions(() =>
		{
			AssertEquals("Count", 2, wrapper.Communications.Count);
			AssertCommunication("1", wrapper.Communications.Single(x => x.SequenceNumeric == 1), "+3164259513", NLConstants.DMSMessageValues.TypeCodeTelephone);
			AssertCommunication("2", wrapper.Communications.Single(x => x.SequenceNumeric == 2), "exporter@mail.nl", NLConstants.DMSMessageValues.TypeCodeMail);
		});
	}

	void AssertCommunication(string sequenceNumeric, ICommunication communication, string id, string typeCode)
	{
		AssertEquals("SequenceNumeric" + sequenceNumeric + "-ID", id, communication.Id);
		AssertEquals("SequenceNumeric" + sequenceNumeric + "-TypeCode", typeCode, communication.TypeCode);
	}

	public void TestPhoneNumber()
	{
		AssertEquals(string.Empty, GetProvider().PhoneNumber);
	}

	public void TestEMailAddress()
	{
		AssertEquals(string.Empty, GetProvider().EMailAddress);
	}

	protected override ContactWrapper GetProvider() => new ContactWrapper(glbStaff);

	protected override void SetUp()
	{
		base.SetUp();

		glbStaff = Factory.New<GlbStaff>();
		glbStaff.GS_FullName = "AA";
		glbStaff.GS_WorkPhone = "+0123456789";
		glbStaff.GS_EmailAddress = "123@WTG.com";
	}

	GlbStaff glbStaff;

	ContactWrapper CreateContactWrapperForOrgContact()
	{
		var party = Factory.New<OrgHeader>();
		var contact = party.Contacts.AddNew();
		contact.OC_ContactName = "Exporter Contact Name";
		contact.OC_Phone = "+3164259513";
		contact.OC_Email = "exporter@mail.nl";
		return new ContactWrapper(contact);
	}
}
