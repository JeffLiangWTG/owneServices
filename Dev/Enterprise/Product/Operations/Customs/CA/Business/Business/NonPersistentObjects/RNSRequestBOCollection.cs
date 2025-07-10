using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business
{
	public class RNSRequestBOCollection : NonPersistentBusinessObjectCollection<RNSRequestBO>
	{
		public RNSRequestBOCollection(ForwardingConsol consol)
		{
			if (consol != null)
			{
				this.consol = consol;
				Load();
			}
		}
		readonly ForwardingConsol consol;

		protected override BusinessObject CreateNonPersistentBusinessObject() => null;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		public IEnumerable<RNSRequestBO> GetSelectedRequestBOs() => this.Cast<RNSRequestBO>().Where(x => x.Selected);

		public void SelectAll() => this.Cast<RNSRequestBO>().ForEach(x => x.Selected = true);

		public void DeselectAll() => this.Cast<RNSRequestBO>().ForEach(x => x.Selected = false);

		public override void Load()
		{
			using (SuspendSettingHasChanges())
			{
				RemoveAll();
				foreach (ForwardingShipment shipment in consol.Shipments)
				{
					var messageBO = new RNSMessagingBO(new RNSPlugInSupportShipmentWrapper(shipment));
					Add(new RNSRequestBO(messageBO, RNSMessageTypes.Codes.ArrivalCertification, consol.Factory, false, false));
				}
			}
		}
	}
}
