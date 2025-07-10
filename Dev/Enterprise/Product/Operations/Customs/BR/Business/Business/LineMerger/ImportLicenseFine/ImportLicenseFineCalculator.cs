using CargoWise.Common;

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseFineCalculator
	{
		public ImportLicenseFineCalculator(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}
		readonly CusEntryLine entryLine;

		public void UpdateImportLicenseFineOnEntryLine()
		{
			var invoiceLine = entryLine.RandomLine;
			if (!invoiceLine.ImportLicenseNumber.IsEmpty
				&& !invoiceLine.ImportLicenseFeeType.IsEmpty
				&& invoiceLine.ImportLicenseType == ImportLicenseType.Codes.PreBoarding
				&& invoiceLine.ImportLicenseAuthorizationDate.IsValid
				&& invoiceLine.Declaration.JE_ExportDate.IsValid
				&& invoiceLine.ImportLicenseAuthorizationDate.Date > invoiceLine.Declaration.JE_ExportDate.Date)
			{
				var tax = BRRefCusTaxOrFee.GetImportLicenseFee(invoiceLine.Factory, invoiceLine.ImportLicenseFeeType, invoiceLine.EffectiveAssessmentDate);
				if (tax != null)
				{
					var feeAmount = entryLine.CL_CustomsValue * (tax.ZZF_Value / 100);
					if (feeAmount < tax.ZZF_Minimum)
					{
						feeAmount = tax.ZZF_Minimum;
					}
					else if (feeAmount > tax.ZZF_Maximum)
					{
						feeAmount = tax.ZZF_Maximum;
					}

					var icmsFee = entryLine.Fees.AddOrUpdate(invoiceLine.ImportLicenseFeeType, feeAmount);
					icmsFee.CF_BaseValue = entryLine.CL_CustomsValue;
					icmsFee.CF_Rate = tax.ZZF_Value;
					icmsFee.CF_MethodOfCalculation = tax.ZZF_Code == Core.Constants.Customs.Universal.RefCusTaxOrFee.Codes.FiftyPercentDiscountCode ? Constants.MethodOfCalculation.FiftyPercent : Constants.MethodOfCalculation.Percentage;
				}
			}
		}
	}
}
