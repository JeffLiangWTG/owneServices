using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public partial class Equipment : IDataObject
	{
		public Equipment()
		{
		}

		public Equipment(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		[MaxLength(35)]
		public ZString? IdentificationNumber { get; set; }
		public List<SealNumber> SealNumberCollection { get; private set; }
	}
}
