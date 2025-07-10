using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Registry.Testing
{
	[TestedType(typeof(McpIslCredentialsSettingControl))]
	class McpIslCredentialsSettingControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
			=> new McpIslCredentialsSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
			=> ((McpIslCredentialsSettingControl)control).McpIslCredentialsGrid.ReadOnly;
	}
}
