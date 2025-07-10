namespace Enterprise.ZArchitecture.Business
{
	public interface IDataVersionLoggingSupported
	{
		bool IsDataVersionsAutoLogged { get; }
		DataVersionLogValueFormatter DataVersionLogValueFormatter { get; }
	}
}
