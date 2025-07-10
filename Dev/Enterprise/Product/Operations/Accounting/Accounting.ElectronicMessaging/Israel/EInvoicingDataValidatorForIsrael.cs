using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Israel
{
	public class EInvoicingDataValidatorForIsrael : BaseEInvoicingDataValidator
	{
		public EInvoicingDataValidatorForIsrael(GlbCompany company) : base(company, shouldSendErrorNotificationEmail: true)
		{
		}

		protected override void RunCore(ILogger logger) => ValidateBatchedTransactions(logger);

		public override IReadOnlyCollection<ZString> ValidateTransaction(InvoicingBase transaction, AccEInvoicingTransactionPivot pivot) => new List<ZString>();
	}
}
