using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDRMessageProcessorHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestClearDeclarationForExceptions()
		{
			EXDRMessageProcessorHelper.ClearDeclaration(null, null, null);
		}

		public void TestClearDeclarationStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var incomingMessage = declaration.Messages.AddNew();
			var outgoingMessage = declaration.Messages.AddNew();
			outgoingMessage.EM_MessageSubType = "ORG";
			EXDRMessageProcessorHelper.ClearDeclaration(incomingMessage, outgoingMessage, declaration);
			AssertEquals(CustomsEntryStatus.ClearOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals(CustomsEntryStatus.ClearOriginal.Code, entryHeader.CH_Status);

			outgoingMessage.EM_MessageSubType = "REP";
			EXDRMessageProcessorHelper.ClearDeclaration(incomingMessage, outgoingMessage, declaration);
			AssertEquals(CustomsEntryStatus.ClearReplacement.Code, declaration.JE_EntryStatus);
			AssertEquals(CustomsEntryStatus.ClearReplacement.Code, entryHeader.CH_Status);

			outgoingMessage.EM_MessageSubType = "WDW";
			EXDRMessageProcessorHelper.ClearDeclaration(incomingMessage, outgoingMessage, declaration);
			AssertEquals(CustomsEntryStatus.ClearWithdrawal.Code, declaration.JE_EntryStatus);
			AssertEquals(CustomsEntryStatus.ClearWithdrawal.Code, entryHeader.CH_Status);
		}

		[ExpectNoExceptions]
		public void TestErrorDeclarationForExceptions()
		{
			EXDRMessageProcessorHelper.ErrorDeclaration(null, null, null);
		}

		public void TestErrorDeclarationStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var incomingMessage = declaration.Messages.AddNew();
			var outgoingMessage = declaration.Messages.AddNew();
			outgoingMessage.EM_MessageSubType = "ORG";
			EXDRMessageProcessorHelper.ErrorDeclaration(incomingMessage, outgoingMessage, declaration);
			AssertEquals(CustomsEntryStatus.ErrorOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals(CustomsEntryStatus.ErrorOriginal.Code, entryHeader.CH_Status);

			outgoingMessage.EM_MessageSubType = "REP";
			EXDRMessageProcessorHelper.ErrorDeclaration(incomingMessage, outgoingMessage, declaration);
			AssertEquals(CustomsEntryStatus.ErrorReplacement.Code, declaration.JE_EntryStatus);
			AssertEquals(CustomsEntryStatus.ErrorReplacement.Code, entryHeader.CH_Status);

			outgoingMessage.EM_MessageSubType = "WDW";
			EXDRMessageProcessorHelper.ErrorDeclaration(incomingMessage, outgoingMessage, declaration);
			AssertEquals(CustomsEntryStatus.ErrorWithdrawal.Code, declaration.JE_EntryStatus);
			AssertEquals(CustomsEntryStatus.ErrorWithdrawal.Code, entryHeader.CH_Status);
		}

		[ExpectNoExceptions]
		public void TestRejectDeclarationForExceptions()
		{
			EXDRMessageProcessorHelper.RejectDeclaration(null, null, null);
		}

		public void TestRejectDeclarationStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var incomingMessage = declaration.Messages.AddNew();
			var outgoingMessage = declaration.Messages.AddNew();
			outgoingMessage.EM_MessageSubType = "ORG";
			EXDRMessageProcessorHelper.RejectDeclaration(incomingMessage, outgoingMessage, declaration);
			AssertEquals(CustomsEntryStatus.FailOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals(CustomsEntryStatus.FailOriginal.Code, entryHeader.CH_Status);

			outgoingMessage.EM_MessageSubType = "REP";
			EXDRMessageProcessorHelper.RejectDeclaration(incomingMessage, outgoingMessage, declaration);
			AssertEquals(CustomsEntryStatus.FailReplacement.Code, declaration.JE_EntryStatus);
			AssertEquals(CustomsEntryStatus.FailReplacement.Code, entryHeader.CH_Status);

			outgoingMessage.EM_MessageSubType = "WDW";
			EXDRMessageProcessorHelper.RejectDeclaration(incomingMessage, outgoingMessage, declaration);
			AssertEquals(CustomsEntryStatus.FailWithdrawal.Code, declaration.JE_EntryStatus);
			AssertEquals(CustomsEntryStatus.FailWithdrawal.Code, entryHeader.CH_Status);
		}

		[ExpectNoExceptions]
		public void TestRevokeDeclarationForExceptions()
		{
			EXDRMessageProcessorHelper.RejectDeclaration(null, null, null);
		}

		public void TestRevokeDeclarationStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var incomingMessage = declaration.Messages.AddNew();
			var outgoingMessage = declaration.Messages.AddNew();
			outgoingMessage.EM_MessageSubType = "ORG";
			EXDRMessageProcessorHelper.RejectDeclaration(incomingMessage, outgoingMessage, declaration);
			AssertEquals(CustomsEntryStatus.FailOriginal.Code, declaration.JE_EntryStatus);
			AssertEquals(CustomsEntryStatus.FailOriginal.Code, entryHeader.CH_Status);

			outgoingMessage.EM_MessageSubType = "REP";
			EXDRMessageProcessorHelper.RejectDeclaration(incomingMessage, outgoingMessage, declaration);
			AssertEquals(CustomsEntryStatus.FailReplacement.Code, declaration.JE_EntryStatus);
			AssertEquals(CustomsEntryStatus.FailReplacement.Code, entryHeader.CH_Status);

			outgoingMessage.EM_MessageSubType = "WDW";
			EXDRMessageProcessorHelper.RejectDeclaration(incomingMessage, outgoingMessage, declaration);
			AssertEquals(CustomsEntryStatus.FailWithdrawal.Code, declaration.JE_EntryStatus);
			AssertEquals(CustomsEntryStatus.FailWithdrawal.Code, entryHeader.CH_Status);
		}
	}
}
