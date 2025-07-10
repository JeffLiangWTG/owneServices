using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.MailManager.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Mail.Module
{
	public class ImplementationMailItemModule : EDIWorkTaskMailModule<EDIProject>
	{
		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return EDISecurityCheckpoints.ImplementationEmails; }
		}

		public override ModuleIdentifier ID
		{
			get { return ClientModuleRegistration.ImplementationEmails; }
		}

		protected override string WorkTaskTypeName
		{
			get { return "installation project"; }
		}

		protected override IReadOnlyList<string> EmailAddressesToIgnoreWhenReplyingAll
		{
			get
			{
				return new string[]
					{
						EDIDataRegistry.Instance.ImplementationDefaultFromEmailAddress.Value,
						"implementation@edi.com.au"
					};
			}
		}

		protected override CustomerServiceEmail GetNewServiceEmail(MailItem selectedMailItem)
		{
			return new CustomerServiceEmail(selectedMailItem, EDIDataRegistry.Instance.ImplementationDefaultFromEmailAddress.Value, IncidentConstants.ImplementationDefaultReplyToName);
		}

		protected override EDIMailItemCollection GetNewEDIMailItemCollection(BusinessObjectFactory factory)
		{
			return new ImplementationMailItemCollection(factory);
		}

		protected override ControllerID WorkTaskControllerID
		{
			get { return ControllerIDs.Project; }
		}

		protected override ModuleIdentifier WorkTaskModuleID
		{
			get { return ModuleIDs.Project; }
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}
	}
}
