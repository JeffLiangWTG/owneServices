using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public class DeliveryOrderCollectionRegistryItem : StronglyTypedRegistryItem<DeliveryOrderCollection>
	{
		public DeliveryOrderCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new DeliveryOrderRegistryDataType(), storage, options))
		{
		}

		#region DeliveryOrderRegistryDataType

		[RegistryEditor("Enterprise.DocumentEngineCore.GUI.Registry.DeliveryOrderRegistryItemEditor, Enterprise.DocumentEngineCore.GUI")]
#if DEBUG
		public
#endif
		class DeliveryOrderRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DeliveryOrderCollection>
		{
		}

		#endregion

		#region FindDeliveryOrderForPrincipal

		public DeliveryOrder FindDeliveryOrderForPrincipal(ZGuid principalPK)
		{
			foreach (DeliveryOrder deliveryOrder in Value)
			{
				if (deliveryOrder.PrincipalPK == principalPK)
				{
					return deliveryOrder;
				}
			}

			return null;
		}

		#endregion
	}
}
