using System.Reflection;
using Enterprise.Customs.MX.Manifest.Business;
using WTG.StaticAnalysis.Annotation;

[assembly: AssemblyTitle("MX.Manifest.Business.Test")]
[assembly: AssemblyDescription("MX.Manifest.Business.Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Mexico)]
[assembly: Enterprise.Registry.Business.Testing.BooleanRegistryItemTest(typeof(MXCustomsDataRegistry), nameof(MXCustomsDataRegistry.EnableMXManifests))]
[assembly: UsesConstants(typeof(Enterprise.Customs.MX.Manifest.Business.Testing.MXMessagingConstants))]

