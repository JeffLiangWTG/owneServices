using System.Reflection;
using System.Runtime.CompilerServices;
using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

[assembly: AssemblyTitle("CN Customs Business")]
[assembly: AssemblyDescription("CN Customs Business")]
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoCNAddInfo.Schema), Enterprise.Core.Constants.CountryCodes.China)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.CN.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
