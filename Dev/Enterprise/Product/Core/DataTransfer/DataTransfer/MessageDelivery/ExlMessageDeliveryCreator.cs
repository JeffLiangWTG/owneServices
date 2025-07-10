using CargoWise.EntityFramework;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DataTransfer.MessageDelivery
{
	class ExlMessageDeliveryCreator : IExlMessageDeliveryCreator
	{
		#region IExlMessageDeliveryCreator Members

		public IMessageProcessor CreateExlMessageDelivery(MessageProcessorCommunicationModesResult modes, BusinessObject bizObjToDeliver, ProcessTaskNotification action, EventInfoProvider eventInfoProvider)
		{
			return new XmlMessageDeliver(modes, bizObjToDeliver, bizObjToDeliver as IJobNumber, new StorageDocsBaseValueObjectDataAdatper(), action, eventInfoProvider);
		}

		#endregion
	}
}
