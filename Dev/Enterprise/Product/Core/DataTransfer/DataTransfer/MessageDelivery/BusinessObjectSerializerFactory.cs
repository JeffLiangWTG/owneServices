using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DataTransfer.MessageDelivery
{
	static class BusinessObjectSerializerFactory
	{
		internal static IBusinessObjectSerializer GetSerializer(XmlInterchange xmlInterchange, IValueObjectDataAdapter dataAdapter, ProcessTaskNotification action, INotifications notifications, IEDICommunicationsMode mode, EventInfoProvider eventInfo)
		{
			IBusinessObjectSerializer bizObjSerializer;

			var context = new ValueObjectExportContext(notifications);
			if (action != null)
			{
				context.SimplifiedXML = WorkflowTriggerActionTypeConstants.IsSimplifiedXml(action.PQ_TriggerType);
			}

			if (xmlInterchange != null)
			{
				if (xmlInterchange.Payload.DataAdapter == null)
				{
					xmlInterchange.Payload.DataAdapter = dataAdapter;
				}
				return new InterchangeSerializer(action, xmlInterchange) { Context = context, Mode = mode };
			}

			if (action == null)
			{
				bizObjSerializer = new XmlBusinessObjectSerializer(dataAdapter, ZString.Empty) { Context = context, Mode = mode };
			}
			else if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendDescartesXml)
			{
				bizObjSerializer = new DxlBusinessObjectSerializer(dataAdapter, action.PQ_MessagePurpose) { Context = context, Mode = mode };
			}
			else if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendNativeXML)
			{
				bizObjSerializer = ObjectFactory.Get<IBusinessObjectSerializer>("NativeXmlSerializer");
			}
			else if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendEDocXml)
			{
				bizObjSerializer = new EDocSerializer(dataAdapter, action, context, mode, eventInfo);
			}
			else
			{
				bizObjSerializer = new XmlBusinessObjectSerializer(dataAdapter, action.PQ_MessagePurpose) { Context = context, Mode = mode };
			}

			return bizObjSerializer;
		}
	}
}
