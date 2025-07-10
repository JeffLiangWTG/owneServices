namespace CargoWise.Common
{
	public class DisposableObjectWithoutLeakListener : Disposable
	{
		public DisposableObjectWithoutLeakListener() : base(false)
		{
		}

		protected override void Dispose(bool isDisposing)
		{
		}
	}
}