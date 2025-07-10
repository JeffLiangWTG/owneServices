using System.Reflection;
[assembly: AssemblyTitle("IT Customs GUI Test")]
[assembly: AssemblyDescription("IT Customs GUI Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Italy)]
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.IT.NCTS.GUI.Test",
	"Enterprise.Customs.IT.TemporaryStorage.GUI.Test")]
