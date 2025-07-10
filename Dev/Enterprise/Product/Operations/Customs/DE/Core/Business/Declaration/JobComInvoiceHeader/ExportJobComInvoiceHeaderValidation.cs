using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ExportJobComInvoiceHeaderValidation : JobComInvoiceHeaderValidation
	{
		public ExportJobComInvoiceHeaderValidation(JobComInvoiceHeader header)
			: base(header)
		{
		}

		protected override bool ShouldValidateNeedAtLeastOneInvoiceSupportingDocument => false;

		protected override void CheckJZ_Calc_BalanceCore()
		{
			if (Parent.JZ_Calc_Balance.Round(2) != 0.00m)
			{
				Parent.JZ_Calc_BalanceInfo.AddWarning(Res.GetString("2eafd732-567c-4e8a-951b-14b0b074d7af", "The total of all invoice lines does not equal the invoice total."));
			}
		}

		protected override void CheckJZ_Weight()
		{
			base.CheckJZ_Weight();
			var grossWeight = new ZWeight(Parent.JZ_Weight, Parent.JZ_WeightUQ);
			if (grossWeight.IsValid && !AnyInvalidInvoiceLineWeights)
			{
				var info = Parent.JZ_WeightInfo;
				var grossWeightInKg = grossWeight.InKilogramsSafe;
				if (grossWeightInKg > 99999999999)
				{
					info.AddMessageError(Res.GetString("F647A0B1-9421-417F-80F4-C8F9113254BA", "Invoice Gross Weight must not exceed 99999999999 KG. ({0} {1})", grossWeightInKg, Core.Constants.Weight.Kilograms));
				}
				else if (grossWeightInKg < invoiceLinesGrossWeightInKg)
				{
					info.AddMessageError(Res.GetString("7A3B7E24-8504-4997-918E-39F14B172830", "Invoice Gross Weight must not be less than the sum of all related invoice line's [35] GWT. ({0} {1})", invoiceLinesGrossWeightInKg, Core.Constants.Weight.Kilograms));
				}
			}
		}

		protected override void CheckJZ_ValuationCode()
		{
			base.CheckJZ_ValuationCode();

			var parent = Parent;
			var targetInfo = parent.JZ_ValuationCodeInfo;
			var invoiceLines = parent.InvoiceLines.Cast<JobComInvoiceLine>().ToArray();
			if (invoiceLines.Any(x => x.EntryInstruction != null))
			{
				var valuationRefCusCodeList = parent.ValuationRefCusCodeList;
				if (valuationRefCusCodeList != null)
				{
					if (invoiceLines.Any(x => x.EntryInstruction.Style4thDigitIs4()))
					{
						CheckAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.C0091);
					}
					if (invoiceLines.Any(x => !x.EntryInstruction.Style4thDigitIs4()))
					{
						CheckAttribute(UniversalReferenceConstants.RefCusCodeListAttributes.Name.A1150);
					}
				}
				if (invoiceLines.Any(x => x.EntryInstruction.Style5thDigitIs0()))
				{
					MessageValidation.CheckEntered(targetInfo, Res.GetString("F124A116-9F75-4D09-B226-2D9A39FCBA42", "You have not entered a Transaction Nature."));
				}

				void CheckAttribute(ZString attributeName)
				{
					if (!valuationRefCusCodeList.HasAttribute(attributeName, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes))
					{
						targetInfo.AddMessageError(Res.GetString("D082E529-3EB3-43F9-B842-60AE1D5EFDC7", "For the selected Type (Procedure) the Transaction Nature code must be from code list {0}.", attributeName));
					}
				}
			}
		}

		public override bool ShouldValidateWaterIncoTermAndTransportMode => Parent.JZ_IncoTerm != Core.Constants.IncoTerms.FreeOnBoard &&
																			Parent.JZ_IncoTerm != Core.Constants.IncoTerms.CostAndFreight &&
																			Parent.JZ_IncoTerm != Core.Constants.IncoTerms.CostInsuranceAndFreight;

		ZBool AnyInvalidInvoiceLineWeights => Parent.Factory.GetValue(ref invalidInvoiceLineWeight, () =>
		{
			invoiceLinesGrossWeightInKg = ZDecimal.Zero;
			var result = false;
			foreach (JobComInvoiceLine line in Parent.InvoiceLines)
			{
				var lineGrossWeight = new ZWeight(line.JI_Weight, line.JI_WeightUQ);
				if (!lineGrossWeight.IsValid)
				{
					result = true;
					break;
				}
				invoiceLinesGrossWeightInKg += lineGrossWeight.InKilogramsSafe;
			}
			return result;
		});
		CachedProperty<ZBool> invalidInvoiceLineWeight;
		ZDecimal invoiceLinesGrossWeightInKg;
	}
}
