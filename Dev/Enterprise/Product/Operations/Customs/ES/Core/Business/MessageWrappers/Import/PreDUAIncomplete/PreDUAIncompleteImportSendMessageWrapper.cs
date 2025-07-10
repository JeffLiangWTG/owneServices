using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class PreDUAIncompleteImportSendMessageWrapper : ImportCommonSendMessageWrapper, IPreDUAIncompleteImportMessageDataProvider
	{
		public PreDUAIncompleteImportSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
			Argument.GreaterThan(entryHeader.MergedLines.Count, 0, nameof(entryHeader.MergedLines));
		}

		public IPDIHeader Header => header ?? (header = new PDIHeaderWrapper(entryHeader));
		PDIHeaderWrapper header;

		public IReadOnlyCollection<IImportCommonLine> Lines
		{
			get
			{
				if (lines == null)
				{
					lines = entryHeader.MergedLines
						.Cast<CusEntryLine>()
						.Select(entryLine => new ImportCommonLineWrapper(entryLine))
						.ToList().AsReadOnly();
				}
				return lines;
			}
		}
		IReadOnlyCollection<ImportCommonLineWrapper> lines;
	}
}
