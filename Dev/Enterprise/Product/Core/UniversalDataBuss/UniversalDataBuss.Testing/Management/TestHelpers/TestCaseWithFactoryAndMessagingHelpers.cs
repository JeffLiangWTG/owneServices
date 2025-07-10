using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.UniversalDataBuss.Management.Testing
{
	public class TestCaseWithFactoryAndMessagingHelpers : TestCaseWithUniversalObjectFactory
	{
		public TestCaseWithFactoryAndMessagingHelpers()
		{
		}

		public TestCaseWithFactoryAndMessagingHelpers(UniversalObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly UniversalObjectFactory factory;

		protected override UniversalObjectFactory NewUniversalObjectFactory()
		{
			return factory ?? base.NewUniversalObjectFactory();
		}

		protected IEDIMessage GetQueuedUniversalShipmentMessage(UniversalShipment shipmentDataObject)
		{
			return GetQueuedUniversalShipmentMessage(Factory, shipmentDataObject);
		}

		static IEDIMessage WriteDataObjectAndReturnMessage(UniversalObjectFactory factory, IDataObject dataObject, string subType, string nameSpace = null, bool save = true, bool createInterchange = false, string interchangeFrom = "", string interchangeTo = "")
		{
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				new XmlWriter().WriteXML(dataObject, stream, nameSpace);

				using (var reader = new StreamReader(stream))
				{
					return GetQueuedUniversalDataMessage(factory, reader.ReadToEnd(), subType, save, createInterchange, interchangeFrom: interchangeFrom, interchangeTo: interchangeTo);
				}
			}
		}

		public static IEDIMessage GetQueuedUniversalShipmentMessage(UniversalObjectFactory factory, UniversalShipment shipmentDataObject, bool save = true)
		{
			return WriteDataObjectAndReturnMessage(factory, shipmentDataObject, EDIMessageSubTypeList.Codes.XmlUniversalShipment, null, save);
		}

		public static UniversalProcessingResult ProcessUniversalShipment(UniversalShipment shipment)
		{
			var uxmlFactory = new UniversalObjectFactory();
			var message = GetQueuedUniversalShipmentMessage(uxmlFactory, shipment);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var sessiontracker = manager.Process(message);
			return new UniversalProcessingResult
			{
				Message = message,
				ServiceTaskLogger = serviceTaskLog,
				SessionTracker = sessiontracker
			};
		}

		protected IEDIMessage GetQueuedUniversalShipmentMessage(string messageText, bool save = true, bool createInterchange = false, string interchangeHeaderText = "", string interchangeBodyText = "", BusinessObjectFactory factoryToUse = null)
		{
			return GetQueuedUniversalShipmentMessage(Factory, messageText, save, createInterchange, interchangeHeaderText, interchangeBodyText, factoryToUse);
		}

		public static IEDIMessage GetQueuedUniversalShipmentMessage(UniversalObjectFactory factory, string messageText, bool save = true, bool createInterchange = false, string interchangeHeaderText = "", string interchangeBodyText = "", BusinessObjectFactory factoryToUse = null)
		{
			return GetQueuedUniversalDataMessage(factory, messageText, EDIMessageSubTypeList.Codes.XmlUniversalShipment, save, createInterchange, interchangeHeaderText, interchangeBodyText, factoryToUse);
		}

		protected IEDIMessage GetQueuedUniversalEventMessage(UniversalEvent eventDataObject, string nameSpace = null)
		{
			return GetQueuedUniversalEventMessage(Factory, eventDataObject, nameSpace);
		}

		public static IEDIMessage GetQueuedUniversalEventMessage(UniversalObjectFactory factory, UniversalEvent eventDataObject, string nameSpace = null)
		{
			return WriteDataObjectAndReturnMessage(factory, eventDataObject, EDIMessageSubTypeList.Codes.XmlUniversalEvent, nameSpace);
		}

		protected IEDIMessage GetQueuedUniversalEventMessage(string messageText, bool save = true, bool createInterchange = false)
		{
			return GetQueuedUniversalEventMessage(Factory, messageText, save, createInterchange);
		}

		protected IEDIMessage GetQueuedUniversalScheduleMessage(string messageText, bool save = true)
		{
			return GetQueuedUniversalScheduleMessage(Factory, messageText, save);
		}

		public static IEDIMessage GetQueuedUniversalScheduleMessage(UniversalObjectFactory factory, string messageText, bool save = true)
		{
			return GetQueuedUniversalDataMessage(factory, messageText, EDIMessageSubTypeList.Codes.XmlUniversalSchedule, save, false);
		}

		public static IEDIMessage GetQueuedUniversalEventMessage(UniversalObjectFactory factory, string messageText, bool save = true, bool createInterchange = false)
		{
			return GetQueuedUniversalDataMessage(factory, messageText, EDIMessageSubTypeList.Codes.XmlUniversalEvent, save, createInterchange);
		}

		protected IEDIMessage GetQueuedUniversalTransactionBatchMessage(string messageText, bool save = true)
		{
			return GetQueuedUniversalTransactionBatchMessage(Factory, messageText, save);
		}

		public static IEDIMessage GetQueuedUniversalTransactionBatchMessage(UniversalObjectFactory factory, string messageText, bool save = true)
		{
			return GetQueuedUniversalDataMessage(factory, messageText, EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch, save);
		}

		public IEDIMessage GetQueuedUniversalActivityMessage(string messageText)
		{
			return GetQueuedUniversalDataMessage(Factory, messageText, EDIMessageSubTypeList.Codes.XmlUniversalActivity, true);
		}

		static IEDIMessage GetQueuedUniversalDataMessage(UniversalObjectFactory factory, string messageText, string subType, bool save, bool createInterchange = false, string interchangeHeaderText = "", string interchangeBodyText = "", BusinessObjectFactory factoryToUse = null, string interchangeFrom = "", string interchangeTo = "")
		{
			var boFactory = factoryToUse ?? factory.BOFactory;
			var message = boFactory.New<IEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageSubType = subType;
			message.EM_MessageText = messageText;

			if (createInterchange)
			{
				CreateInterchange(message, boFactory, interchangeHeaderText, interchangeBodyText, interchangeFrom, interchangeTo);
			}

			if (save)
			{
				if (factoryToUse == null)
				{
					factory.SaveForTesting();
				}
				else
				{
					boFactory.Save();
				}
			}
			return message;
		}

		static IEDIInterchange CreateInterchange(IEDIMessage message, BusinessObjectFactory factory, string interchangeHeaderText = "", string interchangeBodyText = "", string from = "", string to = "")
		{
			var interchange = factory.New<IEDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.XDC;
			interchange.EI_From = string.IsNullOrEmpty(from) ? "A" : from;
			interchange.EI_To = string.IsNullOrEmpty(to) ? "B" : to;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Received;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			var defaultInterchangeBodyText =
					@"<UniversalInterchange>
						<Header>
							<SenderID>A</SenderID>
							<RecipientID>B</RecipientID>
						</Header>
						<Body>
							{0}
						</Body>
					</UniversalInterchange>";
			interchange.EI_HeaderText = interchangeHeaderText;
			interchange.EI_BodyText = string.IsNullOrEmpty(interchangeBodyText) ? string.Format(defaultInterchangeBodyText, message.EM_MessageText) : interchangeBodyText;
			message.EM_EI = interchange.PK;

			return interchange;
		}

		public static IEDIMessage GetQueuedUniversalTransactionMessage(IDataObject universalTransaction, string interchangeFrom, string interchangeTo)
		{
			return WriteDataObjectAndReturnMessage(new UniversalObjectFactory(), universalTransaction, EDIMessageSubTypeList.Codes.XmlUniversalTransaction, createInterchange: true, interchangeTo: interchangeTo, interchangeFrom: interchangeFrom);
		}

		public static void ProcessMessage(IEDIMessage message, ISimpleLogger logger, ITopLevelDataObject topLevelDataObject = null)
		{
			new UniversalMessageProcessingManager(logger).Process(message, topLevelDataObject);
		}

		public static MessageKeyProviderResult GetKeys(IEDIMessage message, ISimpleLogger logger)
		{
			return new UniversalMessageProcessingManager(logger).GetKeysForBlockingParallelImport(message);
		}
	}

	public class UniversalProcessingResult
	{
		public IEDIMessage Message { get; set; }
		public ISimpleLogger ServiceTaskLogger { get; set; }
		public IXmlSessionTracker SessionTracker { get; set; }
	}
}
