using System.Reflection;
using WTG.StaticAnalysis.Annotation;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTitle("Build Tools")]
[assembly: AssemblyDescription("Build Tools")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]
#if DEBUG
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BuildTools.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: UsesConstants(typeof(WTG.DevTools.Definitions.ReleaseRings))]
