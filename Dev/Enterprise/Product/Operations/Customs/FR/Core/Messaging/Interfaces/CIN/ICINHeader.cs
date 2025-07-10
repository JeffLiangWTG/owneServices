using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Messaging.Interfaces.CIN
{
	public interface ICINHeader
	{
		ZDateTime MovementTime { get; }
		ZString CurrentLocation { get; }
		ZString NewLocation { get; }
		ZString CustomsStatus { get; }
		IEnumerable<ICINCustomsDocument> CustomsDocuments { get; }
		ZString CustomsReference { get; }
		IEnumerable<ICINLine> Bills { get; }
		IMessageEnvelope MessageEnvelope { get; }
	}
}
