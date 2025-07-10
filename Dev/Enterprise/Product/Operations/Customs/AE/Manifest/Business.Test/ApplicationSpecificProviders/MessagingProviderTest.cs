using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class MessagingProviderTest : TestCaseWithFactory
{
	[ExpectNoExceptions]
	public void TestMessageStatusProvider()
	{
		NUnit.Framework.Assert.That(new MessagingProvider().MessageStatusProvider, Is.TypeOf<MessageStatusProvider>());
	}

	[ExpectNoExceptions]
	public void TestGetMostSevereValueMessageStatus()
	{
		var provider = new MessagingProvider();
		var header = Factory.New<AsycudaManifestHeader>();
		NUnit.Framework.Assert.That(provider.GetMostSevereValueMessageStatus(header).ToString(), Is.Empty, "No bills, Message Status should be empty.");

		var bill1 = header.Bills.AddNew();
		var bill2 = header.Bills.AddNew();
		bill1.ABL_MessageStatus = "A";
		bill2.ABL_MessageStatus = "B";
		NUnit.Framework.Assert.That(provider.GetMostSevereValueMessageStatus(header).ToString(), Is.Empty, "Empty when multiple bills Message Statuses");

		bill1.ABL_MessageStatus = "B";
		NUnit.Framework.Assert.That(provider.GetMostSevereValueMessageStatus(header).ToString(), Is.EqualTo("B"), "Single bills Message Status");
	}

	[ExpectNoExceptions]
	public void TestGetMostSevereValueCustomsStatus()
	{
		var provider = new MessagingProvider();
		var header = Factory.New<AsycudaManifestHeader>();
		NUnit.Framework.Assert.That(provider.GetMostSevereValueCustomsStatus(header).ToString(), Is.Empty, "No bills, Customs Status should be empty.");

		var bill1 = header.Bills.AddNew();
		var bill2 = header.Bills.AddNew();
		bill1.ABL_BillStatus = "A";
		bill2.ABL_BillStatus = "B";
		NUnit.Framework.Assert.That(provider.GetMostSevereValueCustomsStatus(header).ToString(), Is.Empty, "Empty when multiple bills Customs Statuses");

		bill1.ABL_BillStatus = "B";
		NUnit.Framework.Assert.That(provider.GetMostSevereValueCustomsStatus(header).ToString(), Is.EqualTo("B"), "Single bills Customs Status");
	}
}
