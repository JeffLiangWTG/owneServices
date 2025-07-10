using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Messaging.Interfaces.CIN
{
	public interface ICINExportMessage
	{
		///<summary>
		/// Xml Tag: EnveloppeMessage
		///</summary>
		IMessageEnvelope MessageEnvelope { get; }

		ZDateTime MessageDate { get; }
		ZString MessageFunctionID { get; }
	}
}
