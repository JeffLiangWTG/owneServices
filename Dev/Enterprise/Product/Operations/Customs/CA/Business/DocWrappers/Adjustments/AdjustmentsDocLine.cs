using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public abstract class AdjustmentsDocLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AdjustmentsDocLine(bool isEmpty = false)
		{
			this.IsEmpty = isEmpty;
			tariffFormatter = new TariffFormatter();
		}

		protected abstract AdjustmentsDocLine CreateNewLine();

		public IEnumerable<AdjustmentsDocLine> GetLinesOrderedByDuty(JobComInvoiceLine line, bool isAccounted = false)
		{
			var result = new List<AdjustmentsDocLine>();
			result.Add(GetFirstLine(line, isAccounted));
			var dutiesLeft = line.DutyAndTaxManager.Duties.OrderBy(x => x, new DutyAndTaxComparer()).Skip(1);
			if (!line.Declaration.IsBlanketB2 && dutiesLeft.Any())
			{
				foreach (var duty in dutiesLeft)
				{
					result.Add(GetSubsequentContinualDutyLine(duty, isAccounted));
				}
			}
			return result;
		}

		protected virtual AdjustmentsDocLine GetFirstLine(JobComInvoiceLine invoiceLine, bool isAccounted)
		{
			var result = CreateNewLine();

			var isBlanketB2 = invoiceLine.Declaration.IsBlanketB2;
			var description = invoiceLine.JI_Description;

			result.OriginalLineNo = isBlanketB2 ? ZString.Empty : invoiceLine.CA_OriginalLineNo;
			result.Description = description.SubstringSafe(0, AdjustmentDocHelper.Constant.TruncatedInfoOnOneLine);
			if (description.Length > AdjustmentDocHelper.Constant.TruncatedInfoOnOneLine)
			{
				result.Description += "\r\n" + description.SubstringSafe(AdjustmentDocHelper.Constant.TruncatedInfoOnOneLine, AdjustmentDocHelper.Constant.TruncatedInfoOnOneLine);
			}
			result.SpecialAuthority = invoiceLine.CA_AuthorityNumber;
			result.TariffCode = invoiceLine.CA_99TariffCode;
			result.ClassificationNumber = GetTariff(invoiceLine);

			var dutyAndTaxManager = invoiceLine.DutyAndTaxManager;
			var firstDuty = dutyAndTaxManager.Duties.OrderBy(x => x, new DutyAndTaxComparer()).FirstOrDefault();
			if (firstDuty != null)
			{
				var quantity = !firstDuty.Quantity.IsEmpty ? firstDuty.Quantity : invoiceLine.JI_CustomsQuantity;
				result.Quantity = quantity.IsEmpty ? ZString.Empty : quantity.ToStringTrimZeros(2);
				result.UM = !firstDuty.C1_UnitOfMeasure.IsEmpty ? firstDuty.C1_UnitOfMeasure : invoiceLine.JI_CustomsUnitQty;
				result.CustomsDutyRate = TaxRateFormatter.Format(firstDuty.C1_Rate, firstDuty.C1_RateType);
				result.CustomsDuties = isBlanketB2 ? ZDecimal.Zero : firstDuty.C1_Amount;
			}

			result.VFDCode = invoiceLine.CA_ValueForDutyCode;
			result.SIMACode = dutyAndTaxManager.GetExemptCode(DutyAndTaxTypes.Codes.SIMADuty);

			result.ExciseTaxRate = TaxRateFormatter.Format(dutyAndTaxManager.GetRate(DutyAndTaxTypes.Codes.ExciseTax), dutyAndTaxManager.GetRateType(DutyAndTaxTypes.Codes.ExciseTax));
			result.GSTRate = dutyAndTaxManager.GetExemptCode(DutyAndTaxTypes.Codes.GST);
			if (result.GSTRate.IsEmpty)
			{
				result.GSTRate = TaxRateFormatter.Format(dutyAndTaxManager.GetRate(DutyAndTaxTypes.Codes.GST), dutyAndTaxManager.GetRateType(DutyAndTaxTypes.Codes.GST));
			}
			result.ValueForCurrencyConversion = isBlanketB2 ? ZDecimal.Zero : invoiceLine.CA_CVforCurrConv;
			result.ValueForDuty = isBlanketB2 ? ZDecimal.Zero : dutyAndTaxManager.GetCustomsValueForDuty();
			result.SIMAAssessment = isBlanketB2 ? ZDecimal.Zero : dutyAndTaxManager.GetAmount(DutyAndTaxTypes.Codes.SIMADuty);
			result.ExciseTax = isBlanketB2 ? ZDecimal.Zero : dutyAndTaxManager.GetAmount(DutyAndTaxTypes.Codes.ExciseTax);
			result.ValueForTax = invoiceLine.CA_ValueForTax;
			result.GST = dutyAndTaxManager.GetAmount(DutyAndTaxTypes.Codes.GST);

			return result;
		}

		protected virtual ZString GetTariff(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.JI_FormattedTariff;
		}

		protected virtual AdjustmentsDocLine GetSubsequentContinualDutyLine(DutyAndTax duty, bool isAccounted)
		{
			var result = CreateNewLine();
			result.Quantity = !duty.Quantity.IsEmpty ? duty.Quantity.ToStringTrimZeros(2) : ZString.Empty;
			result.UM = duty.C1_UnitOfMeasure;
			result.CustomsDutyRate = TaxRateFormatter.Format(duty.C1_Rate, duty.C1_RateType);
			result.CustomsDuties = duty.C1_Amount;
			return result;
		}

		protected TariffFormatter tariffFormatter;
		public ZString OriginalLineNo { get; set; }
		public ZString Description { get; set; }
		public ZString SpecialAuthority { get; set; }
		public ZString TariffCode { get; set; }
		public ZString ClassificationNumber { get; set; }
		public ZString Quantity { get; set; }
		public ZString UM { get; set; }
		public ZString VFDCode { get; set; }
		public ZString SIMACode { get; set; }
		public ZString CustomsDutyRate { get; set; }
		public ZString ExciseTaxRate { get; set; }
		public ZString GSTRate { get; set; }
		public ZDecimal ValueForCurrencyConversion { get; set; }
		public ZDecimal ValueForDuty { get; set; }
		public ZDecimal CustomsDuties { get; set; }
		public ZDecimal SIMAAssessment { get; set; }
		public ZDecimal ExciseTax { get; set; }
		public ZDecimal ValueForTax { get; set; }
		public ZDecimal GST { get; set; }
		internal ZBool IsEmpty { get; private set; }
	}
}
