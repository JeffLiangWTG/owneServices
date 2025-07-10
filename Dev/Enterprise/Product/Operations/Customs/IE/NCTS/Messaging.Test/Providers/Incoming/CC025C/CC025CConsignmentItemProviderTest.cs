using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC025CConsignmentItemProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("ConsignmentItemType missing", () => new CC025CConsignmentItemProvider(null));
			});
		}

		public void TestGoodsItemNumber()
		{
			AssertEquals("1", provider.GoodsItemNumber);
		}

		public void TestDeclarationGoodsItemNumber()
		{
			AssertEquals("4", provider.DeclarationGoodsItemNumber);
		}

		public void TestReleaseType()
		{
			AssertEquals("2", provider.ReleaseType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC025CConsignmentItemProvider(new ConsignmentItemType02
			{
				GoodsItemNumber = "1",
				DeclarationGoodsItemNumber = "4",
				ReleaseType = "2"
			});
		}
		CC025CConsignmentItemProvider provider;
	}
}
