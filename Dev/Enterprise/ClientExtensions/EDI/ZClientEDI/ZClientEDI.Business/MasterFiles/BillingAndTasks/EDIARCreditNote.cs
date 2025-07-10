using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EDIARCreditNote : ARCreditNote
	{
		public EDIARCreditNote(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new EDICreditNoteValidation(this);
		}

		protected override Type TypeOfReverseTransaction
		{
			get { return typeof(EDIARInvoice); }
		}

		public bool DisableExchangeRateValidation { get; set; }
	}
}
