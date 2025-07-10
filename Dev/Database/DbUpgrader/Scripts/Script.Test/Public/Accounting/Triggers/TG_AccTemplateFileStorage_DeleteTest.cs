using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Testing
{
	[TestedType(typeof(TG_AccTemplateFileStorage_Delete))]
	class TG_AccTemplateFileStorage_DeleteTest : DBCreateTriggerScriptTest
	{
	}
}
