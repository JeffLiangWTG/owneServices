using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	[TestedType(typeof(CC043CPreviousDocumentWithGoodsItemNumberProvider))]
	sealed class CC043CPreviousDocumentWithGoodsItemNumberProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("previousDocument missing", () => new CC043CPreviousDocumentWithGoodsItemNumberProvider(null));
			});
		}

		public void TestGoodsItemNumber()
		{
			AssertEquals("Goods item number", "6", provider.GoodsItemNumber);
		}

		protected override void SetUp()
		{
			provider = new CC043CPreviousDocumentWithGoodsItemNumberProvider(new PreviousDocumentType04()
			{
				SequenceNumber = "1",
				Type = "TRA",
				ReferenceNumber = "12345",
				ComplementOfInformation = "Test",
				GoodsItemNumber = "6"
			});
		}
		CC043CPreviousDocumentWithGoodsItemNumberProvider provider;
	}
}
