namespace CargoWise.EntityFramework
{
	public interface ICancellable
	{
		string CanCancel();
		string CanReactivate();
		bool IsCancelled { get; set; }
		bool IsCancelledHasChanged { get; }
	}
}
