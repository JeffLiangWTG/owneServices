using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	internal class ARTransactionsTaxInvoiceRegisterSPTest : ScriptTest
	{
		public void TestARTransactionsTaxInvoiceRegister_ChinaDebtor()
		{
			var chinaDebtor = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "SHAHOM");
			AssertStartsWith("PreCondition, UNLOCO is CN port.", "CN", chinaDebtor.OH_RL_NKClosestPort);

			AssertARTransactionsTaxInvoiceRegister(chinaDebtor);
		}

		public void TestARTransactionsTaxInvoiceRegister_NonChinaDebtor()
		{
			var nonChinaDebtor = TestObjectCreator.CreateOrgHeader("AAABBB", false, true, "DEHAM");
			AssertStartsWith("PreCondition, UNLOCO is not CN port.", "DE", nonChinaDebtor.OH_RL_NKClosestPort);

			AssertARTransactionsTaxInvoiceRegister(nonChinaDebtor);
		}

		void AssertARTransactionsTaxInvoiceRegister(OrgHeader debtor)
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();

			AccTransactionHeader result = TestObjectCreator.InsertTransaction("CRD", "AR", ZDateTime.Now, true, debtor.PK, glAccount.PK);
			result.AH_TransactionReference = "Test Tax Inv 1#";
			result.AH_IsCancelled = false;
			result = TestObjectCreator.InsertTransaction("ADJ", "AR", ZDateTime.Now, true, debtor.PK, glAccount.PK);
			result.AH_IsCancelled = true;
			result = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV1", TestObjectCreator.AUD,
														1M, 10M, 0M, 10M, 0M, debtor, glAccount.PK, ZDateTime.Now, true);
			result.AH_TransactionReference = "Test Tax Inv 3#";

			Factory.Save();

			DataTable resultForInvoiceStatus = RunScript("Y", "", "");
			AssertEquals("Count should be 2", 2, resultForInvoiceStatus.Rows.Count);
			AssertEquals("Count should be 1", 1, resultForInvoiceStatus.Select("TransactionType='CRD'").Length);
			AssertEquals("Count should be 1", 1, resultForInvoiceStatus.Select("TransactionType='INV'").Length);
			AssertEquals("Count should be 0", 0, resultForInvoiceStatus.Select("TransactionType='ADJ'").Length);

			resultForInvoiceStatus = RunScript("N", "", "");
			AssertEquals("Count should be 1", 1, resultForInvoiceStatus.Rows.Count);
			AssertEquals("Count should be 0", 0, resultForInvoiceStatus.Select("TransactionType='CRD'").Length);
			AssertEquals("Count should be 0", 0, resultForInvoiceStatus.Select("TransactionType='INV'").Length);
			AssertEquals("Count should be 1", 1, resultForInvoiceStatus.Select("TransactionType='ADJ'").Length);

			resultForInvoiceStatus = RunScript("Y, N", "", "");
			AssertEquals("Count should be 3", 3, resultForInvoiceStatus.Rows.Count);
			AssertEquals("Count should be 1", 1, resultForInvoiceStatus.Select("TransactionType='CRD'").Length);
			AssertEquals("Count should be 1", 1, resultForInvoiceStatus.Select("TransactionType='INV'").Length);
			AssertEquals("Count should be 1", 1, resultForInvoiceStatus.Select("TransactionType='ADJ'").Length);

			DataTable resultForTransactionTypeList = RunScript("", "", "");
			AssertEquals("Count should be 3", 3, resultForTransactionTypeList.Rows.Count);
			AssertEquals("Count should be 1", 1, resultForTransactionTypeList.Select("TransactionType='CRD'").Length);
			AssertEquals("Count should be 1", 1, resultForTransactionTypeList.Select("TransactionType='INV'").Length);
			AssertEquals("Count should be 1", 1, resultForTransactionTypeList.Select("TransactionType='ADJ'").Length);

			resultForTransactionTypeList = RunScript("", "CRD, ADJ", "");
			AssertEquals("Count should be 2", 2, resultForTransactionTypeList.Rows.Count);
			AssertEquals("Count should be 1", 1, resultForTransactionTypeList.Select("TransactionType='CRD'").Length);
			AssertEquals("Count should be 0", 0, resultForTransactionTypeList.Select("TransactionType='INV'").Length);
			AssertEquals("Count should be 1", 1, resultForTransactionTypeList.Select("TransactionType='ADJ'").Length);

			DataTable resultForCancelled = RunScript("", "", "Y");
			AssertEquals("Count should be 1", 1, resultForCancelled.Rows.Count);
			AssertEquals("Count should be 0", 0, resultForCancelled.Select("TransactionType='CRD'").Length);
			AssertEquals("Count should be 0", 0, resultForCancelled.Select("TransactionType='INV'").Length);
			AssertEquals("Count should be 1", 1, resultForCancelled.Select("TransactionType='ADJ'").Length);

			resultForCancelled = RunScript("", "", "N");
			AssertEquals("Count should be 2", 2, resultForCancelled.Rows.Count);
			AssertEquals("Count should be 1", 1, resultForCancelled.Select("TransactionType='CRD'").Length);
			AssertEquals("Count should be 1", 1, resultForCancelled.Select("TransactionType='INV'").Length);
			AssertEquals("Count should be 0", 0, resultForCancelled.Select("TransactionType='ADJ'").Length);

			resultForCancelled = RunScript("", "", "Y, N");
			AssertEquals("Count should be 3", 3, resultForCancelled.Rows.Count);
			AssertEquals("Count should be 1", 1, resultForCancelled.Select("TransactionType='CRD'").Length);
			AssertEquals("Count should be 1", 1, resultForCancelled.Select("TransactionType='INV'").Length);
			AssertEquals("Count should be 1", 1, resultForCancelled.Select("TransactionType='ADJ'").Length);
		}

		public void TestTransactionInvoiceLocalAmountWithOtherTaxes()
		{
			OrgHeader sHAHOM = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "SHAHOM");
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("110212", TestObjectCreator.AUD, 1, sHAHOM);
			TestObjectCreator.CreateARInvoiceLineWithJobCharge(arInvoice, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc1", 1000m, TestObjectCreator.GST1.PK);
			AssertEquals(100m, arInvoice.AH_GSTAmount);
			arInvoice.AH_OSTaxAmountOtherTaxes = arInvoice.AH_LocalTaxAmountOtherTaxes = 200M;

			Factory.Save();

			var result = RunScript("", "", "");
			AssertDataTableAllRows("", result, new[] { "InvoiceAmount" }, new object[][] { new object[] { 1300m } });
		}

		public void TestComplianceDocDateAndComplianceSubType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
				OrgHeader sHAHOM = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "SHAHOM");

				AccTransactionHeader result = TestObjectCreator.InsertTransaction("CRD", "AR", ZDateTime.Now, true, sHAHOM.PK, glAccount.PK);
				result.AH_TransactionReference = "Test Tax Inv 1#";
				result.AH_ComplianceDocumentDate = ZDate.Today;
				result.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;
				result.AH_IsCancelled = false;
				result = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, sHAHOM, glAccount.PK, ZDateTime.Now, true);
				Factory.Save();
				DataTable resultForInvoiceStatus = RunScript("Y", "", "");

				AssertEquals(1, resultForInvoiceStatus.Rows.Count);
				AssertEquals(ZDate.Today, new ZDate(resultForInvoiceStatus.Rows[0]["ComplianceDocDate"]));
				AssertEquals(ChinaComplianceInfo.ComplianceSubTypeCodes.TXA, resultForInvoiceStatus.Rows[0]["ComplianceSubType"].ToString());
			}
		}

		DataTable RunScript(ZString invoiceStatus, ZString transactionTypeList, ZString isCancelled)
		{
			string sql = string.Format(@"
							EXEC ARTransactionsTaxInvoiceRegisterSP 
							'{0}',	--		@Company uniqueidentifier, 
							NULL,	--		@BranchList varchar(8000), 
							NULL,	--		@OrgGroupList varchar(8000),
							'{1}',	--		@InvoiceStatus char(1), 
							'{2}',	--		@TransactionTypeList varchar(9),
							NULL,	--		@PostDateFrom smalldatetime, 
							NULL,	--		@PostDateTo smalldatetime, 
							NULL,	--		@TransactionDateFrom smalldatetime, 
							NULL,	--		@TransactionDateTo smalldatetime, 
							'{3}'	--		@Cancelled  char(1)",
						GlbCompany.CurrentCompany.PK,
						invoiceStatus,
						transactionTypeList,
						isCancelled
						);

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}
