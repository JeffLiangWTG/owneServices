using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public abstract class AutoSendCustomsMessageProcessor : Customs.Business.AutoSendCustomsMessageProcessor
	{
		public AutoSendCustomsMessageProcessor(JobDeclaration declaration) : base(declaration)
		{
		}

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override IEnumerable<Customs.Business.CusEntryHeader> GetEntryHeadersToSendCore()
		{
			var notifier = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			return Declaration.DoMerge(notifier) ? Declaration.ActiveEntryHeaders.Cast<Customs.Business.CusEntryHeader>() : Enumerable.Empty<Customs.Business.CusEntryHeader>();
		}

		protected override ZBool CanSendEntryHeader(Customs.Business.CusEntryHeader entryHeader)
		{
			return base.CanSendEntryHeader(entryHeader) && AutoSendCustomsMessageRules.Any(x => x.CanSendMessage((CusEntryHeader)entryHeader));
		}

		protected override ZString MessageDescription => (NoResString)"France Customs Declaration";

		protected IAutoSendCustomsMessageRule[] AutoSendCustomsMessageRules => AutoSendCustomsMessages.ToArray();

		protected abstract IEnumerable<IAutoSendCustomsMessageRule> AutoSendCustomsMessages { get; }
	}
}
