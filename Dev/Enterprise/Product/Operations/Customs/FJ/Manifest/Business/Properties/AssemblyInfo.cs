using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Definitions;

[assembly: AssemblyTitle("Enterprise.Customs.FJ.Manifest.Business")]
[assembly: AssemblyDescription("Enterprise.Customs.FJ.Manifest.Business")]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.FJ.Manifest.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif

[assembly: ApplicationConfiguration("Enterprise.Customs.FJ.Manifest.Business", "Enterprise.Customs.FJ.Manifest.Business.FJEnterpriseApplicationConfiguration.xml")]
