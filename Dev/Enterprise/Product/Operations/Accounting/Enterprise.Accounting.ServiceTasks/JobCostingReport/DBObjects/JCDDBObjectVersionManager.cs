using System;
using System.Collections.Generic;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport
{
	public class JCDDBObjectVersionManager
	{
		public JCDDBObjectVersionManager(string description, int currentVersion, Action<int> changeVersion, int latestVersion, IEnumerable<JCDDependentDBObject> dbObjects)
		{
			this.Description = description;
			this.currentVersion = currentVersion;
			this.changeVersion = changeVersion;
			this.latestVersion = latestVersion;
			this.dbObjects = dbObjects;
		}

		readonly int currentVersion;
		readonly Action<int> changeVersion;
		readonly int latestVersion;
		readonly IEnumerable<JCDDependentDBObject> dbObjects;

		public string Description { get; }
		public int CurrentVersion { get { return currentVersion; } }
		public Action<int> ChangeVersion { get { return changeVersion; } }
		public int LatestVersion { get { return latestVersion; } }
		public IEnumerable<JCDDependentDBObject> DBObjects { get { return dbObjects; } }
		public bool IsUpToDate { get { return CurrentVersion == LatestVersion; } }
	}
}
