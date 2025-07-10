using System.Reflection;
using Enterprise.Customs.GB.Registry;

[assembly: AssemblyTitle("Enterprise.Customs.GB.ICS.Test")]
[assembly: AssemblyDescription("Testing project for ICS")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.UnitedKingdom)]
[assembly: Enterprise.Registry.Business.Testing.BooleanRegistryItemTest(typeof(GBCustomsDataRegistry), new string[] { nameof(GBCustomsDataRegistry.EnableIcsManifest), nameof(GBCustomsDataRegistry.EnableSSGBManifest) })]
