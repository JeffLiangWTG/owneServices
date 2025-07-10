using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("CargoWise Process Launcher - Security")]

#if DEBUG
[assembly: InternalsVisibleTo("CargoWise.ServiceManager.Next.Launcher.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
