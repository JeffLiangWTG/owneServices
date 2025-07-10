using System.Runtime.CompilerServices;

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Accounting.TaxFramework.Business.Testing, PublicKey = " + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Accounting.Utility.Testing, PublicKey = " + CommonAssemblyInfo.PublicKey)]
#endif
