using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ReceptionSendMessageWrapper : EntryHeaderCommonSendMessageWrapper, IReceptionMessageDataProvider
	{
		public ReceptionSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
			Argument.GreaterThan(entryHeader.MergedLines.Count, 0, nameof(entryHeader.MergedLines));
		}

		public IReceptionHeader Header => header ?? (header = new ReceptionHeaderWrapper(entryHeader));
		ReceptionHeaderWrapper header;

		public IReadOnlyCollection<IT2LLineCommon> Lines
		{
			get
			{
				if (lines == null)
				{
					lines = entryHeader.MergedLines
						.Cast<CusEntryLine>()
						.Select(entryLine => new ReceptionLineWrapper(entryLine))
						.ToList().AsReadOnly();
				}
				return lines;
			}
		}
		IReadOnlyCollection<ReceptionLineWrapper> lines;
	}
}
