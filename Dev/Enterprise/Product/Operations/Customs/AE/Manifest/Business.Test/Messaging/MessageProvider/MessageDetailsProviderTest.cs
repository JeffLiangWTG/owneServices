using System;
using Enterprise.Customs.AE.Business;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class MessageDetailsProviderTest : Customs.Business.Testing.DataProviderTestCase<MessageDetailsProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.Throws<ArgumentNullException>(() => new MessageDetailsProvider(null), "MessageItem is null");
			AssertNoExceptionThrown("Valid arguments", () => new MessageDetailsProvider(messageItem));
		});
	}

	[ExpectNoExceptions]
	public void TestDocumentCode()
	{
		CombineAssertions(() =>
		{
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			NUnit.Framework.Assert.That(GetProvider().DocumentCode, Is.EqualTo(DocumentNameCodeList.HouseBillOfLading.ToString()), "BOL Type STD");

			bill.ABL_BolType = Core.Constants.ShipmentTypes.CoLoadMaster;
			NUnit.Framework.Assert.That(GetProvider().DocumentCode, Is.EqualTo(DocumentNameCodeList.ForwardersBillOfLading.ToString()), "BOL Type CLD");
		});
	}

	[ExpectNoExceptions]
	public void TestDocumentIdentifier() => NUnit.Framework.Assert.That(GetProvider().DocumentIdentifier, Is.EqualTo(EDIMessage.SendersReferencePlaceHolder));

	[ExpectNoExceptions]
	public void TestVersion()
	{
		CombineAssertions(() =>
		{
			messageItem.EntryType = EntryTypes.Codes.Original;
			NUnit.Framework.Assert.That(GetProvider().Version, Is.EqualTo("001"), "New Filing");

			messageItem.EntryType = EntryTypes.Codes.Cancellation;
			NUnit.Framework.Assert.That(GetProvider().Version, Is.Null.Or.Empty, "Cancel Filing - should be [null] or [empty]");

			messageItem.EntryType = EntryTypes.Codes.Change;
			NUnit.Framework.Assert.That(GetProvider().Version, Is.EqualTo("001"), "Amendment Filing - 0 CUSRES Message");

			var cusRes = bill.Messages.AddNew();
			cusRes.EM_MessageType = AEConstants.Messaging.MessageTypes.CUSRES;
			NUnit.Framework.Assert.That(GetProvider().Version, Is.EqualTo("002"), "Amendment Filing - 1 CUSRES Message");

			var contrl = bill.Messages.AddNew();
			contrl.EM_MessageType = AEConstants.Messaging.MessageTypes.CONTRL;
			NUnit.Framework.Assert.That(GetProvider().Version, Is.EqualTo("002"), "Amendment Filing - 1 CUSRES Message");

			messageItem.EntryType = "0";
			NUnit.Framework.Assert.That(GetProvider().Version, Is.Null.Or.Empty, "Other Filing - should be [null] or [empty]");
		});
	}

	[ExpectNoExceptions]
	public void TestMessageFunction() => CombineAssertions(() =>
	{
		messageItem.EntryType = EntryTypes.Codes.Original;
		NUnit.Framework.Assert.That(GetProvider().MessageFunction, Is.EqualTo("9"), "Original");
		messageItem.EntryType = EntryTypes.Codes.Change;
		NUnit.Framework.Assert.That(GetProvider().MessageFunction, Is.EqualTo("9"), "Change");
		messageItem.EntryType = EntryTypes.Codes.Cancellation;
		NUnit.Framework.Assert.That(GetProvider().MessageFunction, Is.EqualTo("1"), "Cancellation");
	});

	protected override MessageDetailsProvider GetProvider() => new MessageDetailsProvider(messageItem);

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<AsycudaManifestHeader>();
		bill = header.Bills.AddNew();
		var messageChooser = new MessageChooser(header, new[] { bill }, false);
		messageItem = new MessageChooserItem(messageChooser, bill, false);
	}
	AsycudaBill bill;
	MessageChooserItem messageItem;
}
