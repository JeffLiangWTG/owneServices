using System;
using CargoWise.DbUpgrader.Scripts.Abstractions;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Testing
{
	class TriggerForTesting : DbCreateTriggerScript
	{
		public override string Name => "TriggerForTesting";

		protected override string InClassScriptText => FormattableString.Invariant($"CREATE TRIGGER {Name} ON [TableForTrigger] AFTER DELETE AS ROLLBACK");
	}
}
