using WTG.ApplicationLogging.Abstractions;

namespace CargoWise.Logging
{
	public static class CategorizedApplicationLoggerFactoryExtensions
	{
		public static ICategorizedApplicationLoggerFactory AsCategorizedApplicationLoggerFactory(this IApplicationLoggerFactory applicationLoggerFactory)
		{
			if (applicationLoggerFactory is ICategorizedApplicationLoggerFactory categorizedApplicationLoggerFactory)
			{
				return categorizedApplicationLoggerFactory;
			}
			return new CategorizedApplicationLoggerFactory(applicationLoggerFactory);
		}
	}
}
