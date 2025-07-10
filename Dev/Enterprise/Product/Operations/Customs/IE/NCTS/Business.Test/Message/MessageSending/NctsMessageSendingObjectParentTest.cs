using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsHeaderMessageSendingObjectParent))]
	sealed class NctsMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new NctsHeaderMessageSendingObjectParent(Factory.New<NctsHeader>());

		public void TestSendIE013_GuaranteeEdit()
		{
			SetupGuaranteeAndSendIE015Message();
			nctsGuarantee.PW_BondAmount += 5000m;
			SendMessage(NCTSOutgoingMessageTypeList.Codes.DeclarationAmendment);
			AssertTransaction(cusGuaranteeHeader, 3, -5000m, -30000m);

			nctsGuarantee.PW_BondAmount -= 8000m;
			SendMessage(NCTSOutgoingMessageTypeList.Codes.DeclarationAmendment);
			AssertTransaction(cusGuaranteeHeader, 4, 8000m, -22000m);
		}

		public void TestSendIE013_GuaranteeRemove()
		{
			SetupGuaranteeAndSendIE015Message();
			nctsHeader.MovementHeader.Guarantees.RemoveAndDeleteAll();
			SendMessage(NCTSOutgoingMessageTypeList.Codes.DeclarationAmendment);
			AssertTransaction(cusGuaranteeHeader, 3, 25000m, 0m);
		}

		public void TestSendIE013_GuaranteeChange()
		{
			SetupGuaranteeAndSendIE015Message();
			var guarantee2 = NctsTestDataProvider.CreateCusGuaranteeHeader(Factory, "2222", nctsHeader.Principal.OrganisationPK);
			nctsGuarantee.PW_CPH_Guarantee = guarantee2.PK;
			nctsGuarantee.PW_BondNumber = guarantee2.CPH_Number;
			SendMessage(NCTSOutgoingMessageTypeList.Codes.DeclarationAmendment);

			AssertTransaction(cusGuaranteeHeader, 3, 25000m, 0m);
			AssertTransaction(guarantee2, 2, -25000m, -25000m);

			var guarantee3 = NctsTestDataProvider.CreateCusGuaranteeHeader(Factory, "3333", nctsHeader.Principal.OrganisationPK);
			nctsGuarantee.PW_CPH_Guarantee = guarantee3.PK;
			nctsGuarantee.PW_BondNumber = guarantee3.CPH_Number;
			SendMessage(NCTSOutgoingMessageTypeList.Codes.DeclarationAmendment);

			var transactions = cusGuaranteeHeader.GetTransactions().ToArray();
			AssertEquals("Original Guarantee Transaction count should still be 3", 3, transactions.Length);
			AssertTransaction(guarantee2, 3, 25000m, 0m);
			AssertTransaction(guarantee3, 2, -25000m, -25000m);
		}

		public void TestSendIE014_Guarantee()
		{
			SetupGuaranteeAndSendIE015Message();
			SendMessage(NCTSOutgoingMessageTypeList.Codes.DeclarationInvalidationRequest);
			AssertTransaction(cusGuaranteeHeader, 3, 25000m, 0m);
		}

		public void TestSendIE014_GuaranteeEdit()
		{
			SetupGuaranteeAndSendIE015Message();
			nctsGuarantee.PW_BondAmount += 5000m;
			SendMessage(NCTSOutgoingMessageTypeList.Codes.DeclarationInvalidationRequest);
			AssertTransaction(cusGuaranteeHeader, 3, 25000m, 0m);
		}

		public void TestSendIE014_GuaranteeChange()
		{
			SetupGuaranteeAndSendIE015Message();
			var guarantee2 = NctsTestDataProvider.CreateCusGuaranteeHeader(Factory, "2222", nctsHeader.Principal.OrganisationPK);
			nctsGuarantee.PW_CPH_Guarantee = guarantee2.PK;
			nctsGuarantee.PW_BondNumber = guarantee2.CPH_Number;
			SendMessage(NCTSOutgoingMessageTypeList.Codes.DeclarationInvalidationRequest);

			AssertTransaction(cusGuaranteeHeader, 3, 25000m, 0m);
			var transactions2 = guarantee2.GetTransactions().ToArray();
			AssertEquals("Transaction count should be 1", 1, transactions2.Length);
		}

		public void TestSendIE015_Guarantee()
		{
			SetupGuaranteeAndSendIE015Message();
			AssertTransaction(cusGuaranteeHeader, 2, -25000m, -25000m);
		}

		void SetupGuaranteeAndSendIE015Message()
		{
			(cusGuaranteeHeader, nctsGuarantee) = NctsTestDataProvider.CreateNctsGuaranteeWithCusGuaranteeHeader(Factory);
			SendMessage(NCTSOutgoingMessageTypeList.Codes.DeclarationData);
		}

		void SendMessage(ZString messageType)
		{
			var sendingObjectParent = new NctsHeaderMessageSendingObjectParent(nctsHeader);
			sendingObjectParent.SendingObjectsCollection[0].MessageType = messageType;
			sendingObjectParent.SendAndSaveMessages();
		}

		void AssertTransaction(CusGuaranteeHeader guarantee, int expectedCount, ZDecimal expectedValue, ZDecimal expectedPendingBalance)
		{
			var transactions = guarantee.GetTransactions().ToArray();
			AssertEquals($"Transaction count should be {expectedCount}", expectedCount, transactions.Length);
			var newTransaction = transactions[expectedCount - 1];
			CombineAssertions(() =>
			{
				AssertEquals("Status", "PND", newTransaction.CPL_TransactionStatus);
				AssertEquals("Type", "TRA", newTransaction.CPL_TransactionType);
				AssertEquals("Reference", nctsHeader.MovementHeader.BM_PaperlessInbondNum, newTransaction.CPL_Reference);
				AssertEquals("Transaction value", expectedValue, newTransaction.CPL_TranValue);
				AssertEquals("Pending Balance", expectedPendingBalance, guarantee.CPH_Calc_PendingBalance.Amount);
			});
		}

		CusGuaranteeHeader cusGuaranteeHeader;
		NctsGuarantee nctsGuarantee;
		NctsHeader nctsHeader => nctsGuarantee?.NctsHeader;
	}
}
