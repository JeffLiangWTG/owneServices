using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USOrganizationsForMID))]
	class USOrganizationsForMIDTest : DbCreateScriptTest
	{
		public void TestSystemCreateTimeUtc()
		{
			var supplierOrgPK = TestDataCreator.CreateOrganisation("Supplier", "Test Supplier", "USPHL");
			var supplierAddressPK = TestDataCreator.CreateAddress(supplierOrgPK, "Supplier Address", "Address 1");

			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var utcNow = DateTime.UtcNow;

			var clusterKey1 = 1;
			var declarationPk1 = CreateJobDeclaration(branchPK, companyPK, "Test001", clusterKey1, createTime: utcNow, supplierAddressPK);
			var invoiceHeader1 = TestDataCreator.CreateJobComInvoiceHeader(declarationPk1, false, clusterKey1);
			var invoiceLine1 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader1, clusterKey1);

			var clusterKey2 = 2;
			var declarationPk2 = CreateJobDeclaration(branchPK, companyPK, "Test002", clusterKey2, createTime: utcNow.AddMinutes(2), supplierAddressPK);
			var invoiceHeader2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPk2, false, clusterKey2);
			var invoiceLine2 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader2, clusterKey2);

			var clusterKey3 = 3;
			var declarationPk3 = CreateJobDeclaration(branchPK, companyPK, "Test003", clusterKey3, createTime: utcNow.AddMinutes(-2), supplierAddressPK);
			var invoiceHeader3 = TestDataCreator.CreateJobComInvoiceHeader(declarationPk3, false, clusterKey3);
			var invoiceLine3 = TestDataCreator.CreateJobComInvoiceLine(invoiceHeader3, clusterKey3);

			var reportSql = @"SELECT Usage FROM USOrganizationsForMID(@companyPK, @dateFrom, @dateTo, null, null, null)";
			using (var command = Db.Connection.Command(reportSql))
			{
				var retList = new List<string>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@dateFrom", SqlDbType.SmallDateTime, utcNow);
				command.AddParameter("@dateTo", SqlDbType.SmallDateTime, utcNow.AddMinutes(1));

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						retList.Add((string)reader["Usage"]);
					}
				}
				AssertEquals(1, retList.Count);
				AssertEquals("ENS", retList[0]);
			}
		}

		Guid CreateJobDeclaration(Guid branchPK, Guid companyPK, string decReference, int clusterKey, DateTime createTime, Guid declarantAddressPK)
		{
			var declarationPK = Guid.NewGuid();
			var declarationSql = @"INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_ClusterKey, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser, JE_OA_DeclarantAddress)
VALUES (@declarationPK, 'US', 'IMP', @branchPK, @companyPK, @decReference, 'ACE', 0, @clusterKey, @createTime, '~BP', GetUtcDate(), '~BP', @declarantAddressPK)";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@decReference", SqlDbType.VarChar, decReference);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@createTime", SqlDbType.SmallDateTime, createTime);
				command.AddParameter("@declarantAddressPK", SqlDbType.UniqueIdentifier, declarantAddressPK);
				command.ExecuteNonQuery();
			}
			return declarationPK;
		}
	}
}
