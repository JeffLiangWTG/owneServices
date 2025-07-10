using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(MyAccountWebContract))]
	public class EDIOrgContactWebContractTest : MyAccountWebContractTest
	{
		protected override MyAccountWebContract GetMyAccountWebContractForTest(OrgContact contact) => new EDIOrgContactWebContract(contact);

		protected override NotificationEmailTemplateRegistryItem GetNotificationEmailTemplateRegistryItem() => throw new NotImplementedException();

		protected override TermsAndConditionsRegistryItem GetTermsAndConditionsRegistryItem() => EDIDataRegistry.Instance.MyAccountContactTermsAndConditionsContent;

		protected override void SetEmailSender(string name, string address) => throw new NotImplementedException();

		protected override bool ShouldTestNotificationEmail => false;
	}

	[TestedType(typeof(EDIOrgContactWebContract))]
	public class EDIOrgContactWebContractBizObjTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new EDIOrgContactWebContract(Factory.NewWithValidTestData<OrgContact>());
	}
}
