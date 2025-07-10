namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Functionality used by BusinessObject that is hidden from the module programmer to limit complexity.
	/// </summary>
	public interface IBusinessObjectCollectionInternals
	{
		bool HasChangesFromDelete { get; set; }
		bool MastersAreInDatabase { get; }
		bool MastersAreDeleted { get; }
		bool IsListChangedSuspended { get; }
		void FireListResetEvent();
		bool HasChangesFromDatabase();
	}
}
