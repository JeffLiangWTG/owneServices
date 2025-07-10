using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	public interface IJobCostingData
	{
		IDataContextDataObject DataContext { get; }
		JobCosting JobCosting { get; set; }
	}
}
