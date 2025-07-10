using System.Reflection;
using Enterprise.Customs.MX.Manifest.Business;

[assembly: AssemblyTitle("MX.Manifest.GUI.Test")]
[assembly: AssemblyDescription("MX.Manifest.GUI.Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Mexico)]
[assembly: Enterprise.Registry.Business.Testing.BooleanRegistryItemTest(typeof(MXCustomsDataRegistry), nameof(MXCustomsDataRegistry.EnableMXManifests))]
