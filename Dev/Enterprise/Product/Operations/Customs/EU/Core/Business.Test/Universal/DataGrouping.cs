namespace Enterprise.Customs.EU.Business.Testing;

public class DataGrouping(string code, string description, string parent)
{
	public string Code { get; } = code;
	public string Description { get; } = description;
	public string Parent { get; } = parent;
}
