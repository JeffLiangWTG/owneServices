using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ImportSiscomexMessageSendingObject))]
	public class ImportSiscomexMessageSendingObjectTest : DeclarationMessageSendingObjectTest
	{
		protected override ZString ExpectedDefaultMessageType => ImportSiscomexActionCodeList.Codes.ANA;

		protected override IReadOnlyList<string> ExpectedMessageTypeCodes => new string[] { "ANA", "ORI" };

		protected override ZString ExpectedMessageTypeForEDIMessage => MessageTypeList.Codes.CDI;

		protected override ZString ExpectedFriendlyNameForMessageManager => MessageTypeList.Descriptions.CDI;

		protected override ZString JobDeclarationMessageType => BRJobMessageTypeList.Codes.ImportSiscomex;

		protected override DeclarationMessageSendingObject CreateNewMessageSendingObject(CusEntryHeader entryHeader) => new ImportSiscomexMessageSendingObject(entryHeader);

		public void TestProperties()
		{
			var sendingObject = GetNewMessageSendingObjectForTesting();
			var entryHeader = sendingObject.Header;
			entryHeader.MovementReferenceNumberSetter("ABC", ZDateTime.BrettsBirthday);
			entryHeader.CH_EntrySubmittedDate = new ZDateTime(2020, 12, 12);
			entryHeader.CH_Status = "XXX";
			var entryInstruction = entryHeader.EntryInstruction;
			entryInstruction.CEI_SubStyle = "Bye";
			AssertEquals(true, sendingObject.ShouldSend);
			AssertEquals(new ZDateTime(2020, 12, 12), sendingObject.SubmittedDate);
			AssertEquals("XXX", sendingObject.CustomsStatus);
			AssertEquals("ABC", sendingObject.GetApplicationReference());
		}

		public void TestShouldSend()
		{
			var sendingObject = GetNewMessageSendingObjectForTesting();
			AssertEquals("ShouldSend", true, sendingObject.ShouldSend);
		}

		public void TestGetMessageText()
		{
			var sendingObject = GetNewMessageSendingObjectForTesting();
			AssertContains("<ListaDeclaracoesTransmissao", sendingObject.GetMessageText());
		}
	}
}
