namespace CargoWise.EntityFramework
{
	public interface ISemaphoreItemInternals
	{
		void Increment();
		void Decrement();
	}
}
