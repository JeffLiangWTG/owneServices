using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class StmModuleFilterDataFile : EmbeddedDataFile, IFixReferencesAndDuplicates
	{
		public StmModuleFilterDataFile()
			: base(@"Public\StmModuleFilter\StmModuleFilter.xml", StmModuleFilterSchema.Constants.TableName, StmModuleFilterUserDataSchema.Constants.TableName)
		{
		}

		public StmModuleFilterDataFile(string fileRelativePath) : base(fileRelativePath, StmModuleFilterSchema.Constants.TableName, StmModuleFilterUserDataSchema.Constants.TableName)
		{
		}

		public override string ResourceRelativeName => "StmModuleFilter.StmModuleFilter.xml";

		void IFixReferencesAndDuplicates.PerformExtraDataManipulationBeforeEnablingConstraints()
		{
			string sqlText = "DELETE FROM dbo.StmModuleFilterUserData WHERE S0_S9 NOT IN (SELECT S9_PK FROM dbo.StmModuleFilter)";
			Db.Connection.ExecuteNonQuery(sqlText);
		}

		protected override string SelectQuery
		{
			get
			{
				return @"SELECT * FROM dbo.StmModuleFilter 
							WHERE S9_IsSystem = 1
							AND S9_FilterType <> 'FRU'
							ORDER BY 1;
						SELECT * FROM dbo.StmModuleFilterUserData
						WHERE S0_S9 IN 
						(
							SELECT S9_PK FROM dbo.StmModuleFilter
							WHERE S9_IsSystem = 1
							AND (
								S9_ModuleID LIKE '%_CT' OR
								S9_ModuleID LIKE 'Tracking%' OR
								S9_ModuleID LIKE 'LinerAndAgency%' OR
								S9_ModuleID LIKE 'IBPM%' OR
								S9_ModuleID LIKE 'Whs%' OR
								S9_ModuleID LIKE 'Commission%' OR
								S9_ModuleID = 'GbCcsukAirInventory')
							AND S9_FilterType <> 'FRU'
						)  order by 1";
			}
		}
	}
}
