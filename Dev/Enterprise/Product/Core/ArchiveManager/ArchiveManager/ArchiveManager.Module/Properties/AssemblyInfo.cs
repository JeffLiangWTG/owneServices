using System.Reflection;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Enterprise.ArchiveManager.Module")]
[assembly: AssemblyDescription("Archive Manager Module")]
[assembly: Enterprise.ZArchitecture.Business.ActionFieldFollow(false)]
#if DEBUG
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Enterprise.ArchiveManager.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
