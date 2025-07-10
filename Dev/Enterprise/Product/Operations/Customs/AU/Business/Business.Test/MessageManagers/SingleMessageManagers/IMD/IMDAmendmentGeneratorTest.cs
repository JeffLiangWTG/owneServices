using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class IMDAmendmentGeneratorTest : CMRAmendmentGeneratorAbstractTest
	{
		public void TestIMDMessageTypeSelectionSetOnMessageBuilder()
		{
			var result = (BaseImportMessageBuilder)iMDAmendmentGenerator.GetBuilder(entryHeader);
			AssertEquals("IMDMEssageBuilder", CMRMessageTypes.Amendment, result.MessageType);
			AssertEquals("Reason is set in message builder", result.AmendmentWithdrawalReason, iMDAmendmentGenerator.Reason);

			declaration.JE_MessageSubType = "SAC";
			result = (BaseImportMessageBuilder)iMDAmendmentGenerator.GetBuilder(entryHeader);
			AssertEquals("IMDMEssageBuilder", CMRMessageTypes.Amendment, result.MessageType);
			AssertEquals("Reason is set in message builder", result.AmendmentWithdrawalReason, iMDAmendmentGenerator.Reason);
		}

		public void TestGetMesssageCollection()
		{
			AssertNotNull("Message Collection", iMDAmendmentGenerator.GetMesssageCollection(entryHeader));
			AssertEquals("Count", 1, iMDAmendmentGenerator.GetMesssageCollection(entryHeader).Count);
		}

		public void TestUniqueIdentifierBeingChanged()
		{
			AssertEquals("Not Supported", false, iMDAmendmentGenerator.UniqueIdentifierBeingChanged);
		}

		public void TestGetBuilderForSAC()
		{
			AssertEquals("IMDMEssageBuilder", typeof(IMDMessageBuilder), iMDAmendmentGenerator.GetBuilder(entryHeader).GetType());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines; //"SAC"
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			declaration.ResetMessageTypeChangeLogs();
			Factory.Save();
			AssertEquals("SACMessageBuilder", typeof(SACMessageBuilder), iMDAmendmentGenerator.GetBuilder(entryHeader).GetType());
		}

		public void TestGetBuilderForSACWithLines()
		{
			AssertEquals("IMDMEssageBuilder", typeof(IMDMessageBuilder), iMDAmendmentGenerator.GetBuilder(entryHeader).GetType());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines; //"SWL"
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			declaration.ResetMessageTypeChangeLogs();
			Factory.Save();
			AssertEquals("SACMessageBuilder", typeof(SACMessageBuilder), iMDAmendmentGenerator.GetBuilder(entryHeader).GetType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			GetSavedBizo();
			iMDAmendmentGenerator = new IMDAmendmentGeneratorForTest(entryHeader, new CMRAmendmentWithdrawalReason());

			AssertEquals("PreCondition: EntryHeader has one message", 1, entryHeader.Messages.Count);
		}

		protected override Type ExpectedMessageType => typeof(CMRIMDMessage);

		protected override BusinessObject GetSavedBizo()
		{
			declaration = JobDeclaration.New(Factory);
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);

			var message = Factory.New<CMRMessage>();
			message.EM_LinkedObject = entryHeader;
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			Factory.Save();
			return entryHeader;
		}

		protected override ICMRAmendmentGeneratorForTest GetGenerator(BusinessObject bizo) => new IMDAmendmentGeneratorForTest((CusEntryHeader)bizo, new CMRAmendmentWithdrawalReason());

		CusEntryHeader entryHeader;
		IMDAmendmentGeneratorForTest iMDAmendmentGenerator;
		JobDeclaration declaration;

		sealed class IMDAmendmentGeneratorForTest : IMDAmendmentGenerator, ICMRAmendmentGeneratorForTest
		{
			public IMDAmendmentGeneratorForTest(CusEntryHeader entryHeader, CMRAmendmentWithdrawalReason reason) : base(entryHeader, reason)
			{
			}

			internal new bool UniqueIdentifierBeingChanged => base.UniqueIdentifierBeingChanged;
			public new ZPropertyInfo[] UniqueIdentifierInfos => base.UniqueIdentifierInfos;
			public new EDIMessage GenerateOriginalMessageCore() => base.GenerateOriginalMessageCore();
			public new EDIMessage GenerateAmendmentMessageCore() => base.GenerateAmendmentMessageCore();
			public new EDIMessage GenerateWithdrawalMessageCore(BusinessObject bizo, BusinessObject bizoInNewFactory) => base.GenerateWithdrawalMessageCore(bizo, bizoInNewFactory);
		}
	}
}
