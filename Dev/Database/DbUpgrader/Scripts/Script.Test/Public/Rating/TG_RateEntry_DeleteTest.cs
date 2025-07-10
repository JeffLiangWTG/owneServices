using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Rating;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Rating
{
	[TestedType(typeof(TG_RateEntry_Delete))]
	class TG_RateEntry_DeleteTest : DBCreateTriggerScriptTest
	{
		public void TestDeleteOrphanedOrgSalesValueAssociationPivot()
		{
			var ratePK = Guid.NewGuid();
			var entry1PK = Guid.NewGuid(); // to be deleted
			var entry2PK = Guid.NewGuid();

			var insertSql = @"
				INSERT INTO dbo.RatingHeader (TH_PK, TH_OH, TH_RateType, TH_GC, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser, TH_SystemCreateTimeUtc, TH_SystemCreateUser)
				VALUES (@ratePK, (SELECT TOP 1 OH_PK FROM dbo.OrgHeader), 'SAL', @companyPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.RateEntry(TI_PK, TI_TH, TI_GC_Publisher, TI_RateCategory, TI_Mode, TI_OriginLRC, TI_RateStartDate, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser)
				VALUES (@entry1PK, @ratePK, @companyPK, 'AIR', 'LSE', 'AU', @startDate, GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.RateEntry(TI_PK, TI_TH, TI_GC_Publisher, TI_RateCategory, TI_Mode, TI_OriginLRC, TI_RateStartDate, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser)
				VALUES (@entry2PK, @ratePK, @companyPK, 'AIR', 'LSE', 'US', @startDate, GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.OrgSalesValueAssociationPivot (SVP_PK, SVP_ActivityId, SVP_ActivityTableCode, SVP_TradeId, SVP_TradeTableCode, SVP_SystemLastEditTimeUtc, SVP_SystemLastEditUser, SVP_SystemCreateTimeUtc, SVP_SystemCreateUser) 
				VALUES (NEWID(), @entry1PK, 'TI', NEWID(), 'PA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.OrgSalesValueAssociationPivot (SVP_PK, SVP_ActivityId, SVP_ActivityTableCode, SVP_TradeId, SVP_TradeTableCode, SVP_SystemLastEditTimeUtc, SVP_SystemLastEditUser, SVP_SystemCreateTimeUtc, SVP_SystemCreateUser) 
				VALUES (NEWID(), @entry2PK, 'TI', NEWID(), 'PA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			using (var command = TestConnection.Command(insertSql))
			{
				command.AddParameter("@ratePK", SqlDbType.UniqueIdentifier, ratePK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, GlbCompanyPK);
				command.AddParameter("@entry1PK", SqlDbType.UniqueIdentifier, entry1PK);
				command.AddParameter("@entry2PK", SqlDbType.UniqueIdentifier, entry2PK);
				command.AddParameter("@startDate", SqlDbType.Date, DateTime.Today.AddMonths(-6));
				command.ExecuteNonQuery();
			}

			var rowCountSql = "SELECT COUNT(*) FROM dbo.OrgSalesValueAssociationPivot WHERE SVP_ActivityTableCode = 'TI'";
			AssertEquals("Check the number of records before delete", 2, TestConnection.ExecuteScalar<int>(rowCountSql));

			var deleteSql = "DELETE FROM dbo.RateEntry WHERE TI_PK = @entryPK";
			using (var command = TestConnection.Command(deleteSql))
			{
				command.AddParameter("@entryPK", SqlDbType.UniqueIdentifier, entry1PK);
				command.ExecuteNonQuery();
			}

			AssertEquals("Check the number of records after delete", 1, TestConnection.ExecuteScalar<int>(rowCountSql));

			var assertionSql = "SELECT COUNT(*) FROM dbo.OrgSalesValueAssociationPivot WHERE SVP_ActivityId = @entryPK";
			using (var command = TestConnection.Command(assertionSql))
			{
				command.AddParameter("@entryPK", SqlDbType.UniqueIdentifier, entry1PK);
				AssertEquals("Has to be deleted by the trigger", 0, (int)command.ExecuteScalar());
			}

			using (var command = TestConnection.Command(assertionSql))
			{
				command.AddParameter("@entryPK", SqlDbType.UniqueIdentifier, entry2PK);
				AssertEquals("Has to be remained", 1, (int)command.ExecuteScalar());
			}
		}

		static Guid GlbCompanyPK
		{
			get
			{
				var glbCompany = new ActiveRowWrapper(GlbCompanySchema.Instance)
				{
					[GlbCompanySchema.GC_Code] = "NUU",
					[GlbCompanySchema.GC_Name] = "Company = NUU"
				};
				glbCompany.Save();

				return glbCompany.PK;
			}
		}
	}
}
