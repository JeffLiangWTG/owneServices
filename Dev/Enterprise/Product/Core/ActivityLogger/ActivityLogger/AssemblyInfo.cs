using System.Reflection;
using System.Runtime.CompilerServices;
[assembly: AssemblyTitle("ActivityLogger")]
[assembly: AssemblyDescription("ActivityLogger")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.ActivityLogger.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
