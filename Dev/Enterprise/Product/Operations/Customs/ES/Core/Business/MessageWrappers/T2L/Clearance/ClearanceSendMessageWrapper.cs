using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ClearanceSendMessageWrapper : EntryHeaderCommonSendMessageWrapper, IClearanceMessageDataProvider
	{
		public ClearanceSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
			Argument.GreaterThan(entryHeader.MergedLines.Count, 0, nameof(entryHeader.MergedLines));
		}

		public IClearanceHeader Header => header ?? (header = new ClearanceHeaderWrapper(entryHeader));
		ClearanceHeaderWrapper header;

		public IReadOnlyCollection<IClearanceLine> Lines
		{
			get
			{
				if (lines == null)
				{
					lines = entryHeader.MergedLines
						.Cast<CusEntryLine>()
						.Select(entryLine => new ClearanceLineWrapper(entryLine))
						.ToList().AsReadOnly();
				}
				return lines;
			}
		}
		IReadOnlyCollection<ClearanceLineWrapper> lines;
	}
}
