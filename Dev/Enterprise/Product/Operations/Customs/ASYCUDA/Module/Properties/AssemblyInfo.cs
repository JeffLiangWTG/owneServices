using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("ASYCUDA Module")]
[assembly: AssemblyDescription("ASYCUDA Module")]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.ASYCUDA.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
