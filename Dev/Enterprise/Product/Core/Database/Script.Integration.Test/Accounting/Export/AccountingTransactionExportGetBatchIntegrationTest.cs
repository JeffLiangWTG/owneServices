using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Export.Testing
{
	class AccountingTransactionExportGetBatchIntegrationTest : TransactionedTestCase
	{
		public void FAT_TestProcedureDoNotDoTableScan()
		{
			var helper = new TestDbHelper(TestConnection);
			var postDate = new DateTime(2020, 5, 10);
			var now = new DateTime(2020, 5, 12);
			helper.InsertAccPeriod(2020, 3, TestDbHelper.DefaultCompanyPK);
			var possibleTypes = new List<string> { "APS", "ARV", "GPS", "GRV", "HEX", "LEX", "LRX", "PPF" };
			var possibleParentTableCodes = new List<string> { "AH", "AL" };
			var random = new Random();

			var org1 = helper.InsertOrgHeader("ORG1", "ORGHEADER1");
			var glAccount1 = helper.InsertGLAccount("1001.10.10", "TestGLAccount 1");
			var glAccount2 = helper.InsertGLAccount("1001.10.20", "TestGLAccount 2");
			var taxIdPK = helper.InsertTaxRate("TAX1");
			var chargeCodePK = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");

			//Add 5000 rows in current company, 5 rows of each parent type with the same BatchNumber zero
			var parentsXBFromCurrentCompany = PopulateGenExportBatchSequence(TestDbHelper.DefaultCompanyPK, helper.DefaultBranchPK, helper.DefaultDepartmentPK);

			//Add 5000 rows in a different company
			var company2PK = helper.InsertCompany("ABC", "ABC Compay", "CNY", "CN", true, true);
			var branch2PK = helper.InsertBranch("BR2", company2PK, "Branch 2");
			var department2PK = helper.InsertDepartment("DP2", "Department 2");
			var parentsXBFromCompany2 = PopulateGenExportBatchSequence(company2PK, branch2PK, department2PK);

			//Add 10,000 rows in GenExportBatchSequence without parent with random XB_Type
			var parentsXBWithoutCompany = new List<Guid>();
			for (int i = 0; i < 10000; i++)
			{
				var randomParent = Guid.NewGuid();
				helper.InsertGenExportBatchSequence(possibleTypes[random.Next(possibleTypes.Count)], i, possibleParentTableCodes[random.Next(possibleParentTableCodes.Count)], randomParent);
				parentsXBWithoutCompany.Add(randomParent);
			}

			TestConnection.ExecuteNonQuery($"UPDATE STATISTICS {GenExportBatchSequenceSchema.Constants.TableName} WITH FULLSCAN");

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				//Check procedure result
				var result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC AccountingTransactionExportGetBatch @CompanyCode = 'EDI', @BatchNumber = 0");
				var parentIDs = result.Rows.Cast<DataRow>().Select(x => x["XB_ParentID"]).Cast<Guid>().ToList();
				Assert("Rows cannot be from another company", !parentIDs.Any(x => parentsXBFromCompany2.Contains(x)));
				Assert("Rows must have a valid parent", !parentIDs.Any(x => parentsXBWithoutCompany.Contains(x)));
				Assert("Rows must be from the current company only", parentIDs.All(x => parentsXBFromCurrentCompany.Contains(x)));
				AssertEquals("Only 5 rows have the same BatchNumber", 5, result.Rows.Count);

				//Check procedure performance
				var totalRows = 0;
				TestConnection.ExecuteReader($"EXEC AccountingTransactionExportGetBatch @CompanyCode = 'EDI', @BatchNumber = 0", _ => totalRows++);
				AssertEquals(5, totalRows);

				var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains("AccountingTransactionExportGetBatch"));
				var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.Last());
				AssertEquals("There should be no table scans.", false, queryPlanAnalyzer.TableScans.Any(x => x.TableName == GenExportBatchSequenceSchema.Constants.TableName));
				AssertEquals("There should be no index scans.", false, queryPlanAnalyzer.IndexScans.Any(x => x.TableName == GenExportBatchSequenceSchema.Constants.TableName));
			}

			List<Guid> PopulateGenExportBatchSequence(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var genExportBatchSequenceParents = new List<Guid>();
				var batchNumber = 0;

				using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, branchPK, departmentPK))
				{
					//Create 5000 rows with always a different batchNumber for the same company, except batchNumber zero has 5 rows, one of each parent type.
					for (int i = 0; i < 1000; i++)
					{
						var uniqueNum = branchPK.ToString().Substring(0, 7) + i;

						//Header Batch
						var transactionHeaderPK = helper.InsertTransactionHeader("AP", "INV", uniqueNum + "INV2", 100m, new DateTime(2020, 08, 05), branchPK, departmentPK, org: org1, companyPK: companyPK);
						helper.InsertGenExportBatchSequence("APS", batchNumber, "AH", transactionHeaderPK);
						genExportBatchSequenceParents.Add(transactionHeaderPK);
						IncrementBatchNumberExceptFirst();

						//taxGLMovement Batch
						var transactionHeader2PK = helper.InsertTransactionHeader("AR", "INV", uniqueNum + "INV3", 110, DateTime.Today, branchPK, departmentPK, companyPK: companyPK);
						var taxConfigurationPK = helper.InsertTaxConfiguration(uniqueNum + "TAX1", companyPK);
						var taxTransactionPK = helper.InsertTaxTransaction(transactionHeader2PK, companyPK, branchPK, departmentPK, taxConfigurationPK, taxIdPK);
						var taxGLMovementPK = helper.InsertTaxGLMovement(taxTransactionPK, glAccount1, glAccount2, 10);
						helper.InsertGenExportBatchSequence("APS", batchNumber, "ATM", taxGLMovementPK);
						genExportBatchSequenceParents.Add(taxGLMovementPK);
						IncrementBatchNumberExceptFirst();

						//taxTransaction Batch
						var transactionHeader3PK = helper.InsertTransactionHeader("AR", "INV", uniqueNum + "INV4", 110, DateTime.Today, branchPK, departmentPK, companyPK: companyPK);
						var taxConfiguration2PK = helper.InsertTaxConfiguration(uniqueNum + "TAX2", companyPK);
						var taxTransaction2PK = helper.InsertTaxTransaction(transactionHeader3PK, companyPK, branchPK, departmentPK, taxConfiguration2PK, taxIdPK);
						helper.InsertGenExportBatchSequence("APS", batchNumber, "ATT", taxTransaction2PK);
						genExportBatchSequenceParents.Add(taxTransaction2PK);
						IncrementBatchNumberExceptFirst();

						//line batch
						var headerPK = helper.InsertTransactionHeader("AP", "INV", uniqueNum + "INV1", 110, postDate, branchPK, departmentPK, postToGL: false, companyPK: companyPK);
						var linePK = helper.InsertTransactionLine(headerPK, null, null, glAccount1, branchPK, departmentPK, null, 100, "CST", postDate, null, 10, 1, "A", companyPK: companyPK);
						helper.InsertGenExportBatchSequence("APS", batchNumber, "AL", linePK);
						genExportBatchSequenceParents.Add(linePK);
						IncrementBatchNumberExceptFirst();

						//cashBasisVAT Batch
						var shipmentPK = helper.InsertShipment(uniqueNum + "SHIP1", helper.ToDate("2015-11-09"));
						var shipmentJobPK = helper.InsertJob(uniqueNum + "SHIP1", companyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", helper.ToDate("2015-11-09"));
						var arInvPK = helper.InsertTransactionHeader("AR", "INV", uniqueNum + "INV5", 55, helper.ToDate("2015-11-09"), branchPK, departmentPK, companyPK: companyPK);
						var line2PK = helper.InsertTransactionLine(arInvPK, shipmentJobPK, chargeCodePK, glAccount1, branchPK, departmentPK, org1, 50, "REV", helper.ToDate("2015-11-09"), helper.ToDate("2015-11-09"), 5, 0.5m, "C", companyPK: companyPK);
						var cashBasisVATPK = helper.InsertCashBasisVAT(line2PK, helper.ToDate("2015-11-09"), companyPK);
						helper.InsertGenExportBatchSequence("APS", batchNumber, "YC", cashBasisVATPK);
						genExportBatchSequenceParents.Add(line2PK); //Here We expect the YC_AL_TransactionLine as XB_ParentID

						batchNumber++;
					}

					void IncrementBatchNumberExceptFirst()
					{
						if (batchNumber > 0)
						{
							batchNumber++;
						}
					}
				}

				return genExportBatchSequenceParents;
			}
		}
	}
}
