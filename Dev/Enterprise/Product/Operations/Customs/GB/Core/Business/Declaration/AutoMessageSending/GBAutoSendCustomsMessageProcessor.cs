using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public abstract class GBAutoSendCustomsMessageProcessor : AutoSendCustomsMessageProcessor
	{
		protected GBAutoSendCustomsMessageProcessor(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override IEnumerable<Customs.Business.CusEntryHeader> GetEntryHeadersToSendCore()
		{
			return Declaration.ActiveEntryHeaders.Cast<Customs.Business.CusEntryHeader>();
		}

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;
	}
}
