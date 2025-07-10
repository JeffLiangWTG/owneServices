using System.Reflection;
using System.Runtime.CompilerServices;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: AssemblyTitle("Mail Manager Module")]
[assembly: AssemblyDescription("Mail Manager")]
[assembly: AssemblyConfiguration("")]
#if DEBUG
[assembly: InternalsVisibleTo("MailManager.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
