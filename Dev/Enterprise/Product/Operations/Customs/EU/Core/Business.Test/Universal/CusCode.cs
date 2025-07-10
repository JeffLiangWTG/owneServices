using System.Collections.Generic;

namespace Enterprise.Customs.EU.Business.Testing;

public class CusCode(string code, string dataGrouping, IReadOnlyCollection<CusCodeAttribute> attributes)
{
	public string Code { get; } = code;
	public string DataGrouping { get; } = dataGrouping;
	public IReadOnlyCollection<CusCodeAttribute> Attributes { get; } = attributes;
}
