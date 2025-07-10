using System.Reflection;
using Enterprise.Customs.JP.Common;

[assembly: AssemblyTitle("JP.Manifest.GUI.Test")]
[assembly: AssemblyDescription("JP.Manifest.GUI.Test")]

[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Japan)]
[assembly: Enterprise.Registry.Business.Testing.BooleanRegistryItemTest(typeof(JPRegistry), [nameof(JPRegistry.EnableHCHForwarderManifest), nameof(JPRegistry.EnableHDFForwarderManifest), nameof(JPRegistry.EnableNVCForwarderManifest), nameof(JPRegistry.EnableVANForwarderManifest)])]
