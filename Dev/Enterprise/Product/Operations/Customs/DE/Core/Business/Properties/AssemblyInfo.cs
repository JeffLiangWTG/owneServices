using System.Reflection;
using System.Runtime.CompilerServices;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

[assembly: AssemblyTitle("DE Customs Business")]
[assembly: AssemblyDescription("DE Customs Business")]
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoEUAddInfo.Schema), Enterprise.Core.Constants.CountryCodes.Germany)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.DE.Business.Test, PublicKey = " + CommonAssemblyInfo.PublicKey)]
#endif
