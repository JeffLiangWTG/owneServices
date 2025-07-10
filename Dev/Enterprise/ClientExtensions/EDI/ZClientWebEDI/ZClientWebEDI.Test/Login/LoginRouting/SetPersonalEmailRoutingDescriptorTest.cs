using System;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class SetPersonalEmailRoutingDescriptorTest : TestCaseWithFactory
	{
		public void TestRoutingUrl()
		{
			var contact = Factory.New<OrgContact>();
			var descriptor = new SetPersonalEmailRoutingDescriptor(contact);
			AssertEquals("Routing page url", "~/Login/UpdateContactInformation.aspx", descriptor.RoutingUrl.ToString());
		}

		public void TestIsRoutingRequired()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			Factory.Save();
			var descriptor = new SetPersonalEmailRoutingDescriptor(contact);
			AssertEquals("No personal email", true, descriptor.IsRoutingRequired);
			contact.Person.PER_EmailAddress = "tester@aaa.com";
			Factory.Save();
			descriptor = new SetPersonalEmailRoutingDescriptor(contact);
			AssertEquals("Personal email exists", false, descriptor.IsRoutingRequired);
		}

		public void TestIsRoutingRequired_SkipPrompt()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			Factory.Save();
			var personAsEDI = contact.Person as EDIGlbPerson;
			personAsEDI.StorePersonalEmailPromptSkip();
			Factory.Save();
			Assert("Precondition", contact.Person.PER_EmailAddress.IsEmpty);
			Assert("Precondition", personAsEDI.ShouldSkipPersonalEmailPrompt());
			var descriptor = new SetPersonalEmailRoutingDescriptor(contact);
			AssertEquals("Personal email setting is skipped", false, descriptor.IsRoutingRequired);
		}

		[TestDate(2020, 01, 01)]
		public void TestIsRoutingRequired_SkipExpired()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			Factory.Save();
			var personAsEDI = contact.Person as EDIGlbPerson;
			personAsEDI.StorePersonalEmailPromptSkip();
			Factory.Save();
			TestDateAttribute.AddDays(8);
			Assert("Precondition", contact.Person.PER_EmailAddress.IsEmpty);
			Assert("Precondition", !personAsEDI.ShouldSkipPersonalEmailPrompt());
			var descriptor = new SetPersonalEmailRoutingDescriptor(contact);
			AssertEquals("Skip has expired", true, descriptor.IsRoutingRequired);
		}

		public void TestIsRoutingRequired_SkipPermanent()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			Factory.Save();
			var personAsEDI = contact.Person as EDIGlbPerson;
			personAsEDI.StorePersonalEmailPromptSkip(true);
			Factory.Save();
			Assert("Precondition", contact.Person.PER_EmailAddress.IsEmpty);
			Assert("Precondition", personAsEDI.ShouldSkipPersonalEmailPrompt());
			var descriptor = new SetPersonalEmailRoutingDescriptor(contact);
			AssertEquals("Personal email setting is permanently skipped", false, descriptor.IsRoutingRequired);
		}

		[TestDate(2020, 4, 15)]
		[TestUtcOffset(0, 0, 0)]
		public void TestIsRoutingRequired_TokenExists()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			Factory.Save();
			Assert("Precondition", contact.Person.PER_EmailAddress.IsEmpty);
			var descriptor = new SetPersonalEmailRoutingDescriptor(contact);
			AssertEquals("No personal email", true, descriptor.IsRoutingRequired);
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			accessControl.CreateLimitedToken(AccessTokenTypes.RegisterPersonalEmail, new AccessTokenInfo("aaa@test.com", contact.PK.ToGuid(), OrgContactSchema.Constants.Prefix), TimeSpan.FromHours(24), 1);
			descriptor = new SetPersonalEmailRoutingDescriptor(contact);
			AssertEquals("Token exists", false, descriptor.IsRoutingRequired);
			var token = Factory.LoadTop1<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_ParentId, contact.PK));
			token.SAT_ExpiresAt = ZDateTime.UtcNow;
			Factory.Save();
			AssertEquals(new ZDateTime(2020, 4, 15), token.SAT_ExpiresAt);
			TestDateAttribute.AddDays(2);
			descriptor = new SetPersonalEmailRoutingDescriptor(contact);
			AssertEquals("Token is expired", true, descriptor.IsRoutingRequired);
		}
	}
}
