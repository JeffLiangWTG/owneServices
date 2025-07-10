using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CargoWise.Main, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Billing.StlCollector.Retriever.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("CargoWise.Main.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
