using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsGuaranteeSadIrispMessageProcessorTest : NctsDepartureSadEtIrispMessageProcessorTest
{
	public void TestNegativeIrispProcessingDeletePendingGuaranteeTransaction()
	{
		(var nctsHeader, var negativeIrispMessage, var negativeIrispInterchange) = SetupDepartureNctsHeaderAndGetDataForTest(MessageNum, NegativeIrispSampleText, NegativeIrispFileName, ZGuid.NewZGuid().ToString(), ValidIdocFileName);
		var guaranteeTransaction = guaranteeHeader.AddTransaction(nctsHeader.JobNumber, "", MessageNum, "", 100m, 0m, PermitTransactionStatusList.Codes.Pending);
		Factory.Save();

		processor.ProcessMessage(negativeIrispMessage);
		Factory.Save();
		AssertProcessingResult(nctsHeader, negativeIrispMessage, negativeIrispInterchange, expectedEntryMessageStatus: NctsMessageStatusList.Codes.MessageSyntaxOrBusinessRuleErrors, expectedEntryHeaderStatus: "", NegativeIrispSampleText, MessageNum);
		AssertGuaranteeTransactionStatusOnSeparateFactory(guaranteeTransaction.PK, PermitTransactionStatusList.Codes.Deleted);
	}

	public void TestNegativeIrispProcessingDoesNotDeleteNotPendingGuaranteeTransaction()
	{
		(var nctsHeader, var negativeIrispMessage, var negativeIrispInterchange) = SetupDepartureNctsHeaderAndGetDataForTest(MessageNum, NegativeIrispSampleText, NegativeIrispFileName, ZGuid.NewZGuid().ToString(), ValidIdocFileName);
		var guaranteeTransaction = guaranteeHeader.AddTransaction(nctsHeader.JobNumber, "", MessageNum, "", 100m, 0m, PermitTransactionStatusList.Codes.Confirmed);
		Factory.Save();

		processor.ProcessMessage(negativeIrispMessage);
		Factory.Save();
		AssertProcessingResult(nctsHeader, negativeIrispMessage, negativeIrispInterchange, expectedEntryMessageStatus: NctsMessageStatusList.Codes.MessageSyntaxOrBusinessRuleErrors, expectedEntryHeaderStatus: "", NegativeIrispSampleText, MessageNum);
		AssertGuaranteeTransactionStatusOnSeparateFactory(guaranteeTransaction.PK, PermitTransactionStatusList.Codes.Confirmed);
	}

	public void TestPositiveIrispProcessingConfirmPendingGuaranteeTransaction()
	{
		(var nctsHeader, var irispEdiMessage, var irispEdiInterchange) = SetupDepartureNctsHeaderAndGetDataForTest(PositiveCLRMessageBundle.MessageNum, PositiveCLRMessageBundle.MessageText, PositiveCLRMessageBundle.ValidResponseFileName, ZGuid.NewZGuid().ToString(), PositiveCLRMessageBundle.ValidIdocFileName);
		var guaranteeTransaction = guaranteeHeader.AddTransaction(nctsHeader.JobNumber, "", PositiveCLRMessageBundle.MessageNum, "", 100m, 0m, PermitTransactionStatusList.Codes.Pending);
		Factory.Save();

		processor.ProcessMessage(irispEdiMessage);
		Factory.Save();
		AssertProcessingResult(nctsHeader, irispEdiMessage, irispEdiInterchange, expectedEntryMessageStatus: NctsMessageStatusList.Codes.Ok, expectedEntryHeaderStatus: NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, PositiveCLRMessageBundle.MessageText.Trim(), PositiveCLRMessageBundle.MessageNum);
		AssertGuaranteeTransactionStatusOnSeparateFactory(guaranteeTransaction.PK, PermitTransactionStatusList.Codes.Confirmed);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var permitHolder = Factory.New<OrgHeader>();
		permitHolder.OH_Code = "ORG";
		guaranteeHeader = CusGuaranteeHeaderTestUtility.SetupGuarantee(Factory, permitHolder, "111", "XYZ");
	}

	CusGuaranteeHeader guaranteeHeader;

	void AssertGuaranteeTransactionStatusOnSeparateFactory(ZGuid transactionPK, ZString expectedTransactionStatus)
	{
		var separateFactory = new BusinessObjectFactory();
		var transactionOnSeparateFactory = separateFactory.Load<SharedCusPermitLineTransaction>(transactionPK);
		AssertNotNull("Guarantee transaction", transactionOnSeparateFactory);
		AssertEquals("Guarantee transaction status", expectedTransactionStatus, transactionOnSeparateFactory.CPL_TransactionStatus);
	}
}
