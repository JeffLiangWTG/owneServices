using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("IT Customs Module")]
[assembly: AssemblyDescription("IT Customs Module")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.IT.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: CargoWise.Common.PreventAssemblyReferences("Enterprise.Customs.IT.Module.Test")]
