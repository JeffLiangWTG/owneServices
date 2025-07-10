using System.Reflection;
using Enterprise.Customs.CL.Manifest.Business;
using Enterprise.Customs.CL.Manifest.Business.Testing;
using WTG.StaticAnalysis.Annotation;

[assembly: AssemblyTitle("CL.Manifest.Business.Test")]
[assembly: AssemblyDescription("CL.Manifest.Business.Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Chile)]
[assembly: Enterprise.Registry.Business.Testing.BooleanRegistryItemTest(typeof(CLCustomsDataRegistry), nameof(CLCustomsDataRegistry.EnableGlobalManifestForChile))]
[assembly: UsesConstants(typeof(CLMessagingConstants))]
