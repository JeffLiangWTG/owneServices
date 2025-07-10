using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Audit Subscription")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.AuditDataServices.Subscription.Testing, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
