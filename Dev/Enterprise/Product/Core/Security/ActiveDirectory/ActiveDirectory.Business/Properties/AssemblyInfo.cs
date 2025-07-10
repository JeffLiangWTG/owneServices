using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Enterprise.Security.ActiveDirectory")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

// The following GUID is for the ID of the typelib if this project is exposed to COM
#pragma warning disable RS0030
[assembly: Guid("ef21a600-da5c-45d7-9e83-dc00bb6ea082")]
#pragma warning restore RS0030
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Security.ActiveDirectory.Testing, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
