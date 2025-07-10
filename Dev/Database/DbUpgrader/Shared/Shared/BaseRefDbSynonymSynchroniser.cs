using System;
using System.Collections.Generic;

namespace Enterprise.DbUpgrader.Shared
{
	public abstract class BaseRefDbSynonymSynchroniser
	{
		public abstract void SynchroniseReferenceDbSynonyms();

		public abstract bool HasErrors();

		public abstract bool HasSuccesses();

		public abstract IEnumerable<string> Errors { get; }

		public abstract IEnumerable<string> Successes { get; }

		public abstract bool IsCancelled { get; }

		public abstract IEnumerable<string> NewReferenceDbs { get; }

		public event EventHandler<StatusChangedEventArgs> StatusChanged;

		public abstract void Cancelled(object sender, EventArgs e);

		protected virtual void OnStatusChange(StatusChangedEventArgs e)
		{
			if (StatusChanged != null)
			{
				StatusChanged(this, e);
			}
		}

		#region EventArgs

		public class StatusChangedEventArgs : EventArgs
		{
			public StatusChangedEventArgs(int percentComplete, string database, StatusCode code)
			{
				PercentComplete = percentComplete;
				Database = database;
				Code = code;
			}

			public int PercentComplete { get; private set; }
			public string Database { get; private set; }
			public StatusCode Code { get; private set; }
		}

		public enum StatusCode
		{
			CreatingDatabase,
			SynchronisingSynonyms,
			Cancelling
		}

		#endregion
	}
}
