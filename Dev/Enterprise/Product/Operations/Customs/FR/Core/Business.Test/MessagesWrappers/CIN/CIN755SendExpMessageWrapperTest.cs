using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessagesWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.CIN.Testing
{
	class CIN755SendExpMessageWrapperTest : TestCaseWithFactory
	{
		public void TestCIN755SendExpMessageWrapperConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new CIN755SendExpMessageWrapper(null));
		}

		[TestDate(2020, 12, 31, 12, 00, 00)]
		public void TestCIN755SendExpMessageWrapperMembers()
		{
			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(entryHeaderItem);
			messageObject.MessageType = EntryActionCodeList.Codes.CIN755;
			var wrapper = new CIN755SendExpMessageWrapper(messageObject);

			AssertEquals(CIN755SendExpMessageWrapper.MessageNumber, wrapper.MessageEnvelope.SchemaID);
			AssertEquals(CINExportEnvelopeWrapper.edifact, wrapper.MessageEnvelope.SchemaVersion);
			AssertEquals("0000000001", wrapper.MessageEnvelope.TransactionId);

			AssertEquals(CIN755NestedEnvelopeWrapper.OACICode, wrapper.NestedCINMessageEnvelope.OACI);

			AssertEquals("", wrapper.NestedCINMessageEnvelope.MRN_ECS);
			AssertEquals(WrapperTestHelper.OfficeOfExit, wrapper.NestedCINMessageEnvelope.BUR_DOUANE);
			AssertEquals(CIN755NestedEnvelopeWrapper.OACIDestination, wrapper.NestedCINMessageEnvelope.DEST_OACI);
			AssertEquals(WrapperTestHelper.CINNumLta, wrapper.NestedCINMessageEnvelope.NUM_LTA);
			AssertContains("UNH+201231#ID_MESSAGE#+755:2'", wrapper.NestedCINMessageEnvelope.CIN);

			entryHeaderItem.Declaration.UnlockDoMergeMutex();
		}

		protected override void SetUp()
		{
			base.SetUp();

			wrapperHelperItem = new WrapperTestHelper();
			entryHeaderItem = wrapperHelperItem.CreateTestCusEntryHeader(false, true);
		}
		WrapperTestHelper wrapperHelperItem;
		CusEntryHeader entryHeaderItem;
	}
}
