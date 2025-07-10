using System.Collections.Generic;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	public interface IVehicleCollectionParent
	{
		List<Vehicle> VehicleCollection  { get; set; }
	}
}
