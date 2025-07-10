using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class DeltaGStatusResolverTest : TestCaseWithFactory
	{
		public void TestCheckIsOriginalError_WhenANTHasError()
		{
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.ANT;
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportErreurResponseMessage.xml");
			AssertEquals("Original is error when action ANT receives error.", true, GetNewBondedWarehouseStatusResolver().CheckIsOriginalError());
		}

		public void TestCheckIsOriginalError_WhenVALHasError()
		{
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.VAL;
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportErreurResponseMessage.xml");
			AssertEquals("Original is error when action VAL receives error.", true, GetNewBondedWarehouseStatusResolver().CheckIsOriginalError());
		}

		public void TestCheckIsOriginalError_WhenStatusIsANN()
		{
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.ANA;
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportANNResponseMessage.xml");
			AssertEquals("Original is error when status ANN is received.", true, GetNewBondedWarehouseStatusResolver().CheckIsOriginalError());
		}

		public void TestCheckIsOriginalError_OtherCase()
		{
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.VAL;
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			AssertEquals("Original is not error when status BAE is received.", false, GetNewBondedWarehouseStatusResolver().CheckIsOriginalError());
		}

		public void TestCheckIsOriginalClear_WhenActionIsVAA_AndStatusIsBAE()
		{
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.VAA;
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			AssertEquals("Original is cleared when action is VAA and status BAE is received.", true, GetNewBondedWarehouseStatusResolver().CheckIsOriginalClear());
		}

		public void TestCheckIsOriginalClear_WhenActionIsEAV_AndStatusIsBAE()
		{
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.EAV;
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			AssertEquals("Original is cleared when action is EAV and status BAE is received.", true, GetNewBondedWarehouseStatusResolver().CheckIsOriginalClear());
		}

		public void TestCheckIsOriginalClear_WhenActionIsVAL_AndStatusIsBAE()
		{
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.VAL;
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			AssertEquals("Original is cleared when action is VAL and status BAE is received.", true, GetNewBondedWarehouseStatusResolver().CheckIsOriginalClear());
		}

		public void TestCheckIsOriginalClear_WhenActionIsOther_AndStatusIsBAE()
		{
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.REC;
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			AssertEquals("Original is not cleared when action is REC and status BAE is received.", false, GetNewBondedWarehouseStatusResolver().CheckIsOriginalClear());
		}

		public void TestCheckIsOriginalClear_OtherCase()
		{
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportANNResponseMessage.xml");
			AssertEquals("Original is not cleared when status non-BAE is received.", false, GetNewBondedWarehouseStatusResolver().CheckIsOriginalClear());
		}

		public void TestCheckIsModificationAccepted_WhenMAPHasNoError()
		{
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.MAP;
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			AssertEquals("Modification is accepted when action MAP receives status BAE.", true, GetNewBondedWarehouseStatusResolver().CheckIsModificationAccepted());
		}

		public void TestCheckIsModificationAccepted_WhenMAPHasError()
		{
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.MAP;
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportErreurResponseMessage.xml");
			AssertEquals("Modification is not accepted when action MAP receives errors.", false, GetNewBondedWarehouseStatusResolver().CheckIsModificationAccepted());
		}

		public void TestCheckIsModificationRejected_WhenMAPHasError()
		{
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.MAP;
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportErreurResponseMessage.xml");
			AssertEquals("Modification is rejected when action MAP receives errors.", true, GetNewBondedWarehouseStatusResolver().CheckIsModificationRejected());
		}

		public void TestCheckIsModificationRejected_WhenMAPHasNoError()
		{
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.MAP;
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			AssertEquals("Modification is not rejected when action MAP receives status BAE.", false, GetNewBondedWarehouseStatusResolver().CheckIsModificationRejected());
		}

		public void TestCheckIsAmendmentError_WhenRECHasError()
		{
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.REC;
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportErreurResponseMessage.xml");
			AssertEquals("Amendment is error when action REC receives error.", true, GetNewBondedWarehouseStatusResolver().CheckIsRectificationError());
		}

		public void TestCheckIsAmendmentError_WhenStatusIsREF()
		{
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportREFResponseMessage.xml");
			AssertEquals("Amendment is error when status REF is received.", true, GetNewBondedWarehouseStatusResolver().CheckIsRectificationError());
		}

		public void TestCheckIsAmendmentError_WhenStatusIsDAN()
		{
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportDANResponseMessage.xml");
			AssertEquals("Amendment is error when status DAN is received.", true, GetNewBondedWarehouseStatusResolver().CheckIsRectificationError());
		}

		public void TestCheckIsAmendmentError_OtherCase()
		{
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			AssertEquals("Amendment is not error when status BAE is received.", false, GetNewBondedWarehouseStatusResolver().CheckIsRectificationError());
		}

		public void TestCheckIsAmendmentClear_WhenStatusIsACC()
		{
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportACCResponseMessage.xml");
			AssertEquals("Amendment is cleared when status ACC is received.", true, GetNewBondedWarehouseStatusResolver().CheckIsRectificationClear());
		}

		public void TestCheckIsAmendmentClear_OtherCase()
		{
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			AssertEquals("Amendment is not cleared when status BAE is received.", false, GetNewBondedWarehouseStatusResolver().CheckIsRectificationClear());
		}

		public void TestCheckHasBeenWithdrawn_WhenStatusIsINV()
		{
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportINVResponseMessage.xml");
			AssertEquals("Withdrawn is completed when status INV is received.", true, GetNewBondedWarehouseStatusResolver().CheckHasBeenWithdrawn());
		}

		public void TestCheckHasBeenWithdrawn_OtherCase()
		{
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportErreurResponseMessage.xml");
			AssertEquals("Withdrawn is rejected when error occurs.", false, GetNewBondedWarehouseStatusResolver().CheckHasBeenWithdrawn());
		}

		public void TestCheckIsWithdrawalError_WhenINVHasError()
		{
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.INV;
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportErreurResponseMessage.xml");
			AssertEquals("Withdrawal is error when action INV receives error.", true, GetNewBondedWarehouseStatusResolver().CheckIsWithdrawalError());
		}

		public void TestCheckIsWithdrawalError_WhenINVHasNoError()
		{
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.INV;
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			AssertEquals("Withdrawal is not error when action INV receives no error.", false, GetNewBondedWarehouseStatusResolver().CheckIsWithdrawalError());
		}

		public void TestShouldUpdateBondedWarehouseIfPendingInward()
		{
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.VAA;
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");

			entry.CH_WarehouseTransactionStatus = ZString.Empty;
			AssertEquals("Should not update Inventory Management because the CH_WarehouseTransactionStatus is not pending.", false, GetNewBondedWarehouseStatusResolver().ShouldUpdateBondedWarehouse());

			entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
			AssertEquals("Should not update Inventory Management because the CH_WarehouseTransactionStatus is not pending.", false, GetNewBondedWarehouseStatusResolver().ShouldUpdateBondedWarehouse());

			entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreatedPending;
			AssertEquals("Should update Inventory Management because the CH_WarehouseTransactionStatus is pending.", true, GetNewBondedWarehouseStatusResolver().ShouldUpdateBondedWarehouse());
		}

		public void TestShouldUpdateBondedWarehouseIfPendingOutward()
		{
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.VAA;
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");

			entry.CH_WarehouseTransactionStatus = ZString.Empty;
			AssertEquals("Should not update bonded warehouse because the CH_WarehouseTransactionStatus is not pending.", false, GetNewBondedWarehouseStatusResolver().ShouldUpdateBondedWarehouse());

			entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			AssertEquals("Should not update bonded warehouse because the CH_WarehouseTransactionStatus is not pending.", false, GetNewBondedWarehouseStatusResolver().ShouldUpdateBondedWarehouse());

			entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreatedPending;
			AssertEquals("Should update bonded warehouse because the CH_WarehouseTransactionStatus is pending.", true, GetNewBondedWarehouseStatusResolver().ShouldUpdateBondedWarehouse());
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			entry = declaration.CustomsEntryHeaders.AddNew();
			entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			entry.CH_CEI_Instruction = entryInstruction.PK;

			var outgoingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			outgoingInterchange.EI_InterchangeNum = "123";
			outgoingMessage = Factory.NewWithValidTestData<FREDIMessage>();
			outgoingMessage.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
			entry.Messages.Add(outgoingMessage);
			outgoingMessage.EM_EI = outgoingInterchange.PK;
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;

			var incomingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			incomingInterchange.EI_InterchangeNum = "123.";
			incomingMessage = Factory.NewWithValidTestData<DeltaCImportFREDIMessage>();
			entry.Messages.Add(incomingMessage);
			incomingMessage.EM_EI = incomingInterchange.PK;
			incomingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

			Factory.Save();
		}

		JobDeclaration declaration;
		CusEntryHeader entry;
		CusEntryInstruction entryInstruction;
		FREDIMessage outgoingMessage;
		FREDIMessage incomingMessage;
		DeltaGStatusResolver GetNewBondedWarehouseStatusResolver() => new DeltaGStatusResolver(entry, incomingMessage);

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
