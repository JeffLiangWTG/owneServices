using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class ARInvoicingWorkflowDescriptor : InvoicingBaseWorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.ARInvoiceCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("e708d59a-42ee-431c-8266-9a114baa54f0", "AR Invoice"); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.ARInvoice; }
		}

		public override BusinessContext[] DocumentBusinessContext => new BusinessContext[] { BusinessContext.ARInvoice };

		public override MessageRecipientPartyType SupportedMessageRecipientPartiesForSpecificAction(ZString triggerAction)
		{
			var result = SupportedMessageRecipientPartiesInCommon;
			if (triggerAction == WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML
				&& GlbCompany.CurrentCompany.OrgProxy.OH_IsShippingProvider)
			{
				result = result | MessageRecipientPartyType.CarrierMessagingDebtor;
			}
			return result;
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return SupportedMessageRecipientPartiesInCommon;
		}

		MessageRecipientPartyType SupportedMessageRecipientPartiesInCommon => MessageRecipientPartyType.InvoiceDebtor |
							MessageRecipientPartyType.OrgProxy |
							MessageRecipientPartyType.Email;

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(base.GetAdditionalWorkflowTriggerActionTypeList(trigger, parent));
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendARInvoice, WorkflowTriggerActionTypeConstants.Descriptions.SendARInvoice);
			result.AddPair(WorkflowTriggerActionTypeConstants.Codes.GenerateARInvoiceToEdocs, WorkflowTriggerActionTypeConstants.Descriptions.GenerateARInvoiceToEdocs);
			return result;
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
			else if (WorkflowTriggerActionTypeConstants.IsDebtorBalanceXml(action.PQ_TriggerType) && invoice != null && invoice.Header != null)
			{
				var xmlModes = GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.XMB);
				result = new XmlMessageDeliver(xmlModes, new DebtorBalanceRecordForExport(invoice.Header, invoice.Factory), invoice, new DebtorBalanceValueObjectDataAdapter(), action);
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
			return factory.NewWithValidTestData(typeof(ARInvoice));
		}

#endif
	}
}
