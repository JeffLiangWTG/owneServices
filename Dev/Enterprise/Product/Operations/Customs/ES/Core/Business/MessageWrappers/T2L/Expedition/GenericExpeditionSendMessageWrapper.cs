using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public abstract class GenericExpeditionSendMessageWrapper : EntryHeaderCommonSendMessageWrapper, IESEDIMessageCollectionProvider
	{
		public GenericExpeditionSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
			Argument.GreaterThan(entryHeader.MergedLines.Count, 0, nameof(entryHeader.MergedLines));
		}

		public IReadOnlyCollection<IExpeditionLine> Lines
		{
			get
			{
				if (lines == null)
				{
					lines = entryHeader.MergedLines.Cast<CusEntryLine>().Select(entryLine => new ExpeditionLineWrapper(entryLine)).ToList().AsReadOnly();
				}
				return lines;
			}
		}
		IReadOnlyCollection<ExpeditionLineWrapper> lines;
	}
}
