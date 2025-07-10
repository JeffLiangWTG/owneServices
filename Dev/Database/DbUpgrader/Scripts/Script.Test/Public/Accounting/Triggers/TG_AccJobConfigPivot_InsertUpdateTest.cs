using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Testing
{
	[TestedType(typeof(TG_AccJobConfigPivot_InsertUpdate))]
	class TG_AccJobConfigPivot_InsertUpdateTest : DBCreateTriggerScriptTest
	{
	}
}
