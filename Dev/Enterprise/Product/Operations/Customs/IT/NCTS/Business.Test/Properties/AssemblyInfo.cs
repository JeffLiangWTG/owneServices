using System.Reflection;

[assembly: AssemblyTitle("IT Customs NCTS Business Test")]
[assembly: AssemblyDescription("IT Customs Business NCTS Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Italy)]
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.IT.NCTS.GUI.Test",
	"Enterprise.Customs.IT.NCTS.DataTransfer.Test")]
