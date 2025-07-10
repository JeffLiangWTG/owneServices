using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.CA;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.CA
{
	[TestedType(typeof(csfn_DutiesOrTaxes))]
	class csfn_DutiesOrTaxesTest : DbCreateScriptTest
	{
		public void TestDutiesOrTaxes()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "CA", "CAD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "CABLO");

			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1);
			var invoicePK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1);
			var invoiceLinePK = TestDataCreator.CreateJobComInvoiceLine(invoicePK, 1);
			TestDataCreator.CreateCusAddInfo("CDT", "Amount=10*ExemptCode=10*TaxType=SIM", "JI", invoiceLinePK);
			TestDataCreator.CreateCusAddInfo("CDT", "Amount=65*ExemptCode=85*Rate=13.00000*RateType=V*TaxType=EXS", "JI", invoiceLinePK);
			TestDataCreator.CreateCusAddInfo("CDT", "Amount=4.45*ExemptCode=99*Override=Y*Rate=0.12350*RateType=S*TaxType=GST*UnitOfMeasure=NMB", "JI", invoiceLinePK);
			TestDataCreator.CreateCusAddInfo("CDT", "Amount=13*Rate=13*RateType=V*TaxType=DTY*UnitOfMeasure=CMT", "JI", invoiceLinePK);
			TestDataCreator.CreateCusAddInfo("CDT", "Amount=55*Rate=5*RateType=S*TaxType=DTY*UnitOfMeasure=CLT", "JI", invoiceLinePK);
			AssertDutiesOrTaxes("SIM", invoiceLinePK, "10", "", 10m);
			AssertDutiesOrTaxes("EXS", invoiceLinePK, "85", "13.00000V", 65m);
			AssertDutiesOrTaxes("GST", invoiceLinePK, "99", "0.12350S", 4.45m);
			AssertDutiesOrTaxes("DTY", invoiceLinePK, DBNull.Value, "13V 5S", 68m);
		}

		void AssertDutiesOrTaxes(string taxType, Guid parentID, object exemptCode, object rateDescription, object amount)
		{
			var sql = @"select ExemptCode, RateDescription, Amount from dbo.csfn_DutiesOrTaxes(@TaxType, @ParentID)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@TaxType", SqlDbType.VarChar, taxType);
				command.AddParameter("@ParentID", SqlDbType.UniqueIdentifier, parentID);
				using (var reader = command.ExecuteReader())
				{
					Assert(reader.Read());
					CombineAssertions(() =>
					{
						AssertEquals("ExemptCode", exemptCode, reader["ExemptCode"]);
						AssertEquals("RateDescription", rateDescription, reader["RateDescription"]);
						AssertEquals("Amount", amount, reader["Amount"]);
					});
				}
			}
		}
	}
}

