using System.Diagnostics.CodeAnalysis;

namespace CargoWise.Common
{
	/// <summary>
	/// Provides services based on an interface
	/// </summary>
	public static class ServiceLocator
	{
		/// <summary>
		/// Provide an interface implementation of type T for the provider.
		/// Will either return the provider if it implements T, or will call GetService if the provider implements IServiceLocator.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="provider"></param>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public static T GetService<T>(object provider)
		{
			if (provider is T)
			{
				return (T)provider;
			}
			else
			{
				IServiceLocator locator = provider as IServiceLocator;
				if (locator != null)
				{
					return (T)locator.GetService(typeof(T));
				}
				else
				{
					return default(T);
				}
			}
		}
	}
}
