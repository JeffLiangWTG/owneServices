using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(csfn_PGALineStatusInline))]
	class csfn_PGALineStatusInlineTest : DbCreateScriptTest
	{
		public void TestGetPGALineStatusInLine()
		{
			AssertPGALineStatusInLine("InvoiceLine2PK", @"FDA - 02; FDA - 02", @"FDA - HOLD INTACT; FDA - HOLD INTACT", @"FDA - 2020-01-02; FDA - 2020-01-02", InvoiceLine2PK);
			AssertPGALineStatusInLine("InvoiceLine1PK", @"FDA - 01; APH - 07", @"FDA - ; APH - ", @"FDA - 2020-01-05; APH - 2020-01-05", InvoiceLine1PK);
		}

		Guid DeclarationPK;
		Guid CompanyPK;
		Guid BranchPK;
		Guid InvoicePK;
		Guid InvoiceLine1PK;
		Guid CusDisposition1PK;
		Guid CusDisposition12PK;
		Guid CusDisposition13PK;
		Guid B7PK;
		Guid InvoiceLine2PK;
		Guid CusDisposition2PK;
		Guid ZZZ_PK;
		Guid ZZK_PK;
		Guid ZZD_PK;

		protected override void SetUp()
		{
			base.SetUp();
			CompanyPK = Guid.NewGuid();
			BranchPK = Guid.NewGuid();
			DeclarationPK = Guid.NewGuid();
			InvoicePK = Guid.NewGuid();
			InvoiceLine1PK = Guid.NewGuid();
			CusDisposition1PK = Guid.NewGuid();
			CusDisposition12PK = Guid.NewGuid();
			CusDisposition13PK = Guid.NewGuid();
			InvoiceLine2PK = Guid.NewGuid();
			CusDisposition2PK = Guid.NewGuid();
			B7PK = Guid.NewGuid();
			ZZZ_PK = Guid.NewGuid();
			ZZK_PK = Guid.NewGuid();
			ZZD_PK = Guid.NewGuid();

			var sql = @"
				INSERT INTO dbo.GlbCompany(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES(@CompanyPK, 'US', 'USD', 'USC', 'US company')
				INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode) VALUES(@BranchPK, @CompanyPK, 'USB', 'US')
				INSERT INTO dbo.JobDeclaration(JE_PK, JE_DataModel, JE_GB, JE_GC, JE_AddInfo, JE_RN_NKTransportNationality, JE_RL_NKFinalDestination, JE_RL_NKOrigin, JE_ClusterKey)
									VALUES(@DeclarationPK, 'US', @BranchPK, @CompanyPK, '', 'US', 'CNHK', 'USLON', 1)
				INSERT INTO dbo.JobComInvoiceHeader (JZ_PK, JZ_DataModel, JZ_JE, JZ_RX_NKInvoice_Currency, JZ_ClusterKey) VALUES (@InvoicePK, 'US', @DeclarationPK, 'AUD', 1);
				INSERT INTO dbo.JobComInvoiceLine (JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey) VALUES (@InvoiceLine1PK, 'US', @InvoicePK, 1);
				INSERT INTO dbo.CusDisposition (CDI_PK, CDI_ParentID, CDI_ParentTableCode, CDI_Type, CDI_StatusKey, CDI_Status, CDI_StatusDate, CDI_Sequence, CDI_Notes)
									VALUES (@CusDisposition1PK, @InvoiceLine1PK, 'JI', 'PLS', 'FDA', '01', '2020-01-05 00:01', 0, '');

				INSERT dbo.CusAddInfo(B7_PK,	B7_Type,	B7_AddInfoData,	B7_ParentTableCode,	B7_ParentID, B7_IsValid, B7_AutoVersion, B7_SystemCreateTimeUtc,B7_SystemCreateUser, B7_SystemLastEditTimeUtc,B7_SystemLastEditUser)
				VALUES ( @B7PK , 'PGA' , 'CertifyingIndividual=IM*InvCurrPGAValue=10000*PGACommercialDescription=PLYWOOD*PGAContactEmail=brendon.paine@wisetechglobal.com*PGAContactName=TryThis*PGAContactPhoneNo=2025551212*PGALineItemNumber=1*PGALineValue=10000*TrackingStatus=ACC'
				, 'JI' , @InvoiceLine1PK , 1 , 2 , '2020-01-25 16:55:00' , 'CR' , '2020-01-25 16:56:00' , '~BP')
				INSERT INTO dbo.CusDisposition (CDI_PK, CDI_ParentID, CDI_ParentTableCode, CDI_Type, CDI_StatusKey, CDI_Status, CDI_StatusDate, CDI_Sequence, CDI_Notes)
									VALUES (@CusDisposition12PK, @B7PK, 'B7', 'PLS', 'APH', '07', '2020-01-05 00:01', 0, '');

				INSERT INTO dbo.JobComInvoiceLine (JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey) VALUES (@InvoiceLine2PK, 'US', @InvoicePK, 1);
				INSERT INTO dbo.CusDisposition (CDI_PK, CDI_ParentID, CDI_ParentTableCode, CDI_Type, CDI_StatusKey, CDI_Status, CDI_StatusDate, CDI_Sequence, CDI_Notes)
									VALUES (@CusDisposition2PK, @InvoiceLine2PK, 'JI', 'PLS', 'FDA', '02', '2020-01-02 00:01', 0, '');
				INSERT INTO dbo.CusDisposition (CDI_PK, CDI_ParentID, CDI_ParentTableCode, CDI_Type, CDI_StatusKey, CDI_Status, CDI_StatusDate, CDI_Sequence, CDI_Notes)
									VALUES (@CusDisposition13PK, @InvoiceLine2PK, 'JI', 'PLS', 'FDA', '02', '2020-01-02 00:01', 0, '');

				IF NOT EXISTS (SELECT NULL FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'US')
				INSERT INTO RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES (@ZZZ_PK, 'US', 'US', NULL)
				
				INSERT INTO RefDatabase_RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadOnly, ZZK_ZZZ_NKDataGrouping) VALUES (@ZZK_PK, 'S70LS', 'S70LS', 1, 'US');
				INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
					VALUES (@ZZD_PK, 'S70LS', '02', 'HOLD INTACT', '2020-01-01', '2079-06-06', 'US')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, CompanyPK);
				command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, BranchPK);
				command.AddParameter("@DeclarationPK", SqlDbType.UniqueIdentifier, DeclarationPK);
				command.AddParameter("@InvoicePK", SqlDbType.UniqueIdentifier, InvoicePK);
				command.AddParameter("@InvoiceLine1PK", SqlDbType.UniqueIdentifier, InvoiceLine1PK);
				command.AddParameter("@CusDisposition1PK", SqlDbType.UniqueIdentifier, CusDisposition1PK);
				command.AddParameter("@CusDisposition12PK", SqlDbType.UniqueIdentifier, CusDisposition12PK);
				command.AddParameter("@CusDisposition13PK", SqlDbType.UniqueIdentifier, CusDisposition13PK);
				command.AddParameter("@B7PK", SqlDbType.UniqueIdentifier, B7PK);
				command.AddParameter("@InvoiceLine2PK", SqlDbType.UniqueIdentifier, InvoiceLine2PK);
				command.AddParameter("@CusDisposition2PK", SqlDbType.UniqueIdentifier, CusDisposition2PK);
				command.AddParameter("@ZZZ_PK", SqlDbType.UniqueIdentifier, ZZZ_PK);
				command.AddParameter("@ZZK_PK", SqlDbType.UniqueIdentifier, ZZK_PK);
				command.AddParameter("@ZZD_PK", SqlDbType.UniqueIdentifier, ZZD_PK);
				command.ExecuteNonQuery();
			}
		}

		void AssertPGALineStatusInLine(string description, string expectedOthers, string expectedDesc, string expectedDate, Guid invoiceLinePK)
		{
			var reportSql = @"select PGALineStatus, PGALineStatusDescription, PGALineStatusDate from csfn_PGALineStatusInline(
			@InvoiceLinePK,
			@DispositionType)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@InvoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@DispositionType", SqlDbType.VarChar, "PLS");
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						AssertEquals(description + " Status", expectedOthers, reader["PGALineStatus"].ToString());
						AssertEquals(description + " Description", expectedDesc, reader["PGALineStatusDescription"].ToString());
						AssertEquals(description + " Date", expectedDate, reader["PGALineStatusDate"].ToString());
					}
				}
			}
		}
	}
}
