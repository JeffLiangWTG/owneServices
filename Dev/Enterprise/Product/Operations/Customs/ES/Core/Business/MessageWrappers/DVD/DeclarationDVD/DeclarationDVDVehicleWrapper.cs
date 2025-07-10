using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DeclarationDVDVehicleWrapper : VehicleCommonWrapper, IDeclarationDVDVehicle
	{
		public DeclarationDVDVehicleWrapper(ZString vin, ZString brandName, ZString modelCode) : base(vin, brandName, modelCode)
		{
			Type = RefCusCodeList.PackageType.Frame;
		}

		public ZString Type { get; }
	}
}
