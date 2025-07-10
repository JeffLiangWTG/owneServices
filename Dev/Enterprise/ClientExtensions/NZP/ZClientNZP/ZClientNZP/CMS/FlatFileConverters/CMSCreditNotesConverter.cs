using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.NZP.CMS
{
	public class CMSCreditNotesConverter : CMSFlatFileConverter
	{
		public CMSCreditNotesConverter(INotifications notify, BusinessObjectFactory factory) : base(notify, factory)
		{
		}

		protected override bool IsOkToExport(Xsd.TxnHeader header)
		{
			return (header.TxnType == Xsd.TxnType.CRD);
		}

		protected override string TransactionCode(Xsd.TxnHeader header)
		{
			return (header.InvTerm == Core.Constants.InvoiceTerms.CashOnDelivery) ? Constants.CashCreditNotesTransactionCode : Constants.CreditNotesTransactionCode;
		}

		protected override void AfterExport(FlatFileDataRowCollection dataRows)
		{
			foreach (CMSFlatFileDataRow row in dataRows)
			{
				row.LineTotal = Math.Abs(row.LineTotal);
				row.FinalPrice = Math.Abs(row.FinalPrice);
				row.TaxTotal = Math.Abs(row.TaxTotal);
			}
		}
	}
}
