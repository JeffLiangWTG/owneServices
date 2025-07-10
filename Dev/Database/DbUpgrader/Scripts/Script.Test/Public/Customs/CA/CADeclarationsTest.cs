using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.CA;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.CA
{
	[TestedType(typeof(CADeclarations))]
	class CADeclarationsTest : DbCreateScriptTest
	{
		public void TestContainersCount()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "CA", "CAD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "CABLO");
			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA");
			TestDataCreator.CreateCusContainer("CNT1", declarationPK, 1, "CA");
			TestDataCreator.CreateCusContainer("CNT2", declarationPK, 1, "CA");

			var containersCount = 0;
			Db.Connection.ExecuteReader($@"SELECT ContainersCount FROM CADeclarations('{companyPK}', '')", reader =>
			{
				containersCount = reader.GetInt32(0);
			});

			AssertEquals(2, containersCount);
		}

		public void TestLocalTransportData()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "CA", "CAD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "CABLO");
			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA");
			var jobDocAndCartagePk = TestDataCreator.CreateJobDocsAndCartage(declarationPK, "JE");
			var organisationPK = TestDataCreator.CreateOrganisation("ORG_1", "Org One");
			var addressPK = TestDataCreator.CreateAddress(organisationPK, "Head Office", "Somewhere");
			Db.Connection.ExecuteNonQuery($@"
UPDATE dbo.JobDocsAndCartage
SET
	JP_DeliveryCartageAdvised = '2022-01-01 01:08:00',
	JP_DeliveryCartageCompleted = '2022-02-02 02:08:00',
	JP_OA_DeliveryCartageCoAddr = '{addressPK}',
	JP_SystemLastEditTimeUtc = GETUTCDATE(),
	JP_SystemLastEditUser = '~BP'
WHERE
	JP_PK ='{jobDocAndCartagePk}'");

			var localTranspIssued = DateTime.MinValue;
			var cartageCompleted = DateTime.MinValue;
			var localTranspCompanyName = string.Empty;
			Db.Connection.ExecuteReader($@"SELECT LocalTranspIssued, CartageCompleted, LocalTranspCompanyName FROM CADeclarations('{companyPK}', '')", reader =>
			{
				localTranspIssued = reader.GetDateTime(0);
				cartageCompleted = reader.GetDateTime(1);
				localTranspCompanyName = reader.GetString(2);
			});

			CombineAssertions(() =>
			{
				AssertEquals("LocalTranspIssued", new DateTime(2022, 1, 1, 1, 8, 0), localTranspIssued);
				AssertEquals("CartageCompleted", new DateTime(2022, 2, 2, 2, 8, 0), cartageCompleted);
				AssertEquals("LocalTranspCompanyName", "Org One", localTranspCompanyName);
			});
		}

		public void TestBilledAmount()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "CA", "CAD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "CABLO");
			var departmentPK = TestDataCreator.CreateOrExistingDepartment("TC1");

			var accTaxRate = TestDataCreator.CreateAccTaxRate("GST", "Standard Rated", "RAT", "CA");

			var disbursementCharge = TestDataCreator.CreateAccCharges(companyPK, "CUSDSB", "Customs Disbursement Charges-GST", "DSB");

			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "CA");
			var jobHeaderPK = TestDataCreator.CreateJobHeader(branchPK, companyPK, declarationPK, departmentPK, "JE", "B001", "00112233", "WRK");
			var accTransHeaderPK = TestDataCreator.CreateAccTransactionHeader(branchPK, companyPK, departmentPK, jobHeaderPK, "AR", "INV", "B055", 20.0m, 3.25m, 28.25m, 1.0m, 28.25m);
			TestDataCreator.CreateAccTransactionLine(branchPK, companyPK, jobHeaderPK, accTransHeaderPK, accTaxRate, departmentPK, 1, "Customs Disbursement Charges", "REV", 4.0m, chargePK: disbursementCharge);
			TestDataCreator.CreateAccTransactionLine(branchPK, companyPK, jobHeaderPK, accTransHeaderPK, accTaxRate, departmentPK, 2, "Customs Disbursement Charges", "REV", 5.0m, chargePK: disbursementCharge);
			TestDataCreator.CreateAccTransactionLine(branchPK, companyPK, jobHeaderPK, accTransHeaderPK, accTaxRate, departmentPK, 3, "Customs Disbursement Charges", "REV", 5.0m);

			var auditUserName = decimal.Zero;
			Db.Connection.ExecuteReader($@"SELECT BilledAmount FROM CADeclarations('{companyPK}', '')", reader =>
			{
				auditUserName = reader.GetDecimal(0);
			});
			AssertEquals(9m, auditUserName);
		}

		public void TestTotalDuty()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "CA", "CAD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "CABLO");
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "CA");
			var entryPK1 = TestDataCreator.CreateCusEntryHeader(declarationPK1, 1, "B3C");
			var cusEntryLinePK1 = TestDataCreator.CreateCusEntryLine(entryPK1, 1);
			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK1, "DTY", 1.1f, 1);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "CA");
			var entryPK2 = TestDataCreator.CreateCusEntryHeader(declarationPK2, 2, "CAD");
			var cusEntryLinePK2 = TestDataCreator.CreateCusEntryLine(entryPK2, 2);
			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK2, "DTY", 2.1f, 2);

			var declarationPK3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B003", "IMP", 3, dataModel: "CA");
			var entryPK3 = TestDataCreator.CreateCusEntryHeader(declarationPK3, 3, "CAD");
			var cusEntryLinePK3 = TestDataCreator.CreateCusEntryLine(entryPK3, 3);
			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK3, "GST", 3.1f, 3);

			var declarationPK4 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B004", "IMP", 4, dataModel: "CA");
			var entryPK4 = TestDataCreator.CreateCusEntryHeader(declarationPK4, 4, "CAD");
			var cusEntryLinePK4 = TestDataCreator.CreateCusEntryLine(entryPK4, 4);
			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK4, "DTY", 2.1f, 4, source: "CUS");

			var declarationPK5 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B005", "IMP", 5, dataModel: "CA");
			var entryPK5 = TestDataCreator.CreateCusEntryHeader(declarationPK5, 5, "CAD");
			var cusEntryLinePK5 = TestDataCreator.CreateCusEntryLine(entryPK5, 5);
			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK5, "DTY", 5.1f, 5);
			TestDataCreator.CreateCusEntryLineFee(cusEntryLinePK5, "DTY", 5.2f, 5);

			var actualTotalDuties = new List<decimal>();
			var expectedTotalDuties = new decimal[] { 10.3m, 0.0m, 0.0m, 2.1m, 1.1m };
			Db.Connection.ExecuteReader($@"SELECT TotalDuty, JE_ClusterKey FROM CADeclarations('{companyPK}', '') ORDER BY JE_ClusterKey DESC", reader =>
			{
				actualTotalDuties.Add(reader.GetDecimal(0));
			});
			AssertArrayEqualsByElements(expectedTotalDuties, actualTotalDuties.ToArray());
		}

		public void TestCADEntryHeaderCharges()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "CA", "CAD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "CABLO");
			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "CA");
			var entryPK = TestDataCreator.CreateCusEntryHeader(declarationPK, 1, "CAD");

			TestDataCreator.CreateCusEntryHeaderCharges(entryPK, "CUD", 2.2m, 1, "CUS");
			TestDataCreator.CreateCusEntryHeaderCharges(entryPK, "CUD", 3.2m, 1, "CW1");
			AssertCADEntryHeaderCharges("CADTotalDuty", 2.2m);

			TestDataCreator.CreateCusEntryHeaderCharges(entryPK, "ADD", 1.5m, 1, "CUS");
			TestDataCreator.CreateCusEntryHeaderCharges(entryPK, "CVD", 1.5m, 1, "CUS");
			TestDataCreator.CreateCusEntryHeaderCharges(entryPK, "SUR", 1.5m, 1, "CUS");
			TestDataCreator.CreateCusEntryHeaderCharges(entryPK, "ADD", 1.7m, 1, "CW1");
			TestDataCreator.CreateCusEntryHeaderCharges(entryPK, "CVD", 1.2m, 1, "CW1");
			TestDataCreator.CreateCusEntryHeaderCharges(entryPK, "SUR", 1.4m, 1, "CW1");
			AssertCADEntryHeaderCharges("CADTotalSIMA", 4.5m);

			TestDataCreator.CreateCusEntryHeaderCharges(entryPK, "GST", 1.2m, 1, "CUS");
			TestDataCreator.CreateCusEntryHeaderCharges(entryPK, "GST", 1.2m, 1, "CUS");
			TestDataCreator.CreateCusEntryHeaderCharges(entryPK, "GST", 3.2m, 1, "CW1");
			TestDataCreator.CreateCusEntryHeaderCharges(entryPK, "GST", 5.2m, 1, "CW1");
			AssertCADEntryHeaderCharges("CADTotalGST", 2.4m);

			TestDataCreator.CreateCusEntryHeaderCharges(entryPK, "FET", 1.3m, 1, "CUS");
			TestDataCreator.CreateCusEntryHeaderCharges(entryPK, "FET", 8.3m, 1, "CW1");
			AssertCADEntryHeaderCharges("CADTotalEXS", 1.3m);

			TestDataCreator.CreateCusEntryHeaderCharges(entryPK, "TOT", 5.5m, 1, "CUS");
			TestDataCreator.CreateCusEntryHeaderCharges(entryPK, "TOT", 15.5m, 1, "CW1");
			AssertCADEntryHeaderCharges("CADTotalEnteredValue", 5.5m);
			AssertCADEntryHeaderCharges("CADTotalDutiesAndTaxes", 10.4m);

			void AssertCADEntryHeaderCharges(string column, decimal value)
			{
				var result = decimal.Zero;
				Db.Connection.ExecuteReader($@"SELECT {column} FROM CADeclarations('{companyPK}', '')", reader =>
				{
					result = reader.GetDecimal(0);
				});
				AssertEquals(value, result);
			}
		}

		public void TestPreCarm()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "CA", "CAD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "CABLO");
			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "CA");
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "CA");
			var isPreCarm = "";
			Db.Connection.ExecuteReader($@"SELECT PreCarm FROM CADeclarations('{companyPK}', '') WHERE JE_PK = '{declarationPK}'", reader =>
			{
				isPreCarm = reader.GetString(0);
			});
			AssertEquals(isPreCarm, "N");

			TestDataCreator.CreateGenPivot(declarationPK1, declarationPK, "PRE");
			Db.Connection.ExecuteReader($@"SELECT PreCarm FROM CADeclarations('{companyPK}', '') WHERE JE_PK = '{declarationPK}'", reader =>
			{
				isPreCarm = reader.GetString(0);
			});
			AssertEquals(isPreCarm, "N");

			var entryPK = TestDataCreator.CreateCusEntryHeader(declarationPK, 1, "B3C");
			TestDataCreator.CreateGenPivot(entryPK, declarationPK, "PRE");
			Db.Connection.ExecuteReader($@"SELECT PreCarm FROM CADeclarations('{companyPK}', '') WHERE JE_PK = '{declarationPK}'", reader =>
			{
				isPreCarm = reader.GetString(0);
			});
			AssertEquals(isPreCarm, "N");

			Db.Connection.ExecuteReader($@"SELECT PreCarm FROM CADeclarations('{companyPK}', '') WHERE JE_PK = '{declarationPK1}'", reader =>
			{
				isPreCarm = reader.GetString(0);
			});
			AssertEquals(isPreCarm, "N");

			TestDataCreator.CreateGenPivot(declarationPK, declarationPK1, "PRE");
			Db.Connection.ExecuteReader($@"SELECT PreCarm FROM CADeclarations('{companyPK}', '') WHERE JE_PK = '{declarationPK1}'", reader =>
			{
				isPreCarm = reader.GetString(0);
			});
			AssertEquals(isPreCarm, "N");

			var declarationPK3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B003", "IMP", 3, dataModel: "CA");
			TestDataCreator.CreateGenPivot(declarationPK3, declarationPK1, "PRE");
			entryPK = TestDataCreator.CreateCusEntryHeader(declarationPK3, 3, "B3C", entryStatus: "CLR");
			Db.Connection.ExecuteReader($@"SELECT PreCarm FROM CADeclarations('{companyPK}', '') WHERE JE_PK = '{declarationPK1}'", reader =>
			{
				isPreCarm = reader.GetString(0);
			});
			AssertEquals(isPreCarm, "Y");

			var declarationPK4 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B004", "IMP", 4, dataModel: "CA");
			TestDataCreator.CreateGenPivot(declarationPK4, declarationPK1, "PRE");
			entryPK = TestDataCreator.CreateCusEntryHeader(declarationPK4, 4, "B3C", entryStatus: "CNF");
			Db.Connection.ExecuteReader($@"SELECT PreCarm FROM CADeclarations('{companyPK}', '') WHERE JE_PK = '{declarationPK1}'", reader =>
			{
				isPreCarm = reader.GetString(0);
			});
			AssertEquals(isPreCarm, "Y");

			var declarationPK5 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B005", "IMP", 5, dataModel: "CA");
			TestDataCreator.CreateGenPivot(declarationPK5, declarationPK1, "PRE");
			entryPK = TestDataCreator.CreateCusEntryHeader(declarationPK5, 5, "B3C", entryStatus: "39");
			Db.Connection.ExecuteReader($@"SELECT PreCarm FROM CADeclarations('{companyPK}', '') WHERE JE_PK = '{declarationPK1}'", reader =>
			{
				isPreCarm = reader.GetString(0);
			});
			AssertEquals(isPreCarm, "Y");
		}

		public void TestImportReportPrintsARInvoicedAmount()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "CA", "CAD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "CABLO");
			var departmentPK = TestDataCreator.CreateOrExistingDepartment("TC1");
			var organisationPK = TestDataCreator.CreateOrganisation("OrgCode X", "Company Name X");
			var addressPK = TestDataCreator.CreateAddress(organisationPK, "Delivery Address", "Address X");
			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B055", "IMP", 1, dataModel: "CA");
			TestDataCreator.CreateDocAddress(addressPK, "", declarationPK, "JE", "IMG");

			var accTaxRate = TestDataCreator.CreateAccTaxRate("GST", "Standard Rated", "RAT", "CA");
			TestDataCreator.CreateAccCharges(companyPK, "CUSDEF", "CUSDEF DESC", "CMT");
			TestDataCreator.CreateAccCharges(companyPK, "AND", "AND DESC", "MRG");

			var jobHeaderPK = TestDataCreator.CreateJobHeader(branchPK, companyPK, declarationPK, departmentPK, "JE", "B055", "00112233", "WRK");
			var accTransHeaderPK = TestDataCreator.CreateAccTransactionHeader(branchPK, companyPK, departmentPK, jobHeaderPK, "AR", "INV", "B055", 20.0m, 3.25m, 28.25m, 1.0m, 28.25m);
			TestDataCreator.CreateAccTransactionLine(branchPK, companyPK, jobHeaderPK, accTransHeaderPK, accTaxRate, departmentPK, 1, "DUE RECEIVER GENERAL (CAD)", "REV", 4.0m, 1.0m, 12.0m, 1.0m, 1);
			TestDataCreator.CreateAccTransactionLine(branchPK, companyPK, jobHeaderPK, accTransHeaderPK, accTaxRate, departmentPK, 2, "AND (CAD)", "REV", 5.0m, 2.6m, 5.65m, 1.0m, 1);

			var expectedInvoicedAmount = 20.0m;
			var expectedOutstandingAmount = 28.25m;
			var expectedBilledAmount = 0m;

			var sql = @"select JE_PK, AllARInvoicesOutstandingAmount, AllARInvoicesInvoicedAmount, BilledAmount from dbo.CADeclarations(@companyPK,'')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var arInvoicesOutstandingAmount = reader.GetDecimal(1);
						var arInvoicesInvoicedAmount = reader.GetDecimal(2);
						var billedAmount = reader.GetDecimal(3);
						AssertEquals(expectedOutstandingAmount, arInvoicesOutstandingAmount);
						AssertEquals(expectedInvoicedAmount, arInvoicesInvoicedAmount);
						AssertEquals(expectedBilledAmount, billedAmount);
					}
				}
			}
		}

		public void TestImporterDeliveryAddressWithCompanyNameOverride()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "CA", "CAD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "CABLO");

			var organisationPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B000", "IMP", 1, dataModel: "CA");
			var addressPK = TestDataCreator.CreateAddress(organisationPK, "SSSS", "Delivery Address", "Address 2", "Vancouver", "BC", "L8K 0A1", "Deliver To Toronto");
			TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", "TRN00001", "REL", "CUS", "CA");
			TestDataCreator.CreateDocAddress(addressPK, "", declarationPK, "JE", "IMG");
			var sql = @"select JE_PK, DeliverToName from dbo.CADeclarations(@companyPK, '')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var addressePKs = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						addressePKs.Add((Guid)reader["JE_PK"], reader.GetString(1));
					}
					AssertEquals("Deliver To Toronto", addressePKs[declarationPK]);
				}
			}
		}

		public void TestImportDeliveryAddress()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "CA", "CAD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "CABLO");

			var shipmentPK1 = TestDataCreator.CreateShipment("S001");
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "S001", "IMP", 1, shipmentPK1, dataModel: "CA");
			TestDataCreator.CreateDocAddress(Guid.Empty, "Company Name 1", shipmentPK1, "JS", "CEG");

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 2, dataModel: "CA");
			TestDataCreator.CreateDocAddress(Guid.Empty, "Company Name 2", declarationPK2, "JE", "IMG");

			var organisationPK3 = TestDataCreator.CreateOrganisation("OrgCode3", "Company Name 3");
			var addressPK3 = TestDataCreator.CreateAddress(organisationPK3, "Delivery Address", "Address 1");
			var declarationPK3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B003", "IMP", 3, dataModel: "CA");
			TestDataCreator.CreateDocAddress(addressPK3, "", declarationPK3, "JE", "IMG");

			var sql = @"select JE_PK, DeliverToName, DeliverToOrgCode from dbo.CADeclarations(@companyPK,'')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var addresses = new Dictionary<Guid, ImportDeliveryAddress>();
					while (reader.Read())
					{
						addresses[reader.GetGuid(0)] = new ImportDeliveryAddress()
						{
							DeliverToName = reader.GetString(1),
							DeliverToOrgCode = reader.GetString(2),
						};
					}
					AssertContainsExactElementsInAnyOrder(new[] { declarationPK1, declarationPK2, declarationPK3 }, addresses.Keys);
					AssertEquals("Company Name 1", addresses[declarationPK1].DeliverToName);
					AssertEquals("", addresses[declarationPK1].DeliverToOrgCode);
					AssertEquals("Company Name 2", addresses[declarationPK2].DeliverToName);
					AssertEquals("", addresses[declarationPK2].DeliverToOrgCode);
					AssertEquals("Company Name 3", addresses[declarationPK3].DeliverToName);
					AssertEquals("OrgCode3", addresses[declarationPK3].DeliverToOrgCode);
				}
			}
		}

		public void TestCADeclarations_IM2AndB3X()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "CA", "CAD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "CABLO");

			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IM2", 1, dataModel: "CA");
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0002", "B3X", 2, dataModel: "CA");

			var reportSql = @"SELECT COUNT(*) FROM CADeclarations(@companyPK, '') WHERE JE_MessageType = @JE_MessageType";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@JE_MessageType", SqlDbType.VarChar, "IM2");

				using (var reader = command.ExecuteReader())
				{
					var count = 0;
					while (reader.Read())
					{
						count = (int)reader.GetValue(0);
					}

					AssertEquals(1, count);
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@JE_MessageType", SqlDbType.VarChar, "B3X");

				using (var reader = command.ExecuteReader())
				{
					var count = 0;
					while (reader.Read())
					{
						count = (int)reader.GetValue(0);
					}

					AssertEquals(1, count);
				}
			}
		}

		public void TestAuditData()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "CA", "CAD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "CABLO");
			TestDataCreator.CreateGlbStaff("HXU", "HXU FULLNAME");

			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA");

			var updateAuditQql = @"
