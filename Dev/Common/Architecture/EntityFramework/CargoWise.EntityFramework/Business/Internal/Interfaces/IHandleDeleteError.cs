namespace CargoWise.EntityFramework
{
	public interface IHandleDeleteError
	{
		bool RollbackAfterDeleteError { get; }
		bool RebindAfterDeleteError { get; }
		bool DisableFormOnDeleteConcurrencyError { get; }
	}
}
