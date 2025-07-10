using CargoWise.Application;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	public sealed class GlowRegistryForTest : IGlowRegistry
	{
		public static GlowRegistryForTest Get()
		{
			GlowRegistryForTest result = ObjectFactory.Get<IGlowRegistry>() as GlowRegistryForTest;
			if (result == null)
			{
				result = new GlowRegistryForTest();
				ObjectFactory.Substitute<IGlowRegistry>(result);
			}
			return result;
		}

		string glowServiceUri = "/";
		public string GlowServiceUri
		{
			get => glowServiceUri;
			set => glowServiceUri = value;
		}

		public bool IsGlowIndexSearchAllowedForModule(string module)
		{
			return false;
		}

		public int MaximumNumberOfModuleFiltersSearchResults => default;

		public bool IndexSearchUsageCollector => false;
	}
}
