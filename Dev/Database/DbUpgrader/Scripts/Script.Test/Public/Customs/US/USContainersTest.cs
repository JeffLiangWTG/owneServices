using System;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USContainers))]
	class USContainersTest : DbCreateScriptTest
	{
		public void TestAvailableAndStorageDate()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");

			var declarationPK = Guid.NewGuid();

			var declarationSql = @"INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_OH_Importer, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_ClusterKey)
VALUES (@declarationPK, 'US', 'IMP', @branchPK, @companyPK, @importerPK, 'BJOB1', 'ACS', 0, 1);
";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}

			TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", "EntryNum1", "ENS", "CUS", "US");
			var jobContainerPK = CreateJobContainer("ABCD1234560");
			var refContainerPK = CreateRefContainerWithCodeMaps();
			CreateCusContainer("ABCD1234560", declarationPK, jobContainerPK, refContainerPK, "US");

			AssertAvailableAndStorageDate(companyPK, importerPK, null, null);

			CreateDestination("DEC", declarationPK, new DateTime(2015, 1, 3), new DateTime(2015, 1, 4));
			AssertAvailableAndStorageDate(companyPK, importerPK, new DateTime(2015, 1, 3), new DateTime(2015, 1, 4));

			var jobConsolPK = CreateJobConsolForContainer(jobContainerPK, "C00001234");
			AssertAvailableAndStorageDate(companyPK, importerPK, null, null);

			CreateDestination("CON", jobConsolPK, new DateTime(2015, 2, 5), new DateTime(2015, 2, 6));
			AssertAvailableAndStorageDate(companyPK, importerPK, new DateTime(2015, 2, 5), new DateTime(2015, 2, 6));

			OverrideFCLAvailableStorage(jobContainerPK, new DateTime(2015, 3, 7), new DateTime(2015, 3, 8));
			AssertAvailableAndStorageDate(companyPK, importerPK, new DateTime(2015, 3, 7), new DateTime(2015, 3, 8));
		}

		void AssertAvailableAndStorageDate(Guid companyPK, Guid importerPK, DateTime? availableDate, DateTime? storageDate, string usContainerCode = "")
		{
			var reportSql = @"select AvailableDate, StorageDate from USContainers (@companyPK, '', '', '', '', '', '', @importerPK, '')";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);

				using (var reader = command.ExecuteReader())
				{
					var totalRecordNumber = 0;
					while (reader.Read())
					{
						if (availableDate.HasValue)
						{
							AssertEquals(availableDate.Value, reader["AvailableDate"]);
						}
						else
						{
							AssertEquals(DBNull.Value, reader["AvailableDate"]);
						}
						if (storageDate.HasValue)
						{
							AssertEquals(storageDate.Value, reader["StorageDate"]);
						}
						else
						{
							AssertEquals(DBNull.Value, reader["StorageDate"]);
						}

						if (!usContainerCode.IsNullOrEmpty())
						{
							AssertEquals(usContainerCode, reader["USContainerCode"].ToString());
						}

						totalRecordNumber++;
					}

					AssertEquals("Total record number.", 1, totalRecordNumber);
				}
			}
		}

		static Guid CreateRefContainerWithCodeMaps()
		{
			var refContainerPK = Guid.NewGuid();
			var sql = string.Format(CultureInfo.InvariantCulture,
				@"
INSERT INTO dbo.RefContainer ( RC_PK ,
                               RC_Code ,
                               RC_ShippingMode ,
                               RC_Description ,
                               RC_Length ,
                               RC_Height ,
                               RC_Width ,
                               RC_ContainerType ,
                               RC_ISOType ,
                               RC_TareWeight ,
                               RC_GrossWeight ,
                               RC_CubicCapacity ,
                               RC_StorageClass ,
                               RC_HandlingRateClass ,
                               RC_FreightRateClass ,
                               RC_IATARateClass ,
                               RC_USContainerCode ,
                               RC_TEU ,
                               RC_IsHighCube ,
                               RC_HasTynes ,
                               RC_HasVents ,
                               RC_IsIso ,
                               RC_IsActive ,
                               RC_ISOEquipmentSizeTypeCode )
VALUES ( '{0}' , -- RC_PK - uniqueidentifier
         'CNT1' ,   -- RC_Code - varchar(10)
         'SEA' ,   -- RC_ShippingMode - varchar(3)
         'Container 1' ,   -- RC_Description - varchar(35)
         40 , -- RC_Length - decimal(9, 3)
         30 , -- RC_Height - decimal(9, 3)
         20 , -- RC_Width - decimal(9, 3)
         'DRY' ,   -- RC_ContainerType - varchar(3)
         '' ,   -- RC_ISOType - char(4)
         400.000 , -- RC_TareWeight - decimal(9, 3)
         13608.000 , -- RC_GrossWeight - decimal(9, 3)
         33.250 , -- RC_CubicCapacity - decimal(9, 3)
         '' ,   -- RC_StorageClass - varchar(3)
         '' ,   -- RC_HandlingRateClass - varchar(4)
         '' ,   -- RC_FreightRateClass - varchar(4)
         '1S' ,   -- RC_IATARateClass - varchar(3)
         '' ,   -- RC_USContainerCode - varchar(2)
         0 , -- RC_TEU - decimal(5, 2)
         0 , -- RC_IsHighCube - bit
         0 , -- RC_HasTynes - bit
         0 , -- RC_HasVents - bit
         0 , -- RC_IsIso - bit
         1 , -- RC_IsActive - bit
         ''     -- RC_ISOEquipmentSizeTypeCode - varchar(10)
);
INSERT INTO dbo.RefContainerCodeMap ( RCM_PK ,
                                      RCM_RC_Container ,
                                      RCM_RN_NKCountry ,
                                      RCM_Usage ,
                                      RCM_Code )
VALUES ( NEWID() , -- RCM_PK - uniqueidentifier
         '{0}' ,   -- RCM_RC_Container - varchar(10)
         'CN' ,   -- RCM_RN_NKCountry - char(2)
         'CIQ' ,   -- RCM_Usage - varchar(3)
         'C1'     -- RCM_Code - varchar(5)
);

INSERT INTO dbo.RefContainerCodeMap ( RCM_PK ,
                                      RCM_RC_Container ,
                                      RCM_RN_NKCountry ,
                                      RCM_Usage ,
                                      RCM_Code )
VALUES ( NEWID() , -- RCM_PK - uniqueidentifier
         '{0}' ,   -- RCM_RC_Container - varchar(10)
         'US' ,   -- RCM_RN_NKCountry - char(2)
         '' ,   -- RCM_Usage - varchar(3)
         'U1'     -- RCM_Code - varchar(5)
);
", refContainerPK);

			using (var command = Db.Connection.Command(sql))
			{
				command.ExecuteNonQuery();
			}

			return refContainerPK;
		}

		static Guid CreateJobContainer(string containerNumber)
		{
			var jobContainerPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobContainer (JC_PK, JC_ContainerNum, JC_OverrideFCLAvailableStorage) 
VALUES (@jobContainerPK, @containerNumber, 0)
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@jobContainerPK", SqlDbType.UniqueIdentifier, jobContainerPK);
				command.AddParameter("@containerNumber", SqlDbType.VarChar, containerNumber);
				command.ExecuteNonQuery();
			}
			return jobContainerPK;
		}

		static void OverrideFCLAvailableStorage(Guid jobContainerPK, DateTime availableDate, DateTime storageDate)
		{
			var sql = @"
UPDATE dbo.JobContainer
SET
	JC_OverrideFCLAvailableStorage = 1,
	JC_FCLAvailable = @availableDate,
	JC_ArrivalCTOStorageStartDate = @storageDate ,
	JC_SystemLastEditTimeUtc = GETUTCDATE(),
	JC_SystemLastEditUser = '~BP'
WHERE JC_PK = @jobContainerPK
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@jobContainerPK", SqlDbType.UniqueIdentifier, jobContainerPK);
				command.AddParameter("@availableDate", SqlDbType.DateTime, availableDate);
				command.AddParameter("@storageDate", SqlDbType.DateTime, storageDate);
				command.ExecuteNonQuery();
			}
		}

		static Guid CreateCusContainer(string containerNumber, Guid declarationPK, Guid jobContainerPK, Guid refContainerPK, string dataModel)
		{
			var cusContainerPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusContainer (CO_PK, CO_ContainerNumber, CO_JE, CO_JC, CO_RC, CO_ClusterKey, CO_DataModel) 
VALUES (@cusContainerPK, @containerNumber, @declarationPK, @jobContainerPK, @refContainerPK, 1, @dataModel)
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@cusContainerPK", SqlDbType.UniqueIdentifier, cusContainerPK);
				command.AddParameter("@containerNumber", SqlDbType.VarChar, containerNumber);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@jobContainerPK", SqlDbType.UniqueIdentifier, jobContainerPK);
				command.AddParameter("@refContainerPK", SqlDbType.UniqueIdentifier, refContainerPK);
				command.AddParameter("@dataModel", SqlDbType.VarChar, dataModel);
				command.ExecuteNonQuery();
			}
			return cusContainerPK;
		}

		static void CreateDestination(string parentType, Guid parentPK, DateTime availableDate, DateTime storageDate)
		{
			var voyagePK = Guid.NewGuid();
			var voyageOriginPK = Guid.NewGuid();
			var voyageDestinationPK = Guid.NewGuid();
			var sailingPK = Guid.NewGuid();
			var transportPK = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.JobVoyage(JV_PK)
	VALUES(@voyagePK)

INSERT INTO dbo.JobVoyOrigin(JA_PK, JA_RL_NKPortOfLoading, JA_JV)
	VALUES(@voyageOriginPK, 'UAODS', @voyagePK)

INSERT INTO dbo.JobVoyDestination(JB_PK, JB_RL_NKPortOfDischarge, JB_JV, JB_AvailabilityDate, JB_StorageDate)
	VALUES(@voyageDestinationPK, 'AUBNE', @voyagePK, @availableDate, @storageDate)

INSERT INTO dbo.JobSailing(JX_PK, JX_JA, JX_JB)
	VALUES (@sailingPK, @voyageOriginPK, @voyageDestinationPK)

INSERT INTO dbo.JobConsolTransport (JW_PK, JW_JX, JW_LegOrder, JW_ParentGUID, JW_ParentType) 
	VALUES (@transportPK, @sailingPK, @legOrder, @parentGUID, @parentType)
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@voyagePK", SqlDbType.UniqueIdentifier, voyagePK);
				command.AddParameter("@voyageOriginPK", SqlDbType.UniqueIdentifier, voyageOriginPK);
				command.AddParameter("@voyageDestinationPK", SqlDbType.UniqueIdentifier, voyageDestinationPK);
				command.AddParameter("@availableDate", SqlDbType.DateTime, availableDate);
				command.AddParameter("@storageDate", SqlDbType.DateTime, storageDate);
				command.AddParameter("@sailingPK", SqlDbType.UniqueIdentifier, sailingPK);
				command.AddParameter("@transportPK", SqlDbType.UniqueIdentifier, transportPK);
				command.AddParameter("@legOrder", SqlDbType.Int, 1);
				command.AddParameter("@parentGUID", SqlDbType.UniqueIdentifier, parentPK);
				command.AddParameter("@parentType", SqlDbType.VarChar, parentType);
				command.ExecuteNonQuery();
			}
		}

		static Guid CreateJobConsolForContainer(Guid jobContainerPK, string consolNumber)
		{
			var jobConsolPK = Guid.NewGuid();

			string sql = @"
INSERT INTO dbo.JobConsol (JK_PK, JK_UniqueConsignRef)
VALUES (@jobConsolPK, @consolNumber)

UPDATE dbo.JobContainer
SET
	JC_JK = @jobConsolPK,
	JC_SystemLastEditTimeUtc = GETUTCDATE(),
	JC_SystemLastEditUser = '~BP'
WHERE
	JC_PK=@jobContainerPK
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@jobConsolPK", SqlDbType.UniqueIdentifier, jobConsolPK);
				command.AddParameter("@consolNumber", SqlDbType.VarChar, consolNumber);
				command.AddParameter("@jobContainerPK", SqlDbType.UniqueIdentifier, jobContainerPK);
				command.ExecuteNonQuery();
			}

			return jobConsolPK;
		}
	}
}
