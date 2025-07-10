using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("ediEnterprise Accounting XML Import/Export")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
#pragma warning disable CS0436 // Type conflicts with imported type
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Accounting.DataTransfer.Testing, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
#pragma warning restore CS0436 // Type conflicts with imported type
