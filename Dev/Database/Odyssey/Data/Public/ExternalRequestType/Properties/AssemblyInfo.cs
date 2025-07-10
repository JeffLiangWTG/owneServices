using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ExternalRequestType")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]

[assembly: UsesConstants(typeof(ExternalRequestTypeSchema))]

// The following GUID is for the ID of the typelib if this project is exposed to COM
#pragma warning disable RS0030
[assembly: Guid("e88eaeb7-ec9f-4d36-bbd7-8dc9016f0188")]
#pragma warning restore RS0030
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.DbUpgrader.Data.ExternalRequestType.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
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
