using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Initialisation.Providers
{
	class ZTypeObjectCache : IObjectCache
	{
		#region IObjectCache Members

		public ICultureProvider CultureProvider
		{
			get { return new CultureProvider(); }
		}

		public IDateTimeProvider DateTimeProvider
		{
			get { return new DateTimeProvider(); }
		}

		public IRoundingProvider RoundingProvider
		{
			get { return new RoundingProvider(); }
		}

		#endregion
	}
}
