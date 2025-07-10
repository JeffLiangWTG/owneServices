using System.Reflection;
using Enterprise.Customs.AE.Registry;

[assembly: AssemblyTitle("Enterprise.Customs.AE.Manifest.Business.Test")]
[assembly: AssemblyDescription("Enterprise.Customs.AE.Manifest.Business.Test")]

[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.UnitedArabEmirates)]
[assembly: Enterprise.Registry.Business.Testing.BooleanRegistryItemTest(typeof(AECustomsRegistry), nameof(AECustomsRegistry.EnableUAESeaExportManifest))]
