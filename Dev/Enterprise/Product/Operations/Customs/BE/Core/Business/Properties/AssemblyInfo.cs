using System.Reflection;
using System.Runtime.CompilerServices;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

[assembly: AssemblyTitle("BE Customs Business")]
[assembly: AssemblyDescription("BE Customs Business")]
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoEUAddInfo.Schema), Enterprise.Core.Constants.CountryCodes.Belgium)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.BE.Business.Test, PublicKey = " + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Customs.BE.NCTS.Business.Test, PublicKey = " + CommonAssemblyInfo.PublicKey)]
#endif
