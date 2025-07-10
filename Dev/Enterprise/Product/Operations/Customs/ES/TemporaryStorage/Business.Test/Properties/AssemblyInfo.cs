using System.Reflection;

[assembly: AssemblyTitle("ES Customs TemporaryStorage Business Test")]
[assembly: AssemblyDescription("ES Customs TemporaryStorage Business Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Spain)]
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.ES.TemporaryStorage.Module.Test")]
