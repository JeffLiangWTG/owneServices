using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(Placement.Outer)]
	public class UberOrder : IDataObject
	{
		public UberWarehouse Warehouse { get; set; }
	}
}
