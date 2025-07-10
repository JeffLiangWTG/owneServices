using System;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Mail.Module
{
	[TestedType(typeof(ImplementationMailItemModule))]
	class ImplementationMailItemModuleTest : EDIWorkTaskMailModuleTestCase<EDIProject>
	{
		public void TestSetEmailAddressAndDisplayName()
		{
			using (ImplementationMailItemModule frm = new ImplementationMailItemModule())
			{
				MailItem item = Factory.NewWithValidTestData<MailItem>();
				item.MI_From = "From@Domain.Com";
				CustomerServiceEmail mailitem = frm.SetEmailAddressAndDisplayName(item);
				AssertEquals("Check that base got called", item.MI_From + ";", mailitem.ToDisplayName);
				AssertEquals("Got our Default email address, to show override worked", EDIDataRegistry.Instance.ImplementationDefaultFromEmailAddress.Value, mailitem.DefaultFromEmailAddress);
			}
		}

		protected override Type ExpectedWorkTaskFormType
		{
			get
			{
				return typeof(EDIProjectForm);
			}
		}

		protected override string ExpectedWorkTaskTypeName
		{
			get
			{
				return "Installation Project";
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ClientModuleRegistration.ImplementationEmails;
		}

		protected override string[] ExpectedEmailAddressesToIgnore
		{
			get
			{
				return new string[] { EDIDataRegistry.Instance.ImplementationDefaultFromEmailAddress.Value, IncidentConstants.ImplementationDefaultReplyToName + " <" + EDIDataRegistry.Instance.ImplementationDefaultFromEmailAddress.Value + ">", "\"Installation team\" <impLementation@edi.com.au>", "IMPLEMENTATION@edi.com.au" };
			}
		}

		protected override string ExpectedMailApplicationCode
		{
			get
			{
				return EDIMailApplication.Implementation;
			}
		}

		protected override Type ExpectedGridCollectionType
		{
			get
			{
				return typeof(ImplementationMailItemCollection);
			}
		}

		public void TestShowRecentItems()
		{
			using (var module = new ImplementationMailItemModule())
			{
				Assert("Should not show recent", !module.ShowRecentItems);
			}
		}

		protected override CargoWise.Schema.SchemaPKColumn WorkTaskPKColumn
		{
			get
			{
				return Enterprise.ZArchitecture.Schema.WorkProjectSchema.PK;
			}
		}

		protected override void SaveCreatedWorkTask(EDIProject workTask)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			workTask.ChangeClientOrganisation(org);
			org.Factory.Save();
			workTask.Factory.Save();
		}
	}
}
