using System;
using System.Linq;

using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public static class EnumerableChargesHelper
	{
		public static Money OSAirTransportAmount(this JobDeclaration declaration)
		{
			var result = declaration.OSAirTransportAmountCore();

			var currencyConverter = ((ICurrencyConverterProvider)declaration).CurrencyConverter;
			var destinationCurrency = declaration.FrtChgAmt().Currency;

			return destinationCurrency != null ? currencyConverter.ConvertExact(result, destinationCurrency) : Money.Empty;
		}

		public static Money FrtChgAmt(this JobDeclaration declaration)
		{
			var currencyConverter = ((ICurrencyConverterProvider)declaration).CurrencyConverter;

			return currencyConverter.Add(declaration.OSAirTransportAmountCore(), declaration.FreightCore());
		}

		static Money OSAirTransportAmountCore(this JobDeclaration declaration)
		{
			return declaration.GetInvoiceApportionedChargesNotIncludedInLines(ChargesProvider.AirFreight.Code);
		}

		static Money FreightCore(this JobDeclaration declaration)
		{
			return declaration.GetInvoiceApportionedChargesNotIncludedInLines(ChargesProvider.InternationalFreight.Code);
		}

		public static Money OthChgAmt(this JobDeclaration declaration)
		{
			// BP: "I believe indeed that Box 67 should be set to total of all additions less total of all deductions" -
			// NB, this doesn't mean that we don't heed the included-in-lines flag; rather it means that we implicitly heed the flag because the charge code implies the flag.
			var additions = declaration.GetInvoiceApportionedChargesDontCareAboutWhetherIncludedInLines(ChargesProvider.AdditionCharge.Code);
			var deductions = declaration.GetInvoiceApportionedChargesDontCareAboutWhetherIncludedInLines(ChargesProvider.DeductionCharge.Code);//if post CIF, this will be deducted by Customs. Validation is in place.

			var currencyConverter = ((ICurrencyConverterProvider)declaration).CurrencyConverter;
			return currencyConverter.Subtract(additions, deductions);
		}

		public static Money InsAmt(this JobDeclaration declaration)
		{
			return declaration.GetInvoiceApportionedChargesNotIncludedInLines(ChargesProvider.InternationalInsurance.Code);
		}

		public static Money DiscAmt(this JobDeclaration declaration)
		{
			return declaration.GetInvoiceApportionedChargesNotIncludedInLines(ChargesProvider.Discount.Code);//Not needed when percentage is entered?
		}

		public static ZDecimal DiscPercentage(this JobDeclaration declaration)
		{
			//FirstOrDefault: Validation should be in place to stop users from entering multiple DIS charges
			return declaration.TopGroupInvoice?.Charges.Cast<GroupInvoiceCharge>().FirstOrDefault(x => x.J7_ChargeType == Customs.Business.CustomsChargeTypeList.Codes.Discount && x.J7_Percentage > 0m)?.J7_Percentage ?? ZDecimal.Zero;
		}

		public static Money VATAdjAmt(this JobDeclaration declaration)
		{
			return declaration.GetInvoiceApportionedChargesNotIncludedInLines(ChargesProvider.VATAdjustment.Code);
		}

		static Money GetInvoiceApportionedChargesNotIncludedInLines(this JobDeclaration declaration, string chargeType)
		{
			return GetInvoiceApportionedCharges(declaration, chargeType, x => !x.J7_IsIncludedInITOT);
		}

		static Money GetInvoiceApportionedChargesDontCareAboutWhetherIncludedInLines(this JobDeclaration declaration, string chargeType)
		{
			return GetInvoiceApportionedCharges(declaration, chargeType, x => true);
		}

		static Money GetInvoiceApportionedCharges(this JobDeclaration declaration, string chargeType, Predicate<JobComInvCharge> funcForComparison)
		{
			var result = Money.Empty;
			var currencyConverter = ((ICurrencyConverterProvider)declaration).CurrencyConverter;

			var charges = declaration.Invoices.Cast<JobComInvoiceHeader>().SelectMany(x => x.GroupCharges.Cast<JobComInvCharge>()).Where(x => x.J7_ChargeType == chargeType && funcForComparison(x));

			foreach (JobComInvCharge charge in charges)
			{
				result = currencyConverter.Add(result, charge.Money);
			}
			return result;
		}
	}
}
