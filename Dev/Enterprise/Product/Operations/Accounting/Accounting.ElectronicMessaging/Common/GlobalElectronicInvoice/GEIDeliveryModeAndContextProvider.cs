using System.Collections.Generic;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class GEIDeliveryModeAndContextProvider
	{
		public IGlobalElectronicInvoiceSerializer Serializer { get; set; }

		public IEnumerable<IEDICommunicationsMode> Modes { get; set; }

		public DeliveryContext Context { get; set; }

		public bool CreateEDIMessageEvenIfThereIsError { get; set; }

		public bool DeliverEDIMessageEvenIfThereIsError { get; set; }
	}
}
