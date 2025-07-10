using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class TEU : IDataObject
	{
		[Mandatory]
		public ZDecimal? NumberOfTEU { get; set; }
		[Mandatory]
		public ZDecimal? TonnesPerTEU { get; set; }
		public ZDecimal? ContainerEmptyWeightPerTEU { get; set; }
		public UnitOfWeight ContainerEmptyWeightPerTEUUnit { get; set; }
	}
}
