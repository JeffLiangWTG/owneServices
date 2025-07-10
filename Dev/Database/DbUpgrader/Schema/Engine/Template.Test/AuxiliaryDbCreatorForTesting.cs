using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema
{
	public class AuxiliaryDbCreatorForTesting : AuxiliaryDbCreator
	{
		public AuxiliaryDbCreatorForTesting(string dbName)
			: this(dbName, null)
		{
		}

		public AuxiliaryDbCreatorForTesting(string dbName, params string[] createDbObjectsScripts)
			: base(dbName)
		{
			this.createDbObjectsScripts = (createDbObjectsScripts ?? Array.Empty<string>()).ToList();
		}

		protected override void SetupDatabaseAfterCreation(DbConnection conn)
		{
			if (createDbObjectsScripts != null)
			{
				conn.IgnoreCommitTracker = true;
				foreach (string createDbObjectsScript in createDbObjectsScripts)
				{
					if (!String.IsNullOrEmpty(createDbObjectsScript))
					{
						conn.ExecuteNonQuery(createDbObjectsScript);
					}
				}
				conn.IgnoreCommitTracker = false;
			}
		}

		public void AddCreateDbObjectScript(string script)
		{
			createDbObjectsScripts.Add(script);
		}

		protected override bool UseTempPathForDbFiles
		{
			get { return true; }
		}

		readonly List<string> createDbObjectsScripts;
	}
}
