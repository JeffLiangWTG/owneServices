using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	///<summary>
	/// Xml Tag:EnveloppeMessage
	///</summary>
	public interface IMessageEnvelope
	{
		///<summary>
		/// Xml Tag:schemaID
		///</summary>
		ZString SchemaID { get; }

		///<summary>
		/// Xml Tag:schemaVersion
		///</summary>
		ZString SchemaVersion { get; }

		///<summary>
		/// Xml Tag:partyId
		///</summary>
		ZString PartnerId { get; }

		///<summary>
		/// Xml Tag:transactionId
		///</summary>
		ZString TransactionId { get; }

		///<summary>
		/// Xml Tag:numseq
		///</summary>
		ZShort NumSeq { get; }
	}
}
