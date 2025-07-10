using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("GB Customs CCSUK Interface")]
[assembly: AssemblyDescription("GB Customs CCSUK Interface")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.GB.Ccsuk.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Customs.GB.DataTransfer.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: InternalsVisibleTo("Enterprise.Customs.GB.Ccsuk.ServiceTasks, PublicKey=" + CommonAssemblyInfo.PublicKey)]

