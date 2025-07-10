using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public partial class GreenhouseGasEmission : IDataObject
	{
		public ZDecimal? CO2ePerTonne { get; set; }
		public UnitOfWeight CO2ePerTonneUnit { get; set; }
		public ZDecimal? CO2eDistanceInKm { get; set; }
		public ZDecimal? CO2ePerTEU { get; set; }
		public UnitOfWeight	CO2ePerTEUUnit { get; set; }
		public ZDecimal? CO2e { get; set; }
		public UnitOfWeight CO2eUnit { get; set; }
		[MaxLength(3)]
		public ZString? CO2eStatus { get; set; }
		public CodeDescriptionPair CO2eDescriptiveStatus { get; set; }
		public List<GreenhouseGas> GreenhouseGasCollection { get; private set; }
	}
}
