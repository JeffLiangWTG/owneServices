using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Definitions;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using WTG.StaticAnalysis.Annotation;

[assembly: AssemblyTitle("IE Customs Business")]
[assembly: AssemblyDescription("IE Customs Business Project")]
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoEUAddInfo.Schema), Enterprise.Core.Constants.CountryCodes.Ireland)]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.IE.Business.Test, PublicKey = " + CommonAssemblyInfo.PublicKey)]
[assembly: UsesConstants(typeof(CL047_ControlResult))]
#endif

[assembly: ApplicationConfiguration("Enterprise.Customs.IE.Business", "Enterprise.Customs.IE.Business.IEEnterpriseApplicationConfiguration.xml")]
