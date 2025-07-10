using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	abstract class NCTSDepartureWithGuaranteeMessageProcessorAbstractTest<TMessageProcessor, TInboundEDIMessage, TOutboundEDIMessage, TDataProvider> :
		NCTSDepartureMessageProcessorAbstractTest<TMessageProcessor, TInboundEDIMessage, TOutboundEDIMessage, TDataProvider>
			where TMessageProcessor : MessageAttacheeMessageProcessor<TInboundEDIMessage, TDataProvider>
			where TInboundEDIMessage : InboundEDIMessage
			where TOutboundEDIMessage : OutboundEDIMessage
	{
		protected override (NctsHeader declaration, NctsDepartureMovementHeader messageAttachee, EDIMessage outgoingMessage, TInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			NctsGuarantee nctsGuarantee;
			(cusGuaranteeHeader, nctsGuarantee) = NctsTestDataProvider.CreateNctsGuaranteeWithCusGuaranteeHeader(Factory);

			var lrnReference = "LRN1234";
			cusGuaranteeHeader.AddTransaction(lrnReference, "Pending Transaction", "", "", -25000m, 0, status: Customs.Business.PermitTransactionStatusList.Codes.Pending);

			var header = nctsGuarantee.NctsHeader;
			header.BH_JobReference = "B00001000";
			header.BH_GB = Branch.PK;
			var movementHeader = header.MovementHeader;
			movementHeader.BM_PaperlessInbondNum = lrnReference;

			var incomingMessage = CreateNewIncomingMessage(incomingMessageText);
			var outgoingMessage = Factory.New<NCTSOutboundEDIMessage>();
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			outgoingMessage.EM_ApplicationReference = TransactionID;
			outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			outgoingMessage.EM_GB = Branch.PK;
			outgoingMessage.EM_SystemCreateUser = Staff.GS_Code;
			movementHeader.Messages.Add(outgoingMessage);

			Factory.Save();

			return (header, movementHeader, outgoingMessage, incomingMessage);
		}

		protected void AssertTransaction(NctsDepartureMovementHeader messageAttachee, int expectedCount, ZDecimal expectedValue, ZDecimal expectedPendingBalance, ZString expectedStatus)
		{
			var transactions = cusGuaranteeHeader.GetTransactions().ToArray();
			AssertEquals($"Transaction count should be {expectedCount}", expectedCount, transactions.Length);
			var newTransaction = transactions[expectedCount - 1];
			AssertEquals("Status", expectedStatus, newTransaction.CPL_TransactionStatus);
			AssertEquals("Type", "TRA", newTransaction.CPL_TransactionType);
			AssertEquals("Reference", messageAttachee.BM_PaperlessInbondNum, newTransaction.CPL_Reference);
			AssertEquals("Transaction value", expectedValue, newTransaction.CPL_TranValue);
			AssertEquals("Pending Balance", expectedPendingBalance, cusGuaranteeHeader.CPH_Calc_PendingBalance.Amount);
		}

		protected CusGuaranteeHeader cusGuaranteeHeader;
	}
}
