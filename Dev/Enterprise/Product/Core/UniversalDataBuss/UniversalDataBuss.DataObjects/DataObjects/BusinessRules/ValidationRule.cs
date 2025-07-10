using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class ValidationRule : IValidationRule
	{
		[Mandatory, MaxLength(20)]
		public ZString? Code { get; set; }
		public ZInt? Sequence { get; set; }
		[MaxLength(2048)]
		public ZString? MessageLog { get; set; }
		[MaxLength(7)]
		public ZString? Result { get; set; }
	}
}
