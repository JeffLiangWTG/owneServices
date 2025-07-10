
namespace Enterprise.UniversalDataBuss.Integration
{
	public interface ITransactionDataContextManager : IDataContextManager
	{
		bool ManagesTransactions { get; }
		ITopLevelDataObjectWriter GetTransactionDataObjectWriter(IDataWritingManager writeManager);
	}
}
