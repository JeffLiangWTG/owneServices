using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.COD;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.COD
{
	public class CODWrapper : ICOD
	{
		public CODWrapper(CusEntryHeader entryHeader, ZString actionCode, List<IArticle> articles, List<IGen> gens)
		{
			this.entryHeader = entryHeader;
			this.actionCode = actionCode;
			this.articles = articles;
			this.gens = gens;
		}

		readonly List<IArticle> articles;
		readonly List<IGen> gens;
		readonly CusEntryHeader entryHeader;
		readonly ZString actionCode;

		public Messaging.Interfaces.Common.IMessageEnvelope MessageEnvelope => messageEnvelope ?? (messageEnvelope = new CODMessageEnvelopWrapper(entryHeader));
		Messaging.Interfaces.Common.IMessageEnvelope messageEnvelope;

		public ZString ActionCode => actionCode;

		public ZString FileReference => entryHeader.CH_BGMReference;

		public IEnumerable<IArticle> Items => articles;

		public IEnumerable<IGen> Gens => gens;
	}
}
