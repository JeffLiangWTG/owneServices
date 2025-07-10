using System.Reflection;
using System.Runtime.CompilerServices;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

[assembly: AssemblyTitle("CA Customs Business")]
[assembly: AssemblyDescription("CA Customs Business")]
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoCAAddInfo.Schema), Enterprise.Core.Constants.CountryCodes.Canada)]

#if DEBUG

[assembly: InternalsVisibleTo("Enterprise.Customs.CA.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]

#endif
