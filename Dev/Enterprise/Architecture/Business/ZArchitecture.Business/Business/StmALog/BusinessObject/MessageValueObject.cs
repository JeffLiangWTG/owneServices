using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public class MessageValueObject : NonPersistentBusinessObject, IObsoleteValidation, IDisposable
	{
		public MessageValueObject(IEDIMessage message)
		{
			this.Message = Argument.NotNull(message, "IEDIMessage message");

			if (Message.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalEvent)
			{
				eventValueObject = GetEventValueObject();
				DataContext = eventValueObject.DataContext;
			}
			else
			{
				DataContext = GetDataContext();
			}
		}

		IXmlEventValueObject GetEventValueObject()
		{
			var deserializer = ObjectFactory.GetDesignerSafe<IXmlEventDeserializer>();

			using (var messageTextReader = Message.GetEM_MessageTextReader())
			{
				return deserializer.Parse(messageTextReader, new DummyLogger());
			}
		}

		IDataContextDataObject GetDataContext()
		{
			var deserializer = ObjectFactory.GetDesignerSafe<IXmlDataContextDeserializer>();

			using (var messageTextReader = Message.GetEM_MessageTextReader())
			{
				return deserializer.Parse(messageTextReader, new DummyLogger());
			}
		}

		internal IXmlEventValueObjectContextValueList ContextList
		{
			get { return eventValueObject == null ? null : eventValueObject.Context; }
		}

		readonly IXmlEventValueObject eventValueObject;

		public IDataContextDataObject DataContext
		{
			get;
			private set;
		}

		public IEDIMessage Message
		{
			get;
			private set;
		}

		public ZString Sender
		{
			get { return Message.Interchange == null ? ZString.Empty : Message.Interchange.EI_From; }
		}

		public ZString Recipient
		{
			get { return Message.Interchange == null ? ZString.Empty : Message.Interchange.EI_To; }
		}

		public void Dispose()
		{
			eventValueObject?.Dispose();
		}
	}
}
