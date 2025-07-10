using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Outer)]
	public partial class HazardousMaterial : IDataObject
	{
		[MaxLength(10)]
		public ZString? Code { get; set; }
		public CodeDescriptionPair5Char CodeType { get; set; }

		public List<UNDG> UNDGCollection { get; set; }
	}
}

