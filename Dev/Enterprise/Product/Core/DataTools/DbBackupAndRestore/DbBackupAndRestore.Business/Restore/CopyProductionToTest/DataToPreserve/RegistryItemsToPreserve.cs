using System.Globalization;
using System.IO;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public class RegistryItemsToPreserve : PreserveTestValueScripts
	{
		#region PreserveTestValueScripts Members

		protected override string GetDataToPreserveFilter(string targetDbName)
		{
			return string.Format(CultureInfo.InvariantCulture, @"WHERE SD_PreserveTestValue = 1");
		}

		public override string GetClearDataToBeOverwrittenByTestDataScript(string auxDbName, string targetDbName)
		{
			return string.Format(CultureInfo.InvariantCulture, ClearDataToBeOverwrittenByTestDataScript, auxDbName, AuxTableName, targetDbName, TargetTableName, GetOldRegistryItemsToPreserve().Replace("'", "''"));
		}

		protected override string TargetTableName
		{
			get { return "StmData"; }
		}

		const string ClearDataToBeOverwrittenByTestDataScript = @"
-- DELETE DATA WHICH WILL BE RE-POPULATED FROM {1} TABLE --
DECLARE @SqlCommand nvarchar(max)
IF EXISTS (SELECT NULL FROM sys.databases WHERE name = '{0}')
BEGIN
	SET @SqlCommand = 'DELETE tgt FROM [{2}]..{3} tgt LEFT JOIN [{0}]..{1} aux ON tgt.SD_Name = aux.SD_Name WHERE aux.SD_Name is not null';
	EXEC (@SqlCommand);
END

IF EXISTS (SELECT NULL FROM [{2}].sys.columns c INNER JOIN [{2}].sys.tables t ON c.object_id = t.object_id WHERE c.name = 'SD_PreserveTestValue' AND t.name = '{3}')
BEGIN
	SET @SqlCommand = 'DELETE tgt FROM [{2}]..{3} tgt WHERE tgt.SD_PreserveTestValue = 1';
	EXEC (@SqlCommand);
END
ELSE
BEGIN
	SET @SqlCommand = 'DELETE tgt FROM [{2}]..{3} tgt WHERE tgt.SD_Name in (
{4}
)';
	EXEC (@SqlCommand);
END
";

		string GetOldRegistryItemsToPreserve()
		{
			string oldRegistryValues;
			const string oldRegItemListResourceName = "Enterprise.DataTools.DbBackupAndRestore.Business.Restore.CopyProductionToTest.DataToPreserve.OldRegistryToPreserveList.txt";

			using (var oldRegItemListStream = this.GetType().Assembly.GetManifestResourceStream(oldRegItemListResourceName))
			using (var oldRegItemListReader = new StreamReader(oldRegItemListStream))
			{
				oldRegistryValues = oldRegItemListReader.ReadToEnd();
			}
			return oldRegistryValues;
		}

		#endregion
	}
}
