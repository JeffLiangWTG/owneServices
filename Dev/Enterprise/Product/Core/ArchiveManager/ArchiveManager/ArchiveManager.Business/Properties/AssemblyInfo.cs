using System.Reflection;
using System.Runtime.CompilerServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Enterprise.ArchiveManager.Business")]
[assembly: AssemblyDescription("Archive Manager Business")]
[assembly: AssemblyConfiguration("")]
[assembly: Enterprise.ZArchitecture.Business.ActionFieldFollow(false)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.ArchiveManager.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
