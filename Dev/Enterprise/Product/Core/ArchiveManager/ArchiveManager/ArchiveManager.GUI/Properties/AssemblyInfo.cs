using System.Reflection;
using System.Runtime.CompilerServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Enterprise.ArchiveManager.GUI")]
[assembly: AssemblyDescription("Archive Manager GUI")]
[assembly: Enterprise.ZArchitecture.Business.ActionFieldFollow(false)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.ArchiveManager.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
