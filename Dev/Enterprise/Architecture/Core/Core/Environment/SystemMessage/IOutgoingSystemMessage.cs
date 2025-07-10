namespace Enterprise.ZArchitecture.Environment
{
	using System.IO;
	using CargoWise.EntityFramework;

	/// <summary>
	/// Create outgoing system messages.
	/// System messages are messages sent between client ediEnterprises and ediProd
	/// for licencing, eRequests, etc.
	/// </summary>
	public interface IOutgoingSystemMessage
	{
		/// <summary>
		/// Create a compressed, encrypted message from a stream.
		/// </summary>
		/// <remarks>The compressed, encrypted message will be converted to Base 64 and wrapped in an XML element with name set to "messageName"</remarks>
		/// <param name="messageName">A message name from SystemMessageList.Descriptions</param>
		/// <param name="recipientId">Nine character recipient licence code. Use null to send to ediProd.</param>
		void CreateSecure(BusinessObjectFactory factory, string messageName, Stream messageStream, string recipientId = null);

		/// <summary>
		/// Create a message directly from an XML string. The XML will not be compressed or encrypted.
		/// </summary>
		/// <param name="recipientId">Nine character recipient licence code. Null to send to ediProd.</param>
		void Create(BusinessObjectFactory factory, string xmlMessageBody, string recipientId = null);
	}

#if DEBUG
	public class DebugOnlyOutgoingSystemMessage : IOutgoingSystemMessage
	{
		public DebugOnlyOutgoingSystemMessage()
		{
			Initialize();
		}

		static public void Initialize()
		{
			CreateSecureCalls = 0;
			CreateCalls = 0;
			Factory = null;
			MessageName = null;
			MessageStream = null;
			RecipientId = null;
			XmlMessageBody = null;
		}

		static public int CreateSecureCalls;
		static public int CreateCalls;
		static public BusinessObjectFactory Factory;
		static public string MessageName;
		static public byte[] MessageStream;
		static public string RecipientId;
		static public string XmlMessageBody;

		public void Create(BusinessObjectFactory factory, string xmlMessageBody, string recipientId = null)
		{
			++CreateCalls;
			Factory = factory;
			XmlMessageBody = xmlMessageBody;
			RecipientId = recipientId;
		}

		public void CreateSecure(BusinessObjectFactory factory, string messageName, Stream messageStream, string recipientId = null)
		{
			++CreateSecureCalls;
			Factory = factory;
			MessageName = messageName;
			MessageStream = ((MemoryStream)messageStream).ToArray();
			RecipientId = recipientId;
		}
	}
#endif

}
