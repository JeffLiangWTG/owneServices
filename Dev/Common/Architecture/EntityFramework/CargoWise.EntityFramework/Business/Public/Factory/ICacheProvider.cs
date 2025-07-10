namespace CargoWise.EntityFramework
{
	public interface ICacheProvider
	{
		T GetCachedValue<T>(object key, GetValueDelegate<T> getValueDelegate);
	}
}
