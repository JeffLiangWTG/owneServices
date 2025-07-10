using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class Box40AmendmentImportSendMessageWrapper : ImportCommonSendMessageWrapper, IBox40AmendmentImportMessageDataProvider
	{
		public Box40AmendmentImportSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
			Argument.GreaterThan(entryHeader.MergedLines.Count, 0, nameof(entryHeader.MergedLines));
		}

		public IReadOnlyCollection<IBox40AmendmentLine> Lines
		{
			get
			{
				if (lines == null)
				{
					lines = entryHeader.MergedLines
						.Cast<CusEntryLine>()
						.Select(entryLine => new Box40AmendmentLineWrapper(entryLine))
						.ToList().AsReadOnly();
				}
				return lines;
			}
		}
		ReadOnlyCollection<Box40AmendmentLineWrapper> lines;
	}
}
