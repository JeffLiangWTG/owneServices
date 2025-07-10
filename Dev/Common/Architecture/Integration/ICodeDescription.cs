namespace CargoWise.Integration
{
	public interface ICodeDescription
	{
		object PK { get; }
		string Code { get; }
		string Description { get; }
	}
}
