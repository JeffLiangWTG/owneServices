using System;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Mail.Module
{
	[TestedType(typeof(CustomerServiceMailItemModule))]
	class CustomerServiceMailItemModuleTest : EDIWorkTaskMailModuleTestCase<SupportIncident>
	{
		protected override Type ExpectedWorkTaskFormType
		{
			get
			{
				return typeof(SupportIncidentForm);
			}
		}

		protected override string ExpectedWorkTaskTypeName
		{
			get
			{
				return "Incident";
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ClientModuleRegistration.EDICustomerServiceEmails;
		}

		protected override string[] ExpectedEmailAddressesToIgnore
		{
			get
			{
				return new string[] { "support@cargowise.com", "Support <support@cargowise.com>", "\"Customer Service\" <suPPort@edi.com.au>", "SUPPORT@edi.com.au", "support@wisetechglobal.com", "Support <support@wisetechglobal.com>", };
			}
		}

		protected override string ExpectedMailApplicationCode
		{
			get
			{
				return EDIMailApplication.CustomerService;
			}
		}

		protected override Type ExpectedGridCollectionType
		{
			get
			{
				return typeof(CustomerServiceMailItemCollection);
			}
		}

		protected override CargoWise.Schema.SchemaPKColumn WorkTaskPKColumn
		{
			get
			{
				return Enterprise.ZArchitecture.Schema.IncidentMainSchema.PK;
			}
		}
	}
}
