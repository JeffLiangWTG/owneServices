using System.Reflection;
using Enterprise.Customs.CO.Manifest.Business;

[assembly: AssemblyTitle("CO.Manifest.Business.Test")]
[assembly: AssemblyDescription("CO.Manifest.Business.Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Colombia)]
[assembly: Enterprise.Registry.Business.Testing.BooleanRegistryItemTest(typeof(COCustomsDataRegistry), nameof(COCustomsDataRegistry.EnableCOManifests))]
