using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business
{
	public static class InsuranceRuleEngine
	{
		public static ZDecimal GetInsuranceFlatValue(JobDeclaration declaration, JobComInvoiceHeader invoice)
		{
			var rule = GetCusCalculationRule(declaration);

			if (rule != null && rule.CalculationRuleRateCollection.Count > 0)
			{
				var invValueInRuleCurrency = invoice.CurrencyConverter.ConvertExact(new Money(invoice.JZ_InvoiceAmount, invoice.Invoice_Currency), rule.Currency);
				var rate = GetRateFromRuleCollection(rule.CalculationRuleRateCollection, invValueInRuleCurrency.Amount);
				if (rate != null && rate.FlatRate > 0)
				{
					return invoice.CurrencyConverter.ConvertExact(new Money(new ZDecimal(rate.FlatRate), rule.Currency), invoice.Invoice_Currency).Amount;
				}
			}
			return 0;
		}

		static CusCalculationRule GetCusCalculationRule(JobDeclaration declaration)
		{
			var factory = declaration.Factory;
			var valuationDate = declaration.DateOfValuation.ToOffset();
			return factory.GetCachedValue($"GetApplicableInsurance_{declaration.JE_OH_Importer}_{declaration.JE_TransportMode}_{valuationDate}",
				() => new CusCalculationRule.Loader(factory).LoadApplicableInsuranceRule(declaration.JE_OH_Importer, declaration.JE_TransportMode, valuationDate),
				CacheStalenessPolicy.StaleWhenDataTableChanges(CusCalculationRuleSchema.Constants.TableName, factory)
			);
		}

		static CusCalculationRuleRate GetRateFromRuleCollection(CusCalculationRuleRateCollection rates, ZDecimal value)
		{
			rates.Sort(nameof(CusCalculationRuleRate.ValueFrom));
			CusCalculationRuleRate lastRate = null;
			if (rates[0].ValueFrom <= value)
			{
				lastRate = rates[0];

				foreach (CusCalculationRuleRate rate in rates)
				{
					if (rate.ValueFrom <= value)
					{
						lastRate = rate;
					}
				}
			}
			return lastRate;
		}

		public static ZDecimal GetInsuranceUpliftPercent(JobDeclaration declaration, JobComInvoiceHeader invoice)
		{
			var rule = GetCusCalculationRule(declaration);

			if (rule != null && rule.CalculationRuleRateCollection.Count > 0)
			{
				var invValueInRuleCurrency = invoice.CurrencyConverter.ConvertExact(new Money(invoice.JZ_InvoiceAmount, invoice.Invoice_Currency), rule.Currency);
				var rate = GetRateFromRuleCollection(rule.CalculationRuleRateCollection, invValueInRuleCurrency.Amount);
				if (rate != null && rate.Uplift > 0)
				{
					return rate.Uplift;
				}
			}

			var supplierImporterLink = GetLinkBetweenSupplierAndImporter(declaration.Supplier, declaration.Importer);

			if(supplierImporterLink?.OL_InsuranceUplift != null)
			{
				return supplierImporterLink.OL_InsuranceUplift;
			}

			return 0;
		}

		static OrgSupplierBuyerLink GetLinkBetweenSupplierAndImporter(OrgHeader supplier, OrgHeader importer)
		{
			OrgSupplierBuyerLink result = null;
			if (supplier != null && importer != null)
			{
				var links = importer.SupplierLinks.Find(new ZQuery(OrgSupplierBuyerLinkSchema.OL_OH_Supplier, supplier.PK));
				if (links.Length > 0)
				{
					result = (OrgSupplierBuyerLink)links[0];
				}
			}
			return result;
		}
	}
}
