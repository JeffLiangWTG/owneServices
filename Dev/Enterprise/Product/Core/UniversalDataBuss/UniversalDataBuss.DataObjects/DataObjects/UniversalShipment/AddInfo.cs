using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class AddInfo : IDataObject
	{
		public static AddInfo New(ZString key, ZString value)
		{
			return new AddInfo() { Key = key, Value = value };
		}

		[MaxLength(128), Mandatory, CandidateKey]
		public ZString? Key { get; set; }
		[MaxLength(1024), Mandatory, AllowLineControlWhiteSpace]
		public ZString? Value { get; set; }
	}
}

