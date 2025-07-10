using System.Reflection;
using Enterprise.Customs.CL.Manifest.Business;

[assembly: AssemblyTitle("CL.Manifest.GUI.Test")]
[assembly: AssemblyDescription("CL.Manifest.GUI.Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Chile)]
[assembly: Enterprise.Registry.Business.Testing.BooleanRegistryItemTest(typeof(CLCustomsDataRegistry), nameof(CLCustomsDataRegistry.EnableGlobalManifestForChile))]
