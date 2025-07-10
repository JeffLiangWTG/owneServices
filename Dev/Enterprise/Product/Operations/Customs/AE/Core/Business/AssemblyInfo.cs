using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Definitions;

[assembly: AssemblyTitle("AE Customs Business")]
[assembly: AssemblyDescription("AE Customs Business")]
[assembly: ApplicationConfiguration("Enterprise.Customs.AE.Business", "Enterprise.Customs.AE.Business.AEEnterpriseApplicationConfiguration.xml")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.AE.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
