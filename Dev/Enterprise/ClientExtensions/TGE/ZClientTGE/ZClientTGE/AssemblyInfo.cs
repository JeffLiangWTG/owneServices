using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Enterprise.Client.TGE")]
[assembly: AssemblyDescription("Enterprise.Client.TGE")]

#if DEBUG
[assembly: InternalsVisibleTo("ZClientTGE.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("ZClientTGE.ServiceTask, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("ZClientTGE.ServiceTask.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
