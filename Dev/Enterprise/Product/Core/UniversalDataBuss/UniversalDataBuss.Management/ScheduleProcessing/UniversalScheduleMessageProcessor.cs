using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalSchedule = Enterprise.UniversalDataBuss.DataObjects.Universal.Schedule;

namespace Enterprise.UniversalDataBuss.Management.ScheduleProcessing
{
	class UniversalScheduleMessageProcessor : ITopLevelDataObjectProcessor
	{
		public MessageStatus ProcessDataObject(IEDIMessage message, ITopLevelDataObject dataObject, IUniversalObjectFactory factory, IXmlSessionTracker logger)
		{
			return ImportSchedule((UniversalSchedule)dataObject, ((UniversalObjectFactory)factory).BOFactory, logger);
		}

		public MessageKeyProviderResult GetKeys(IEDIMessage message, ITopLevelDataObject dataObject, IUniversalObjectFactory factory, IXmlSessionTracker logger)
		{
			return new MessageKeyProviderResult(Enumerable.Empty<(string KeyValue, string KeySource)>());
		}

		public void ValidateDataObject(ITopLevelDataObject dataObject, IXmlSessionTracker logger)
		{
		}

		MessageStatus ImportSchedule(UniversalSchedule dataObject, BusinessObjectFactory factory, IXmlSessionTracker logger)
		{
			if (dataObject.Transport.Sea != null && dataObject.Transport.Air == null && dataObject.Transport.Rail == null && dataObject.Transport.Road == null)
			{
				var importer = (IUniversalScheduleImporter)Activator.CreateInstance(ObjectFactory.GetType("IUniversalScheduleImporter"), factory, logger);
				return importer.ImportUniversalSchedule(dataObject);
			}

			return MessageStatus.Rejected;
		}
	}
}
