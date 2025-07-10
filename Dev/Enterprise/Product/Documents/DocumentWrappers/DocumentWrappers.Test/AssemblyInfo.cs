using System.Runtime.CompilerServices;
using Enterprise.MasterFiles.Business.Testing;

[assembly: CountrySpecificTest("ER")]
#pragma warning disable CS0436 // Type conflicts with imported type
[assembly: InternalsVisibleTo("Enterprise.Customs.GB.DocumentWrappers.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#pragma warning restore CS0436 // Type conflicts with imported type
