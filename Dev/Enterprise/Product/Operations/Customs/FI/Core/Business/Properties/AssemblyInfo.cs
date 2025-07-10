using System.Reflection;
using System.Runtime.CompilerServices;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

[assembly: AssemblyTitle("FI Customs Business")]
[assembly: AssemblyDescription("FI Customs Business Project")]
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoEUAddInfo.Schema), Enterprise.Core.Constants.CountryCodes.Finland)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.FI.Business.Test, PublicKey = " + CommonAssemblyInfo.PublicKey)]
#endif
