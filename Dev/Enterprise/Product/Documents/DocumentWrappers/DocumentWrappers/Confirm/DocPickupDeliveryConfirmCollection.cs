using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocPickupDeliveryConfirmCollection : DocumentWrapperCollection<DocPickupDeliveryConfirm>
	{
		public DocPickupDeliveryConfirmCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocPickupDeliveryConfirmCollection(CommonPickupDeliveryConfirmCollection confirms)
			: base(confirms.Factory)
		{
			foreach (CommonPickupDeliveryConfirm confirm in confirms)
			{
				Add(DocPickupDeliveryConfirm.New(confirm, Factory));
			}
		}

		public ZBool HasAtLeastOneDeliveredLeg
		{
			get
			{
				foreach (DocPickupDeliveryConfirm leg in this)
				{
					if (!leg.DeliverySignedFor.IsEmpty)
					{
						return ZBool.True;
					}
				}
				return ZBool.False;
			}
		}
	}
}
