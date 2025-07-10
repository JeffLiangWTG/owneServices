using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class DocManagerSchemaSynchroniserForTest : DocManagerSchemaSynchronisationWrapper
	{
		public DocManagerSchemaSynchroniserForTest(IUpgradeManager manager, string testDocManagerDbToUpgrade, DbConnection connection)
			: base(manager, testDocManagerDbToUpgrade, connection)
		{
		}

		protected override IAuxiliaryDbCreator GetTemplateDbCreator()
		{
			return new TemplateDbCreatorForTest(Manager, TemplateDb, CreateObjectsScript);
		}

		const string CreateObjectsScript = @"
			CREATE TABLE TestTable
			(
				SC_PK UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
				SC_Col2 CHAR(1) NOT NULL,
				SC_Col3 VARCHAR(30) NULL,
				SC_Image VARCHAR(max),
				CONSTRAINT PK_TestTable PRIMARY KEY NONCLUSTERED (SC_PK)
			)
			CREATE TABLE dbo.StorageDocs
			(
				SC_PK UNIQUEIDENTIFIER NOT NULL,
				SC_IsDeleted Bit NOT NULL,
			)
";

		public string TemplateDb_Exposed
		{
			get { return TemplateDb; }
		}
	}
}
