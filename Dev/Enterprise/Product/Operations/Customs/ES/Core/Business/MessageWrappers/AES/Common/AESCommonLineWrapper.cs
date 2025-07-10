using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class AESCommonLineWrapper : IAESCommonLine
	{
		public AESCommonLineWrapper(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
			Argument.GreaterThan(entryLine.InvoiceLines.Count, 0, nameof(entryLine.InvoiceLines));
			randomLine = entryLine.RandomLine;
		}
		protected readonly CusEntryLine entryLine;
		protected readonly JobComInvoiceLine randomLine;

		public ZString SequenceNumber => entryLine.CL_LineNumber.ToString();

		public ZDecimal StatisticalValue => entryLine.CL_StatisticalValue;
	}
}
