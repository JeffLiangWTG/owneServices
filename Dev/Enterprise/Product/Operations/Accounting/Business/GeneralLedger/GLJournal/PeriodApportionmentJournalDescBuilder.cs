using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using AccGenericCharge = Enterprise.Accounting.Business.GenericCharge.GenericCharge;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournal
{
	public class PeriodApportionmentJournalDescBuilder
	{
		public PeriodApportionmentJournalDescBuilder(bool isMasterJournal)
		{
			this.IsMasterJournal = isMasterJournal;
		}

		public bool IsMasterJournal { get; }
		public InvoicingBase InvoicingBase { get; set; }
		public AccGLHeader LineAccount { get; set; }
		public AccGenericCharge Charge { get; set; }
		public ZString Currency { get; set; }
		public ZString RefCurrencyCode { get; set; }
		public ZDecimal OSAmountClearedToDate { get; set; }
		public ZDecimal LocalAmountClearedToDate { get; set; }
		public ZDecimal OSAmountYetToBeCleared { get; set; }
		public ZDecimal LocalAmountYetToBeCleared { get; set; }
		public ZDecimal OSNonRecoverableTaxAmountClearedToDate { get; set; }
		public ZDecimal LocalNonRecoverableTaxAmountClearedToDate { get; set; }
		public ZDecimal OSNonRecoverableTaxAmountYetToBeCleared { get; set; }
		public ZDecimal LocalNonRecoverableTaxAmountYetToBeCleared { get; set; }
		public ZInt PostPeriod { get; set; }
		public bool PresentTaxAmountNotRecoverable { get; set; }

		public ZString BuildHeaderDescription()
		{
			if (IsMasterJournal)
			{
				return Res.GetString(
					"976e6308-6b41-4a03-b250-49262cfc4fb1",
					"MASTER JOURNAL FOR {0} {1} {2}",
					InvoicingBase.AH_Ledger,
					InvoicingBase.AH_TransactionType,
					InvoicingBase.InvoiceNumber
				);
			}
			else
			{
				return Res.GetString(
					"21b6f1ea-d38c-45df-8227-59708fee957b",
					"Sub Journal For {0} {1} {2} Period {3}",
					InvoicingBase.AH_Ledger,
					InvoicingBase.AH_TransactionType,
					InvoicingBase.InvoiceNumber,
					PostPeriod
				);
			}
		}

		public ZString BuildLineDescription()
		{
			if (IsMasterJournal)
			{
				return Res.GetString(
					"976e6308-6b41-4a03-b250-49262cfc4fb1",
					"MASTER JOURNAL FOR {0} {1} {2}",
					InvoicingBase.AH_Ledger,
					InvoicingBase.AH_TransactionType,
					InvoicingBase.InvoiceNumber
				);
			}
			else
			{
				var line1 = Res.GetString(
					"bb0f164f-ae7b-48de-986a-bb4769968385",
					"{0}",
					LineAccount.AG_DescriptionMultilingual
				);

				var line2 = Res.GetString(
					"24b948b2-e634-484b-a613-79517585ed81",
					"{0} {1} {2}, Organization: {3}",
					InvoicingBase.AH_Ledger,
					InvoicingBase.AH_TransactionType,
					InvoicingBase.InvoiceNumber,
					InvoicingBase.Header?.OH_Code
				);

				var line3 = Res.GetString(
					"4fb66c18-b211-4111-a8f4-abd2b9bec1f9",
					"Charge: {0} GL Account: {1} for period {2}",
					Charge.VC_Code,
					LineAccount.AccountNum,
					PostPeriod
				);

				var line4 = Res.GetString(
					"5d16aa44-d5a6-4b56-a9d2-d09fda6bb9da",
					"Amount Cleared To Date: {0} {1} {2} {3}",
					Currency,
					OSAmountClearedToDate,
					RefCurrencyCode,
					LocalAmountClearedToDate
				);

				var line5 = Res.GetString(
					"b5202d3a-f3df-4da2-89e3-d8e5981c80f6",
					"Amount Yet to be Cleared: {0} {1} {2} {3}",
					Currency,
					OSAmountYetToBeCleared,
					RefCurrencyCode,
					LocalAmountYetToBeCleared
				);

				if (PresentTaxAmountNotRecoverable)
				{
					line4 = Res.GetString(
						"678899AF-C74C-423C-B962-D1C134201A68",
						"{0} (includes Tax Not Recoverable {1} {2} {3} {4})",
						line4,
						Currency,
						OSNonRecoverableTaxAmountClearedToDate,
						RefCurrencyCode,
						LocalNonRecoverableTaxAmountClearedToDate
					);

					line5 = Res.GetString(
						"3A3914EC-10B3-456D-85D3-598D40AE22A9",
						"{0} (includes Tax Not Recoverable {1} {2} {3} {4})",
						line5,
						Currency,
						OSNonRecoverableTaxAmountYetToBeCleared,
						RefCurrencyCode,
						LocalNonRecoverableTaxAmountYetToBeCleared
					);
				}

				return new ZStringBuilder(new string[] { line1, line2, line3, line4, line5 }).ToStringWithNewLineBetweenAppends();
			}
		}
	}
}
