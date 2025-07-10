using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DUASimplifiedImportSendMessageWrapper : DUAImportCommonSendMessageWrapper, IDUASimplifiedImportMessageDataProvider
	{
		public DUASimplifiedImportSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
			Argument.GreaterThan(entryHeader.MergedLines.Count, 0, nameof(entryHeader.MergedLines));
		}

		public IDUAImportCommonHeader Header => header ?? (header = new DUAImportCommonHeaderWrapper(entryHeader));
		DUAImportCommonHeaderWrapper header;

		public IReadOnlyCollection<IDUAImportCommonLine> Lines
		{
			get
			{
				if (lines == null)
				{
					lines = entryHeader.MergedLines
						.Cast<CusEntryLine>()
						.Select(entryLine => new DUAImportCommonLineWrapper(entryLine, IsCanary))
						.ToList().AsReadOnly();
				}
				return lines;
			}
		}
		IReadOnlyCollection<DUAImportCommonLineWrapper> lines;
	}
}
