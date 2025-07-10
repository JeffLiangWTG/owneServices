using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class DataLoadingModuleMessageManagerTest : FormalEntrySingleMessageManagerTest
	{
		public void TestStatus()
		{
			manager = new DataLoadingModuleMessageManager(entry, dlmMessageSendingAction);
			manager.GenerateOriginalMessages(entry);
			AssertEquals("Status is calculated", MessageStatusList.Codes.Sent, entry.CH_Status);
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "Data Loading Module is currently not supporting Withdrawal")]
		public void TestGenerateWithdrawalMessagesForExport()
		{
			manager = new DataLoadingModuleMessageManager(entry, dlmMessageSendingAction);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusEntryHeader entryLoaded = factory2.Load<CusEntryHeader>(entry.PK);
			entryLoaded.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
			manager.GenerateWithdrawalMessages(entryLoaded);
		}

		public void TestMessageFriendlyName()
		{
			entry.CH_BGMReference = "B12312233";
			manager = new DataLoadingModuleMessageManager(entry, dlmMessageSendingAction);
			AssertEquals("MessageFriendlyName", "B12312233", manager.MessageFriendlyName);
		}

		public void TestCanSendOriginal()
		{
			entry.CH_Status = MessageStatusList.Codes.NotSent;
			Factory.Save();
			manager = new DataLoadingModuleMessageManager(entry, dlmMessageSendingAction);
			AssertEquals("CanSendOriginal", true, manager.CanSendOriginal);

			entry.CH_Status = MessageStatusList.Codes.Sent;
			Factory.Save();
			AssertEquals("CanSendOriginal", true, manager.CanSendOriginal);
		}

		public void TestCanSendWithdrawal()
		{
			manager = new DataLoadingModuleMessageManager(entry, dlmMessageSendingAction);
			AssertEquals("CanSendWithdrawal", false, manager.CanSendWithdrawal);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();

			actions = new CAMessageSendingActionCollection(declaration, MessageSendingMessageType.Original);
			dlmMessageSendingAction = actions.FindFirstElement(MessageType.DataLoadingModule);
			manager = (DataLoadingModuleMessageManager)GetTestManager(entry);
		}

		protected override SingleMessageManager GetNewSingleMessageManager()
		{
			return GetTestManager(entry);
		}

		protected override FormalEntrySingleMessageManager GetTestManager(Customs.Business.CusEntryHeader entryHeader)
		{
			return new DataLoadingModuleMessageManager((CusEntryHeader)entryHeader, dlmMessageSendingAction);
		}

		JobDeclaration declaration;
		CusEntryHeader entry;
		DataLoadingModuleMessageManager manager;
		CAMessageSendingAction dlmMessageSendingAction;
		CAMessageSendingActionCollection actions;

		#endregion Implementation
	}
}
