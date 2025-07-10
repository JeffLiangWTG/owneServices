using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.FR.Business.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class SupplierBuyerLinkGroupChargesPopulator
	{
		public SupplierBuyerLinkGroupChargesPopulator(JobComInvoiceGroupHeader groupHeader)
		{
			GroupHeader = Argument.NotNull(groupHeader, nameof(groupHeader));
		}

		JobComInvoiceGroupHeader GroupHeader { get; }

		public void PopulateCharges()
		{
			var jobDeclaration = GroupHeader.JobDeclaration;
			if (jobDeclaration != null && GroupHeader.AllJobComInvoiceHeaders.Count > 0)
			{
				var chargeCollection = GroupHeader.Charges;
				var existingSystemCalculatedChargesToAbandon = chargeCollection.Cast<GroupInvoiceCharge>().Where(charge => charge.IsSystemCalculated).ToList();

				var currencyConverter = GroupHeader.AllJobComInvoiceHeaders.FirstOrDefault().CurrencyConverter;
				var money = Money.Empty;
				foreach (JobComInvoiceHeader invoice in GroupHeader.AllJobComInvoiceHeaders)
				{
					var supplierBuyerLink = invoice.SupplierBuyerLink;
					if (Rule.ShouldApplyRule(jobDeclaration))
					{
						var percentage = Rule.GetPercentage(supplierBuyerLink);
						money = currencyConverter.Add(money, invoice.InvoiceAmount * percentage / 100);
					}
				}

				if (!money.IsEmpty)
				{
					var currenciesInInvoices = GroupHeader.AllJobComInvoiceHeaders.Select(x => x.Invoice_Currency).Distinct();
					var currency = currenciesInInvoices.Count() == 1 ? (currenciesInInvoices.FirstOrDefault() ?? RefCurrency.LoadFromCurrencyCode(GroupHeader.Factory, Core.Constants.CurrencyCodes.EuropeanUnion))
						: RefCurrency.LoadFromCurrencyCode(GroupHeader.Factory, Core.Constants.CurrencyCodes.EuropeanUnion);

					var existingChargeForRule = chargeCollection.Cast<GroupInvoiceCharge>().FirstOrDefault(charge => charge.J7_ChargeType == Rule.GetChargeCode(jobDeclaration) && charge.IsSystemCalculated);
					if (existingChargeForRule != null)
					{
						existingChargeForRule.J7_Amount = currencyConverter.ConvertExact(money, currency).Amount;
						existingChargeForRule.J7_RX_NKCurrency = currency.Code;
						existingSystemCalculatedChargesToAbandon.Remove(existingChargeForRule);
					}
					else
					{
						var newCharge = chargeCollection.AddNew(Rule.GetChargeCode(jobDeclaration));
						newCharge.IsSystemCalculated = true;
						newCharge.J7_Amount = currencyConverter.ConvertExact(money, currency).Amount;
						newCharge.J7_RX_NKCurrency = currency.Code;
					}
				}

				foreach (var chargeToAbandon in existingSystemCalculatedChargesToAbandon)
				{
					chargeCollection.RemoveAndDelete(chargeToAbandon);
				}
			}
		}

		ISupplierBuyerLinkChargeRule Rule => rule ?? (rule = new SupplierBuyerLinkInsuranceChargeRule());
		ISupplierBuyerLinkChargeRule rule;
	}
}
