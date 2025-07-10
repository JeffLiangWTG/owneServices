using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Abstractions;

namespace Enterprise.DbUpgrader.Transformation.DataModification
{
	public abstract class CreateTriggerTransformation : DataTransformation, ITriggerTransformation
	{
		public override string UserDescription => FormattableString.Invariant($"Creating trigger {Trigger.Name}");

		protected override void OfflinePostUpgradeTransform()
		{
			if (manager.SchemaVersionBeforeUpgrade.Minor == -1 && Trigger.Text.Equals(Db.Connection.ExecuteScalar($"SELECT OBJECT_DEFINITION(OBJECT_ID(N'{Trigger.Name}', N'TR'))")))
			{
				return; // Skip Transform that has already run
			}

			Db.Connection.ExecuteNonQuery(Trigger.Text);
		}

		public string TriggerName => Trigger.Name;

		IDbScript trigger;
		IDbScript Trigger => trigger ?? (trigger = GetTrigger());

		protected abstract IDbScript GetTrigger();
	}
}
