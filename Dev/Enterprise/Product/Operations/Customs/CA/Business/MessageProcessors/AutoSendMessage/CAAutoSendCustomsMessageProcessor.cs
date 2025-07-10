using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public abstract class CAAutoSendCustomsMessageProcessor : Customs.Business.AutoSendCustomsMessageProcessor
	{
		protected CAAutoSendCustomsMessageProcessor(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		protected abstract ZString EntryType { get; }

		protected override IEnumerable<Customs.Business.CusEntryHeader> GetEntryHeadersToSendCore()
		{
			yield return Declaration.GetEntryHeaderFor(EntryType);
		}

		protected sealed override ZBool SendCustomsMessageCore(INotifications notifications, Customs.Business.CusEntryHeader entryHeader)
		{
			var entry = (CusEntryHeader)entryHeader;
			return SendMessageCore(notifications, entry);
		}

		protected abstract ZBool SendMessageCore(INotifications notifications, CusEntryHeader entryHeader);
	}
}
