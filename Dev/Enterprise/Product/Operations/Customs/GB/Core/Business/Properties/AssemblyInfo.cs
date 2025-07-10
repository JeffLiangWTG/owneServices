using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Definitions;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

[assembly: AssemblyTitle("GB Customs Business")]
[assembly: AssemblyDescription("GB Customs Business")]
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoEUAddInfo.Schema), Enterprise.Core.Constants.CountryCodes.UnitedKingdom)]
[assembly: ApplicationConfiguration("Enterprise.Customs.GB.Business", "Enterprise.Customs.GB.Business.GBEnterpriseApplicationConfiguration.xml")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.GB.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
