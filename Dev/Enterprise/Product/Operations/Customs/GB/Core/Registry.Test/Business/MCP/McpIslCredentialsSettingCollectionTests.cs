using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Registry.Testing
{
	[TestedType(typeof(McpIslCredentialsSettingCollection))]
	public class McpIslCredentialsSettingCollectionTests : RegistryBusinessObjectCollectionTemplateTestCase<McpIslCredentialsSettingCollection>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override McpIslCredentialsSettingCollection GetCollectionToTest()
			=> new McpIslCredentialsSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
			=> new McpIslCredentialsSetting();
	}
}
