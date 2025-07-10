using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Enterprise.UniversalCopy.GUI")]
[assembly: AssemblyDescription("Enterprise.UniversalCopy.GUI")]
[assembly: AssemblyCulture("")]
#pragma warning disable CS0436 // Type conflicts with imported type
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.UniversalCopy.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.UniversalCopy.GUI.Winzor.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
#pragma warning restore CS0436 // Type conflicts with imported type

// The following GUID is for the ID of the typelib if this project is exposed to COM
#pragma warning disable RS0030
[assembly: Guid("6A5C2362-3393-427D-A4F9-1637F19D7E61")]
#pragma warning restore RS0030

[assembly: CLSCompliant(false)]
