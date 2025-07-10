using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Mail.Module
{
	public class CustomerServiceMailItemModule : EDIWorkTaskMailModule<SupportIncident>
	{
		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return EDISecurityCheckpoints.SupportEmails; }
		}

		public override ModuleIdentifier ID
		{
			get { return ClientModuleRegistration.EDICustomerServiceEmails; }
		}

		protected override string WorkTaskTypeName
		{
			get { return "incident"; }
		}

		protected override IReadOnlyList<string> EmailAddressesToIgnoreWhenReplyingAll
		{
			get
			{
				return new string[]
					{
						"support@cargowise.com",
						"support@edi.com.au",
						"support@wisetechglobal.com"
					};
			}
		}

		protected override EDIMailItemCollection GetNewEDIMailItemCollection(BusinessObjectFactory factory)
		{
			return new CustomerServiceMailItemCollection(factory);
		}

		protected override ControllerID WorkTaskControllerID
		{
			get { return ClientControllerRegistration.SupportIncident; }
		}

		protected override ModuleIdentifier WorkTaskModuleID
		{
			get { return ClientModuleRegistration.SupportIncident; }
		}
	}
}
