using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Database.Abstractions;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Core
{
	public static class EnterpriseApplicationConfiguration
	{
		const string MandatoryStateIntializers = nameof(MandatoryStateIntializers);

		internal static IEnumerable<(bool IsResourceUri, string ResourceUriOrResourceName, string AssemblyFileName)> ConfigurationLocations
		{
			get
			{
				yield return (true, "assembly://Enterprise.ZArchitecture.Core/Enterprise.ZArchitecture.Core.Configuration/EnterpriseApplicationConfiguration.xml", string.Empty);
				yield return (true, "assembly://Enterprise.ZArchitecture.Core/Enterprise.ZArchitecture.Core.Configuration/BusinessObjectPrefixTypesConfiguration.xml", string.Empty);
				yield return (true, "assembly://Enterprise.ZArchitecture.Core/Enterprise.ZArchitecture.Core.Configuration/DebugOnlyConfiguration.xml", string.Empty);
				yield return (true, "assembly://Enterprise.ZArchitecture.Core/Enterprise.ZArchitecture.Core.Configuration/UniversalDataContextManagersConfiguration.xml", string.Empty);
				yield return (true, "assembly://Enterprise.ZArchitecture.Core/Enterprise.ZArchitecture.Core.Configuration/WorkflowDescriptorsConfiguration.xml", string.Empty);
				yield return (true, "assembly://Enterprise.ZArchitecture.Core/Enterprise.ZArchitecture.Core.Configuration/EDICommunicationPartyApplicationDescriptorsConfiguration.xml", string.Empty);
				yield return (true, "assembly://Enterprise.ZArchitecture.Core/Enterprise.ZArchitecture.Core.Configuration/ExternalRequestSupportedAddressTypesProvidersConfiguration.xml", string.Empty);
				yield return (true, "assembly://Enterprise.ZArchitecture.Core/Enterprise.ZArchitecture.Core.Configuration/ProcessTaskTypesConfiguration.xml", string.Empty);
				yield return (true, "assembly://Enterprise.ZArchitecture.Core/Enterprise.ZArchitecture.Core.Configuration/StlDynamicCollectorsListConfiguration.xml", string.Empty);
				yield return (true, "assembly://Enterprise.ZArchitecture.Core/Enterprise.ZArchitecture.Core.Configuration/StlCustomCollectorsListConfiguration.xml", string.Empty);
				yield return (true, "assembly://Enterprise.ZArchitecture.Core/Enterprise.ZArchitecture.Core.Configuration/RegistryItemSetsConfiguration.xml", string.Empty);
				yield return (true, "assembly://Enterprise.ZArchitecture.Core/Enterprise.ZArchitecture.Core.Configuration/ProductWarehouseConfiguration.xml", string.Empty);
				yield return (true, "assembly://Enterprise.ZArchitecture.Core/Enterprise.ZArchitecture.Core.Configuration/ProductionRulesConfiguration.xml", string.Empty);
				yield return (true, "assembly://Enterprise.ZArchitecture.Core/Enterprise.ZArchitecture.Core.Configuration/PackingConfiguration.xml", string.Empty);
				yield return (true, "assembly://Enterprise.ZArchitecture.Core/Enterprise.ZArchitecture.Core.Configuration/TransitWarehouseConfiguration.xml", string.Empty);
				yield return (true, "assembly://Enterprise.ZArchitecture.Core/Enterprise.ZArchitecture.Core.Configuration/AccountingApplicationConfiguration.xml", string.Empty);
				yield return (true, "assembly://Enterprise.ZArchitecture.Core/Enterprise.ZArchitecture.Core.Configuration/WorkflowConfiguration.xml", string.Empty);
				yield return (true, "assembly://Enterprise.ZArchitecture.Core/Enterprise.ZArchitecture.Core.Configuration/LandTransportConfiguration.xml", string.Empty);
				yield return (true, "assembly://Enterprise.ZArchitecture.Core/Enterprise.ZArchitecture.Core.Configuration/HotKeyMonitorConfiguration.xml", string.Empty);

				foreach (var configurationMetaData in GetConfigurationFromAssemblyMetaData())
				{
					yield return (false, configurationMetaData.ConfigurationResourceName, configurationMetaData.AssemblyFilePath);
				}
			}
		}

		static IEnumerable<(string AssemblyFilePath, string ConfigurationResourceName)> GetConfigurationFromAssemblyMetaData()
		{
			var binPath = AssemblyLoader.GetBinPath();
			foreach (var attribute in AssemblyMetaDataReader.GetAttributes<ApplicationConfigurationAttribute>(true))
			{
				var assemblyName = attribute.AssemblyName;
				if (!assemblyName.EndsWith(".dll", System.StringComparison.InvariantCultureIgnoreCase))
				{
					assemblyName += ".dll";
				}
				yield return (Path.Combine(binPath, assemblyName), attribute.ResourceName);
			}
		}

		public static void ConfigureObjectFactory()
		{
			foreach (var configurationLocation in ConfigurationLocations)
			{
				if (configurationLocation.IsResourceUri)
				{
					ObjectFactory.Configure(configurationLocation.ResourceUriOrResourceName);
				}
				else
				{
					ObjectFactory.ConfigureUsingFile(configurationLocation.AssemblyFileName, configurationLocation.ResourceUriOrResourceName);
				}
			}

			if (Globals.IsWinzor)
			{
				ObjectFactory.Configure("assembly://WinzorFramework.RemoteClientServices/WinzorFramework.RemoteClientServices.Configuration/RemoteClientServicesConfiguration.xml");
			}

			foreach (IMandatoryStateInitializer initializer in ObjectFactory.Get<IEnumerable>(MandatoryStateIntializers))
			{
				initializer.Initialize();
			}

			GlobalServiceProvider.Configure(new EnterpriseApplicationServiceProvider());
		}

#if DEBUG
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method called via Reflection from Enterprise.ReflectionTestDeadCodeTest.TestNoDeadCode()")]
		[TypeFactoryAnnotationMethod]
		static IEnumerable<string> TypeFactoryAnnotation()
		{
			return ConfigurationLocations.SelectMany(x => x.IsResourceUri ? ObjectFactory.TypeFactoryAnnotation(x.ResourceUriOrResourceName) : ObjectFactory.TypeFactoryAnnotationUsingFile(x.AssemblyFileName, x.ResourceUriOrResourceName));
		}
#endif
	}
}
