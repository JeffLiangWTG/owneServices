using System.Runtime.CompilerServices;

#pragma warning disable CS0436 // Type conflicts with imported type
[assembly: InternalsVisibleTo("Enterprise.Customs.CA.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Customs.JP.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#pragma warning restore CS0436 // Type conflicts with imported type
