

using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Analysis_TurnoverByDebtorPeriodAnalysisPreYearTest : ScriptTest
	{
		public void TestResultDoesNotDuplicateForMultipleAPRelatedParties()
		{
			OrgRelatedParty relatedParty1 = Factory.New<OrgRelatedParty>();
			relatedParty1.PR_GC = GlbCompany.CurrentCompany.PK;
			relatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			relatedParty1.PR_FreightDirection = RelatedPartyDirectionList.Codes.AP;
			relatedParty1.PR_OH_RelatedParty = TestObjectCreator.AALSHI.PK;
			relatedParty1.PR_OH_Parent = TestObjectCreator.ABIGAS.PK;
			relatedParty1.PR_Location = "1";

			OrgRelatedParty relatedParty2 = Factory.New<OrgRelatedParty>();
			relatedParty2.PR_GC = GlbCompany.CurrentCompany.PK;
			relatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			relatedParty2.PR_FreightDirection = RelatedPartyDirectionList.Codes.AP;
			relatedParty2.PR_OH_RelatedParty = TestObjectCreator.AALSHI.PK;
			relatedParty2.PR_OH_Parent = TestObjectCreator.ABIGAS.PK;
			relatedParty1.PR_Location = "2";
			Factory.Save();

			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.ABIGAS.ARSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.LocalClient.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.LocalClient2.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.ABIGAS, glAccount.PK);
			Factory.Save();

			DataTable resultForSettlementGroup = RunScript(LedgerTypes.AccountsReceivable);

			AssertEquals("Result should have only one record", 1, resultForSettlementGroup.Rows.Count);
		}

		public void TestResultDoesNotDuplicateForMultipleARRelatedParties()
		{
			OrgRelatedParty relatedParty1 = Factory.New<OrgRelatedParty>();
			relatedParty1.PR_GC = GlbCompany.CurrentCompany.PK;
			relatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.ARSettlementGroup;
			relatedParty1.PR_FreightDirection = RelatedPartyDirectionList.Codes.AP;
			relatedParty1.PR_OH_RelatedParty = TestObjectCreator.AALSHI.PK;
			relatedParty1.PR_OH_Parent = TestObjectCreator.ABIGAS.PK;
			relatedParty1.PR_Location = "1";

			OrgRelatedParty relatedParty2 = Factory.New<OrgRelatedParty>();
			relatedParty2.PR_GC = GlbCompany.CurrentCompany.PK;
			relatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.ARSettlementGroup;
			relatedParty2.PR_FreightDirection = RelatedPartyDirectionList.Codes.AP;
			relatedParty2.PR_OH_RelatedParty = TestObjectCreator.AALSHI.PK;
			relatedParty2.PR_OH_Parent = TestObjectCreator.ABIGAS.PK;
			relatedParty1.PR_Location = "2";
			Factory.Save();

			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.ABIGAS.ARSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.LocalClient.APSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.LocalClient2.APSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.ABIGAS, glAccount.PK);
			Factory.Save();

			DataTable resultForSettlementGroup = RunScript(LedgerTypes.AccountsPayable);

			AssertEquals("Result should have only one record", 1, resultForSettlementGroup.Rows.Count);
		}

		public void TestARSettlementGroup()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.ABIGAS.ARSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.LocalClient.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.LocalClient2.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.ABIGAS, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.LocalClient, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV3", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.LocalClient2, glAccount.PK);
			Factory.Save();

			DataTable resultForSettlementGroup = RunScript(LedgerTypes.AccountsReceivable);

			AssertEquals("Result for SettlementGroup list", 3, resultForSettlementGroup.Rows.Count);
			AssertEquals("All result has ABIGAS as settlemet code", 3, resultForSettlementGroup.Select("SettlementCode = 'ABIGAS'").Length);
		}

		public void TestAPSettlementGroup()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.AALSHI.APSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.Creditor1.APSettlementGroupPK = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.Creditor2.APSettlementGroupPK = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.AALSHI, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.Creditor1, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV3", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.Creditor2, glAccount.PK);
			Factory.Save();

			DataTable resultForSettlementGroup = RunScript(LedgerTypes.AccountsPayable);

			AssertEquals("Result for SettlementGroup list", 3, resultForSettlementGroup.Rows.Count);
			AssertEquals("All result has AALSHI as settlemet code", 3, resultForSettlementGroup.Select("SettlementCreditorCode = 'AALSHI'").Length);
		}

		DataTable RunScript(string ledger)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
SELECT * 
FROM Analysis_TurnoverByDebtorPeriodAnalysisPreYear(
'{0}',	--@Company
'',		--@BranchPKList			     
'',		--@SalesRepList			    
'',		--@SalesRepRoll			    
'INV',	--@TransactionTypeList	    
'',		--@ExcludeGSV				
'{1}',	--@P1StDate				
'{2}',	--@P1EdDate				
NULL,	--@P2StDate				
NULL,	--@P2EdDate				
NULL,	--@P3StDate				
NULL,	--@P3EdDate				
NULL,	--@P4StDate				
NULL,	--@P4EdDate				
NULL,	--@P5StDate				
NULL,	--@P5EdDate				
NULL,	--@P6StDate				
NULL,	--@P6EdDate				
NULL,	--@P7StDate				
NULL,	--@P7EdDate				
NULL,	--@P8StDate				
NULL,	--@P8EdDate				
NULL,	--@P9StDate				
NULL,	--@P9EdDate				
NULL,	--@P10StDate				
NULL,	--@P10EdDate				
NULL,	--@P11StDate				
NULL,	--@P11EdDate				
NULL,	--@CurrentPerStDate		
NULL,	--@CurrentPerEdDate		
'{3}'	--@LedgerType				
)  
",
			GlbCompany.CurrentCompany.PK,
			ZDateTime.Today.AddDays(-1).ToISO8601String(),
			ZDateTime.Today.AddDays(1).ToISO8601String(),
			ledger
			));
		}
	}
}


