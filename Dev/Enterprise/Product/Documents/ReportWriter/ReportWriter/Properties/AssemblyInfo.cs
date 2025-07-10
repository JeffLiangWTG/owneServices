using System.Reflection;
using System.Runtime.CompilerServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Report Writer")]
[assembly: AssemblyDescription("Report Writer")]
[assembly: AssemblyConfiguration("")]
#if DEBUG
[assembly: InternalsVisibleTo("ReportWriter.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
