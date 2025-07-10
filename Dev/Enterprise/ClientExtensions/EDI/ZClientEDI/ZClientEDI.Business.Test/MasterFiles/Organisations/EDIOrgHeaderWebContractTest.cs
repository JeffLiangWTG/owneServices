using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDIOrgHeaderWebContract))]
	public class EDIOrgHeaderWebContractTest : MyAccountWebContractTest
	{
		protected override MyAccountWebContract GetMyAccountWebContractForTest(OrgContact contact) => new EDIOrgHeaderWebContract(contact);

		protected override NotificationEmailTemplateRegistryItem GetNotificationEmailTemplateRegistryItem() => EDIDataRegistry.Instance.MyAccountTermsAndConditionsNotificationEmailTemplate;

		protected override TermsAndConditionsRegistryItem GetTermsAndConditionsRegistryItem() => EDIDataRegistry.Instance.MyAccountTermsAndConditionsContent;

		protected override void SetEmailSender(string name, string address)
		{
			EDIDataRegistry.Instance.MyAccountTermsAndConditionsNotificationEmailSenderName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, name);
			EDIDataRegistry.Instance.MyAccountTermsAndConditionsNotificationEmailSenderAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, address);
		}
	}

	[TestedType(typeof(EDIOrgHeaderWebContract))]
	public class EDIOrgHeaderWebContractBizObjTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new EDIOrgHeaderWebContract(Factory.NewWithValidTestData<OrgContact>());
	}
}