UPDATE dbo.JobDeclaration
SET
	JE_AuditDateUtc = '2021-05-18',
	JE_AuditReference = 'Test Reference',
	JE_GS_NKAuditUser = 'HXU',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_DeclarationReference = 'B0001'";
			using (var command = Db.Connection.Command(updateAuditQql))
			{
				command.ExecuteNonQuery();
			}

			var reportSql = @"SELECT AuditDate,AuditReference,AuditUser,AuditUserName FROM CADeclarations(@companyPK, '')";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					var count = 0;
					while (reader.Read())
					{
						count++;
						AssertEquals(new DateTime(2021, 5, 18), (DateTime)reader["AuditDate"]);
						AssertEquals("Test Reference", reader["AuditReference"].ToString());
						AssertEquals("HXU", reader["AuditUser"].ToString());
						AssertEquals("HXU FULLNAME", reader["AuditUserName"].ToString());
					}

					AssertEquals(1, count);
				}
			}
		}

		public void TestCustomsCommencedFields()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "CA", "CAD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "CABLO");

			var customsCommencedDate1 = new DateTime(2022, 07, 27, 10, 17, 00);
			var customCommencedUser1 = "BLU";
			var customsCommencedDate2 = new DateTime(2022, 07, 28, 10, 22, 00);
			var customCommencedUser2 = "ALU";

			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, customsCommencedDate: customsCommencedDate1, customCommencedUser: customCommencedUser1, dataModel: "CA");
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0002", "IM2", 2, customsCommencedDate: customsCommencedDate2, customCommencedUser: customCommencedUser2, dataModel: "CA");

			var reportSql = @"SELECT CCCDate, CCCUser FROM CADeclarations(@companyPK, '') WHERE JE_MessageType = @JE_MessageType";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@JE_MessageType", SqlDbType.VarChar, "IMP");

				using (var reader = command.ExecuteReader())
				{
					var count = 0;
					while (reader.Read())
					{
						count++;
						AssertEquals(new DateTime(2022, 07, 27, 10, 17, 00), (DateTime)reader["CCCDate"]);
						AssertEquals("BLU", reader["CCCUser"].ToString());
					}

					AssertEquals(1, count);
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@JE_MessageType", SqlDbType.VarChar, "IM2");

				using (var reader = command.ExecuteReader())
				{
					var count = 0;
					while (reader.Read())
					{
						count++;
						AssertEquals(new DateTime(2022, 07, 28, 10, 22, 00), (DateTime)reader["CCCDate"]);
						AssertEquals("ALU", reader["CCCUser"].ToString());
					}

					AssertEquals(1, count);
				}
			}
		}

		public void TestBondInfo()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "CA", "CAD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "CABLO");
			var declaraction = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA", addInfo: "BondType=9*BondNo=12122*SuretyCode=ABC");

			var reportSql = @"SELECT JE_BondType,JE_BondNo,JE_SuretyCode FROM CADeclarations(@companyPK, '') WHERE JE_PK = @JE_PK";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@JE_PK", SqlDbType.UniqueIdentifier, declaraction);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						AssertEquals("9", reader["JE_BondType"].ToString());
						AssertEquals("12122", reader["JE_BondNo"].ToString());
						AssertEquals("ABC", reader["JE_SuretyCode"].ToString());
					}
				}
			}
		}

		class ImportDeliveryAddress
		{
			public string DeliverToName { get; set; }
			public string DeliverToOrgCode { get; set; }
		}
	}
}
