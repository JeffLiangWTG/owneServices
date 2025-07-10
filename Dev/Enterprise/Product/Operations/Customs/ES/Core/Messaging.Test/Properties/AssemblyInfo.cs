using System.Reflection;

[assembly: AssemblyTitle("ES Customs Messaging Test")]
[assembly: AssemblyDescription("ES Customs Messaging Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Spain)]
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.ES.Business.Test",
	"Enterprise.Customs.ES.ExitControl.Business.Test",
	"Enterprise.Customs.ES.GUI.Test",
	"Enterprise.Customs.ES.Manifest.H7.Business.Test",
	"Enterprise.Customs.ES.Module.Test",
	"Enterprise.Customs.ES.NCTS.Business.Test",
	"Enterprise.Customs.ES.NCTS.Messaging.Test",
	"Enterprise.Customs.ES.ServiceTasks.Test",
	"Enterprise.Customs.ES.TemporaryStorage.Business.Test")]

namespace Enterprise.Customs.ES.Messaging.Testing
{
	internal static class Constants
	{
		public const string ProjectRelativePath = @"Enterprise\Product\Operations\Customs\ES\Core\Messaging.Test\";
	}
}
