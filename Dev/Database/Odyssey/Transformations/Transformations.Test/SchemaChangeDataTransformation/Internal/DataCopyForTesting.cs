using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations
{
	sealed class DataCopyForTesting : SchemaChangeDataTransformation
	{
		public const string NonExistingTable = "DataCopyForTesting_NonExistingTable";

		public override string UserDescription
		{
			get { return "This is just a test Data Copy transformation"; }
		}

		protected override SourceTableCollection GetSourceTables()
		{
			SourceColumn codeColumn = new SourceColumn("RN_Code", "char(2)");
			SourceColumn descColumn = new SourceColumn("RN_Desc", "varchar(35)");
			SourceTableForTesting countryTable = new SourceTableForTesting(Db.DatabaseName, "RefCountry", new SourceColumn[] { codeColumn, descColumn }, codeColumn.Name + " in ('AU','NZ')");

			SourceColumn statesCountryColumn = new SourceColumn("RW_RN_NKCountryCode", "char(2)", "AU");
			SourceColumn statesCodeColumn = new SourceColumn("RW_Code", "char(3)");
			SourceColumn statesDescColumn = new SourceColumn("RW_Description", "varchar(35)");
			SourceColumn statesInexistingColumn = new SourceColumn("InexistingColumnName", "char(5)", "'ABCDE'");
			SourceTableForTesting statesTable = new SourceTableForTesting(Db.DatabaseName, "RefCountryStates", new SourceColumn[] { statesCountryColumn, statesCodeColumn, statesDescColumn, statesInexistingColumn }, statesCountryColumn.Name + " in (SELECT RN_Code FROM dbo.RefCountry WHERE RN_Code in ('AU','NZ'))");

			SourceTableCollection result = new SourceTableCollection();
			result.Add(countryTable);
			result.Add(statesTable);

			return result;
		}

		protected override void PasteToTarget()
		{
			string sqlText = "ALTER TABLE dbo.RefCountry ADD RN_NumberOfStates int";
			Db.Connection.ExecuteNonQuery(sqlText);

			sqlText = String.Format("UPDATE dbo.RefCountry SET RN_NumberOfStates = (SELECT count(*) FROM {0} WHERE RW_RN_NKCountryCode = RN_Code) FROM dbo.RefCountry WHERE exists(SELECT null FROM {0} WHERE RW_RN_NKCountryCode = RN_Code)", SourceTables["RefCountryStates"].FullyQualifiedName);
			Db.Connection.ExecuteNonQuery(sqlText);
		}

		public SourceTableCollection SourceTables_Exposed
		{
			get { return (SourceTableCollection)SourceTables; }
		}
	}
}
