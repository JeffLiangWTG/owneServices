using System.IO;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DataTransfer.MessageDelivery
{
	public class XmlMessageDeliver : IMessageProcessor
	{
		public XmlMessageDeliver(MessageProcessorCommunicationModesResult modes, BusinessObject bizObjToDeliver, IValueObjectDataAdapter dataAdapter, ProcessTaskNotification action, EventInfoProvider eventInfoProvider = null)
			: this(modes, bizObjToDeliver, bizObjToDeliver as IJobNumber, dataAdapter, null, action, eventInfoProvider)
		{
		}

		public XmlMessageDeliver(MessageProcessorCommunicationModesResult modes, BusinessObject bizObjToDeliver, XmlInterchange interchange, ProcessTaskNotification action, EventInfoProvider eventInfoProvider = null)
			: this(modes, bizObjToDeliver, bizObjToDeliver as IJobNumber, null, interchange, action, eventInfoProvider)
		{
		}

		public XmlMessageDeliver(MessageProcessorCommunicationModesResult modes, BusinessObject bizObjToDeliver, IJobNumber jobNumberSource, IValueObjectDataAdapter dataAdapter, ProcessTaskNotification action, EventInfoProvider eventInfoProvider = null)
			: this(modes, bizObjToDeliver, jobNumberSource, dataAdapter, null, action, eventInfoProvider)
		{
		}

		public XmlMessageDeliver(MessageProcessorCommunicationModesResult modes, BusinessObject bizObjToDeliver, GetXmlInterchangeDelegate getXmlInterchangeDelegate, ProcessTaskNotification action, EventInfoProvider eventInfoProvider = null)
			: this(modes, bizObjToDeliver, bizObjToDeliver as IJobNumber, null, null, action, eventInfoProvider)
		{
			this.getXmlInterchangeDelegate = getXmlInterchangeDelegate;
		}

		public XmlMessageDeliver(MessageProcessorCommunicationModesResult modes, BusinessObject bizObjToDeliver, IJobNumber jobNumberSource, IValueObjectDataAdapter dataAdapter, XmlInterchange interchange, ProcessTaskNotification action, EventInfoProvider eventInfo = null)
		{
			Modes = modes;
			bizObj = bizObjToDeliver;
			this.action = action;
			this.dataAdapter = dataAdapter;
			xmlInterchange = interchange;
			jobNumber = GetJobNumberForInitialisation(jobNumberSource);
			triggerObj = (jobNumberSource as BusinessObject) ?? bizObj;
			eventInfoProvider = eventInfo;
		}

		public MessageProcessorCommunicationModesResult Modes { get; }
		IMessageProcessorCommunicationModesResult IMessageProcessor.GetDestinations() => Modes;

		public IBusinessObjectSerializer Serializer { get; private set; }
		readonly BusinessObject bizObj;
		readonly BusinessObject triggerObj;
		readonly ZString jobNumber;
		readonly IValueObjectDataAdapter dataAdapter;
		readonly ProcessTaskNotification action;
		XmlInterchange xmlInterchange;
		readonly EventInfoProvider eventInfoProvider;
		public delegate XmlInterchange GetXmlInterchangeDelegate();
		readonly GetXmlInterchangeDelegate getXmlInterchangeDelegate;

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			var exportContext = new ValueObjectExportContext(notifications);
			var deliveryContext = GetDeliveryContext();
			deliveryContext.Notifications = notifications;

			if (action != null)
			{
				exportContext.SimplifiedXML = WorkflowTriggerActionTypeConstants.IsSimplifiedXml(action.PQ_TriggerType);
			}

			if (Modes.CommunicationModes.Count == 0)
			{
				notifications.AddWarning(Res.GetString("A4D5A706-3BFE-4AAC-A507-EA36EA8AC701", "No communication modes found."));
			}
			else
			{
				if (xmlInterchange == null && getXmlInterchangeDelegate != null)
				{
					xmlInterchange = getXmlInterchangeDelegate();
				}
				foreach (var mode in Modes.CommunicationModes)
				{
					token.ThrowIfCancellationRequested();
					Serializer = BusinessObjectSerializerFactory.GetSerializer(xmlInterchange, dataAdapter, action, exportContext, mode, eventInfoProvider);

					var stream = (SubStreamableStream)GetXmlStreamToDeliver(bizObj, dataAdapter, exportContext);
					if (stream != null)
					{
						deliveryContext.Factory.SubscribeForDispose(stream);
						Delivery.Deliver(deliveryContext, mode, new DeliveryStreamWrapperUXML(stream, deliveryContext.ParentInfo));
					}
				}
			}
		}

		protected virtual Stream GetXmlStreamToDeliver(BusinessObject bizObjXmlInterchange, IValueObjectDataAdapter dataAdapter, INotifications context)
		{
			return Serializer.SerializeToStream(bizObj);
		}

		protected virtual DeliveryContext GetDeliveryContext()
		{
			return new DeliveryContext(bizObj.Factory)
			{
				ParentInfo = EntityInfo.New(bizObj),
				TriggerObjectInfo = EntityInfo.New(triggerObj),
				PurposeCode = (action != null) ? action.PQ_MessagePurpose : ZString.Empty,
			};
		}

		#region Get Delivery

		public TriggerActionCommunicationModeSubstitutor.ExtraDataSubstitutionDelegate ExtraDataSubstitution;

		EDIMessageDelivery Delivery
		{
			get
			{
				if (delivery == null)
				{
					delivery = new EDIMessageDelivery(jobNumber);
					delivery.ExtraDataSubstitution = Substitution;
				}
				return delivery;
			}
		}
		EDIMessageDelivery delivery;

		ZString Substitution(CommunicationModeSubstitutorProperty property, ZString data)
		{
			if (action != null)
			{
				return TriggerActionCommunicationModeSubstitutor.Substitute(action, bizObj, eventInfoProvider?.Event, property, data, ExtraDataSubstitution);
			}
			else if (ExtraDataSubstitution != null)
			{
				return ExtraDataSubstitution(null, bizObj, data);
			}
			else
			{
				return data;
			}
		}

		ZString GetJobNumberForInitialisation(IJobNumber jobNumberSource)
		{
			ZString result = null;
			var jobNumber = jobNumberSource ?? bizObj as IJobNumber;

			if (jobNumber != null)
			{
				result = jobNumber.JobNumber;
			}

			return result;
		}
		#endregion
	}
}
