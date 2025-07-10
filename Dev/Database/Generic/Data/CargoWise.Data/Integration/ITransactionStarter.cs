namespace CargoWise.Integration
{
	public interface ITransactionStarter
	{
		ITransactionManager BeginTransactionWithManager();
	}
}