using System.Reflection;
using System.Runtime.CompilerServices;
using Enterprise.BufferManagement.Module;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using WTG.StaticAnalysis.Annotation;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Enterprise.BufferManagement.Service")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]
[assembly: UsesConstants(typeof(CCPMConstants))]
[assembly: UsesConstants(typeof(QualityIterationFilterTypeList))]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.BufferManagement.Service.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
