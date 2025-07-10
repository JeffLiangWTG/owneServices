using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class EmptyDocumentDeliveryService : IDocumentDeliveryService
	{
		public void Deliver(IDocumentSupportable documentSupportable, IReadOnlyCollection<IDocumentDelivery> documentDeliveries, bool useDraftWatermark = false)
		{
		}

		public void Deliver(IDocumentSupportable documentSupportable, string emailSubject, IReadOnlyCollection<IDocumentDelivery> documentDeliveries, DocDeliveryContact recipient, bool useDraftWatermark = false)
		{
		}

		public IEDICommunicationSettings GetCommunicationSettings(IBusiness parent, INotifications notifications) => new EmptyEDICommunicationSettings();
	}
}
