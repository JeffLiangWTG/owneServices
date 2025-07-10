using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;
using PermitRuleCodeList = Enterprise.Customs.EU.Business.PermitRuleCodeList;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(CusGuaranteeHeader))]
	public class CusGuaranteeHeaderTest : CusGuaranteeHeaderAbstractTest
	{
		public void TestConfirmedCusGuaranteeLineTransactions()
		{
			AddCusGuaranteeLineTransaction(guaranteeHeader, PermitTransactionStatusList.Codes.Confirmed);
			AddCusGuaranteeLineTransaction(guaranteeHeader, PermitTransactionStatusList.Codes.Pending);
			AddCusGuaranteeLineTransaction(guaranteeHeader, PermitTransactionStatusList.Codes.Confirmed);
			AddCusGuaranteeLineTransaction(guaranteeHeader, PermitTransactionStatusList.Codes.Deleted);

			var confirmedCusGuaranteeLineTransactions = guaranteeHeader.ConfirmedCusGuaranteeLineTransactions;

			AssertEquals("ConfirmedCusGuaranteeLineTransactions Only has Confirmed Transactions", 0, confirmedCusGuaranteeLineTransactions.Count(x => x.CPL_TransactionStatus != PermitTransactionStatusList.Codes.Confirmed));
		}

		public void TestDefaultPW_BondFiledPortForGuarantee()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not define Office in rule with code CUS", string.Empty, guaranteeHeader.DefaultPW_BondFiledPortForGuarantee);
				AddCusGuaranteeRule(PermitRuleCodeList.Codes.CUS, "1Office");
				AssertEquals("The first and unique Office in rule with code CUS", "1Office", guaranteeHeader.DefaultPW_BondFiledPortForGuarantee);
				AddCusGuaranteeRule(PermitRuleCodeList.Codes.CUS, "2Office");
				AssertEquals("The first Office in rule with code CUS", "1Office", guaranteeHeader.DefaultPW_BondFiledPortForGuarantee);
			});
		}

		public void TestGetPendingDebtTransactions()
		{
			var transaction1 = AddCusGuaranteeLineTransaction(guaranteeHeader, "CON", transactionType: "ADJ", reference: "ref1", 1);

			var transaction2 = AddCusGuaranteeLineTransaction(guaranteeHeader, "CON", transactionType: "ADJ", reference: "ref2", -2);
			var transaction3 = AddCusGuaranteeLineTransaction(guaranteeHeader, "CON", transactionType: "ADJ", reference: "ref2", 2);

			var transaction4 = AddCusGuaranteeLineTransaction(guaranteeHeader, "CON", transactionType: "ADJ", reference: "ref3", 3);
			var transaction5 = AddCusGuaranteeLineTransaction(guaranteeHeader, "CON", transactionType: "ADJ", reference: "ref3", -5);

			var transaction6 = AddCusGuaranteeLineTransaction(guaranteeHeader, "DEL", transactionType: "ADJ", reference: "ref4", 3);
			var transaction7 = AddCusGuaranteeLineTransaction(guaranteeHeader, "DEL", transactionType: "ADJ", reference: "ref4", -5);

			var transaction8 = AddCusGuaranteeLineTransaction(guaranteeHeader, "PND", transactionType: "TRA", reference: "ref5", 3);
			var transaction9 = AddCusGuaranteeLineTransaction(guaranteeHeader, "PND", transactionType: "TRA", reference: "ref5", -5);

			var transaction10 = AddCusGuaranteeLineTransaction(guaranteeHeader, "CON", transactionType: "OBA", reference: "ref6", 3);
			var transaction11 = AddCusGuaranteeLineTransaction(guaranteeHeader, "CON", transactionType: "OBA", reference: "ref6", -5);

			var transaction12 = AddCusGuaranteeLineTransaction(guaranteeHeader, "CON", transactionType: "CUS", reference: "ref7", -10);

			var guaranteeHeader2 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader2.CPH_Number = "other";
			var transaction13 = AddCusGuaranteeLineTransaction(guaranteeHeader2, "CON", transactionType: "CUS", reference: "ref7", 20);

			Factory.Save();

			var transactions1 = new CusGuaranteeLineTransactionCollection(guaranteeHeader);
			var transactions2 = new CusGuaranteeLineTransactionCollection(guaranteeHeader2);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] Total CusGuarantee Transactions 1", 12, transactions1.Count);
				AssertEquals("[PRECONDITION] Total CusGuarantee Transactions 2", 1, transactions2.Count);

				transactions1 = guaranteeHeader.GetPendingDebtTransactions();
				AssertEquals("Total Transaction (for first guarantee)", 5, transactions1.Count);
				AssertEquals("Transaction 1 is not in the list for first guarantee (no debt)", false, transactions1.Contains(transaction1));

				AssertEquals("Transaction 2 is not in the list for first guarantee (no debt)", false, transactions1.Contains(transaction2));
				AssertEquals("Transaction 3 is not in the list for first guarantee (no debt)", false, transactions1.Contains(transaction3));

				AssertEquals("Transaction 4 is in the list when for first guarantee (with debt and correct status and type)", true, transactions1.Contains(transaction4));
				AssertEquals("Transaction 5 is in the list when for first guarantee (with debt and correct status and type)", true, transactions1.Contains(transaction5));

				AssertEquals("Transaction 6 is not in the list when for first guarantee (with debt and correct type but wrong status)", false, transactions1.Contains(transaction6));
				AssertEquals("Transaction 7 is not in the list when for first guarantee (with debt and correct type but wrong status)", false, transactions1.Contains(transaction7));

				AssertEquals("Transaction 8 is in the list when for first guarantee (with debt and correct status and type)", true, transactions1.Contains(transaction8));
				AssertEquals("Transaction 9 is in the list when for first guarantee (with debt and correct status and type)", true, transactions1.Contains(transaction9));

				AssertEquals("Transaction 10 is not in the list when for first guarantee (with debt and correct status but wrong type)", false, transactions1.Contains(transaction10));
				AssertEquals("Transaction 11 is not in the list when for first guarantee (with debt and correct status but wrong type)", false, transactions1.Contains(transaction11));

				AssertEquals("Transaction 12 is in the list when for first guarantee (with debt and correct status and type)", true, transactions1.Contains(transaction12));
				AssertEquals("Transaction 13 is not in the list for first guarantee because it is not associated to the correct guarantee", false, transactions1.Contains(transaction13));

				transactions2 = guaranteeHeader2.GetPendingDebtTransactions();
				AssertEquals("Total Transaction (for second guarantee)", 0, transactions2.Count);
				AssertEquals("Transaction 12 is not in the list for second guarantee because it is not associated to the correct guarantee", false, transactions2.Contains(transaction12));
				AssertEquals("Transaction 13 is not in the list for second guarantee (no debt)", false, transactions2.Contains(transaction13));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
		}
		CusGuaranteeHeader guaranteeHeader;

		CusGuaranteeLineTransaction AddCusGuaranteeLineTransaction(CusGuaranteeHeader guarantee, string transactionStatus, string transactionType = PermitTransactionTypeList.Codes.ADJ, string reference = "", decimal tranValue = 0)
		{
			var transaction = guarantee.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_TransactionType = transactionType;
			transaction.CPL_TransactionStatus = transactionStatus;
			transaction.CPL_Reference = reference;
			transaction.CPL_TranValue = tranValue;
			return transaction;
		}

		CusGuaranteeRule AddCusGuaranteeRule(string code, string valueFrom)
		{
			var rule = guaranteeHeader.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = code;
			rule.CPR_ValueFrom = valueFrom;
			return rule;
		}
	}
}
