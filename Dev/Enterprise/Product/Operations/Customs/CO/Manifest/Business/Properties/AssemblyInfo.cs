using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("CO.Manifest.Business")]
[assembly: AssemblyDescription("CO.Manifest.Business")]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.CO.Manifest.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
