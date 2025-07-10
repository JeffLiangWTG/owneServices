using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AE.Business;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class ManifestMessageBillProviderTest : TestCaseWithFactory
{
	[ExpectNoExceptions]
	public void TestGetAttachee()
	{
		var message = Factory.New<EDIMessage>();
		var bill1 = Factory.New<AsycudaBill>();
		bill1.ABL_SenderReference = "REF_1";

		var bill2 = Factory.New<AsycudaBill>();
		bill2.ABL_SenderReference = "REF_2";

		var mockDataProvider = new Mock<ICUSRESDataProvider>();
		mockDataProvider.Setup(x => x.OutgoingAccessReference).Returns("REF_1");
		var provider = new ManifestMessageBillProvider();
		NUnit.Framework.Assert.That(provider.GetAttachee(message, mockDataProvider.Object), Is.EqualTo(bill1).Using(CustomComparers.TypeComparison));
	}
}
