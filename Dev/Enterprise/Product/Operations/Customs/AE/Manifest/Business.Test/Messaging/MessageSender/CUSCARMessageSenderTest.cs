using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class CUSCARMessageSenderTest : TestCaseWithFactory
{
	public void TestSendBillsMessage()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		var items = header.Bills.Cast<ISelectionItem>();
		var chooser = new MessageChooser(header, items, false);
		chooser.ChooserItems.Cast<MessageChooserItem>().Single().EntryType = EntryTypes.Codes.Original;
		chooser.SelectAll();
		var sender = new CUSCARMessageSender();
		var successfulMessageCount = sender.SendBillMessage(header, chooser);
		header.Factory.Save();

		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(successfulMessageCount, Is.EqualTo(1), "Successful Message Count");
			var message = bill.Messages.Cast<EDIMessage>().Single();
			NUnit.Framework.Assert.That(message.EM_MessageType, Is.EqualTo(AEConstants.Messaging.MessageTypes.CUSCAR).Using(CustomComparers.TypeComparison), "EM_MessageType");
			NUnit.Framework.Assert.That(message.EM_MessageSubType, Is.EqualTo(Common.Shared.MessageSubTypeCodes.Codes.Original).Using(CustomComparers.TypeComparison), "EM_MessageSubType");
			NUnit.Framework.Assert.That(message.EM_SystemCreateUser, Is.EqualTo(GlbStaff.CurrentUser.GS_Code), "Create User");
			NUnit.Framework.Assert.That(message.EM_SendWithMessageErrors, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Send With Message Error");
			AssertNotNullOrEmpty("Message Text", message.EM_MessageText);
			AssertEquals("SNT", bill.Logs
				.Find(a => a.SL_SE_NKEvent == Events.CustomsManifestStatus.Code)
				.OrderByDescending(b => b.SL_PostedTimeUtc)
				.First()
				.SL_Reference);
		});
	}

	[ExpectNoExceptions]
	public void TestMessageSubType()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		var items = header.Bills.Cast<ISelectionItem>();
		var chooser = new MessageChooser(header, items, false);
		chooser.ChooserItems.Cast<MessageChooserItem>().Single().EntryType = EntryTypes.Codes.Cancellation;
		chooser.SelectAll();
		var sender = new CUSCARMessageSender();
		sender.SendBillMessage(header, chooser);

		CombineAssertions(() =>
		{
			var message = bill.Messages.Cast<EDIMessage>().Single();
			NUnit.Framework.Assert.That(message.EM_MessageType, Is.EqualTo(AEConstants.Messaging.MessageTypes.CUSCAR).Using(CustomComparers.TypeComparison), "EM_MessageType");
			NUnit.Framework.Assert.That(message.EM_MessageSubType, Is.EqualTo(Common.Shared.MessageSubTypeCodes.Codes.Cancellation).Using(CustomComparers.TypeComparison), "EM_MessageSubType");
			bill.Messages.DeleteAll();

			chooser.ChooserItems.Cast<MessageChooserItem>().Single().EntryType = EntryTypes.Codes.Change;
			sender.SendBillMessage(header, chooser);
			message = bill.Messages.Cast<EDIMessage>().Single();
			NUnit.Framework.Assert.That(message.EM_MessageSubType, Is.EqualTo(Common.Shared.MessageSubTypeCodes.Codes.Change).Using(CustomComparers.TypeComparison), "EM_MessageSubType");
			bill.Messages.DeleteAll();

			chooser.ChooserItems.Cast<MessageChooserItem>().Single().EntryType = EntryTypes.Codes.Original;
			sender.SendBillMessage(header, chooser);
			message = bill.Messages.Cast<EDIMessage>().Single();
			NUnit.Framework.Assert.That(message.EM_MessageSubType, Is.EqualTo(Common.Shared.MessageSubTypeCodes.Codes.Original).Using(CustomComparers.TypeComparison), "EM_MessageSubType");
		});
	}

	[ExpectNoExceptions]
	public void TestSendWithMessageErrors()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		var items = header.Bills.Cast<ISelectionItem>();
		var chooser = new MessageChooser(header, items, false);
		chooser.ChooserItems.Cast<MessageChooserItem>().Single().EntryType = EntryTypes.Codes.Original;
		chooser.SelectAll();
		var sender = new CUSCARMessageSender();
		sender.SendBillMessage(header, chooser);

		CombineAssertions(() =>
		{
			var message = bill.Messages.Cast<EDIMessage>().Single();
			NUnit.Framework.Assert.That(message.EM_SendWithMessageErrors, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Send With Message Error");
			bill.Messages.DeleteAll();

			header.AddRowMessageError("Just for Test");
			chooser = new MessageChooser(header, items, false);
			chooser.SelectAll();
			sender.SendBillMessage(header, chooser);
			message = bill.Messages.Cast<EDIMessage>().Single();
			NUnit.Framework.Assert.That(message.EM_SendWithMessageErrors, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Send With Message Error");
		});
	}

	public void TestSendWithMessageWhenContainerIsNullInPacks()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		var items = header.Bills.Cast<ISelectionItem>();
		var pack = bill.Packs.AddNew();
		var chooser = new MessageChooser(header, items, false);
		chooser.ChooserItems.Cast<MessageChooserItem>().Single().EntryType = EntryTypes.Codes.Cancellation;
		chooser.SelectAll();
		var sender = new CUSCARMessageSender();
		AssertNoExceptionThrown(() => sender.SendBillMessage(header, chooser));
	}
}
