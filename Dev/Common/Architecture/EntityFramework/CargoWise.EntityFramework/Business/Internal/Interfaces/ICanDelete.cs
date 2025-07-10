using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Implementing CanDelete will force ZGrid to verify if a business object CanDelete before allowing
	/// it to be deleted.
	/// </summary>
	public interface ICanDelete
	{
		bool CanDelete { get; }
		MultilingualString ReasonForNotAbleToDelete { get; }
		void OnCannotDelete();
		MultilingualString GetWarningBeforeBeingDeleted();
	}
}
