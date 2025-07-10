using System.Reflection;
using Enterprise.Customs.BR.Registry;

[assembly: AssemblyTitle("BR.Manifest.Business.Test")]
[assembly: AssemblyDescription("BR.Manifest.Business.Test")]

#if DEBUG
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Brazil)]
[assembly: Enterprise.Registry.Business.Testing.BooleanRegistryItemTest(typeof(BRCustomsDataRegistry), nameof(BRCustomsDataRegistry.EnableUCRManifest))]
#endif
