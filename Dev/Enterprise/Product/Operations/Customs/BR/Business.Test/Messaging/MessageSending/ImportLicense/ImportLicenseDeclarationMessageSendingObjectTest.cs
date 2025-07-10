using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ImportLicenseMessageSendingObject))]
	public class ImportLicenseDeclarationMessageSendingObjectTest : DeclarationMessageSendingObjectTest
	{
		protected override ZString ExpectedDefaultMessageType => ImportLicenseActionCodeList.Codes.ORI;

		protected override IReadOnlyList<string> ExpectedMessageTypeCodes => new string[] { "ORI" };

		protected override ZString ExpectedMessageTypeForEDIMessage => MessageTypeList.Codes.LIC;

		protected override ZString ExpectedFriendlyNameForMessageManager => MessageTypeList.Descriptions.LIC;

		protected override ZString JobDeclarationMessageType => BRJobMessageTypeList.Codes.ImportLicense;

		protected override DeclarationMessageSendingObject CreateNewMessageSendingObject(CusEntryHeader entryHeader) => new ImportLicenseMessageSendingObject(entryHeader);

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
			AssertEquals(ZString.Empty, sendingObject.GetApplicationReference());
		}

		public void TestShouldSend()
		{
			var sendingObject = GetNewMessageSendingObjectForTesting();
			AssertEquals("ShouldSend", true, sendingObject.ShouldSend);
		}

		public void TestGetMessageText()
		{
			var sendingObject = GetNewMessageSendingObjectForTesting();
			AssertContains("<identificador-li />", sendingObject.GetMessageText());
		}
	}
}
