using System.Runtime.CompilerServices;

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.ServiceManager.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Winzor.Architecture.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
