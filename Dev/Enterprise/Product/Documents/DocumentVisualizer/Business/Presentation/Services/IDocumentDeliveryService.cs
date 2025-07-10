using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface IDocumentDeliveryService
	{
		void Deliver(IDocumentSupportable documentSupportable, IReadOnlyCollection<IDocumentDelivery> documentDeliveries, bool useDraftWatermark);

		void Deliver(IDocumentSupportable documentSupportable, string emailSubject, IReadOnlyCollection<IDocumentDelivery> documentDeliveries, DocDeliveryContact recipient, bool useDraftWatermark);

		IEDICommunicationSettings GetCommunicationSettings(IBusiness parent, INotifications notifications);
	}
}
