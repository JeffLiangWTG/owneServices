using System.Reflection;
using Enterprise.Customs.CL.Manifest.Business.Testing;
using WTG.StaticAnalysis.Annotation;

[assembly: AssemblyTitle("CL.Manifest.ServiceTasks.Test")]
[assembly: AssemblyDescription("CL.Manifest.ServiceTasks.Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Chile)]
[assembly: UsesConstants(typeof(CLMessagingConstants))]
