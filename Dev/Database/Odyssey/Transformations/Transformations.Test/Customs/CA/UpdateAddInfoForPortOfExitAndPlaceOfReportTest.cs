using System;
using System.Text;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.CA
{
	[TestedType(typeof(UpdateAddInfoForPortOfExitAndPlaceOfReport))]
	public sealed class UpdateAddInfoForPortOfExitAndPlaceOfReportTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
			=> new UpdateAddInfoForPortOfExitAndPlaceOfReport(batchSize: 1, offlineTransformationTimeLimitInSeconds: 1D);

		TransformationTestDataCreator dataCreator;
		Guid companyPK, branchPK;
		Guid je_pk1, je_pk2, je_pk3, je_pk4, je_pk5, je_pk6, je_pk7, je_pk8;
		Guid sd_pk1, sd_pk2, sd_pk3;
		int je_clusterKey = 1;

		protected override void PrepareTestData()
		{
			je_pk1 = CreateJobDeclarationWithAddInfo("CarrierCode=A1X-*CarrierName=DHL EXPRESS (CANADA)*PlaceOfReport=NB-Forest City*PortOfExit=ON-Toronto - Pearson Int. Airport");
			CreateGenAddOnColumn("CA_PortOfExit", "ON-Toronto - Pearson Int. Airport", je_pk1);
			CreateGenAddOnColumn("CA_PlaceOfReport", "NB-FOREST CITY", je_pk1);
			CreateGenAddOnColumn("CA_CarrierCode", "A1X-", je_pk1);

			je_pk2 = CreateJobDeclarationWithAddInfo("CarrierCode=A1X-*CarrierName=DHL EXPRESS (CANADA)*PortOfExit=NS-Truro");
			je_pk3 = CreateJobDeclarationWithAddInfo("CarrierCode=A1X-*CarrierName=DHL EXPRESS (CANADA)");

			je_pk4 = CreateJobDeclarationWithAddInfo("CarrierCode=A1X-*CarrierName=DHL EXPRESS (CANADA)*PortOfExit=AAAAAAAAAA*PlaceOfReport=BBBBBBBBBB");
			CreateGenAddOnColumn("CA_PortOfExit", "AAAAAAAAAA", je_pk4);
			CreateGenAddOnColumn("CA_PlaceOfReport", "BBBBBBBBBB", je_pk4);

			je_pk5 = CreateJobDeclarationWithAddInfo("PlaceOfReport=NB_Forest City*PortOfExit=ON_Toronto - Pearson Int. Airport");
			CreateGenAddOnColumn("CA_PortOfExit", "ON_Toronto - Pearson Int. Airport", je_pk5);
			CreateGenAddOnColumn("CA_PlaceOfReport", "NB_FOREST CITY", je_pk5);

			sd_pk1 = CreateRegistryItem("ON-Brampton");
			sd_pk2 = CreateRegistryItem("BC-Kingsgate");
			sd_pk3 = CreateRegistryItem("BC_Kingsgate");
			je_pk6 = CreateJobDeclarationWithAddInfo("CarrierCode=A1X-*CarrierName=DHL EXPRESS (CANADA)*PlaceOfReport=NB-Forest City*PortOfExit={Inv"); // Test online pre-upgrade
			je_pk7 = CreateJobDeclarationWithAddInfo("CarrierCode=A1X-*CarrierName=DHL EXPRESS (CANADA)*PlaceOfReport=NB-Forest City*PortOfExit=ON-Toronto - Pearson Int'Airport");  // Test regex matching
			je_pk8 = CreateJobDeclarationWithAddInfo("CarrierCode=A1X-*CarrierName=DHL EXPRESS (CANADA)*PortOfExit={Inv"); // Test online pre-upgrade
		}

		Guid CreateJobDeclarationWithAddInfo(string addInfo)
		{
			var je_pk = Guid.NewGuid();
			je_clusterKey++;
			dataCreator.CreateDeclaration(je_pk, "Ref" + je_clusterKey, je_clusterKey, branchPK, companyPK, addInfo: addInfo, dataModel: "CA", messageType: "EXP");
			return je_pk;
		}

		Guid CreateGenAddOnColumn(string xa_name, string xa_data, Guid je_pk)
		{
			return dataCreator.CreateGenAddOnColumn(xa_name, xa_data, "JE", je_pk);
		}

		Guid CreateRegistryItem(string registryValue)
		{
			var sd_pk = Guid.NewGuid();
			var sql = $@"
			INSERT INTO StmData(SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue)
			VALUES(@SD_PK, 'DataLoadingModuleDefaultPlaceOfReport', NEWID(), NEWID(), 'STR',  @SD_BinaryValue)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@SD_PK", sd_pk, StmDataSchema.PK);
				command.AddParameterBasedOnDbColumn("@SD_BinaryValue", Encoding.Unicode.GetBytes(registryValue), StmDataSchema.SD_BinaryValue);
				command.ExecuteNonQuery();
			}

			return sd_pk;
		}

		protected override void AssertTransformationResults()
		{
			AssertJobDeclarationAddInfoHasValue(je_pk1, "PortOfExit", "0497");
			AssertJobDeclarationAddInfoHasValue(je_pk1, "PlaceOfReport", "0212");
			AssertJobDeclarationAddInfoHasValue(je_pk1, "CarrierCode", "A1X-");
			AssertJobDeclarationAddInfoHasValue(je_pk2, "PortOfExit", "0022");
			AssertJobDeclarationAddInfoHasValue(je_pk3, "PortOfExit", "");
			AssertJobDeclarationAddInfoHasValue(je_pk4, "PortOfExit", "AAAA");
			AssertJobDeclarationAddInfoHasValue(je_pk4, "PlaceOfReport", "BBBB");
			AssertJobDeclarationAddInfoHasValue(je_pk5, "PortOfExit", "0497");
			AssertJobDeclarationAddInfoHasValue(je_pk5, "PlaceOfReport", "0212");
			AssertRegistryItemHasValue(sd_pk1, "0480");
			AssertRegistryItemHasValue(sd_pk2, "0818");
			AssertRegistryItemHasValue(sd_pk3, "0818");
			AssertJobDeclarationAddInfoNotExists(je_pk6, "PortOfExit");
			AssertJobDeclarationAddInfoHasValue(je_pk6, "PlaceOfReport", "0212");
			AssertJobDeclarationAddInfoHasValue(je_pk6, "CarrierCode", "A1X-");
			AssertJobDeclarationAddInfoHasValue(je_pk7, "PortOfExit", "0497");
			AssertJobDeclarationAddInfoHasValue(je_pk7, "PlaceOfReport", "0212");
			AssertGenAddonColumnNotExists("CA_PortOfExit");
			AssertGenAddonColumnNotExists("CA_PlaceOfReport");
			AssertJobDeclarationAddInfoNotExists(je_pk8, "PortOfExit");
			AssertJobDeclarationAddInfoNotExists(je_pk8, "PlaceOfReport");
		}

		void AssertJobDeclarationAddInfoHasValue(Guid je_pk, string addInfoKey, string expectedValue)
		{
			var actualAddInfoValue = string.Empty;
			var sql = $@"
			SELECT AddInfo.Value
			FROM JobDeclaration 
				CROSS APPLY dbo.csfn_GetAddInfoValueFromCodeInlineToReturnEmptyIfNull(JE_AddInfo, @AddInfoKey) AS AddInfo
			WHERE JE_PK = @JE_PK";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@JE_PK", je_pk, JobDeclarationSchema.PK);
				command.AddParameter("@AddInfoKey", System.Data.SqlDbType.VarChar, addInfoKey);
				actualAddInfoValue = (string)command.ExecuteScalar();
			}

			AssertEquals($"Assert JobDeclaration.JE_AddInfo has '*{addInfoKey}={expectedValue}'", expectedValue, actualAddInfoValue);
		}

		void AssertJobDeclarationAddInfoNotExists(Guid je_pk, string addInfoKey)
		{
			var sql = @"
			FROM dbo.JobDeclaration
			WHERE JE_PK = @JE_PK
				AND JE_AddInfo LIKE '%PortOfExit=%'";

			var rowExists = Db.Connection.Exists(sql, x => x.AddParameterBasedOnDbColumn("@JE_PK", je_pk, JobDeclarationSchema.PK));
			Assert($"Assert JobDeclaration.JE_AddInfo has no '*{addInfoKey}' addinfo", !rowExists);
		}

		void AssertGenAddonColumnNotExists(string xa_name)
		{
			var sql = @"
			FROM GenAddonColumn
			WHERE XA_Name = @XA_Name
				AND XA_ParentTableCode = 'JE'";

			var rowExists = Db.Connection.Exists(sql, x => x.AddParameter("@XA_Name", System.Data.SqlDbType.VarChar, xa_name));

			Assert($"Assert GenAddonColumn has no row with XA_Name={xa_name}", !rowExists);
		}

		void AssertRegistryItemHasValue(Guid sd_pk, string expectedValue)
		{
			var actualValue = string.Empty;
			var sql = $@"SELECT SD_BinaryValue FROM StmData WHERE SD_PK = @SD_PK";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@SD_PK", sd_pk, StmDataSchema.PK);
				actualValue = Encoding.Unicode.GetString((byte[])command.ExecuteScalar());
			}

			AssertEquals(expectedValue, actualValue);
		}

		public void TestOfflinePostUpgradeTimeLimitAndOnlinePostUpgradeTakingOver()
		{
			Db.Connection.ExecuteNonQuery("DELETE JobDeclaration");
			je_pk1 = CreateJobDeclarationWithAddInfo("CarrierCode=A1X-*CarrierName=DHL EXPRESS (CANADA)*PortOfExit=ON-Toronto - Pearson Int. Airport"); // Should be processed by Online Post-Upgrade
			for (var i = 0; i < 100; i++)
			{
				CreateJobDeclarationWithAddInfo("CarrierCode=A1X-*CarrierName=DHL EXPRESS (CANADA)*PortOfExit=ON-Toronto - Pearson Int. Airport");
			}
			je_pk2 = CreateJobDeclarationWithAddInfo("CarrierCode=A1X-*CarrierName=DHL EXPRESS (CANADA)*PortOfExit=ON-Toronto - Pearson Int. Airport"); // Should be processed by Offline Post-Upgrade

			var transformation = new UpdateAddInfoForPortOfExitAndPlaceOfReportForTest();
			transformation.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertJobDeclarationAddInfoHasValue(je_pk1, "PortOfExit", "ON-Toronto - Pearson Int. Airport");
			AssertJobDeclarationAddInfoHasValue(je_pk2, "PortOfExit", "0497");

			transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			AssertJobDeclarationAddInfoHasValue(je_pk1, "PortOfExit", "0497");
			AssertJobDeclarationAddInfoHasValue(je_pk2, "PortOfExit", "0497");
		}

		protected override void SetUp()
		{
			base.SetUp();
			dataCreator = new TransformationTestDataCreator();
			companyPK = dataCreator.CreateCompany(Guid.NewGuid(), "CP1", "CA", "CAD");
			branchPK = dataCreator.CreateBranch("BR1", "BR001", companyPK);
		}
	}

	class UpdateAddInfoForPortOfExitAndPlaceOfReportForTest : UpdateAddInfoForPortOfExitAndPlaceOfReport
	{
		public UpdateAddInfoForPortOfExitAndPlaceOfReportForTest()
			: base(batchSize: 1000, offlineTransformationTimeLimitInSeconds: 1)
		{
		}

		protected override string UpdateJobDeclarationQuery => base.UpdateJobDeclarationQuery + @"
WAITFOR DELAY '00:00:02'
";
	}
}
