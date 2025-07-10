using CargoWise.EntityFramework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	public interface ILandedCostDataReader
	{
		Enterprise.Integration.LandedCosting.ILandedCostHeader ReadIntoBusinessObject();
		void CollectTransportLogisticsCost(BusinessObject bizObj, ITransportLogisticsCostCollectionParent transportLogisticsCostCollectionSupporter);
	}
}
