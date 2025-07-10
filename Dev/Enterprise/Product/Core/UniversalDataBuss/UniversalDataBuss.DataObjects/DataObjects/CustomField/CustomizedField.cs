using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class CustomizedField : IDataObject
	{
		public static CustomizedField New(ZString key, IZType value)
		{
			var result = new CustomizedField();
			result.Key = key;
			result.Value = SimpleTypeFormatter.FormatForMultiTypedStringElementTargetInDataObject(value);
			result.DataType = new DataTypeConverter().ToEnumValue(value.GetType());

			return result;
		}

		[MaxLength(KeyMaxLength), Mandatory, CandidateKey]
		public ZString? Key { get; set; }
		[MaxLength(UniversalXmlInfo.MaxStringLength), Mandatory, AllowLineControlWhiteSpace]
		public ZString? Value { get; set; }
		[Mandatory, CandidateKey]
		public DataType? DataType { get; set; }

		public const int KeyMaxLength = 64;
		public const int ValueMaxLength = UniversalXmlInfo.MaxStringLength;
	}
}

