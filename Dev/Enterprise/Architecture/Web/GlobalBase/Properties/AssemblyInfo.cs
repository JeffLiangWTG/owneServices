using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: SuppressMessage("CargoWiseOne", "CW1018:HttpApplicationRule", Justification = "Proposed new base ZEnterpriseGlobal to replace EnterpriseHttpApplication.")]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.ZArchitecture.Web.GlobalBase.Tests, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.ZArchitecture.Web.GUI, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
