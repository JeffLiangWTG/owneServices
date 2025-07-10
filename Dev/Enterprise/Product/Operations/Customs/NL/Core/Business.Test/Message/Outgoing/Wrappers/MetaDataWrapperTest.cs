using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class MetaDataWrapperTest : DataProviderTestCase<MetaDataWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new MetaDataWrapper(null));
	}

	public void TestWCOTypeCode()
	{
		AssertEquals("Meta Data Wrapper - WCOTypeCode", NLEDIMessage.WCOTypePlaceHolder, wrapper.WCOTypeCode);
	}

	public void TestCommunicationMetaData()
	{
		CombineAssertions(() =>
		{
			AssertNotNull("Meta Data Wrapper - CommunicationMetaData", wrapper.CommunicationMetaData);
			AssertType<CommunicationMetaDataWrapper>("Meta Data Wrapper - CommunicationMetaData", wrapper.CommunicationMetaData);
		});
	}

	[TestDate(2021, 12, 12)]
	public void TestDeclaration()
	{
		CombineAssertions(() =>
		{
			AssertNotNull("Meta Data Wrapper - Declaration", wrapper.CommunicationMetaData);
			AssertType<DeclarationWrapper>("Meta Data Wrapper - Declaration", wrapper.Declaration);
		});
	}

	public void TestH1Message()
	{
		CombineAssertions(() =>
		{
			AssertNotNull("WCOTypeCode", wrapper.WCOTypeCode);
			AssertEquals("WCOTypeCode value", NLEDIMessage.WCOTypePlaceHolder, wrapper.WCOTypeCode);

			AssertNotNull("CommunicationMetaData", wrapper.CommunicationMetaData);
			AssertType<CommunicationMetaDataWrapper>(wrapper.CommunicationMetaData);

			AssertNotNull("Declaration", wrapper.Declaration);
			AssertType<DeclarationWrapper>(wrapper.Declaration);
		});
	}

	public void TestGetSentMessageForComparison()
	{
		CombineAssertions(() =>
		{
			var wrapper = new MetaDataWrapperForTest(sendingObject);
			var message = WrapperTestHelper.CreateSentMessage(entryHeader, ExportSendMessageTypes.Codes.DEC, "EXA");
			message.EM_SystemCreateTimeUtc = new ZDateTime(2023, 09, 10, 10, 25, 00);
			AssertSame("Matched", message, wrapper.GetSentMessageForComparisonExposed(entryHeader.Messages, ExportSendMessageTypes.Codes.DEC));

			var message2 = WrapperTestHelper.CreateSentMessage(entryHeader, ExportSendMessageTypes.Codes.DEC, "EXA");
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2023, 09, 11, 10, 25, 00);
			AssertSame("Match latest", message2, wrapper.GetSentMessageForComparisonExposed(entryHeader.Messages, ExportSendMessageTypes.Codes.DEC));

			var message3 = WrapperTestHelper.CreateSentMessage(entryHeader, ExportSendMessageTypes.Codes.DEC, "EXA");
			message3.EM_SystemCreateTimeUtc = new ZDateTime(2023, 09, 12, 10, 25, 00);
			message3.EM_Status = EDIMessageStatusList.Codes.Queued;
			AssertSame("EM_Status not match", message2, wrapper.GetSentMessageForComparisonExposed(entryHeader.Messages, ExportSendMessageTypes.Codes.DEC));

			AssertNull("EM_MessageType not match", wrapper.GetSentMessageForComparisonExposed(entryHeader.Messages, ExportSendMessageTypes.Codes.CRE));
		});
	}

	public void TestHasReleasedMessage()
	{
		CombineAssertions(() =>
		{
			var wrapper = new MetaDataWrapperForTest(sendingObject);
			var message = WrapperTestHelper.CreateReleasedMessage(entryHeader, NLIncomingMessageSubTypeList.Codes.CC529C, StatusNameCodes.Released);
			AssertEquals("EM_MessageSubType not match", false, wrapper.HasReleasedMessageExposed(entryHeader.Messages, NLIncomingMessageSubTypeList.Codes.CC429A, EntryStatus.ReleasedAndTaxed));

			var message2 = WrapperTestHelper.CreateReleasedMessage(entryHeader, NLIncomingMessageSubTypeList.Codes.CC429A, StatusNameCodes.Released);
			message2.EM_Status = EDIMessageStatusList.Codes.Queued;
			AssertEquals("EM_Status not match", false, wrapper.HasReleasedMessageExposed(entryHeader.Messages, NLIncomingMessageSubTypeList.Codes.CC429A, EntryStatus.ReleasedAndTaxed));

			WrapperTestHelper.CreateReleasedMessage(entryHeader, NLIncomingMessageSubTypeList.Codes.CC429A, StatusNameCodes.Released);
			AssertEquals("EntryStatus not match", false, wrapper.HasReleasedMessageExposed(entryHeader.Messages, NLIncomingMessageSubTypeList.Codes.CC429A, EntryStatus.ProvisionalRelease));

			WrapperTestHelper.CreateReleasedMessage(entryHeader, NLIncomingMessageSubTypeList.Codes.CC429A, StatusNameCodes.Released);
			AssertEquals("Matched", true, wrapper.HasReleasedMessageExposed(entryHeader.Messages, NLIncomingMessageSubTypeList.Codes.CC429A, EntryStatus.ReleasedAndTaxed));

			AssertEquals("Matched with multiple entryStatus", true, wrapper.HasReleasedMessageExposed(entryHeader.Messages, NLIncomingMessageSubTypeList.Codes.CC429A, EntryStatus.ProvisionalRelease, EntryStatus.ReleasedAndTaxed));
		});
	}

	protected override MetaDataWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.Declarant.Header.Contacts.AddNew();

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "H1";

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_LineNo = 1;
		invoiceLine.JI_CEI = entryInstruction.PK;
		var shutterUpperer = new SendsMessagesToCustomsShutterUpperer(false);
		declaration.DoMerge(shutterUpperer);
		entryHeader = declaration.CustomsEntryHeaders.Cast<Declaration.CusEntryHeader>().FirstOrDefault();
		sendingObject = new JobDeclarationMessageSendingObject(entryHeader);
		sendingObject.MessageType = "INV";
		wrapper = new MetaDataWrapper(sendingObject);
	}
	Declaration.CusEntryHeader entryHeader;
	JobDeclarationMessageSendingObject sendingObject;
	MetaDataWrapper wrapper;

	sealed class MetaDataWrapperForTest : MetaDataWrapper
	{
		public MetaDataWrapperForTest(JobDeclarationMessageSendingObject provider) : base(provider)
		{
		}

		public NLEDIMessage GetSentMessageForComparisonExposed(EDIMessageCollection messages, ZString messageSubType) => GetSentMessageForComparison(messages, messageSubType);

		public ZBool HasReleasedMessageExposed(EDIMessageCollection messages, ZString messageSubType, params ZString[] entryStatusArray) => HasReleasedMessage(messages, messageSubType, entryStatusArray);
	}
}
