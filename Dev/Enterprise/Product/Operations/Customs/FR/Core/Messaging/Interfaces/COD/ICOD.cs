using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.COD
{
	public interface ICOD
	{
		Common.IMessageEnvelope MessageEnvelope { get; }
		ZString ActionCode { get; } //Entete/codact
		ZString FileReference { get; } //Entete/refdos
		IEnumerable<IArticle> Items { get; } //Articles
		IEnumerable<IGen> Gens { get; } //Gens
	}
}
