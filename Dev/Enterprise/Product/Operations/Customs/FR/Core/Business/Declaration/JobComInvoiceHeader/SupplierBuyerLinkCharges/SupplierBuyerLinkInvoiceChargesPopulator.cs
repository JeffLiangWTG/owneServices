using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.FR.Business.Interfaces;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class SupplierBuyerLinkInvoiceChargesPopulator
	{
		public SupplierBuyerLinkInvoiceChargesPopulator(JobComInvoiceHeader invoiceHeader)
		{
			InvoiceHeader = Argument.NotNull(invoiceHeader, nameof(invoiceHeader));
		}

		JobComInvoiceHeader InvoiceHeader { get; }

		public void PopulateCharges()
		{
			if (InvoiceHeader.IsAttachedToPersistentDeclaration)
			{
				var chargeCollection = InvoiceHeader.Charges;
				var existingSystemCalculatedChargesToAbandon = chargeCollection.Cast<InvoiceCharge>().Where(charge => charge.IsSystemCalculated).ToList();

				var supplierBuyerLink = InvoiceHeader.SupplierBuyerLink;
				if (supplierBuyerLink != null)
				{
					var ruleResults = AllRules.Where(rule => rule.ShouldApplyRule(InvoiceHeader.JobDeclaration)).Select(rule => (ChargeCode: rule.GetChargeCode(InvoiceHeader.JobDeclaration), Percentage: rule.GetPercentage(supplierBuyerLink)));
					foreach (var ruleResult in ruleResults)
					{
						if (ruleResult.Percentage != 0)
						{
							var existingChargeForRule = chargeCollection.Cast<InvoiceCharge>().FirstOrDefault(charge => charge.J7_ChargeType == ruleResult.ChargeCode);
							if (existingChargeForRule != null)
							{
								if (existingChargeForRule.IsSystemCalculated)
								{
									existingChargeForRule.J7_Percentage = ruleResult.Percentage;
									existingSystemCalculatedChargesToAbandon.Remove(existingChargeForRule);
								}
							}
							else
							{
								var newCharge = chargeCollection.AddNew(ruleResult.ChargeCode);
								newCharge.IsSystemCalculated = true;
								newCharge.J7_Percentage = ruleResult.Percentage;
							}
						}
					}
				}

				foreach (var chargeToAbandon in existingSystemCalculatedChargesToAbandon)
				{
					chargeCollection.RemoveAndDelete(chargeToAbandon);
				}
			}
		}

		ISupplierBuyerLinkChargeRule[] allRules;
		IEnumerable<ISupplierBuyerLinkChargeRule> AllRules => allRules
			?? (allRules = new ISupplierBuyerLinkChargeRule[] {
				new SupplierBuyerLinkRFLChargeRule() ,
				new SupplierBuyerLinkBCMChargeRule()
			});
	}
}
