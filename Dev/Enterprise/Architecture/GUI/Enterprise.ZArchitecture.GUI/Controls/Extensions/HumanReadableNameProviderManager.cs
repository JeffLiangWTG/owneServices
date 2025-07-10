using System.Collections.Generic;

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI
{
	public class HumanReadableNameProviderManager : IHumanReadableNameProvider
	{
#if DEBUG

		internal static int CountOfRegisteredProviders { get; set; }
		internal static int CountOfUnRegisteredProviders { get; set; }

		internal static HumanReadableNameProviderManager HumanReadableNameProviderManagerForTest()
		{
			return new HumanReadableNameProviderManager();
		}

		internal List<IHumanReadableNameProvider> ProvidersExposedForTests
		{
			get { return providers; }
		}

#endif
		readonly List<IHumanReadableNameProvider> providers;

		HumanReadableNameProviderManager()
		{
			providers = new List<IHumanReadableNameProvider>();
		}

		public ZString GetHumanReadableName(ZPropertyInfo propertyInfo)
		{
			foreach (var provider in providers.ToArray())
			{
				var result = provider.GetHumanReadableName(propertyInfo);
				if (!result.IsEmpty && result.IsValid)
				{
					providers.Remove(provider);
					providers.Insert(0, provider);
					return result;
				}
			}
			return ZString.Empty;
		}

		public static void UnregisterCaptionProvider(IHumanReadableNameProvider humanReadableNameProvider, BusinessObjectFactory factory)
		{
			lock (factory)
			{
				var service = factory.ServiceContainer.GetService<IHumanReadableNameProvider>();
				var manager = service as HumanReadableNameProviderManager;
				if (manager != null)
				{
					manager.providers.Remove(humanReadableNameProvider);
#if DEBUG
					CountOfUnRegisteredProviders++;
#endif
					if (manager.providers.Count == 0)
					{
						factory.ServiceContainer.RemoveService<IHumanReadableNameProvider>();
					}
				}
			}
		}

		public static void RegisterCaptionProvider(IHumanReadableNameProvider humanReadableNameProvider, BusinessObjectFactory factory)
		{
			lock (factory)
			{
				var service = factory.ServiceContainer.GetService<IHumanReadableNameProvider>();
				if (service == null)
				{
					service = new HumanReadableNameProviderManager();
					factory.ServiceContainer.AddService(service);
				}
				var manager = service as HumanReadableNameProviderManager;
				if (manager != null)
				{
					manager.providers.Add(humanReadableNameProvider);
#if DEBUG
					CountOfRegisteredProviders++;
#endif
				}
			}
		}
	}
}
