
namespace CargoWise.EntityFramework
{
	public interface ICanDetach
	{
		bool CanDetach { get; }
		string ReasonNotToBeAbleToDetach { get; }
		string GetWarningBeforeBeingDetached();
	}
}
