
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.NZP.CMS
{
	public class CMSCreditSalesConverter : CMSFlatFileConverter
	{
		public CMSCreditSalesConverter(INotifications notify, BusinessObjectFactory factory) : base(notify, factory)
		{
		}

		protected override bool IsOkToExport(Xsd.TxnHeader header)
		{
			return (header.TxnType != Xsd.TxnType.CRD && !NZPDataRegistry.Instance.InvoiceTermsInTFile.ContainsCode(header.InvTerm));
		}

		protected override string TransactionCode(Xsd.TxnHeader header)
		{
			return Constants.CreditTransactionCode;
		}
	}
}
