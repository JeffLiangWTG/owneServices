using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.AESVersion3_0
{
	public abstract class AESLineProvider : IAESLine
	{
		protected AESLineProvider(CusEntryLine entryLine)
		{
			EntryLine = CargoWise.Common.Argument.NotNull(entryLine, nameof(entryLine));
			EntryHeader = EntryLine.Header;
			RandomInvoiceLine = EntryLine.RandomLine;
			RandomInvoiceHeader = RandomInvoiceLine?.InvoiceHeader;
			Declaration = EntryHeader.Declaration;
		}
		protected readonly JobComInvoiceLine RandomInvoiceLine;
		protected readonly JobComInvoiceHeader RandomInvoiceHeader;
		protected readonly CusEntryLine EntryLine;
		protected readonly CusEntryHeader EntryHeader;
		protected readonly JobDeclaration Declaration;

		public int LineNumber => EntryLine.CL_LineNumber;

		public virtual bool StatisticalValueSpecified => EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => !x.JI_RX_NKLinePriceCurr.IsEmpty || x.Charges.Cast<InvoiceLineCharge>().Any(y => y.J7_ChargeType.Equals(EU.Business.ChargeTypeList.Codes.StatisticalValue)));

		public decimal StatisticalValue => CachedValueHelper.GetValue(ref statisticalValue, () =>
		{
			var result = 0m;
			ZDecimal totalStatisticalValueOfLines = EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_Calc_StatisticalValue);

			if (!totalStatisticalValueOfLines.IsEmpty)
			{
				result = totalStatisticalValueOfLines.FormatDecimal(2);

				if (Declaration.IsTransitionPeriodAES30 && !EntryHeader.EntryInstruction.Style4thDigitIs4())
				{
					if (totalStatisticalValueOfLines > 0 && totalStatisticalValueOfLines < 1)
					{
						result = 1;
					}
					else
					{
						result = totalStatisticalValueOfLines.FormatDecimal(0);
					}
				}
			}
			return result;
		});
		CachedValue<decimal> statisticalValue;

		public decimal SupplementaryQuantity => CachedValueHelper.GetValue(ref supplementaryQuantity, () => EntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.JI_CustomsSecondQuantity).RoundAndNormalize(3));
		CachedValue<decimal> supplementaryQuantity;
	}
}
