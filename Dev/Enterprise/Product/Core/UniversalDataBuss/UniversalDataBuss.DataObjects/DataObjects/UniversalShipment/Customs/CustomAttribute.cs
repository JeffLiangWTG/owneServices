using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Outer)]
	public class CustomAttribute : IDataObject
	{
		public static CustomAttribute New(ZString key, ZString value)
		{
			return new CustomAttribute() { Key = key, Value = value };
		}

		[MaxLength(50), Mandatory, CandidateKey]
		public ZString? Key { get; set; }
		[MaxLength(20), Mandatory, AllowLineControlWhiteSpace]
		public ZString? Value { get; set; }
	}
}
