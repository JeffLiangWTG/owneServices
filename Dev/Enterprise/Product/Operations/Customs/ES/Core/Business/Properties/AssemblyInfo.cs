using System.Reflection;
using System.Runtime.CompilerServices;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
[assembly: AssemblyTitle("ES Customs Business")]
[assembly: AssemblyDescription("ES Customs Business")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.ES.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: UniversalCopyAddInfoPropertyDefinition(typeof(AutoEUAddInfo.Schema), Enterprise.Core.Constants.CountryCodes.Spain)]
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.ES.Business.Test",
	"Enterprise.Customs.ES.DataTransfer",
	"Enterprise.Customs.ES.DataTransfer.Test",
	"Enterprise.Customs.ES.DocumentWrappers",
	"Enterprise.Customs.ES.DocumentWrappers.Test",
	"Enterprise.Customs.ES.ExitControl.Business",
	"Enterprise.Customs.ES.ExitControl.Business.Test",
	"Enterprise.Customs.ES.ExitControl.GUI",
	"Enterprise.Customs.ES.ExitControl.GUI.Test",
	"Enterprise.Customs.ES.GUI",
	"Enterprise.Customs.ES.GUI.Test",
	"Enterprise.Customs.ES.Manifest.H7.Business",
	"Enterprise.Customs.ES.Manifest.H7.Business.Test",
	"Enterprise.Customs.ES.Manifest.H7.GUI",
	"Enterprise.Customs.ES.Manifest.H7.GUI.Test",
	"Enterprise.Customs.ES.Module",
	"Enterprise.Customs.ES.Module.Test",
	"Enterprise.Customs.ES.NCTS.Business",
	"Enterprise.Customs.ES.NCTS.Business.Test",
	"Enterprise.Customs.ES.NCTS.GUI",
	"Enterprise.Customs.ES.NCTS.GUI.Test",
	"Enterprise.Customs.ES.NCTS.Module",
	"Enterprise.Customs.ES.NCTS.Module.Test",
	"Enterprise.Customs.ES.ServiceTasks",
	"Enterprise.Customs.ES.ServiceTasks.Test",
	"Enterprise.Customs.ES.TemporaryStorage.Business",
	"Enterprise.Customs.ES.TemporaryStorage.Business.Test",
	"Enterprise.Customs.ES.TemporaryStorage.GUI",
	"Enterprise.Customs.ES.TemporaryStorage.GUI.Test",
	"Enterprise.Customs.ES.TemporaryStorage.Module",
	"Enterprise.Customs.ES.TemporaryStorage.Module.Test")]
