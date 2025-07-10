using System.Reflection;
using Enterprise.Customs.IN.Registry;

[assembly: AssemblyTitle("IN Customs Manifest Business Test")]
[assembly: AssemblyDescription("IN Customs Manifest Business Test")]

[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.India)]
[assembly: Enterprise.Registry.Business.Testing.BooleanRegistryItemTest(typeof(INCustomsDataRegistry), new[] { nameof(INCustomsDataRegistry.INEnableConsolGeneralManifest), nameof(INCustomsDataRegistry.INEnableImportGeneralManifest) })]
