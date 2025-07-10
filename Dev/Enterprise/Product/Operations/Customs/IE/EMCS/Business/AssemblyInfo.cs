using System.Reflection;
using System.Runtime.CompilerServices;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

[assembly: AssemblyTitle("IE Customs EMCS Business")]
[assembly: AssemblyDescription("IE Customs EMCS Business")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.IE.EMCS.Business.Test, PublicKey = " + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoEUAddInfo.Schema), Enterprise.Core.Constants.CountryCodes.Ireland)]
