using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public abstract class AccComplianceDocumentWorkflowDescriptor : WorkflowDescriptor
	{
		public override bool RequiresClient => false;

		public override bool RequiresBranch => true;

		public override bool RequiresDepartment => true;

		public override bool SupportsEventTracking => true;

		public override bool IncludeWorkflowTriggerActionXMLDebtorBalance => false;

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy;
		}
	}
}
