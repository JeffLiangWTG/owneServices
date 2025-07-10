#if DEBUG

using System;
using System.Collections.Immutable;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using CargoWise.Database.Abstractions.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class ClientDbSchemaCreationForTesting
	{
		public void RunClientDbCreateScripts(bool recreateObjectIfExists)
		{
			var extensionObjects = GlobalServiceProvider.Instance.GetService<IExtensionObjectsSource>()?.ExtensionObjects;
			if (extensionObjects is null)
			{
				return;
			}

			// create client-specific tables, don't drop them if they already exist (they may contain data)
			RunClientDbCreateScripts(extensionObjects.TableCreationScripts, false);

			// drop and re-create views/functions/stored procedures etc
			RunClientDbCreateScripts(extensionObjects.ViewAndRoutineCreationScripts.CastArray<DatabaseObjectCreateScript>(), recreateObjectIfExists);
		}

		#region Implementation

		void RunClientDbCreateScripts(ImmutableArray<DatabaseObjectCreateScript> scripts, bool recreateObjectIfExists)
		{
			foreach (var script in scripts)
			{
				bool isIndex = script.CreateScript.ToLower().StartsWith("create index");

				if (recreateObjectIfExists && !isIndex && IsObjectNameSpecifiedAndObjectExists(script))
				{
					try
					{
						Db.Connection.ExecuteNonQuery(script.DropScript); // Running client-specific db scripts
					}
					catch (Exception x)
					{
						throw new InvalidOperationException("Invalid DropScript: " + script.DropScript, x);
					}
				}
				if (!IsObjectNameSpecifiedAndObjectExists(script))
				{
					try
					{
						Db.Connection.ExecuteNonQuery(script.CreateScript); // Running client-specific db scripts
					}
					catch (Exception x)
					{
						throw new InvalidOperationException("Invalid CreateScript: " + script.CreateScript, x);
					}
				}
			}
		}

		bool IsObjectNameSpecifiedAndObjectExists(DatabaseObjectCreateScript script)
		{
			if (script.IsObjectNameSpecified)
			{
				var sql = @"
SELECT ObjectExists = CONVERT(bit,
	CASE
		WHEN EXISTS(SELECT NULL FROM sys.objects WHERE name = @name) THEN 1
		WHEN EXISTS(SELECT NULL FROM sys.indexes WHERE name = @name) THEN 1
		ELSE 0
	END
	)
";
				return Db.Connection.ExecuteScalar<bool>(sql
					, (cmd) =>
					{
						cmd.AddParameter("@name", System.Data.SqlDbType.NVarChar, 128, script.ObjectName);
					});
			}

			return false;
		}

		#endregion // Implementation
	}
}

#endif
