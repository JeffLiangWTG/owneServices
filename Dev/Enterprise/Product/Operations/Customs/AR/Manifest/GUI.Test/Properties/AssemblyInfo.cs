using System.Reflection;
using Enterprise.Customs.AR.Manifest.Business;

[assembly: AssemblyTitle("AR.Manifest.GUI.Test")]
[assembly: AssemblyDescription("AR.Manifest.GUI.Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Argentina)]
[assembly: Enterprise.Registry.Business.Testing.BooleanRegistryItemTest(typeof(ARCustomsDataRegistry), nameof(ARCustomsDataRegistry.EnableARManifests))]
