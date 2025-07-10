using System.Reflection;

[assembly: AssemblyTitle("ES Customs NCTS Business Test")]
[assembly: AssemblyDescription("ES Customs NCTS Business Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Spain)]
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.ES.NCTS.GUI.Test")]
