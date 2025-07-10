using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class APPaymentApprovalWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.APPaymentApprovalWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("489E2B0D-66A0-4760-832A-347F9BAF6237", "AP Payment Approval"); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.APPaymentProcessing; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(PaymentApprovalBase); }
		}

		public override BusinessContext[] DocumentBusinessContext => new BusinessContext[] { BusinessContext.PaymentApproval };

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy;
		}

		public override bool SupportsApplyWorkflowTemplate
		{
			get { return true; }
		}

		public override bool SupportsSetFieldTriggerAction(IBaseTrigger trigger, IBusiness bizo)
		{
			return false;
		}

		public override bool SupportsWorkflowTriggerActionUniversalTransactionBatchXML
		{
			get { return false; }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool AreTasksCompanySpecific
		{
			get { return true; }
		}

#if DEBUG

		public override BusinessObject GetBizOForTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData(typeof(APPaymentApprovalWithAuthorisation));
		}

#endif
	}
}
