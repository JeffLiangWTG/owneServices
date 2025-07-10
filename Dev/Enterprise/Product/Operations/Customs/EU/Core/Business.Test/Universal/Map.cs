namespace Enterprise.Customs.EU.Business.Testing;

public class Map(string type, string cw1Value, string customsValue, string dataGrouping)
{
	public string Type { get; } = type;
	public string Cw1Value { get; } = cw1Value;
	public string CustomsValue { get; } = customsValue;
	public string DataGrouping { get; } = dataGrouping;
}
