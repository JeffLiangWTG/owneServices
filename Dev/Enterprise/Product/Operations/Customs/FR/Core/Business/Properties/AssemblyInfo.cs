using System.Reflection;
using System.Runtime.CompilerServices;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

[assembly: AssemblyTitle("FR Customs Business")]
[assembly: AssemblyDescription("FR Customs Business")]
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoEUAddInfo.Schema), Enterprise.Core.Constants.CountryCodes.France)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.FR.Business.Test, PublicKey = " + CommonAssemblyInfo.PublicKey)]
#endif
