using System.Collections.Generic;

namespace Enterprise.Customs.EU.Business.Testing;

public class CusCodeType(string typeCode, string description, IReadOnlyCollection<CusCodeAttributeType> attributeTypes, IReadOnlyCollection<CusCode> cusCodes)
{
	public string TypeCode { get; } = typeCode;
	public string Description { get; } = description;
	public IReadOnlyCollection<CusCodeAttributeType> AttributeTypes { get; } = attributeTypes;
	public IReadOnlyCollection<CusCode> CusCodes { get; } = cusCodes;
}
