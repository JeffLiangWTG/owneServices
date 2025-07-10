using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.TaxFramework.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework
{
	public interface IWithholdingAPJournalCreator
	{
		ITaxDetails Create(BusinessObjectFactory factory, InvoicingBase invoice, ZDate date);
	}

	public interface ITaxDetails
	{
		Dictionary<APJournal, IMatchTransactionDetails> JournalDetails { get; }
		ZDecimal NotionalWHT { get; }
		ZDecimal RealizedWHT { get; }
	}

	public class WithholdingAPJournalCreator : IWithholdingAPJournalCreator
	{
		ITaxDetails IWithholdingAPJournalCreator.Create(BusinessObjectFactory factory, InvoicingBase invoice, ZDate date)
		{
			var journalDetails = new Dictionary<APJournal, IMatchTransactionDetails>();
			var taxParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
			var whtAmountsWithJournalDetails = ObjectFactory.Get<ITaxProcessor>().GetPaymentRetentionMatchTransactionDetails(taxParent);

			if (whtAmountsWithJournalDetails != default && whtAmountsWithJournalDetails.MatchDetails?.Length > 0)
			{
				foreach (IMatchTransactionDetails journalDetail in whtAmountsWithJournalDetails.MatchDetails)
				{
					var journal = factory.New<APJournal>();
					journal.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.PaymentBasisWithholding;
					journal.AH_OH = invoice.AH_OH;
					journal.AH_AG = journalDetail.GLAccountPK;
					journal.AH_RX_NKTransactionCurrency = journalDetail.Currency;
					journal.DebitCreditSign = journalDetail.OSAmount > 0 ? DebitCreditDataEntry.CR : DebitCreditDataEntry.DR;
					((IMatching)journal).OSPartialPaymentAmount = journalDetail.OSAmount;
					journal.AH_OSExTaxAmount = Math.Abs(journalDetail.OSAmount);
					journal.AH_LocalExTaxAmount = Math.Abs(journalDetail.LocalAmount);
					journal.AH_Desc = Res.GetString("08774F68-072B-41F1-A527-8F6FF25BAA0C", "{2} Withholding AP Journal - {0} {1}", invoice.AH_TransactionType, invoice.AH_TransactionNum, Core.Constants.TransactionCategory.Codes.PaymentBasisWithholding);
					journal.AH_GB = journalDetail.BranchPK;
					journal.AH_GE = journalDetail.DepartmenPK;
					journal.AH_PostDate = Journal.Journal.GetPostDate(date);
					journal.AH_InvoiceDate = date;
					journal.AH_TransactionCreatedByMatching = false;
					journal.EnableCheckSubAccountsForGLHeader = false;

					journalDetail.PK = journal.PK;
					journalDetail.RealisationDate = journal.AH_PostDate.Date;

					journalDetails.Add(journal, journalDetail);
				}

				return new TaxDetails()
				{
					RealizedWHT = whtAmountsWithJournalDetails.RealizedWHT,
					NotionalWHT = whtAmountsWithJournalDetails.NotionalWHT,
					JournalDetails = journalDetails
				};
			}

			return null;
		}

		class TaxDetails : ITaxDetails
		{
			public Dictionary<APJournal, IMatchTransactionDetails> JournalDetails { get; set; }
			public ZDecimal NotionalWHT { get; set; }
			public ZDecimal RealizedWHT { get; set; }
		}
	}
}
