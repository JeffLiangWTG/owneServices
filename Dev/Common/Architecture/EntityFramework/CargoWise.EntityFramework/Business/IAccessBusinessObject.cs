namespace CargoWise.EntityFramework
{
	public interface IAccessBusinessObject
	{
		object this[string propertyName] { get; }
		bool IsPropertyReadOnly(string propertyName);
	}
}
