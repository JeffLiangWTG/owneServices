using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Definitions;

[assembly: AssemblyTitle("JP Customs Common")]
[assembly: AssemblyDescription("JP Customs Common")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.JP.Common.Test, PublicKey = " + CommonAssemblyInfo.PublicKey)]
#endif

[assembly: ApplicationConfiguration("Enterprise.Customs.JP.Common", "Enterprise.Customs.JP.Common.JPEnterpriseApplicationConfiguration.xml")]
