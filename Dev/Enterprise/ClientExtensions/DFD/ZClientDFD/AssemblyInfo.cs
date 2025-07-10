using System.Reflection;
using System.Runtime.CompilerServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ZClientEDI.Business")]
#if DEBUG
[assembly: InternalsVisibleTo("ZClientDFD.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: InternalsVisibleTo("ZClientDFD.ServiceTasks, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("ZClientDFD.ServiceTasks.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
