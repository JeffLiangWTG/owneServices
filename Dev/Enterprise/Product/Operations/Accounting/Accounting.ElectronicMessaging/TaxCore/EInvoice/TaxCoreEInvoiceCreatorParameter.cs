using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using UniversalTransactionInfo = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	public class TaxCoreEInvoiceCreatorParameter
	{
		public UniversalTransactionInfo UniversalTransaction { get; set; }

		public GlbStaff CreateUser { get; set; }

		public EInvoicingCertificateCredential CertificateCredential { get; set; }

		public ZString TaxFileCode { get; set; }

		public ZString TFNCode { get; set; }

		public ZString CountryCode { get; set; }

		public IAdditionalTransactionInfoForTaxCoreEInvoice AdditionalTransactionInfo { get; set; }

		public INotifications Notifications { get; set; }

		public GlbBranch Branch { get; set; }
	}
}
