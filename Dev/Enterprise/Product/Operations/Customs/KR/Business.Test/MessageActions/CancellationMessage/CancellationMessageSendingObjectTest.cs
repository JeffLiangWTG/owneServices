using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CancellationMessageSendingObject))]
	sealed class CancellationMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			return new CancellationMessageSendingObject(entry);
		}

		public void TestPopulate()
		{
			var entrynum = CreateEntryForImport();

			var message = SetUpMessageData("GOVCBR5BF_0.xml", "5BF");
			var amendCancelMessageSendingObject = new CancellationMessageSendingObject(importEntry);
			Factory.Save();
			amendCancelMessageSendingObject.Populate(message);
			AssertEquals("신청", amendCancelMessageSendingObject.CancellationReason);
			AssertEquals(ZDateTime.Empty, amendCancelMessageSendingObject.DateOfApplication);

			entrynum.CE_ExpiryDate = new ZDateTime("2020-01-01");
			message = SetUpMessageData("GOVCBR5BF_1.xml", "5BF");
			Factory.Save();
			amendCancelMessageSendingObject.Populate(message);
			AssertEquals("사유", amendCancelMessageSendingObject.CancellationReason);
			AssertEquals(ZDateTime.Empty, amendCancelMessageSendingObject.DateOfApplication);

			entrynum.CE_ExpiryDate = ZDateTime.Empty;
			message = SetUpMessageData("GOVCBR5BF.xml", "5BF");
			Factory.Save();
			amendCancelMessageSendingObject.Populate(message);
			AssertEquals("신청사유", amendCancelMessageSendingObject.CancellationReason);
			AssertEquals(new ZDateTime(2014, 05, 06), amendCancelMessageSendingObject.DateOfApplication);
		}

		CusEntryHeader importEntry;
		public CusEntryNumber CreateEntryForImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			importEntry = declaration.CustomsEntryHeaders.AddNew();

			var entryNumber = importEntry.EntryNumbers.AddNew();
			entryNumber.CE_EntryNum = "1234520000045M";
			entryNumber.CE_EntryType = "IMP";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ParentID = importEntry.PK;
			entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;
			entryNumber.CE_EntryStatus = ZString.Empty;

			return entryNumber;
		}

		EDIMessage SetUpMessageData(string fileName, string messageType)
		{
			var fileReader = new TestFileReader(typeof(CancellationMessageSendingObjectTest));
			var messageText = fileReader.GetEmbeddedFileText("Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing", fileName);
			var message = importEntry.Messages.AddNew();
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_LinkTable = CusEntryHeader.Schema.TableName;
			message.EM_LinkUniqueID = importEntry.PK;
			message.EM_LinkedObject = importEntry;
			message.EM_MessageText = messageText;
			return message;
		}
	}
}
