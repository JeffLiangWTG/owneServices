using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Brazil
{
	class EInvoicingDataValidatorForBrazil : BaseEInvoicingDataValidator
	{
		public EInvoicingDataValidatorForBrazil(GlbCompany company)
			: base(company, shouldSendErrorNotificationEmail: true)
		{
		}

		protected override void RunCore(ILogger logger)
			=> ValidateBatchedTransactions(logger);

		public override IReadOnlyCollection<ZString> ValidateTransaction(InvoicingBase transaction, AccEInvoicingTransactionPivot pivot)
		{
			var errorMessages = new List<ZString>();

			Check_ComplianceNumberIsRequired(transaction, errorMessages);

			return errorMessages;
		}

		static void Check_ComplianceNumberIsRequired(InvoicingBase transaction, List<ZString> errors)
		{
			var taxTransactions = transaction.Factory.Load<AccTaxTransaction>(new ZQuery(AccTaxTransactionSchema.ATT_AH, transaction.PK));
			var isCampinas = taxTransactions != null && taxTransactions.Any(x => x.TaxAuthorityCode == "IMCPQ" && x.ATT_TaxSystemCode == "ISS");

			if (!isCampinas && transaction.AH_TransactionReference.IsEmpty)
			{
				errors.Add(Res.GetString("F86F0456-C02B-435E-ABEC-DAAD79E21C4A", "Compliance number is missing."));
			}
		}
	}
}
