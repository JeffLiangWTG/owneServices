using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.eNett_Integration
{
	class eNettTransactionLineBuilder : TransactionLineBuilder
	{
		public eNettTransactionLineBuilder(INotificationManager notifier, TransactionBuilderConfig config)
			: base(notifier, config)
		{
		}

		protected override ZString GetXMLChargeCodeValue(TxnLine xmlInvoiceLine)
		{
			ZString result = xmlInvoiceLine.eNettChargeCodeMapping;
			if (result.IsEmpty)
			{
				result = xmlInvoiceLine.ChargeCode;
			}
			return result;
		}

		protected override OrgHeader GetOrganisationForChargeCodeMapping(InvoicingLineBase invoiceLine)
		{
			OrgHeader result = null;
			if (AccountingConfigurationRegistry.Instance.ENettRegistration.Value != null)
			{
				ZGuid orgPK = AccountingConfigurationRegistry.Instance.ENettRegistration.Value.OrganisationPK;
				if (orgPK.IsValid)
				{
					result = invoiceLine.Factory.Load<OrgHeader>(orgPK);
				}
			}
			return result;
		}
	}
}
