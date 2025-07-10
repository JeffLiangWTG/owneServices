using System.Runtime.CompilerServices;

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Accounting.DataTransfer, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.DataTransfer.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
