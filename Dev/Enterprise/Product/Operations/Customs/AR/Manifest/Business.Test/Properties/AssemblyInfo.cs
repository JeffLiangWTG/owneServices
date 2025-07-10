using System.Reflection;
using Enterprise.Customs.AR.Manifest.Business;

[assembly: AssemblyTitle("AR.Manifest.Business.Test")]
[assembly: AssemblyDescription("AR.Manifest.Business.Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Argentina)]
[assembly: Enterprise.Registry.Business.Testing.BooleanRegistryItemTest(typeof(ARCustomsDataRegistry), nameof(ARCustomsDataRegistry.EnableARManifests))]

