using System;
using Enterprise.DbUpgrader.Resource.Version;

namespace Enterprise.ServiceManager.Host
{
	[Serializable]
	public class DatabaseVersionException : Exception
	{
		public DatabaseVersionException(string message) : base(message)
		{
		}

		public DatabaseVersionException(string message, Exception ex) : base(message, ex)
		{
		}

#if NETFRAMEWORK
		protected DatabaseVersionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		internal DatabaseVersionException(VersionLabel databaseSchemaVersion, VersionLabel databaseScriptVersion, VersionLabel databaseTransformationVersion, VersionLabel applicationSchemaVersion, VersionLabel applicationScriptVersion, VersionLabel applicationTransformationVersion)
			: this(GetDbVersionErrorMessage(databaseSchemaVersion, databaseScriptVersion, databaseTransformationVersion, applicationSchemaVersion, applicationScriptVersion, applicationTransformationVersion))
		{
		}

		static string GetDbVersionErrorMessage(VersionLabel databaseSchemaVersion, VersionLabel databaseScriptVersion, VersionLabel databaseTransformationVersion, VersionLabel applicationSchemaVersion, VersionLabel applicationScriptVersion, VersionLabel applicationTransformationVersion)
		{
			string AddAdminMessage(string message)
			{
				return $"{message}\r\n\r\nPlease contact your system administrator to resolve this.";
			}

			string AddVersionsMessage(string message)
			{
				return AddAdminMessage($@"{message}

Database Schema: {databaseSchemaVersion}
Application Schema: {applicationSchemaVersion}

Database Script: {databaseScriptVersion}
Application Script: {applicationScriptVersion}

Database Transformation: {databaseTransformationVersion}
Application Transformation: {applicationTransformationVersion})");
			}

			if (databaseScriptVersion.CompareTo(applicationScriptVersion) == 0
				&& databaseTransformationVersion.CompareTo(applicationTransformationVersion) == 0)
			{
				if (databaseSchemaVersion.CompareTo(applicationSchemaVersion) == 0)
				{
					throw new ArgumentException("All versions are the same.");
				}

				if (!databaseSchemaVersion.IsMajorDiff(applicationSchemaVersion.Major)
					&& databaseSchemaVersion.Minor == -1)
				{
					return AddAdminMessage("The Database is in restore state.");
				}
			}

			return AddVersionsMessage(databaseSchemaVersion.Minor == -1
				? "The Database Version does not match the Application Version. Database may be in restore state."
				: "The Database Version does not match the Application Version.");
		}
	}
}
