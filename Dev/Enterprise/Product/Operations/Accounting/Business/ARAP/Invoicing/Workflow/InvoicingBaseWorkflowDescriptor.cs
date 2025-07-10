using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract class InvoicingBaseWorkflowDescriptor : WorkflowDescriptor
	{
		public override Type WorkflowProviderType
		{
			get { return typeof(InvoicingBase); }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool AreTasksCompanySpecific
		{
			get { return true; }
		}

		public override bool SupportsSetFieldTriggerAction(IBaseTrigger trigger, IBusiness bizo)
		{
			return false;
		}

		public override bool SupportsOtherCompanyAPInvoiceImport
		{
			get { return false; }
		}

		public override bool SupportsWorkflowTriggerActionXML
		{
			get { return true; }
		}

		public override bool SupportsWorkflowTriggerActionUniversalTransactionXML
		{
			get { return true; }
		}

		protected IValueObjectDataAdapter FinancialInvoiceDataAdapter
		{
			get
			{
				if (financialInvoiceDataAdapter == null)
				{
					financialInvoiceDataAdapter = (IValueObjectDataAdapter)ObjectFactory.Get("UnapprovedTransactionFinancialInvoiceDataAdapter");
				}
				return financialInvoiceDataAdapter;
			}
		}
		IValueObjectDataAdapter financialInvoiceDataAdapter;

		internal XmlMessageDeliver GetWorkflowTriggerActionForXmlActionType(InvoicingBase invoice, MessageProcessorCommunicationModesResult xmlModes, ProcessTaskNotification action)
		{
			return new XmlMessageDeliver(xmlModes, invoice, invoice, FinancialInvoiceDataAdapter, action);
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, BusinessObject bizObj, ZString partyType)
		{
			InvoicingBase invoice = bizObj as InvoicingBase;
			if ((partyType == MessageRecipientPartyTypeList.Codes.InvoiceDebtor || partyType == MessageRecipientPartyTypeList.Codes.CarrierMessagingDebtor)
				&& invoice != null)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(invoice.Header, ZString.Empty));
			}
		}
	}
}
