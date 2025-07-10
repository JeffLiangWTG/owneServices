using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class Box44ImportSendMessageWrapper : ImportCommonSendMessageWrapper, IBox44ImportMessageDataProvider
	{
		public Box44ImportSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
			Argument.GreaterThan(entryHeader.MergedLines.Count, 0, nameof(entryHeader.MergedLines));
		}

		public IReadOnlyCollection<IBox44Line> Lines
		{
			get
			{
				if (lines == null)
				{
					lines = entryHeader.MergedLines
						.Cast<CusEntryLine>()
						.Where(entryLine => entryLine.HasSupportingDocumentsToSend())
						.Select(entryLine => new Box44LineWrapper(entryLine))
						.ToList().AsReadOnly();
				}
				return lines;
			}
		}
		IReadOnlyCollection<Box44LineWrapper> lines;
	}
}
