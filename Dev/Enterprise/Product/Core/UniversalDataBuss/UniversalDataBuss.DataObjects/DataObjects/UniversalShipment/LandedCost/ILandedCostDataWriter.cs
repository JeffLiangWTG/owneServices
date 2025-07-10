using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	public interface ILandedCostDataWriter
	{
		List<TransportLogisticsCost> PopulateTransportLogisticsCostCollection(BusinessObject bizObj);
		LandedCostDetail PopulateLandedCostDetail(BusinessObject bizObj);
	}
}
