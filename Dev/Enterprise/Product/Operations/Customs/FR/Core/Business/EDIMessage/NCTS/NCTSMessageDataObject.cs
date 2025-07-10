using System.Xml.Linq;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public abstract class NCTSMessageDataObject<TMessage> : MessageDataObject<TMessage>
		where TMessage : class
	{
		public NCTSMessageDataObject(NCTSFREDIMessage message) : base(message)
		{
		}

		protected override TMessage GetResponseMessage()
		{
			var elements = XElement.Parse(EDIMessage.EM_MessageText);
			var messageBody = elements.Element("MessageBody");
			var type = messageBody.FirstNode;

			return Extensions.Deserialize<TMessage>(type.ToString());
		}
	}
}
