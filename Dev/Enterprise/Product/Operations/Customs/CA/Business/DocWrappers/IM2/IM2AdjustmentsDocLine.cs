using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;

namespace Enterprise.Customs.CA.Business
{
	public class IM2AdjustmentsDocLine : AdjustmentsDocLine
	{
		public IM2AdjustmentsDocLine(bool isEmpty = false) : base(isEmpty)
		{
		}

		protected override AdjustmentsDocLine CreateNewLine()
		{
			return new IM2AdjustmentsDocLine();
		}

		public IEnumerable<IM2AdjustmentsDocLine> GetLinesOrderedByDuty(CusEntryLine entryLine, bool isNewDeclaration)
		{
			var result = new List<IM2AdjustmentsDocLine>();
			if (entryLine != null)
			{
				var classificEntryLine = (IClassificationLine1)entryLine;
				foreach (var line2 in classificEntryLine.ClassificationLines)
				{
					if (!result.Any())
					{
						var classLine = new ClassificationLine(classificEntryLine, line2, null);
						result.Add(GetFirstLine(entryLine, classLine, isNewDeclaration));
					}
					else
					{
						result.Add(GetSubsequentContinualDutyLine(new ClassificationLine(null, line2, null)));
					}
				}
				if (!result.Any())
				{
					result.Add(GetFirstLine(entryLine, new ClassificationLine(classificEntryLine, null, null), isNewDeclaration));
				}
			}
			return result;
		}

		internal IM2AdjustmentsDocLine GetFirstLine(CusEntryLine entryLine, ClassificationLine classLine, bool isNewDeclaration)
		{
			var result = new IM2AdjustmentsDocLine();
			result.OriginalLineNo = isNewDeclaration ? entryLine.CA_B2LineNo : new ZString(entryLine.CL_LineNumber.ToString());
			result.Description = classLine.Description.SubstringSafe(0, AdjustmentDocHelper.Constant.TruncatedInfoOnOneLine);
			if (classLine.Description.Length > AdjustmentDocHelper.Constant.TruncatedInfoOnOneLine)
			{
				result.Description += "\r\n" + classLine.Description.SubstringSafe(AdjustmentDocHelper.Constant.TruncatedInfoOnOneLine, AdjustmentDocHelper.Constant.TruncatedInfoOnOneLine);
			}
			var line1 = classLine.Line1;
			result.SpecialAuthority = line1.AuthorityNumber;
			result.TariffCode = line1.TariffCode;
			result.ClassificationNumber = line1.ClassificationNumber;
			result.Quantity = classLine.Quantity;
			result.UM = classLine.UnitOfMeasureCode;
			result.VFDCode = line1.ValueForDutyCode;
			result.SIMACode = line1.SIMACode;
			var excise = TaxRateFormatter.Format(line1.ExciseTaxRateToPrint, line1.ExciseTaxRateType);
			result.ExciseTaxRate = excise.IsEmpty ? line1.ExciseExemptionCode : excise;
			result.GSTRate = line1.GSTExemptionCode;
			if (result.GSTRate.IsEmpty)
			{
				result.GSTRate = TaxRateFormatter.Format(line1.RateOfGST, line1.GSTRateType);
			}

			result.ValueForCurrencyConversion = line1.ValueForCurrency;
			result.ValueForDuty = line1.ValueForDuty;
			result.SIMAAssessment = line1.SIMAAssessment;
			result.ExciseTax = line1.ExciseTaxAmount;
			result.ValueForTax = line1.ValueForTax;
			result.GST = line1.GSTAmount;
			if (classLine.Line2 != null)
			{
				result.CustomsDutyRate = TaxRateFormatter.Format(classLine.Line2.CustomsDutyRate, classLine.Line2.CustomsDutyRateType);
				result.CustomsDuties = classLine.Line2.CustomsDutyAmount;
			}
			return result;
		}

		static IM2AdjustmentsDocLine GetSubsequentContinualDutyLine(ClassificationLine classLine)
		{
			var result = new IM2AdjustmentsDocLine();
			result.Quantity = classLine.Quantity;
			result.UM = classLine.UnitOfMeasureCode;
			result.CustomsDutyRate = TaxRateFormatter.Format(classLine.Line2.CustomsDutyRate, classLine.Line2.CustomsDutyRateType);
			result.CustomsDuties = classLine.Line2.CustomsDutyAmount;
			return result;
		}
	}
}
