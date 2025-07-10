namespace Enterprise.Customs.EU.Business.Testing;

public class CusCodeAttributeType(string name, string dataGrouping)
{
	public string Name { get; } = name;
	public string DataGrouping { get; } = dataGrouping;
}
