using System.Reflection;
using Enterprise.Customs.JP.Common;

[assembly: AssemblyTitle("JP.Manifest.Business.Test")]
[assembly: AssemblyDescription("JP.Manifest.Business.Test")]

[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Japan)]
[assembly: Enterprise.Registry.Business.Testing.BooleanRegistryItemTest(typeof(JPRegistry), [nameof(JPRegistry.EnableHCHForwarderManifest), nameof(JPRegistry.EnableHDFForwarderManifest), nameof(JPRegistry.EnableNVCForwarderManifest), nameof(JPRegistry.EnableVANForwarderManifest)])]
