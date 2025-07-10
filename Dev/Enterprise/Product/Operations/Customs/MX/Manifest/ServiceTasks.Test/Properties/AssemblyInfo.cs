using System.Reflection;
using Enterprise.Customs.MX.Manifest.Business.Testing;
using WTG.StaticAnalysis.Annotation;

[assembly: AssemblyTitle("MX.Manifest.ServiceTasks.Test")]
[assembly: AssemblyDescription("MX.Manifest.ServiceTasks.Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Mexico)]
[assembly: UsesConstants(typeof(MXMessagingConstants))]
