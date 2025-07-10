using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("GraphEngine")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
#pragma warning disable RS0030
[assembly: Guid("6e422e79-47f2-464e-bd07-5b60233efbd2")]
#pragma warning restore RS0030
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Scheduler.GraphEngine.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
