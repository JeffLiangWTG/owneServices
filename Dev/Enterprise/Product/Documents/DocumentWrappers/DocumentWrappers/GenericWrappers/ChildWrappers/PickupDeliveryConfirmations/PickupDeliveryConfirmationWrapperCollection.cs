using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PickupDeliveryConfirmationsWrapperCollection : GenericWrapperCollection<PickupDeliveryConfirmationsWrapper>
	{
		public PickupDeliveryConfirmationsWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public PickupDeliveryConfirmationsWrapperCollection(FreightWrapper parentWrapper, BusinessObjectFactory factory, IEnumerable<CommonPickupDeliveryConfirm> confirmationCollection)
			: base(factory)
		{
			foreach (CommonPickupDeliveryConfirm confirmation in confirmationCollection)
			{
				if (!ContainsWrappedObject(confirmation.PK))
				{
					PickupDeliveryConfirmationsWrapper wrapper = new PickupDeliveryConfirmationsWrapper(confirmation, Factory);
					wrapper.ParentWrapper = parentWrapper;
					Add(wrapper);
				}
			}
		}
	}
}
