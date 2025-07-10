using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public abstract class ReceiptPaymentBaseWorkflowDescriptor : WorkflowDescriptor
	{
		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy;
		}

		public override bool SupportsSetFieldTriggerAction(IBaseTrigger trigger, IBusiness bizo)
		{
			return false;
		}

		public override bool SupportsApplyWorkflowTemplate
		{
			get { return true; }
		}

		public override bool SupportsWorkflowTriggerActionUniversalTransactionBatchXML
		{
			get { return true; }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool AreTasksCompanySpecific
		{
			get { return true; }
		}
	}
}
