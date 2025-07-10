using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class APInvoicingWorkflowDescriptor : InvoicingBaseWorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.APInvoiceCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("cc4d2a4b-964f-429d-a105-8d0ff34f4a00", "AP Invoice"); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.APInvoice; }
		}

		public override bool SupportsWorkflowTriggerActionXML
		{
			get { return false; }
		}

		public override bool SupportsTransactionAllocationAndPost => true;

		public override BusinessContext[] DocumentBusinessContext => new BusinessContext[] { BusinessContext.APInvoice };

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Email;
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source)
		{
			var action = source.Action;
			IProcessor result = null;
			InvoicingBase invoice = (InvoicingBase)source.Job;

			if (WorkflowTriggerActionTypeConstants.IsStandardXml(action.PQ_TriggerType))
			{
				var xmlModes = GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.XML);

				result = GetWorkflowTriggerActionForXmlActionType(invoice, xmlModes, action);
			}
			else
			{
				result = base.GetWorkflowTriggerActionCore(source);
			}
			return result;
		}

#if DEBUG

		public override BusinessObject GetBizOForTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData(typeof(APInvoice));
		}

#endif
	}
}
