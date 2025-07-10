
namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Indicate if a strict concurreny error happens.
	/// E.g., we can set a business context in SetConflictWithCriticalFieldsBusinessContext, then judge it.
	/// </summary>
	public interface IConflictWithCriticalFields
	{
		void SetConflictWithCriticalFieldsBusinessContext();
	}
}
