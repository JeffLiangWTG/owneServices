using CargoWise.DbUpgrader.Scripts.Abstractions;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Testing
{
	class TransformForTesting : CreateTriggerTransformation
	{
		protected override IDbScript GetTrigger() => new TriggerForTesting();
	}
}
