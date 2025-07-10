namespace CargoWise.ServiceManager.Next.Shared;

public interface IServiceManagerInitializer
{
	void InitializeApplication();
	void InitializeDatabase();
}
