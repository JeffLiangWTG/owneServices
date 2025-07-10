using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	//NPBO for calculations and operations such as merging and determining aggregate fees
	public class TaxStruct : NonPersistentBusinessObject, IObsoleteValidation, IDocSADHLineTaxBoxSupporter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public class Schema : Tax_CusAddInfoOnlyForPIVOT.Schema
		{
		}

		#region Properties

		/// <summary>
		/// In invoice currency (box 22 currency)
		/// </summary>
		public ZString G4_Amount
		{
			get;
			private set;
		}

		/// <summary>
		/// In declaration currency (always GBP for GB)
		/// </summary>
		public ZString G4_Amount_InDeclarationCurrency
		{
			get;
			protected set;
		}

		/// <summary>
		/// In invoice currency (box 22 currency)
		/// </summary>
		public ZDecimal G4_BaseAmount
		{
			get;
			private set;
		}

		/// <summary>
		/// In declaration currency (always GBP for GB)
		/// </summary>
		public ZDecimal G4_BaseAmount_InDeclarationCurrency
		{
			get;
			private set;
		}

		public ZDecimal G4_CalculatedPercentage
		{
			get; private set;
		}

		public ZDecimal G4_BaseQuantity
		{
			get;
			private set;
		}

		public ZString G4_MethodOfPayment
		{
			get; private set;
		}

		public ZString G4_RateDuty
		{
			get; private set;
		}

		public ZString G4_RateOverride
		{
			get; private set;
		}

		public ZString G4_RateSuspension
		{
			get; private set;
		}

		public ZString G4_Type
		{
			get; protected set;
		}

		public ZString G4_BaseQuantityUQ
		{
			get; private set;
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public void AddTax(IEuTax tax)
		{
			G4_Type = tax.G4_Type;  // e.g. B00 for VAT
			G4_RateDuty = tax.G4_RateDuty;  // e.g. S for standard
			G4_RateOverride = tax.G4_RateOverride;
			G4_RateSuspension = tax.G4_RateSuspension;
			G4_MethodOfPayment = tax.G4_MethodOfPayment;  // e.g. F for deferred
			G4_BaseQuantityUQ = tax.G4_BaseQuantityUQ;

			if (G4_CalculatedPercentage != 0 && G4_CalculatedPercentage != tax.G4_CalculatedPercentage)
			{
				ErrorReporter.ReportOnce("BGB-EU-TaxStruct", "Cannot add tax objects that have different base percentages.  That's like adding apples to pears and asking how many oranges you have.");
			}

			G4_BaseAmount += tax.G4_BaseAmount;  // e.g. 10000 (pounds)
			G4_BaseAmount_InDeclarationCurrency += ConvertToDeclarationCurrencyFromInvoiceCurrencyIfNecessary(tax.G4_BaseAmount);
			G4_BaseQuantity += tax.G4_BaseQuantity;  // e.g. 20 (excisable items)

			if (tax.G4_Amount.IsEmpty)  // e.g. 150 (15% VAT on 10000)
			{
				G4_Amount = "";
			}
			else
			{
				ZDecimal oldValue = 0;
				ZDecimal.TryParse(G4_Amount, out oldValue);
				ZDecimal additionalValue = 0;
				ZDecimal.TryParse(tax.G4_Amount, out additionalValue);
				ZDecimal newAmount = oldValue + additionalValue;
				G4_Amount = newAmount.ToString(2);
				G4_Amount_InDeclarationCurrency = ConvertToDeclarationCurrencyFromInvoiceCurrencyIfNecessary(newAmount).ToString(2);
			}
		}

		public ZString Box47b
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				var baseAmountInDeclarationCurrencyFormatted = G4_BaseAmount_InDeclarationCurrency.IsEmpty ? "" : G4_BaseAmount_InDeclarationCurrency.ToString(2);
				var baseAmountFormatted = G4_BaseAmount.IsEmpty ? "" : G4_BaseAmount.ToString(2);
				result.AppendIfNotEmpty(string.IsNullOrEmpty(baseAmountInDeclarationCurrencyFormatted) ? baseAmountFormatted : baseAmountInDeclarationCurrencyFormatted);
				result.AppendIfNotEmpty(G4_BaseQuantity.IsEmpty ? "" : "Q" + G4_BaseQuantity.ToString(3));
				return result.ToStringWithDelimiterBetweenAppends(" ");
			}
		}

		public ZString Box47c1
		{
			get
			{
				ZString result = G4_RateDuty;
				if (!G4_RateSuspension.IsEmpty)
				{
					if (result.Length < 2)
					{
						result = result.PadRight(2);
					}
					result += G4_RateSuspension;
				}
				return result;
			}
		}

		internal static ZDecimal ConvertToDeclarationCurrencyFromInvoiceCurrencyIfNecessary(ZDecimal amount)
		{
			// Always stored in declaration currency!... so don't convert, jackass
			return amount;
		}

		#region ISADHLineTaxBoxSupporter Members

		ZString IDocSADHLineTaxBoxSupporter.Type => G4_Type;

		ZString IDocSADHLineTaxBoxSupporter.TaxBase => Box47b;

		ZString IDocSADHLineTaxBoxSupporter.Rate => Box47c1;

		ZString IDocSADHLineTaxBoxSupporter.RateDuty => G4_RateDuty;

		ZString IDocSADHLineTaxBoxSupporter.RateOverride => G4_RateOverride;

		ZString IDocSADHLineTaxBoxSupporter.AmountInDeclarationCurrency => G4_Amount_InDeclarationCurrency;

		ZString IDocSADHLineTaxBoxSupporter.MethodOfPayment => G4_MethodOfPayment;

		ZString IDocSADHLineTaxBoxSupporter.NationalFeeTypeCode => null;

		ZString IDocSADHLineTaxBoxSupporter.DeclarationMethodOfPayment => null;

		#endregion
	}
}
