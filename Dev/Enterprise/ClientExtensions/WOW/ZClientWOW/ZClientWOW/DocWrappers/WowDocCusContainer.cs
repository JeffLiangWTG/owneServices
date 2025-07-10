using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentWrappers.Customs.AU;

namespace Enterprise.Client.Wow
{
	public class WowDocCusContainer : DocCusContainer
	{
		#region Constructors and Type Overriding

		WowDocCusContainer(CusContainer cusContainer, BusinessObjectFactory factoryToWrap)
			: base(cusContainer, factoryToWrap)
		{
		}

		public static new DocCusContainer New(CusContainer cusContainer, BusinessObjectFactory factoryToWrap)
		{
			return (cusContainer == null) ? null : new WowDocCusContainer(cusContainer, factoryToWrap);
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		#endregion

		public ZString WareHouseLocation
		{
			get
			{
				ZString result = ZString.Empty;
				if (CusContainer.OrderDeliveryContainers != null &&
					CusContainer.OrderDeliveryContainers.Length > 0 &&
					CusContainer.OrderDeliveryContainers[0].OrderLineDelivery != null)
				{
					result = CusContainer.OrderDeliveryContainers[0].OrderLineDelivery.J4_OA_NKDeliveryPoint;
				}
				return result;
			}
		}

		new WoolworthsCusContainer CusContainer
		{
			get { return (WoolworthsCusContainer)base.CusContainer; }
		}
	}
}
