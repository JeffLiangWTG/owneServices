using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class TermsAndConditionsRoutingDescriptorTest : TestCaseWithFactory
	{
		public void TestRoutingUrl()
		{
			var contact = Factory.New<OrgContact>();
			var descriptor = new TermsAndConditionsRoutingDescriptor(contact);
			AssertEquals("Routing page url", "~/Login/TermsAndConditions.aspx", descriptor.RoutingUrl.ToString());
		}

		public void TestIsRoutingRequired()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			Factory.Save();
			var contractTemplate = new NotificationEmailTemplate();
			contractTemplate.EmailBody = "Sample web contract";
			EDIDataRegistry.Instance.MyAccountTermsAndConditionsNotificationEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, contractTemplate);
			EDIDataRegistry.Instance.MyAccountContactTermsAndConditionsContent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, contractTemplate);
			var descriptor = new TermsAndConditionsRoutingDescriptor(contact);
			AssertEquals("User hasn't accepted either org or contact terms", true, descriptor.IsRoutingRequired);
			new EDIOrgHeaderWebContract(contact).SignWebContract("aaa");
			descriptor = new TermsAndConditionsRoutingDescriptor(contact);
			AssertEquals("User hasn't accepted contact terms", true, descriptor.IsRoutingRequired);
			new EDIOrgContactWebContract(contact).SignWebContract("aaa");
			descriptor = new TermsAndConditionsRoutingDescriptor(contact);
			AssertEquals("User has accepted both terms", false, descriptor.IsRoutingRequired);
		}
	}
}