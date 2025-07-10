using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Transformation.DataModification
{
	public enum TransformationSection
	{
		OnlinePreUpgrade,
		OfflinePreUpgrade,
		OfflinePostUpgrade,
		OnlinePostUpgrade,
	}

	public interface IOnlineTransformationProvider
	{
		IEnumerable<IOnlineTransformation> AllTasks { get; }

		IEnumerable<IOnlineTransformation> GetRunningTasks();

		void OnTaskCompleted(IOnlineTransformation task);

		IEnumerable<string> DeletedPendingTasks { get; }
	}
	public interface IOnlineTransformation
	{
		void Run(Action<string> logInformation, CancellationToken token);
		string UserDescription { get; }
	}

	public abstract partial class DataTransformation : IOnlineTransformation
	{
		public void Initialise(VersionLabel version
#if DEBUG
				= null
#endif
			, IUpgradeManager manager
#if DEBUG
			= null
#endif
			)
		{
			this.version = version;
			this.manager = manager;
			InitialiseCore();
		}

		protected virtual void InitialiseCore()
		{
		}

		public virtual bool IsRequired
		{
			get
			{
				var result = version == null || version.CompareTo(manager.TransformationVersionBeforeUpgrade) > 0;
				return result;
			}
		}

		public VersionLabel Version => version;

		public abstract string UserDescription { get; }

		class ManagerWrapper : IUpgradeManager
		{
			public ManagerWrapper(Action<string> logInformation)
			{
				this.logInformation = logInformation;
			}
			readonly Action<string> logInformation;

			#region blocked
			public VersionLabel SchemaVersionBeforeUpgrade => throw new NotImplementedException();

			public VersionLabel TransformationVersionBeforeUpgrade => throw new NotImplementedException();

			public bool IsHosted => throw new NotImplementedException();

			public bool? IsInternalSystem => throw new NotImplementedException();

			public bool? IsUATSystem => throw new NotImplementedException();

			public void ActivateSubtaskProgress(int numOfSubtasks)
			{
				throw new NotImplementedException();
			}

			public void ActivateTaskProgress(int numOfTasks)
			{
				throw new NotImplementedException();
			}

			public bool GetUserConfirmation(string title, string message, string[] detailLines)
			{
				throw new NotImplementedException();
			}

			public void IncrementNumberOfTasks(int numOfTasksToAdd)
			{
				throw new NotImplementedException();
			}

			public bool ShowErrorWithRetry(string errorMessage)
			{
				throw new NotImplementedException();
			}

			public void ShowInfoMessage(string infoMessage)
			{
				logInformation(infoMessage);
			}

			public void ShowTaskError(string errorMessage)
			{
				throw new NotImplementedException();
			}

			public void StartNonEstimatedTask(string task)
			{
				throw new NotImplementedException();
			}

			public void StartSubtask(string subtask)
			{
			}

			public void StartTask(string task)
			{
			}

			public void UpdateCurrentProgress(int currentProgress)
			{
				throw new NotImplementedException();
			}

			public bool ManagerKeyExists(DbConnection connection, string dbName)
			{
				throw new NotImplementedException();
			}

			public void SetManagerKey(DbConnection connection, string dbName)
			{
				throw new NotImplementedException();
			}
			#endregion
		}

		void IOnlineTransformation.Run(Action<string> logInformation, CancellationToken token)
		{
			// this allows the 'standard' manager code to work in the online postupgrade environment.
			// otherwise, we'd still need to support IUpgradeManager AND logger
			// if you touch anything here it will blow up if it isn't part of ILogger
			manager = new ManagerWrapper(logInformation);
			Run(TransformationSection.OnlinePostUpgrade, token);
		}

#if DEBUG
		// TODO: This is to allow tests to pass to expose the rest of the work required
		public void Run()
		{
			Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
		}
#endif

		public
#if DEBUG
			virtual
#endif
			void Run(TransformationSection transformationSection, CancellationToken token)
		{
			var methodInfo = GetType().GetMethod(transformationSection.ToString() + "Transform", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			if (methodInfo.DeclaringType != typeof(DataTransformation))
			{
				ShowStartTransformation();
				switch (transformationSection)
				{
					case TransformationSection.OnlinePreUpgrade:
						OnlinePreUpgradeTransform();
						break;
					case TransformationSection.OfflinePreUpgrade:
						OfflinePreUpgradeTransform();
						break;
					case TransformationSection.OfflinePostUpgrade:
						AddSchemaCheckBlocking();
						try { OfflinePostUpgradeTransform(); }
						finally { RemoveSchemaCheckBlocking(); }
						break;
					case TransformationSection.OnlinePostUpgrade:
						AddSchemaCheckBlocking();
						try { OnlinePostUpgradeTransform(token); }
						finally { RemoveSchemaCheckBlocking(); }
						break;
				}
				TransformationCompleted();
			}
		}

		protected string TemplateDb => templateDbName ?? (templateDbName = UpgUtils.GetTemplateDbName());
		string templateDbName;

		#region Implementation

		protected IUpgradeManager manager;
		protected VersionLabel version;

		protected virtual void OnlinePreUpgradeTransform() { }
		protected virtual void OfflinePreUpgradeTransform() { }
		protected virtual void OfflinePostUpgradeTransform() { }
		protected virtual void OnlinePostUpgradeTransform(CancellationToken token)
		{
		}

		protected void ShowStartTransformation()
		{
			manager?.StartSubtask(UserDescription);
		}

		protected void TransformationCompleted()
		{
			ShowInfo("Completed: " + UserDescription);
		}

		protected void ShowInfo(string message)
		{
			manager?.ShowInfoMessage("\t" + message);
		}

		#endregion

		#region Test

		partial void AddSchemaCheckBlocking();

		partial void RemoveSchemaCheckBlocking();

		#endregion // Test
	}
}

#region Test
#if DEBUG
namespace Enterprise.DbUpgrader.Transformation.DataModification
{
	abstract partial class DataTransformation
	{
		string[] baseline;

		bool IsSupertype(string type)
		{
			for (var me = GetType(); me != typeof(object); me = me.BaseType)
			{
				if (me.FullName.Equals(type, StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		partial void AddSchemaCheckBlocking()
		{
			if (baseline is null)
			{
				using var sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.DbUpgrader.Transformation.Common.PreventSchemaChecksBaseline.txt"));
				baseline = sr.ReadToEnd().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
			}
			if (!baseline.Any(IsSupertype))
			{
				Db.Connection.OnExecute += PreventSchemaChecks;
			}
		}

		partial void RemoveSchemaCheckBlocking() => Db.Connection.OnExecute -= PreventSchemaChecks;

		void PreventSchemaChecks(DbCommand command)
		{
			var sanitised = command.CommandText.Replace("[", "").Replace("]", "");
			if (ContainsInsensitive(sanitised, "sys.columns") || ContainsInsensitive(sanitised, "sys.tables") ||
				ContainsInsensitive(sanitised, "sys.objects") || ContainsInsensitive(sanitised, "INFORMATION_SCHEMA"))
			{
				RemoveSchemaCheckBlocking();
				throw new InvalidOperationException(@"Post-upgrade transformations must not verify the existence of tables or columns; these steps may still need to exist,
but should be turned into pre-upgrade transforms to prevent data loss that would have occurred had column existence controlled whether the upgrade step had run.
If you are using sys.objects, you may need to use a more specific view e.g. sys.triggers");
			}
		}

		static bool ContainsInsensitive(string a, string b) => a.IndexOf(b, StringComparison.InvariantCultureIgnoreCase) >= 0;
	}
}
#endif
#endregion // Test
