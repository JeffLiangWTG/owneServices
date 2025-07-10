using System.Reflection;
using System.Runtime.CompilerServices;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

[assembly: AssemblyTitle("GB Customs EMCS Business")]
[assembly: AssemblyDescription("GB Customs EMCS Business")]
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoEUAddInfo.Schema), Enterprise.Core.Constants.CountryCodes.UnitedKingdom)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.GB.EMCS.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
