using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	public class CalculateDeliveryDueDateTransportModeList
	{
		public CalculateDeliveryDueDateTransportModeList(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, "factory");
		}
		readonly BusinessObjectFactory factory;

		CalculateDeliveryDueDateTransportModeCollection DefaultTransportModeList
		{
			get
			{
				return factory.GetCachedValue<CalculateDeliveryDueDateTransportModeCollection>("CalculateDeliveryDueDateTransportModeList.DefaultTransportModeList", delegate
				{
					var result = new CalculateDeliveryDueDateTransportModeCollection();
					result.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
					result.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, false);
					result.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, false);
					result.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, false);

					return result;
				});
			}
		}

		public CalculateDeliveryDueDateTransportModeCollection GetDefaultCalculateDeliveryDueDateOptions()
		{
			var result = new CalculateDeliveryDueDateTransportModeCollection();
			result.AddRange(DefaultTransportModeList);
			return result;
		}
	}
}
