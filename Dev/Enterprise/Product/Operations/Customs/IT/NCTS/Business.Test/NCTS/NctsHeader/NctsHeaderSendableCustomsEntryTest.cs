using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderSendableCustomsEntry))]
sealed class NctsHeaderSendableCustomsEntryTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NctsHeaderSendableCustomsEntry(nctsHeader: null));
	}

	public void TestConsumeGuaranteeWithNullGuaranteeHeader_Phase4()
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
		var permitHeader = CusGuaranteeHeaderTestUtility.SetupGuarantee(Factory, permitHolder, "ARG", "1234");
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var guarantee = nctsHeader.Guarantees.AddNew();
		guarantee.PW_BondAmount = 999;
		guarantee.PW_BondNumber = "1234";
		Factory.Save();

		var message = Factory.New<ITEDIMessage>();
		ISendableCustomsEntry sendableCustomsEntry = new NctsHeaderSendableCustomsEntry(nctsHeader);
		sendableCustomsEntry.ConsumeGuarantee(Factory, message);

		AssertEquals("No Permit line transaction must be found", 0, permitHeader.CusGuaranteeLineTransactions.Count);
	}

	public void TestConsumeGuaranteeWithNullGuaranteeHeader_Phase5()
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
		var permitHeader = CusGuaranteeHeaderTestUtility.SetupGuarantee(Factory, permitHolder, "ARG", "1234");
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
		guarantee.PW_BondAmount = 999;
		guarantee.PW_BondNumber = "1234";
		Factory.Save();

		var message = Factory.New<ITEDIMessage>();
		ISendableCustomsEntry sendableCustomsEntry = new NctsHeaderSendableCustomsEntry(nctsHeader);
		sendableCustomsEntry.ConsumeGuarantee(Factory, message);

		AssertEquals("No Permit line transaction must be found", 0, permitHeader.CusGuaranteeLineTransactions.Count);
	}

	public void TestConsumeGuarantee_Phase4()
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
		var permitHeader = SetupGuarantee(permitHolder, "ARG", "1234", 999);
		var nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.Principal.E2_OA_Address = permitHolder.MainAddress.PK;
		nctsHeader.Guarantees.RemoveAndDeleteAll();
		nctsHeader.BH_JobReference = "ARG001001";

		var guarantee = nctsHeader.Guarantees.AddNew();
		guarantee.PW_BondAmount = 999;
		guarantee.PW_BondNumber = "1234";
		guarantee.PW_BondType = "3";

		var message = Factory.New<ITEDIMessage>();
		message.IsTransmitMessage = false;
		message.EM_MessageNum = "123456";
		ISendableCustomsEntry sendableCustomsEntry = new NctsHeaderSendableCustomsEntry(nctsHeader);
		sendableCustomsEntry.ConsumeGuarantee(Factory, message);
		Factory.Save();

		var automatedConsumeGuaranteeTransactions = permitHeader.CusGuaranteeLineTransactions.Where(x => x.CPL_TransactionType == PermitTransactionTypeList.Codes.TRA).ToArray();
		AssertEquals("When NctsHeader has a Guarantee added and match a registered GuaranteeHeader, ConsumeTransaction", 1, automatedConsumeGuaranteeTransactions.Length);
		AssertGuaranteeLineTransaction("ConsumeTransaction for Guarantee", "ARG001001", "123456", -999, automatedConsumeGuaranteeTransactions[0]);
	}

	public void TestConsumeGuarantee_Phase5()
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
		var permitHeader1 = SetupGuarantee(permitHolder, "ARG", "1234", 999);
		var permitHeader2 = SetupGuarantee(permitHolder, "ARG", "5678", 666);

		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		nctsHeader.Principal.E2_OA_Address = permitHolder.MainAddress.PK;
		nctsHeader.MovementHeader.Guarantees.RemoveAndDeleteAll();
		nctsHeader.BH_JobReference = "ARG001001";

		var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
		guarantee1.PW_BondAmount = 999;
		guarantee1.PW_BondNumber = "1234";
		guarantee1.PW_BondType = "3";

		var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
		guarantee2.PW_BondAmount = 333;
		guarantee2.PW_BondNumber = "5678";
		guarantee2.PW_BondType = "3";

		var message = Factory.New<ITEDIMessage>();
		message.IsTransmitMessage = false;
		message.EM_MessageNum = "123456";
		ISendableCustomsEntry sendableCustomsEntry = new NctsHeaderSendableCustomsEntry(nctsHeader);
		sendableCustomsEntry.ConsumeGuarantee(Factory, message);
		Factory.Save();

		var automatedConsumeGuaranteeTransactions1 = permitHeader1.CusGuaranteeLineTransactions.Where(x => x.CPL_TransactionType == PermitTransactionTypeList.Codes.TRA).ToArray();
		AssertEquals("When NctsHeader has a Guarantee added and match a registered GuaranteeHeader, ConsumeTransaction", 1, automatedConsumeGuaranteeTransactions1.Length);
		AssertGuaranteeLineTransaction("ConsumeTransaction for Guarantee 1", "ARG001001", "123456", -999, automatedConsumeGuaranteeTransactions1[0]);

		var automatedConsumeGuaranteeTransactions2 = permitHeader2.CusGuaranteeLineTransactions.Where(x => x.CPL_TransactionType == PermitTransactionTypeList.Codes.TRA).ToArray();
		AssertEquals("When NctsHeader has another Guarantee added and match a registered GuaranteeHeader, ConsumeTransaction", 1, automatedConsumeGuaranteeTransactions2.Length);
		AssertGuaranteeLineTransaction("ConsumeTransaction for Guarantee 2", "ARG001001", "123456", -333, automatedConsumeGuaranteeTransactions2[0]);
	}

	public void TestConsumeGuarantee_Phase5_WhenGuaranteeAmountIsExceeded()
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
		var permitHeader = SetupGuarantee(permitHolder, "ARG", "1234", 999);

		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		nctsHeader.Principal.E2_OA_Address = permitHolder.MainAddress.PK;
		nctsHeader.MovementHeader.Guarantees.RemoveAndDeleteAll();
		nctsHeader.BH_JobReference = "ARG001001";

		var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
		guarantee1.PW_BondAmount = 9999;
		guarantee1.PW_BondNumber = "1234";
		guarantee1.PW_BondType = "3";

		var message = Factory.New<ITEDIMessage>();
		message.IsTransmitMessage = false;
		message.EM_MessageNum = "123456";
		ISendableCustomsEntry sendableCustomsEntry = new NctsHeaderSendableCustomsEntry(nctsHeader);
		sendableCustomsEntry.ConsumeGuarantee(Factory, message);
		Factory.Save();

		var automatedConsumeGuaranteeTransactions1 = permitHeader.CusGuaranteeLineTransactions.Where(x => x.CPL_TransactionType == PermitTransactionTypeList.Codes.TRA).ToArray();
		AssertEquals("When NctsHeader has a Guarantee added and match a registered GuaranteeHeader, ConsumeTransaction", 1, automatedConsumeGuaranteeTransactions1.Length);
		AssertGuaranteeLineTransaction("ConsumeTransaction that exceeds amount for Guarantee", "ARG001001", "123456", -9999, automatedConsumeGuaranteeTransactions1[0]);
	}

	public void TestConsumeGuaranteeWithNctsHeaderHavingNoGuarantees_Phase4()
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
		var permitHeader = CusGuaranteeHeaderTestUtility.SetupGuarantee(Factory, permitHolder, "ARG", "1234");
		var nctsHeader = Factory.NewDepartureNctsHeader();
		Factory.Save();

		var message = Factory.New<ITEDIMessage>();
		ISendableCustomsEntry sendableCustomsEntry = new NctsHeaderSendableCustomsEntry(nctsHeader);
		sendableCustomsEntry.ConsumeGuarantee(nctsHeader.Factory, message);

		AssertEquals("No Permit line transaction must be found", 0, permitHeader.CusGuaranteeLineTransactions.Count);
	}

	public void TestConsumeGuaranteeWithNctsHeaderHavingNoGuarantees_Phase5()
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
		var permitHolder = Factory.NewWithValidTestData<OrgHeader>();
		var permitHeader = CusGuaranteeHeaderTestUtility.SetupGuarantee(Factory, permitHolder, "ARG", "1234");
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		Factory.Save();

		var message = Factory.New<ITEDIMessage>();
		ISendableCustomsEntry sendableCustomsEntry = new NctsHeaderSendableCustomsEntry(nctsHeader);
		sendableCustomsEntry.ConsumeGuarantee(Factory, message);

		AssertEquals("No Permit line transaction must be found", 0, permitHeader.CusGuaranteeLineTransactions.Count);
	}

	public void TestPreProcessBeforeSending_Phase4()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var movementHeader = nctsHeader.MovementHeader;
		ISendableCustomsEntry sendableCustomsEntry = new NctsHeaderSendableCustomsEntry(nctsHeader);

		CombineAssertions(() =>
		{
			movementHeader.BM_EntryDate = ZDate.Empty;
			sendableCustomsEntry.PreProcessBeforeSending();
			AssertEquals("Case 1: Calling PreProcessBeforeSending when BM_EntryDate was originally empty, output BM_EntryDate", ZDate.Today, movementHeader.BM_EntryDate);

			movementHeader.BM_EntryDate = ZDate.BrettsBirthday;
			sendableCustomsEntry.PreProcessBeforeSending();
			AssertEquals("Case 2: Calling PreProcessBeforeSending when BM_EntryDate was originally filled, output BM_EntryDate", ZDate.Today, movementHeader.BM_EntryDate);
		});
	}

	public void TestPreProcessBeforeSending_Phase5()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var movementHeader = nctsHeader.MovementHeader;
		ISendableCustomsEntry sendableCustomsEntry = new NctsHeaderSendableCustomsEntry(nctsHeader);

		CombineAssertions(() =>
		{
			movementHeader.BM_EntryDate = ZDateTime.Empty;
			sendableCustomsEntry.PreProcessBeforeSending();
			AssertEquals("Case 1: Calling PreProcessBeforeSending when BM_EntryDate was originally empty, output BM_EntryDate", ZDateTime.Empty, movementHeader.BM_EntryDate);

			movementHeader.BM_EntryDate = ZDateTime.BrettsBirthday;
			sendableCustomsEntry.PreProcessBeforeSending();
			AssertEquals("Case 2: Calling PreProcessBeforeSending when BM_EntryDate was originally filled, output BM_EntryDate", ZDateTime.BrettsBirthday, movementHeader.BM_EntryDate);
		});
	}

	[TestDate]
	public void TestPreProcessBeforeSending_NewDeclaration_Phase5ValuationDateIsSet()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var movementHeader = nctsHeader.MovementHeader;
		ISendableCustomsEntry sendableCustomsEntry = new NctsHeaderSendableCustomsEntry(nctsHeader, EDIMessageTypeList.Codes.NewDeclaration);

		nctsHeader.MovementHeader.BM_ValuationDate = ZDateTime.BrettsBirthday;

		sendableCustomsEntry.PreProcessBeforeSending();

		AssertEquals(ZDateTime.Now, nctsHeader.MovementHeader.BM_ValuationDate);
	}

	[TestDate]
	public void TestPreProcessBeforeSending_Amendment_Phase5ValuationDateIsSet()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var movementHeader = nctsHeader.MovementHeader;
		ISendableCustomsEntry sendableCustomsEntry = new NctsHeaderSendableCustomsEntry(nctsHeader, EDIMessageTypeList.Codes.Amendment);

		sendableCustomsEntry.PreProcessBeforeSending();
		AssertEquals(ZDateTime.Now, nctsHeader.MovementHeader.BM_ValuationDate);

		nctsHeader.MovementHeader.BM_ValuationDate = ZDateTime.BrettsBirthday;
		sendableCustomsEntry.PreProcessBeforeSending();
		AssertEquals(ZDateTime.BrettsBirthday, nctsHeader.MovementHeader.BM_ValuationDate);
	}

	[TestDate]
	public void TestPreProcessBeforeSending_Phase4ValuationDateIsNotSet()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var movementHeader = nctsHeader.MovementHeader;
		ISendableCustomsEntry sendableCustomsEntry = new NctsHeaderSendableCustomsEntry(nctsHeader);

		sendableCustomsEntry.PreProcessBeforeSending();
		AssertEquals(ZDateTime.Empty, nctsHeader.MovementHeader.BM_ValuationDate);
	}

	public void TestMarksAsSent_Phase4()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var movementHeader = nctsHeader.MovementHeader;
		var goodsItem1 = movementHeader.GoodsItems.AddNew();
		var goodsItem2 = movementHeader.GoodsItems.AddNew();
		var sentMessage = new Mock<IMessageType>().Object;

		ISendableCustomsEntry sendableCustomsEntry = new NctsHeaderSendableCustomsEntry(nctsHeader);
		sendableCustomsEntry.MarkAsSent(sentMessage);
		CombineAssertions("MessageStatus", () =>
		{
			AssertEquals("BH_MessageStatus", NctsMessageStatusList.Codes.DepartureDeclarationSent, nctsHeader.BH_MessageStatus);
			AssertEquals("GoodsItem1->BY_Status", "", goodsItem1.BY_Status);
			AssertEquals("GoodsItem2->BY_Status", "", goodsItem2.BY_Status);
		});

		nctsHeader.BH_MessageStatus = ZString.Empty;
		goodsItem1.PreviousDocuments.AddNew().CSI_Procedure = "MRN";
		goodsItem1.PreviousDocuments.AddNew().CSI_Procedure = "A3";
		sendableCustomsEntry.MarkAsSent(sentMessage);
		CombineAssertions("MessageStatus", () =>
		{
			AssertEquals("BH_MessageStatus", NctsMessageStatusList.Codes.DepartureDeclarationSent, nctsHeader.BH_MessageStatus);
			AssertEquals("GoodsItem1->BY_Status", EntryLineCustomsStatusList.Codes.Sent, goodsItem1.BY_Status);
			AssertEquals("GoodsItem2 (with no NBGroupedPreviousDocuments)->BY_Status", "", goodsItem2.BY_Status);
		});

		nctsHeader.BH_MessageStatus = ZString.Empty;
		goodsItem1.BY_Status = "";
		goodsItem2.BY_Status = EntryLineCustomsStatusList.Codes.Rejected;
		sendableCustomsEntry.MarkAsSent(sentMessage);
		CombineAssertions("MessageStatus should be update only if applicable", () =>
		{
			AssertEquals("BH_MessageStatus", NctsMessageStatusList.Codes.DepartureDeclarationSent, nctsHeader.BH_MessageStatus);
			AssertEquals("GoodsItem1->BY_Status", EntryLineCustomsStatusList.Codes.Sent, goodsItem1.BY_Status);
			AssertEquals("GoodsItem2 (Non Sendable in Nb Message)->BY_Status", EntryLineCustomsStatusList.Codes.Rejected, goodsItem2.BY_Status);
		});
	}

	public void TestMarksAsSent_Phase5()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationInitial;
		var goodsItem2 = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		var sentMessage = new Mock<IMessageType>().Object;

		ISendableCustomsEntry sendableCustomsEntry = new NctsHeaderSendableCustomsEntry(nctsHeader);
		sendableCustomsEntry.MarkAsSent(sentMessage);

		CombineAssertions("MessageStatus", () =>
		{
			AssertNullOrEmpty("BM_CustomsStatus is empty", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_Phase = 015", "015", movementHeader.BM_Phase);
			AssertEquals("BM_MessageStatus = SNT ", "SNT", movementHeader.BM_MessageStatus);
		});
	}

	public void TestMarksAsSent_MessageStatusIsCancellationMessage_Phase5AndMessageTypeIsCancellation()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();

		var mrn = CusEntryNumber.New(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, nctsHeader.Branch.Company.GC_RN_NKCountryCode);
		mrn.CE_EntryNum = "MRN123";

		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationInitial;
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		var sentMessage = new Mock<IMessageType>().Object;

		ISendableCustomsEntry sendableCustomsEntry = new NctsHeaderSendableCustomsEntry(nctsHeader, EDIMessageTypeList.Codes.Cancellation);
		sendableCustomsEntry.MarkAsSent(sentMessage);

		CombineAssertions("MessageStatus", () =>
		{
			AssertNullOrEmpty("BM_CustomsStatus is empty", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_Phase = 014", "014", movementHeader.BM_Phase);
			AssertEquals("BM_MessageStatus = SNT ", "SNT", movementHeader.BM_MessageStatus);
		});
	}

	public void TestMarksAsSent_Phase5AndPhaseStatusIsAmendment()
	{
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();

		var movementHeader = nctsHeader.MovementHeader;
		var sentMessage = new Mock<IMessageType>().Object;

		ISendableCustomsEntry sendableCustomsEntry = new NctsHeaderSendableCustomsEntry(nctsHeader, EDIMessageTypeList.Codes.Amendment);

		movementHeader.BM_Phase = "013";
		sendableCustomsEntry.MarkAsSent(sentMessage);

		CombineAssertions("MessageStatus", () =>
		{
			AssertNullOrEmpty("BM_CustomsStatus is empty", movementHeader.BM_CustomsStatus);
			AssertEquals("BM_Phase = 013", "013", movementHeader.BM_Phase);
			AssertEquals("BM_MessageStatus = SNT ", "SNT", movementHeader.BM_MessageStatus);
		});
	}

	public void TestMarksAsSentStatusOverrideLog()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.BH_MessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
		var sentMessage = new Mock<IMessageType>().Object;

		ISendableCustomsEntry sendableCustomsEntry = new NctsHeaderSendableCustomsEntry(nctsHeader);
		sendableCustomsEntry.MarkAsSent(sentMessage);
		AssertEquals("Has Status Override Log", false, nctsHeader.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CSO"));

		nctsHeader.BH_JobReference = "A0001";
		nctsHeader.BH_MessageStatus = NctsMessageStatusList.Codes.Ok;
		nctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;
		sendableCustomsEntry.MarkAsSent(sentMessage);
		AssertEquals("Has Status Override Log", true, nctsHeader.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CSO" && x.SL_Reference == "Entry A0001 Sent in status: MOK, DRL"));
	}

	public void TestCustomsProfile()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.BH_MessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
		nctsHeader.BH_CustomsProfile = "CUSPROF";
		ISendableCustomsEntry sendableCustomsEntry = new NctsHeaderSendableCustomsEntry(nctsHeader);

		AssertEquals("ISendableCustomsEntry.CustomsProfile", "CUSPROF", sendableCustomsEntry.CustomsProfile);
	}

	EU.Business.CusGuaranteeHeader SetupGuarantee(OrgHeader permitHolder, string valueFrom, string bondNumber, decimal initialTransactionValue)
	{
		var permitHeader = CusGuaranteeHeaderTestUtility.SetupGuarantee(Factory, permitHolder, valueFrom, bondNumber);
		var openingBalanceTransaction = permitHeader.CusGuaranteeLineTransactions.AddNew();
		openingBalanceTransaction.CPL_Reference = "Opening";
		openingBalanceTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
		openingBalanceTransaction.CPL_TranValue = initialTransactionValue;
		return permitHeader;
	}

	void AssertGuaranteeLineTransaction(string message, string jobReference, string messageNumber, decimal transactionValue, BaseCusGuaranteeLineTransaction cusGuaranteeLineTransaction)
	{
		CombineAssertions(message, () =>
		{
			CusGuaranteeLineTestHelper.AssertGuaranteeLineTransaction(
				cusGuaranteeLineTransaction,
				appId: messageNumber,
				comment: $"NCTS departure {jobReference}",
				transactionDate: ZDateTime.Now,
				transactionType: PermitTransactionTypeList.Codes.TRA,
				transactionCategory: PermitTransactionCategoryList.Codes.CUM,
				transactionStatus: PermitTransactionStatusList.Codes.Pending,
				reference: jobReference,
				transactionValue: transactionValue);
		});
	}
}
