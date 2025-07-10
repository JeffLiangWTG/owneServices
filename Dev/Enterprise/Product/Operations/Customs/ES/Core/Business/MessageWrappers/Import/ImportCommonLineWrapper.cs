using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ImportCommonLineWrapper : IImportCommonLine
	{
		public ImportCommonLineWrapper(CusEntryLine cusEntryLine)
		{
			entryLine = Argument.NotNull(cusEntryLine, nameof(cusEntryLine));
			Argument.GreaterThan(entryLine.InvoiceLines.Count, 0, nameof(entryLine.InvoiceLines));
		}

		protected readonly CusEntryLine entryLine;

		public ZInt LineNumber => entryLine.CL_LineNumber;

		public IReadOnlyCollection<ZString> Containers => containers ?? (containers = entryLine.Containers.ToList().AsReadOnly());
		IReadOnlyCollection<ZString> containers;

		public ZString GoodsDescription => entryLine.RandomLine.JI_Description;

		public ZString TariffCode => entryLine.Tariff;

		public ZString OriginCountry => entryLine.CountryOfOriginCode;

		public ZString RequestedCPC => entryLine.ProcedureCode.Left(2);

		public ZString PreviousCPC => entryLine.ProcedureCode.SubstringSafe(2, 2);
	}
}
