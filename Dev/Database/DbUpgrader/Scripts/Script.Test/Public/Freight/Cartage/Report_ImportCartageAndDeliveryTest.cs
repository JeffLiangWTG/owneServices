using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Cartage;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Cartage.Testing
{
	[TestedType(typeof(Report_ImportCartageAndDelivery))]
	internal class Report_ImportCartageAndDeliveryTest : DbCreateScriptTest
	{
		public void TestDeliveryModeOnDeclarationJob()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "AU", "AUD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var declarationPK1 = CreateJobDeclarationData("AU", branchPK, companyPK, 1);
			CreateJobDocsAndCartageData(declarationPK1);
			var sql = @"select DeliveryMode from dbo.Report_ImportCartageAndDelivery(@Country, @Company,
	@Delivered,
	@JobType,
	@CartageType,
	@PortOfDischarge,
	@JW_FromAT,   
 	@JW_ToAT,
 	@Vessel,
 	@VoyageFlight,
	@IncludeArchivedVoyages,
	@JS_TransportMode,
	@JE_TransportMode,
	@JS_ContainerMode,
	@JE_ContainerMode)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@Country", SqlDbType.VarChar, string.Empty);
				command.AddParameter("@Company", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@Delivered", SqlDbType.VarChar, "All");
				command.AddParameter("@JobType", SqlDbType.VarChar, "All");
				command.AddParameter("@CartageType", SqlDbType.VarChar, "Other");
				command.AddParameter("@PortOfDischarge", SqlDbType.VarChar, string.Empty);
				command.AddParameter("@JW_FromAT", SqlDbType.VarChar, "2017-1-10");
				command.AddParameter("@JW_ToAT", SqlDbType.VarChar, string.Empty);
				command.AddParameter("@Vessel", SqlDbType.VarChar, string.Empty);
				command.AddParameter("@VoyageFlight", SqlDbType.VarChar, string.Empty);
				command.AddParameter("@IncludeArchivedVoyages", SqlDbType.VarChar, string.Empty);
				command.AddParameter("@JS_TransportMode", SqlDbType.VarChar, string.Empty);
				command.AddParameter("@JE_TransportMode", SqlDbType.VarChar, string.Empty);
				command.AddParameter("@JS_ContainerMode", SqlDbType.VarChar, string.Empty);
				command.AddParameter("@JE_ContainerMode", SqlDbType.VarChar, string.Empty);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						AssertEquals("Delivery Mode from Declaration Job", "PSL", reader["DeliveryMode"].ToString());
					}
				}
			}
		}

		Guid CreateJobDeclarationData(string countryCode, Guid branchpk, Guid companyPK, int clusterKey)
		{
			var declarationPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_DateAtFinalDestination, JE_DeclarationReference, JE_ClusterKey)
VALUES (@declarationPK, @countryCode, @messageType, @branchPK, @companyPK, @destinationDate, @declarationReference, @clusterKey)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@countryCode", SqlDbType.VarChar, countryCode);
				command.AddParameter("@declarationReference", SqlDbType.VarChar, "B0000000001");
				command.AddParameter("@messageType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchpk);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@destinationDate", SqlDbType.DateTime, new DateTime(2017, 1, 11));
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return declarationPK;
		}

		void CreateJobDocsAndCartageData(Guid declarationPK1)
		{
			var sql = @"INSERT INTO dbo.JobDocsAndCartage (JP_PK, JP_IsValid, JP_FCLDeliveryEquipmentNeeded, JP_ParentID, JP_ParentTableCode) VALUES (newid(), '1', @mode, @declarationPK, 'JE')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK1);
				command.AddParameter("@mode", SqlDbType.VarChar, "PSL");
				command.ExecuteNonQuery();
			}
		}
	}
}

