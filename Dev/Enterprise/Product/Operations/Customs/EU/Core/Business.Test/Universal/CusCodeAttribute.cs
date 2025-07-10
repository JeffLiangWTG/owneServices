namespace Enterprise.Customs.EU.Business.Testing;

public class CusCodeAttribute(string name, string value)
{
	public string Name { get; } = name;
	public string Value { get; } = value;
}
