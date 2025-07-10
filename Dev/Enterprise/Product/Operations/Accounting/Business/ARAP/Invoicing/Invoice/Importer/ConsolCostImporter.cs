using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public interface IConsolCostImporter
	{
		void ImportCostsToCollection(APInvoiceConsolCostCollection apConsolCostCollection, IEnumerable<JobConsolCost> consolCosts, bool syncParentInfo);

		void ImportCostIntoCosting(InvoicingBase apInvoice, JobConsolCost costToImport, JobConsolCost targetConsolCost, bool syncParentInfo);

		void ImportChargeIntoCosting(JobConsolCost targetConsolCost, IEnumerable<IApportionmentChargeToImport> apportionmentChargesToImport, bool invertSign);
	}

	public class ConsolCostImporter : IConsolCostImporter
	{
#if DEBUG
		public static IConsolCostImporter CreateSelfMockingImporter_TestOnly(IConsolCostImporter mockingSelf)
		{
			return new ConsolCostImporter(mockingSelf);
		}

#endif
		public ConsolCostImporter() : this(null)
		{
		}

		ConsolCostImporter(IConsolCostImporter thisAsIConsolCostImporter)
		{
			ThisAsIConsolCostImporter = thisAsIConsolCostImporter ?? this;
		}

		IConsolCostImporter ThisAsIConsolCostImporter { get; }

		void IConsolCostImporter.ImportCostsToCollection(APInvoiceConsolCostCollection apConsolCostCollection, IEnumerable<JobConsolCost> consolCosts, bool syncParentInfo)
		{
			consolCosts.ForEach(consolCost => {
				var addedConsolCost = apConsolCostCollection.AddNew();
				ThisAsIConsolCostImporter.ImportCostIntoCosting(apConsolCostCollection.ParentAPInvoice, consolCost, addedConsolCost, syncParentInfo);
			});
		}

		void IConsolCostImporter.ImportCostIntoCosting(InvoicingBase apInvoice, JobConsolCost costToImport, JobConsolCost targetConsolCost, bool syncParentInfo)
		{
			if (syncParentInfo)
			{
				targetConsolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(costToImport.E6_ParentID, costToImport.E6_ParentTableCode);
			}

			using (targetConsolCost.GetValidationSuspender())
			using (targetConsolCost.GetSuspenderForConsolCostImporter())
			{
				targetConsolCost.E6_AC_ChargeCode = costToImport.E6_AC_ChargeCode;

				if (!ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AP, apInvoice.IsLocalCurrencyTransaction, apInvoice.AH_GC))
				{
					if (apInvoice.UseJobExchangeRate)
					{
						if (costToImport.E6_RX_NKCurrency == apInvoice.AH_RX_NKTransactionCurrency)
						{
							targetConsolCost.E6_ExchangeRate = costToImport.E6_ExchangeRate;
						}
						else
						{
							targetConsolCost.E6_ExchangeRate = costToImport.GetExchangeRateBasedOnJobBillingExchangeRateConfiguration(apInvoice.TransactionCurrency, targetConsolCost.Creditor);
						}
					}
				}

				if (apInvoice.AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency && !targetConsolCost.HasContext(BusinessContext.LegacyXMLImport))
				{
					targetConsolCost.E6_RX_NKCurrency = costToImport.E6_RX_NKCurrency;

					var isCreditorInvoicedExRateApplied = apInvoice.Factory.ServiceContainer.GetService<CreditorInvoicedExchangeRateProvider>()?.TrySetCreditorInvoicedExchangeRate(targetConsolCost) ?? false;

					if (!isCreditorInvoicedExRateApplied)
					{
						targetConsolCost.E6_ExchangeRate = costToImport.E6_ExchangeRate;
					}
				}

				var invertSign = apInvoice is APCreditNote;
				var gSTInclusiveAmountToSet = GetAmountToSet(costToImport.GSTInclusiveAmount, costToImport.E6_RX_NKCurrency, costToImport.E6_ExchangeRate, targetConsolCost.E6_RX_NKCurrency, targetConsolCost.E6_ExchangeRate, invertSign);
				var oSCostAmountToSet = GetAmountToSet(costToImport.E6_OSCostAmount, costToImport.E6_RX_NKCurrency, costToImport.E6_ExchangeRate, targetConsolCost.E6_RX_NKCurrency, targetConsolCost.E6_ExchangeRate, invertSign);
				var oSGSTAmountToSet = GetAmountToSet(costToImport.E6_OSGSTAmount_Calc, costToImport.E6_RX_NKCurrency, costToImport.E6_ExchangeRate, targetConsolCost.E6_RX_NKCurrency, targetConsolCost.E6_ExchangeRate, invertSign);

				if (gSTInclusiveAmountToSet == 0)
				{
					targetConsolCost.GSTInclusiveAmountNeedUpdate = true;
				}

				try
				{
					targetConsolCost.GSTInclusiveAmount = gSTInclusiveAmountToSet;
					targetConsolCost.E6_OSCostAmount = oSCostAmountToSet;

					targetConsolCost.E6_AT_TaxRate = costToImport.E6_AT_TaxRate;
					targetConsolCost.SetTaxDateSafe(costToImport.E6_TaxDate);

					if (targetConsolCost.IsCostGSTApplicable)
					{
						if (!targetConsolCost.E6_AT_TaxRate.IsValid)
						{
							targetConsolCost.SetGSTRateAndTaxMessage();
						}
					}
					else
					{
						targetConsolCost.E6_AT_TaxRate = ZGuid.Empty;
					}

					targetConsolCost.E6_A9_VATClass = costToImport.E6_A9_VATClass;

					if (targetConsolCost.IsCostGSTApplicable)
					{
						if (costToImport.E6_OSGSTAmount_Calc != 0m)
						{
							targetConsolCost.E6_OSGSTAmount_Calc = oSGSTAmountToSet;
						}
					}
					else
					{
						targetConsolCost.E6_OSGSTAmount_Calc = 0m;
					}

					targetConsolCost.CopyPaymentBasesFromSource(costToImport.PK);

					targetConsolCost.E6_ApportionmentMethod = costToImport.E6_ApportionmentMethod;
					targetConsolCost.E6_PPDCLT = costToImport.E6_PPDCLT;
					targetConsolCost.E6_CostGovtChargeCode = costToImport.E6_CostGovtChargeCode;
					targetConsolCost.E6_SellGovtChargeCode = costToImport.E6_SellGovtChargeCode;
					targetConsolCost.E6_PlaceOfSupply = costToImport.E6_PlaceOfSupply;
					targetConsolCost.E6_PlaceOfSupplyType = costToImport.E6_PlaceOfSupplyType;
					targetConsolCost.E6_SupplyType = costToImport.E6_SupplyType;

					targetConsolCost.RelatedConsolCostPK = costToImport.PK;
					if (targetConsolCost.E6_TaxDate.IsEmpty)
					{
						targetConsolCost.UpdateCostTaxDateBasedOnRegistry();
					}
					ThisAsIConsolCostImporter.ImportChargeIntoCosting(targetConsolCost, costToImport.ApportionmentCharges.Cast<IApportionmentChargeToImport>(), invertSign);
				}
				finally
				{
					if (gSTInclusiveAmountToSet == 0)
					{
						targetConsolCost.GSTInclusiveAmountNeedUpdate = false;
					}
				}
			}

			targetConsolCost.Validation.ValidateAll();

			using (var delayedListForCheckingDatesMismatch = new DisposableList(targetConsolCost.ApportionmentCharges.Count))
			{
				foreach (ApportionSplitCharge charge in targetConsolCost.ApportionmentCharges)
				{
					charge.Validation.ValidateAll();
					delayedListForCheckingDatesMismatch.Add(charge.ApportionedChargeTaxDateMismatchWithConsolCostTaxDateDelayedCheckSuspender.GetSuspender());
				}
			}
		}

		void IConsolCostImporter.ImportChargeIntoCosting(JobConsolCost targetConsolCost, IEnumerable<IApportionmentChargeToImport> apportionmentChargesToImport, bool invertSign)
		{
			if (!apportionmentChargesToImport.Any())
			{
				return;
			}

			foreach (var charge in apportionmentChargesToImport)
			{
				using (targetConsolCost.GetValidationSuspender())
				{
					var correspondingCharge = targetConsolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().FirstOrDefault(x => x.JR_JH == charge.JR_JH)
						?? targetConsolCost.ApportionmentCharges.AddDefaultChargeForJob(charge.JR_JH, charge.JR_GB, charge.JR_GE);

					correspondingCharge.JR_OSCostAmt = GetAmountToSet(charge.JR_OSCostAmt, charge.JR_RX_NKCostCurrency, charge.JR_OSCostExRate, targetConsolCost.E6_RX_NKCurrency, targetConsolCost.E6_ExchangeRate, invertSign);

					using (correspondingCharge.GetSuspenderForConsolCostImporter())
					{
						correspondingCharge.JR_GB = charge.JR_GB;
					}
					correspondingCharge.JR_GE = charge.JR_GE;

					correspondingCharge.JR_E6 = ZGuid.Empty;
					correspondingCharge.JR_IsUsedForApportionment = correspondingCharge.JR_OSCostAmt != 0M;
					correspondingCharge.JR_E6 = targetConsolCost.PK;
					correspondingCharge.RelatedApportionChargeFromDB = charge as JobCharge;
				}
			}

			targetConsolCost.UpdateShipmentInfosOnCharges();

			foreach (ApportionSplitCharge originatingCharge in targetConsolCost.ApportionmentCharges)
			{
				originatingCharge.JR_IsUsedForApportionment = apportionmentChargesToImport.Any(x => x.JR_JH == originatingCharge.JR_JH);
			}

			targetConsolCost.SplitApportionAmount();
			targetConsolCost.PushUnApportionedAmountBasedOnRepresentation(targetConsolCost.E6_OSCostAmount, JobChargeSchema.JR_OSCostAmt);
			targetConsolCost.ApportionGSTCharges();
		}

		decimal GetAmountToSet(ZDecimal originalAmount, ZString originalCurrencyCode, ZDecimal originalExRate, ZString destinationCurrencyCode, ZDecimal destinationExRate, bool invertSign)
		{
			ZDecimal result = 0m;

			if (!originalCurrencyCode.IsEmpty && !destinationCurrencyCode.IsEmpty)
			{
				if (originalCurrencyCode == destinationCurrencyCode)
				{
					result = originalAmount;
				}
				else if (originalCurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					result = Env.CurrentCompany.ExchangeRate.LocalToForeign(originalAmount, destinationExRate, destinationCurrencyCode);
				}
				else if (originalCurrencyCode != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency &&
						destinationCurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					result = Env.CurrentCompany.ExchangeRate.ForeignToLocal(originalAmount, originalExRate);
				}
				else if (originalCurrencyCode != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency &&
							destinationCurrencyCode != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					var localUnroundedAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocalWithoutRounding(originalAmount, originalExRate);
					result = Env.CurrentCompany.ExchangeRate.LocalToForeign(localUnroundedAmount, destinationExRate, destinationCurrencyCode);
				}

				if (invertSign)
				{
					result *= -1;
				}
			}

			return result;
		}
	}
}
