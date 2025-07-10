using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	public class ProfitShareMasterFreightInvoiceNegator
	{
		public ProfitShareMasterFreightInvoiceNegator(BusinessObjectFactory postingFactory, AgentChargePostingDetails postingDetails)
		{
			this.PostingDetails = postingDetails;
			this.Factory = postingFactory;
		}

		public bool NegateInvoices(TransactionCreatorHashtable transactions)
		{
			var result = false;
			var distinctARInvoices = new Dictionary<ZGuid, InvoicingBase>();
			foreach (InvoicingBase masterFreightCollectAPInvoice in PostingDetails.AgentInvoices)
			{
				var negatedARFRTInvoice = NegateInvoice(masterFreightCollectAPInvoice);
				distinctARInvoices[negatedARFRTInvoice.PK] = negatedARFRTInvoice;
				result = true;
			}
			foreach (var distinctARInvoice in distinctARInvoices.Values)
			{
				ChangeInvoiceTypeBasedOnTotal(distinctARInvoice);
				if (!transactions.ContainsARTransaction(distinctARInvoice.PK))
				{
					transactions.AddARInvoice(distinctARInvoice);
				}
			}

			return result;
		}

		#region Implementation

		InvoicingBase NegateInvoice(InvoicingBase aPInvoice)
		{
			var agentInvoice = GetARInvoiceForAgent(aPInvoice);
			bool useLocalAmounts = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency == agentInvoice.AH_RX_NKTransactionCurrency && agentInvoice.AH_ExchangeRate == 1M;

			int numberOfLines = aPInvoice.Lines.Count;
			for (int index = 0; index < numberOfLines; index++)
			{
				var line = aPInvoice.Lines[index];
				var newAPLine = (InvoicingLineBase)aPInvoice.Lines.AddNew();
				var chargeCode = line.ChargeCode;
				var glPostingAccounts = line.GetDefaultGLPostingAccounts();
				newAPLine.AL_AG = glPostingAccounts.RevenueAccount;
				newAPLine.AL_GB_TaxBranch = aPInvoice.AH_GB_TaxBranch;
				newAPLine.AL_AT = line.AL_AT;
				newAPLine.SetTaxDateSafe(line.AL_TaxDate);
				newAPLine.AL_A9_VATClass = line.AL_A9_VATClass;
				newAPLine.AL_ExchangeRate = line.AL_ExchangeRate;
				using (newAPLine.GetLocalAmountCalculationSuspender())
				using (newAPLine.GetOSAmountCalculationSuspender())
				{
					newAPLine.AL_OSExTaxAmount = -line.AL_OSExTaxAmount;
					newAPLine.AL_LocalExTaxAmount = -line.AL_LocalExTaxAmount;
				}
				newAPLine.AL_GB = line.AL_GB;
				newAPLine.AL_GE = line.AL_GE;
				newAPLine.AL_OSTaxAmount = -line.AL_OSTaxAmount;
				newAPLine.AL_LocalTaxAmount = -line.AL_LocalTaxAmount;
				newAPLine.AL_GovtChargeCode = line.AL_GovtChargeCode;
				newAPLine.AL_PlaceOfSupply = line.AL_PlaceOfSupply;
				newAPLine.AL_SupplyType = line.AL_SupplyType;

				var newARLine = (InvoicingLineBase)agentInvoice.Lines.AddNew();
				newARLine.AL_AG = glPostingAccounts.RevenueAccount;
				newARLine.AL_GB_TaxBranch = agentInvoice.AH_GB_TaxBranch;
				newARLine.AL_AT = line.AL_AT;
				newARLine.SetTaxDateSafe(line.AL_TaxDate);
				newARLine.AL_A9_VATClass = line.AL_A9_VATClass;
				newARLine.AL_GovtChargeCode = line.AL_GovtChargeCode;
				newARLine.AL_PlaceOfSupply = line.AL_PlaceOfSupply;
				newARLine.AL_SupplyType = line.AL_SupplyType;

				if (agentInvoice is ARInvoice)
				{
					if (useLocalAmounts)
					{
						newARLine.AL_OSExTaxAmount = -line.AL_LocalExTaxAmount;
						newARLine.AL_LocalExTaxAmount = -line.AL_LocalExTaxAmount;
					}
					else
					{
						newARLine.AL_OSExTaxAmount = GetCrossCurrencyConvertedAmount(-line.AL_OSExTaxAmount, line.TransactionCurrency, line.AL_ExchangeRate,
							agentInvoice.TransactionCurrency, agentInvoice.AH_ExchangeRate);
					}
					newARLine.AL_GB = line.AL_GB;
					newARLine.AL_GE = line.AL_GE;
					if (useLocalAmounts)
					{
						newARLine.AL_OSTaxAmount = -line.AL_LocalTaxAmount;
						newARLine.AL_LocalTaxAmount = -line.AL_LocalTaxAmount;
					}
					else
					{
						newARLine.AL_OSTaxAmount = GetCrossCurrencyConvertedAmount(-line.AL_OSTaxAmount, line.TransactionCurrency, line.AL_ExchangeRate,
							agentInvoice.TransactionCurrency, agentInvoice.AH_ExchangeRate);
					}
				}
				else if (agentInvoice is ARCreditNote)
				{
					if (useLocalAmounts)
					{
						newARLine.AL_OSExTaxAmount = line.AL_LocalExTaxAmount;
						newARLine.AL_LocalExTaxAmount = line.AL_LocalExTaxAmount;
					}
					else
					{
						newARLine.AL_OSExTaxAmount = GetCrossCurrencyConvertedAmount(line.AL_OSExTaxAmount, line.TransactionCurrency, line.AL_ExchangeRate,
							agentInvoice.TransactionCurrency, agentInvoice.AH_ExchangeRate);
					}
					newARLine.AL_GB = line.AL_GB;
					newARLine.AL_GE = line.AL_GE;
					if (useLocalAmounts)
					{
						newARLine.AL_OSTaxAmount = line.AL_LocalTaxAmount;
						newARLine.AL_LocalTaxAmount = line.AL_LocalTaxAmount;
					}
					else
					{
						newARLine.AL_OSTaxAmount = GetCrossCurrencyConvertedAmount(line.AL_OSTaxAmount, line.TransactionCurrency, line.AL_ExchangeRate,
							agentInvoice.TransactionCurrency, agentInvoice.AH_ExchangeRate);
					}
				}

				newARLine.AL_Desc = GetLineDescription(chargeCode, newARLine, line.Job);
				newAPLine.AL_Desc = GetLineDescription(chargeCode, newAPLine, line.Job);
			}

			if (agentInvoice != null && aPInvoice != null)
			{
				foreach (JobConsolCost cost in PostingDetails.AllConsolCosts)
				{
					if (cost.E6_AH_APInvoice == aPInvoice.PK)
					{
						cost.E6_AH_ARInvoice = agentInvoice.PK;
					}
				}
			}
			return agentInvoice;
		}

		ZDecimal GetCrossCurrencyConvertedAmount(ZDecimal sourceOSAmount, RefCurrency sourceCurrency, ZDecimal sourceExRate, RefCurrency destCurrency, ZDecimal destExRate)
		{
			ZDecimal result = 0m;
			if (sourceCurrency != destCurrency || sourceExRate != destExRate)
			{
				ZDecimal unRoundedLocalAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocalWithoutRounding(sourceOSAmount, sourceExRate);
				result = Env.CurrentCompany.ExchangeRate.LocalToForeign(unRoundedLocalAmount, destExRate, destCurrency.RX_Code);
			}
			else
			{
				result = sourceOSAmount;
			}
			return result;
		}

		InvoicingBase GetARInvoiceForAgent(InvoicingBase aPInvoice)
		{
			InvoicingBase result = null;
			InvoicingBase[] candidateInvoices = PostingDetails.ChargePoster.GetInvoices(aPInvoice.Header);

			candidateInvoices = candidateInvoices.Where(x => x.ConsolNumberFromConsolidatedInvoiceRef == PostingDetails.Consol.JK_UniqueConsignRef && x.AH_GB_TaxBranch == aPInvoice.AH_GB_TaxBranch).ToArray();

			foreach (InvoicingBase invoice in candidateInvoices)
			{
				if (invoice.AH_RX_NKTransactionCurrency == Core.Constants.CurrencyCodes.UnitedStates)
				{
					result = invoice;
					break;
				}
			}
			if (result == null)
			{
				foreach (InvoicingBase invoice in candidateInvoices)
				{
					if (invoice.AH_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						result = invoice;
						break;
					}
				}
			}
			if (result == null)
			{
				foreach (InvoicingBase invoice in candidateInvoices)
				{
					if (invoice.AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						result = invoice;
						break;
					}
				}
			}

			if (result == null)
			{
				PostingChargeKey key = new PostingChargeKey(aPInvoice.AH_OH, "", PostingDetails.Consol.JK_UniqueConsignRef, ZGuid.Empty, ZGuid.Empty, 0, placeOfSupply: aPInvoice.AH_PlaceOfSupply, taxBranch: aPInvoice.AH_GB_TaxBranch);
				AgentInvoiceCreationPostingChargeCollection agentInvoiceCreation = new AgentInvoiceCreationPostingChargeCollection();
				agentInvoiceCreation.Key = key;
				agentInvoiceCreation.SetDebtor(aPInvoice.AH_OH);
				agentInvoiceCreation.SetInvoicePostingBranch(aPInvoice.AH_GB);
				agentInvoiceCreation.SetInvoicePostingDepartment(aPInvoice.AH_GE);
				agentInvoiceCreation.SetIsBillInLocalCurrency(false);
				agentInvoiceCreation.SetJobNumber(PostingDetails.Consol.JK_UniqueConsignRef);

				Job job = GetFirstValidJobFromAPInvoice(aPInvoice);
				agentInvoiceCreation.SetJobPK(job.PK);
				agentInvoiceCreation.SetPostingJob(job);
				agentInvoiceCreation.PostingCurrency = aPInvoice.AH_RX_NKTransactionCurrency;
				agentInvoiceCreation.PostingCurrencyExchangeRate = aPInvoice.AH_ExchangeRate;

				agentInvoiceCreation.SetIsPositiveCost(aPInvoice.AH_LocalTotal < 0);
				result = PostingDetails.ChargePoster.Post(agentInvoiceCreation);
			}

			return result;
		}

		Job GetFirstValidJobFromAPInvoice(InvoicingBase invoice)
		{
			Job result = null;
			foreach (InvoicingLineBase line in invoice.Lines)
			{
				if (line.Job != null)
				{
					result = Factory.Load<Job>(line.AL_JH);
					break;
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		ZString GetLineDescription(AccChargeCode chargeCode, InvoicingLineBase line, JobHeader job)
		{
			ZString result = ZString.Empty;
			result = Res.GetString("313d57e8-2345-475e-bf43-e1f95d2ae094", "{0} - Job Number: {1}", chargeCode.AC_Desc, job.JH_JobNum);
			return result;
		}

		void ChangeInvoiceTypeBasedOnTotal(InvoicingBase negatedARInvoice)
		{
			if (negatedARInvoice != null)
			{
				var totalLines = negatedARInvoice.Lines.Sum(x => ((InvoicingLineBase)x).AL_LineAmount);

				if (totalLines < 0 && negatedARInvoice.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Invoice )
				{
					negatedARInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.CreditNote;
				}
				else if (totalLines > 0 && negatedARInvoice.AH_TransactionType == ZArchitecture.Core.TransactionTypes.CreditNote)
				{
					negatedARInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
				}
			}
		}

		readonly AgentChargePostingDetails PostingDetails;
		readonly BusinessObjectFactory Factory;

		#endregion
	}
}
