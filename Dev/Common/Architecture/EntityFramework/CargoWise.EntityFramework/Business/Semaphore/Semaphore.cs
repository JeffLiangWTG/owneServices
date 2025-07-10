namespace CargoWise.EntityFramework
{
	public class Semaphore : ISemaphoreItemInternals
	{
		public bool IsSuspended
		{
			get { return Count > 0; }
		}

		int Count;

		#region ISemaphoneItemInternals Members

		void ISemaphoreItemInternals.Increment()
		{
			Count++;
		}

		void ISemaphoreItemInternals.Decrement()
		{
			Count--;
		}

		#endregion
	}
}
