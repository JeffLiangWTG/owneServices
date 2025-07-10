using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enterprise.DataTools.DbBackupAndRestore.Business.Restore.CopyProductionToTest.ClearDataScripts;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	static class CopyProductionToTestScriptManager
	{
		#region Public

		public static string GetPopulateTemporaryDataScript(string auxDbName, string targetDbName)
		{
			var builder = new StringBuilder();
			builder.Append(SaveTestDbInfoHeader);

			foreach (var scripts in CopyProductionToTestScriptsCollection)
			{
				builder.Append(scripts.GetPopulateTemporaryDataScript(auxDbName, targetDbName));
			}

			return builder.ToString();
		}

		public static string GetClearDataToBeOverwrittenByTestDataScript(string auxDbName, string targetDbName)
		{
			var builder = new StringBuilder();
			builder.Append(ClearProductionSpecificInfoHeader);

			var clearDataScriptBuilder = new ClearDataScriptBuilder(targetDbName);
			foreach (var clearDataInTestDatabaseScript in GetClearDataScripts())
			{
				clearDataScriptBuilder.AddComment().ForStarting(clearDataInTestDatabaseScript);
				clearDataInTestDatabaseScript.BuildScript(clearDataScriptBuilder);
			}
			builder.Append(clearDataScriptBuilder.Build());

			foreach (var scripts in CopyProductionToTestScriptsCollection.Reverse())
			{
				builder.Append(scripts.GetClearDataToBeOverwrittenByTestDataScript(auxDbName, targetDbName));
			}

			return builder.ToString();
		}

		public static string GetCopyTempDbDataToTestDbScript(string auxDbName, string targetDbName)
		{
			var builder = new StringBuilder();
			builder.Append(CopyPrevioslySevedTestDataHeader);

			foreach (var scripts in CopyProductionToTestScriptsCollection)
			{
				builder.Append(scripts.GetCopyTempDbDataToTestDbScript(auxDbName, targetDbName));
			}

			return builder.ToString();
		}

		public static string GetCreateTemporaryDbScript(string auxDbName)
		{
			return string.Format(CreateTemporaryDbScript, auxDbName);
		}

		public static string GetDropTemporaryDbScript(string auxDbName)
		{
			return string.Format(DropTemporaryDbScript, auxDbName);
		}

		#endregion

		/// <summary>
		/// Add your derived from CopyProductionToTestScripts class to the list below in order of dependency. 
		/// i.e. tables that depend on other tables first
		/// </summary>
		static IPreserveTestValueScripts[] CopyProductionToTestScriptsCollection => new IPreserveTestValueScripts[]
		{
			new RegistryItemsToPreserve(),
			new StmServiceHostToPreserve(),
			new StmScheduleTaskToPreserve(),
			new StmUpgradeToPreserve(),
			new NumberFountainsToPreserve(),
			new StmTranslationFeedbackToPreserve(),
			new StmTranslationFeedbackResourceToPreserve(),
			new RegistryItemsGetMinTransformationVersion(),
			new GlbStaffToPreserve(),
			new StorageDocsToDeleteToPreserve(),
			new GlowThemeItemToPreserve(),
			new GlowThemeTmplToPreserve(),
			new EDICommunicationModeToPreserve(),
		};

		public static IEnumerable<IClearDataScript> GetClearDataScripts()
		{
			yield return new ClearPrintAndMailJobsScript();
			yield return new CancelAnyQueuedMessages();
			yield return new ClearActiveDirectoryRelatedData();
			yield return new LicenceConsumptionFixScript();
			yield return new ClearIncidentApprovalsScript();
			yield return new ClearEDICommunicationMode();
			yield return new ClearDotNetVersionDetails();
			yield return new ClearStmUsageDataScript();
		}

		#region Copy Production To Test Scripts

		#region Headers

		const string SaveTestDbInfoHeader = @"
-----------------------------------------------
-- SAVE TEST DB INFO IN A TEMPORARY DATABASE --
-----------------------------------------------

";

		const string ClearProductionSpecificInfoHeader = @"
------------------------------------
-- CLEAR PRODUCTION SPECIFIC INFO --
------------------------------------

";

		const string CopyPrevioslySevedTestDataHeader = @"
-----------------------------------------------------------
-- COPY PREVIOUSLY SAVED TEST DATA FROM TEMPORARY TABLES --
-----------------------------------------------------------

";
		#endregion

		#region Temporary Database

		const string CreateTemporaryDbScript =
@"-----------------------------------------------
-- CREATE A TEMPORARY DATABASE --
-----------------------------------------------

-- CREATE DATABASE TO STORE TEST DB INFO
IF not exists (SELECT null FROM sys.databases WHERE name = '{0}')
BEGIN
	CREATE DATABASE [{0}]
END";

		const string DropTemporaryDbScript = @"
-----------------------------
-- DROP TEMPORARY DATABASE --
-----------------------------

IF EXISTS (SELECT null FROM sys.databases WHERE name = '{0}')
BEGIN
	DROP DATABASE [{0}]
END";

		#endregion

		#endregion
	}
}
