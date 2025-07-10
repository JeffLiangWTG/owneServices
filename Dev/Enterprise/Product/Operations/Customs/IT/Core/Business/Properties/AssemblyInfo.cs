using System.Reflection;
using System.Runtime.CompilerServices;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using WTG.StaticAnalysis.Annotation;

[assembly: AssemblyTitle("IT Customs Business")]
[assembly: AssemblyDescription("IT Customs Business")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.IT.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Customs.IT.NCTS.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoEUAddInfo.Schema), Enterprise.Core.Constants.CountryCodes.Italy)]
[assembly: UsesConstants(typeof(Enterprise.Customs.DataTransfer.Universal.Constants))]

[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.IT.Business.XmlSerializers",
	"Enterprise.Customs.IT.GUI",
	"Enterprise.Customs.IT.Module",
	"Enterprise.Customs.IT.DataTransfer",
	"Enterprise.Customs.IT.ServiceTasks",
	"Enterprise.Customs.IT.Business.Test",
	"Enterprise.Customs.IT.GUI.Test",
	"Enterprise.Customs.IT.H7.Business",
	"Enterprise.Customs.IT.H7.Business.Test",
	"Enterprise.Customs.IT.NCTS.GUI",
	"Enterprise.Customs.IT.Module.Test",
	"Enterprise.Customs.IT.DataTransfer.Test",
	"Enterprise.Customs.IT.ServiceTasks.Test",
	"Enterprise.Customs.IT.NCTS.Business",
	"Enterprise.Customs.IT.NCTS.Business.Test",
	"Enterprise.Customs.IT.NCTS.GUI.Test",
	"Enterprise.Customs.IT.NCTS.Module.Test",
	"Enterprise.Customs.IT.TemporaryStorage.Business",
	"Enterprise.Customs.IT.TemporaryStorage.Business.Test",
	"Enterprise.Customs.IT.TemporaryStorage.GUI",
	"Enterprise.Customs.IT.TemporaryStorage.GUI.Test")]
