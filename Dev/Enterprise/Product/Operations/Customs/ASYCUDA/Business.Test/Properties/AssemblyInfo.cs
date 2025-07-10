using System.Reflection;
[assembly: AssemblyTitle("ASYCUDA.Business.Test")]
[assembly: AssemblyDescription("ASYCUDA.Business.Test")]

[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Eritrea)]
[assembly: Enterprise.Registry.Business.Testing.BooleanRegistryItemTest(typeof(Enterprise.Integration.Customs.SG.ISGCustomsRegistry), nameof(Enterprise.Integration.Customs.SG.ISGCustomsRegistry.ACCESSEnable))]
