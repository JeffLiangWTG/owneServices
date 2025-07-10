using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Ajax")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]

#pragma warning disable RS0030
[assembly: Guid("6e5a2e1f-05ab-47eb-bef9-b52808aca19e")]
#pragma warning restore RS0030
#if DEBUG
#pragma warning disable CS0436 // Type conflicts with imported type
[assembly: InternalsVisibleTo("Enterprise.ZArchitecture.Web.GUI.Ajax.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#pragma warning restore CS0436 // Type conflicts with imported type
#endif
