using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ExportLineCommonWrapper : IExportLineCommon
	{
		public ExportLineCommonWrapper(CusEntryLine cusEntryLine)
		{
			entryLine = Argument.NotNull(cusEntryLine, nameof(cusEntryLine));
			Argument.GreaterThan(entryLine.InvoiceLines.Count, 0, nameof(entryLine.InvoiceLines));
		}

		protected readonly CusEntryLine entryLine;

		public ZInt GoodsItemNumber => entryLine.CL_LineNumber;

		public ZDecimal GrossWeightInKG => GrossWeightInKGCore;
		protected virtual ZDecimal GrossWeightInKGCore
		{
			get
			{
				var grossWeight = entryLine.EffectiveGrossWeight.InKilogramsSafe;
				return grossWeight > 1 ? (ZDecimal)Math.Ceiling(grossWeight) : grossWeight;
			}
		}

		public ZDecimal NetWeightInKG => NetWeightInKGCore;

		protected virtual ZDecimal NetWeightInKGCore => entryLine.EffectiveCustomsWeight.InKilogramsSafe.Round(3);

		public ZDecimal OtherUnitsNumber => entryLine.ThirdQuantity;

		public ZString OtherUnitsQualifier => entryLine.ThirdUQ.ConvertCargoWiseToES(entryLine.Factory);

		public ZDecimal TotalGoodValueInEuros => entryLine.CL_StatisticalValue;
	}
}
