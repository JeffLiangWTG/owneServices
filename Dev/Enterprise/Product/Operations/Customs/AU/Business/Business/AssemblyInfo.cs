using System.Reflection;
using System.Runtime.CompilerServices;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

[assembly: AssemblyTitle("AUCustoms Business")]
[assembly: AssemblyDescription("AUCustoms Business")]
[assembly: AssemblyConfiguration("")]
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoAUAddInfo.Schema), Enterprise.Core.Constants.CountryCodes.Australia)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.AU.Declaration.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
