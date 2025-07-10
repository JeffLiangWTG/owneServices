using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.GB.Business
{
	public interface IGBDeclarationMessageSender : IDeclarationMessageSender
	{
		bool CanSendMessage(JobDeclaration declaration, IEnumerable<EU.Business.Declaration.CusEntryHeader> entryCollection, ISendsMessagesToCustoms sendMessagesToCustoms, CusdecMessageFunction functionNewDeletedAmended);
		bool AnyEntryHeadersAreFailed(IEnumerable<EU.Business.Declaration.CusEntryHeader> cusEntryHeaderCollection);
	}
}
