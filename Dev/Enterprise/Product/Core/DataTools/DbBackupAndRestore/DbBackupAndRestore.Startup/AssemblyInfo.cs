using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("ediEnterprise Database Backup and Restore")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.DbBackupAndRestore.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.DbBackupAndRestore.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
