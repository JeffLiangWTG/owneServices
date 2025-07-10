using System.Reflection;
[assembly: AssemblyTitle("ES Customs Business Test")]
[assembly: AssemblyDescription("ES Customs Business Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Spain)]
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.ES.ExitControl.Business.Test",
	"Enterprise.Customs.ES.ExitControl.GUI.Test",
	"Enterprise.Customs.ES.GUI.Test",
	"Enterprise.Customs.ES.Manifest.H7.Business.Test",
	"Enterprise.Customs.ES.Module.Test",
	"Enterprise.Customs.ES.NCTS.Business.Test",
	"Enterprise.Customs.ES.NCTS.GUI.Test",
	"Enterprise.Customs.ES.ServiceTasks.Test",
	"Enterprise.Customs.ES.TemporaryStorage.Business.Test",
	"Enterprise.Customs.ES.TemporaryStorage.GUI.Test")]
