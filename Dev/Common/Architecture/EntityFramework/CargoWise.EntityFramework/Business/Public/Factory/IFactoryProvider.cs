namespace CargoWise.EntityFramework
{
	public interface IFactoryProvider
	{
		BusinessObjectFactory Factory { get; }
	}
}
