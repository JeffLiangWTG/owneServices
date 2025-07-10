using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("GB Customs CDS Interface")]
[assembly: AssemblyDescription("GB Customs CDS Interface")]
[assembly: InternalsVisibleTo("Enterprise.Customs.GB.DocumentWrappers, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.GB.CDS.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
