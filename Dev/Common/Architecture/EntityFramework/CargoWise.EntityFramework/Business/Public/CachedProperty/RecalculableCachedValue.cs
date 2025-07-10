namespace CargoWise.EntityFramework
{
	public class RecalculableCachedValue<T> : CachedValue<T>
	{
		public RecalculableCachedValue(GetValueDelegate<T> getValueDelegate) : base(getValueDelegate) { }

		public void InvalidateCache() => cached = false;
	}
}
