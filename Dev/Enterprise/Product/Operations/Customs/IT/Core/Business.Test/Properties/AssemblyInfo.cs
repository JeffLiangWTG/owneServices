using System.Reflection;
[assembly: AssemblyTitle("IT Customs Business Test")]
[assembly: AssemblyDescription("IT Customs Business Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Italy)]
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.IT.ServiceTasks.Test",
	"Enterprise.Customs.IT.GUI.Test",
	"Enterprise.Customs.IT.NCTS.Business.Test",
	"Enterprise.Customs.IT.NCTS.GUI.Test",
	"Enterprise.Customs.IT.TemporaryStorage.Business.Test",
	"Enterprise.Customs.IT.TemporaryStorage.GUI.Test")]

namespace Enterprise.Customs.IT.Business.Testing;

internal static class TestConstants
{
	public const string ProjectRelativePath = @"Enterprise\Product\Operations\Customs\IT\Core\Business.Test\";
}

