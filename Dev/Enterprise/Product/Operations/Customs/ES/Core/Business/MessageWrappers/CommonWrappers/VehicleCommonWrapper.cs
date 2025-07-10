using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class VehicleCommonWrapper : IVehicleCommon
	{
		public VehicleCommonWrapper(ZString vin, ZString brandName, ZString modelCode)
		{
			Chassis = vin;
			Brand = brandName;
			Model = modelCode;
		}

		public ZString Chassis { get; }

		public ZString Brand { get; }

		public ZString Model { get; }
	}
}
