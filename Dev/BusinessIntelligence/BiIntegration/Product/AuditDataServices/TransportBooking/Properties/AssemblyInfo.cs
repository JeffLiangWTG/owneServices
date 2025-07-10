using System.Runtime.CompilerServices;

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.AuditDataServices.Subscription.Testing, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.AuditDataServices.TransportBooking.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
