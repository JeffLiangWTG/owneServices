using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	/// <summary>
	/// Clears links to TransactionsLines on JobCharges where JobCharges are linked to REV or CST transaction lines
	/// </summary>
	public class ReverseInvoicingTransformer : BaseIntegrationTransformer
	{
		public ReverseInvoicingTransformer(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void TransformCore(TransactionHeaderWithLines transaction)
		{
			Factory.SetContext(BusinessContext.ChargeProcessingForAPTransactionPosting);
			try
			{
				if (transaction is JobRevenueJournal)
				{
					RunTransformationForJRJ((JobRevenueJournal)transaction);
				}
				else if (transaction is InvoicingBase)
				{
					InvoicingBase invoice = (InvoicingBase)transaction;

					if (invoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable)
					{
						RunTransformationForARInvoice(invoice);
					}
					else if (invoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable || invoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.UnapprovedPayableTransactions)
					{
						RunTransformationForAPOrUAInvoice(invoice);
					}
				}
			}
			finally
			{
				Factory.RemoveContext(BusinessContext.ChargeProcessingForAPTransactionPosting);
			}
		}

		void RunTransformationForJRJ(JobRevenueJournal journal)
		{
			if (journal.IsAutoJRJ)
			{
				var aRCharges = Factory.Load<Charge>(new ZQuery(JobChargeSchema.JR_AL_ARLine, journal.Lines.GetPKs()));

				foreach (Charge aRCharge in aRCharges)
				{
					aRCharge.ClearRevenueLink();
					aRCharge.ClearRevenueAmount();
					aRCharge.JR_OH_SellAccount = ZGuid.Empty;

					if (aRCharge.JR_OSCostAmt == 0M && aRCharge.JR_OSSellAmt == 0M && !aRCharge.IsCostPosted && !aRCharge.JR_IsApportioned)
					{
						aRCharge.Delete();
					}
				}

				var aPCharges = Factory.Load<Charge>(new ZQuery(JobChargeSchema.JR_AL_APLine, journal.Lines.GetPKs()));
				var jobConsolCostPKs = new HashSet<ZGuid>();
				foreach (Charge aPCharge in aPCharges)
				{
					aPCharge.ClearCostLink();
					if (!aPCharge.JR_IsApportioned)
					{
						aPCharge.ClearCostAmount();
						aPCharge.JR_OH_CostAccount = ZGuid.Empty;
					}
					else
					{
						aPCharge.JR_GB_InternalBranch = ZGuid.Empty;
						aPCharge.JR_GE_InternalDept = ZGuid.Empty;
						aPCharge.JR_JH_InternalJob = ZGuid.Empty;
						jobConsolCostPKs.Add(aPCharge.JR_E6);
					}

					if (aPCharge.JR_OSCostAmt == 0M && aPCharge.JR_OSSellAmt == 0M && !aPCharge.IsRevenuePosted && !aPCharge.JR_IsApportioned)
					{
						aPCharge.Delete();
					}
				}

				if (jobConsolCostPKs.Count > 0)
				{
					var jobConsolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.PK, jobConsolCostPKs));
					foreach (var jobConsolCost in jobConsolCosts)
					{
						if (jobConsolCost.E6_AH_APInvoice == journal.PK)
						{
							jobConsolCost.E6_AH_APInvoice = jobConsolCost.ApportionmentCharges.Cast<ApportionSplitCharge>()
								.Where(t => t.IsCostPostedWithJobRevenueJournal)
								.Select(t => t.APLine.AL_AH)
								.FirstOrDefault();
						}
					}
				}

				Charges = aRCharges.Union(aPCharges).ToArray();
			}
			else
			{
				var aRCharges = TransformARCharges(journal);
				var aPCharges = TransformAPCharges(journal);
				Charges = aRCharges.Union(aPCharges).ToArray();
			}
		}

		Charge[] TransformARCharges(JobRevenueJournal journal)
		{
			var aRCharges = Factory.Load<Charge>(GetFilter(journal, JobChargeSchema.JR_AL_ARLine));
			foreach (Charge aRCharge in aRCharges)
			{
				aRCharge.ClearRevenueLink();
				aRCharge.ClearRevenueAmount();
				if (aRCharge.JR_OSCostAmt == 0M && aRCharge.JR_OSSellAmt == 0M && !aRCharge.IsCostPosted && !aRCharge.JR_IsApportioned)
				{
					aRCharge.Delete();
				}
			}
			return aRCharges;
		}

		Charge[] TransformAPCharges(JobRevenueJournal journal)
		{
			var aPCharges = Factory.Load<Charge>(GetFilter(journal, JobChargeSchema.JR_AL_APLine));
			foreach (Charge aPCharge in aPCharges)
			{
				aPCharge.ClearCostLink();
				aPCharge.ClearCostAmount();
				if (aPCharge.JR_OSCostAmt == 0M && aPCharge.JR_OSSellAmt == 0M && !aPCharge.IsRevenuePosted)
				{
					aPCharge.Delete();
				}
			}
			return aPCharges;
		}

		void RunTransformationForARInvoice(InvoicingBase invoice)
		{
			var surchargeLines = ObjectFactory.Get<ISurchargeCalculator>().GetSurchargeLines(invoice);

			new CFXJournalReverser().ReverseJournal(invoice);
			Charges = (Charge[])Factory.Load(typeof(Charge), GetFilter(invoice, JobChargeSchema.JR_AL_ARLine));
			foreach (Charge aRCharge in Charges)
			{
				if (aRCharge.InvoicingJob != null)
				{
					aRCharge.InvoicingJob.ClearProfitShareReferences();
				}

				if (surchargeLines.Any(x => x.PK == (ZGuid)aRCharge.JR_AL_ARLineInfo.OriginalValue))
				{
					using (aRCharge.Calculations.SuspendCalculations())
					{
						aRCharge.JR_OH_SellAccount = ZGuid.Empty;
						aRCharge.JR_EstimatedRevenue = 0m;
						aRCharge.JR_LocalSellAmt = 0m;
						aRCharge.JR_OSSellAmt = 0m;
					}
				}

				aRCharge.ClearRevenueLink();

				if (AccountingConfigurationRegistry.Instance.ResetAddressInChargeWhenARInvoiceReversed.Value)
				{
					aRCharge.ClearSellAddress();
				}

				if (aRCharge.ChargeCode.PK == AccountingMasterFilesRegistry.Instance.RevenueTaxExpenseRecoveryChargeCode.GetFallBackValueAtAllLevels(aRCharge.JR_GC.ToGuid(), Guid.Empty, Guid.Empty))
				{
					aRCharge.Delete();
				}
			}
		}

		void RunTransformationForAPOrUAInvoice(InvoicingBase invoice)
		{
			bool isCreditNote = invoice is CreditNote;
			Dictionary<ZGuid, JobConsolCost> relatedJobConsolCosts = new Dictionary<ZGuid, JobConsolCost>();

			Charges = GetAPLinkedCharges(invoice);
			var consolCosts = new APInvoiceConsolCostCollection(Factory, invoice);
			try
			{
				consolCosts.Load();

				foreach (Charge aCharge in Charges)
				{
					using (aCharge.GetValidationSuspender())
					{
						var line = GetRelatedLine(invoice, aCharge);
						if (line != null)
						{
							JobConsolCost cost = null;
							if (aCharge.JR_E6.IsValid)
							{
								cost = (JobConsolCost)consolCosts.FindByPK(aCharge.JR_E6);
							}
							bool isCostNegative = cost != null && cost.E6_OSCostAmount < 0;
							bool isCreditNoteWithoutCostAndChargeAmountIsPositive = isCreditNote && cost == null && aCharge.JR_LocalCostAmt > 0;
							bool doResetAmounts = isCreditNoteWithoutCostAndChargeAmountIsPositive || isCostNegative;
							ZDecimal invoiceLocalCostAmount = doResetAmounts ? 0 : aCharge.JR_LocalCostAmt;
							ZDecimal invoiceOSCostAmount = doResetAmounts ? 0 : aCharge.JR_OSCostAmt;
							aCharge.ClearCostLink();
							aCharge.ClearCostAmount();
							aCharge.JR_APInvoiceNum = ZString.Empty;
							aCharge.JR_APInvoiceDate = ZDateTime.Empty;
							aCharge.JR_APDocumentReceivedDate = ZDateTime.Empty;
							aCharge.JR_PaymentDate = ZDateTime.Empty;
							aCharge.JR_CostReference = line.GetCostReferenceFromDescriptionIfAny();
							aCharge.JR_OSCostAmt = invoiceOSCostAmount;
							aCharge.JR_LocalCostAmt = invoiceLocalCostAmount;

							if (aCharge.InvoicingJob != null)
							{
								aCharge.InvoicingJob.ClearProfitShareReferences();
							}

							if (cost != null && !relatedJobConsolCosts.ContainsKey(aCharge.JR_E6))
							{
								relatedJobConsolCosts.Add(aCharge.JR_E6, cost);
							}
						}
					}
				}

				foreach (JobConsolCost cost in relatedJobConsolCosts.Values)
				{
					using (cost.GetValidationSuspender())
					{
						cost.E6_InvoiceNum = "";
						cost.E6_InvoiceDate = ZDateTime.Empty;
						cost.E6_DocumentReceivedDate = ZDateTime.Empty;
						cost.E6_CostReference = ZString.Empty;
						cost.E6_PaymentDate = ZDateTime.Empty;
						cost.E6_PaymentType = "";
						cost.E6_AB_BankAccount = ZGuid.Empty;
						cost.E6_AK_ChequeBook = ZGuid.Empty;
						// Postpone clearing E6_AH_APInvoice to avoid creating new Default Charges when Consol Cost has InvoicingBaseConsolCostCalculationStrategy
						cost.E6_AH_APInvoice = ZGuid.Empty;
						if (cost.E6_OSCostAmount < 0)
						{
							cost.E6_OSCostAmount = 0M;
							cost.E6_LocalCostAmount = 0M;
						}
						cost.ApportionGSTCharges();
					}
				}
			}
			finally
			{
				consolCosts.RemoveAll();
			}

			UACreditNote uaCreditNote = invoice as UACreditNote;
			if (uaCreditNote != null && uaCreditNote.IsRelatedToClaim)
			{
				ZQuery apInvoiceQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, invoice.AH_TransactionBelongsToGroup);
				apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
				apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
				apInvoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);
				APInvoice apInvoice = Factory.LoadTop1<APInvoice>(apInvoiceQuery);
				if (apInvoice != null)
				{
					apInvoice.AH_TransactionBelongsToGroup = ZGuid.Empty;
					invoice.AH_TransactionBelongsToGroup = ZGuid.Empty;
				}
			}
		}

		InvoicingLineBase GetRelatedLine(InvoicingBase originalInvoice, Charge charge)
		{
			if (originalInvoice.ReverseInvoice != null)
			{
				foreach (InvoicingLineBase line in originalInvoice.ReverseInvoice.Lines)
				{
					if (charge.JR_AL_APLine == line.CopiedFromPK)
					{
						return line;
					}
				}
			}
			else if (originalInvoice.IsUAInvoiceOrCreditNote)
			{
				return (InvoicingLineBase)originalInvoice.Lines.FindByPK(charge.JR_AL_APLine);
			}

			return null;
		}

		Charge[] GetAPLinkedCharges(InvoicingBase invoice)
		{
			return Factory.Load<Charge>(GetFilter(invoice, JobChargeSchema.JR_AL_APLine));
		}

		public Charge[] Charges;

		ZQuery GetFilter(TransactionHeaderWithLines transaction, SchemaColumn column)
		{
			ZQuery filter = new ZQuery();
			List<ZGuid> pkList = new List<ZGuid>();
			foreach (TransactionLine line in transaction.Lines)
			{
				pkList.Add(line.PK);
			}
			filter.AddToFilter(column, pkList);
			return filter;
		}
	}
}

