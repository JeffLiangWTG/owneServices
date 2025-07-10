using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ExceptionTypes")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]

[assembly: UsesConstants(typeof(ProcessWorkflowExceptionTypeSchema))]

// The following GUID is for the ID of the typelib if this project is exposed to COM
#pragma warning disable RS0030
[assembly: Guid("300630d1-ab24-477f-bee5-512bec83226f")]
#pragma warning restore RS0030
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.DbUpgrader.Data.ExceptionTypes.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
