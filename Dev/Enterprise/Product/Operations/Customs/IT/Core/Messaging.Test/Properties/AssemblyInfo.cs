using System.Reflection;
[assembly: AssemblyTitle("IT Customs Messaging Test")]
[assembly: AssemblyDescription("IT Customs Messaging Test")]
[assembly: Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Italy)]
[assembly: CargoWise.Common.PreventAssemblyReferences()]

namespace Enterprise.Customs.IT.Messaging.Testing;

internal static class TestConstants
{
	public const string ProjectRelativePath = @"Enterprise\Product\Operations\Customs\IT\Core\Messaging.Test\";
}
