using System;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSFINGoodsItemProvider))]
	sealed class CUSFINGoodsItemProviderTest : InboundDataProviderTestCase<ICUSFINGoodsItem, CUSFINGoodsItemProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSFINGoodsItemProvider(null));
		}

		[ExpectNoExceptions]
		public void TestReferencedRegistrationNumber()
		{
			goodsItem.IdentificationByRegistration.ReferencedRegistrationNumber = "ATB150000960420195875";
			NUnit.Framework.Assert.That(dataProvider.ReferencedRegistrationNumber, Is.EqualTo("ATB150000960420195875").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMRN()
		{
			goodsItem.IdentificationByRegistration.MRN = "24DE123050554788M5";
			NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo("24DE123050554788M5"));
		}

		[ExpectNoExceptions]
		public void TestReferencedSequenceNumber()
		{
			goodsItem.IdentificationByRegistration.ReferencedSequenceNumber = "3";
			NUnit.Framework.Assert.That(dataProvider.ReferencedSequenceNumber, Is.EqualTo("3").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestQuantity()
		{
			CombineAssertions(() =>
			{
				goodsItem.Quantity = "A";
				NUnit.Framework.Assert.That(dataProvider.Quantity, Is.EqualTo(0).Using(CustomComparers.TypeComparison), "Character is not numeric");
				goodsItem.Quantity = "2.3";
				NUnit.Framework.Assert.That(dataProvider.Quantity, Is.EqualTo(0).Using(CustomComparers.TypeComparison), "Decimal is not integer");
				goodsItem.Quantity = "3";
				NUnit.Framework.Assert.That(dataProvider.Quantity, Is.EqualTo(3).Using(CustomComparers.TypeComparison), "Parsed as ZInt");
			});
		}
		[ExpectNoExceptions]
		public void TestCancellationFlag()
		{
			goodsItem.CancellationFlag = "J";
			NUnit.Framework.Assert.That(dataProvider.CancellationFlag, Is.EqualTo("J").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			goodsItem = new SCFINGSummaryDeclarationGoodsItem
			{
				IdentificationByRegistration = new SCFINGSummaryDeclarationGoodsItemIdentificationByRegistration()
			};
			dataProvider = new CUSFINGoodsItemProvider(goodsItem);
		}
		SCFINGSummaryDeclarationGoodsItem goodsItem;
		ICUSFINGoodsItem dataProvider;

		protected override CUSFINGoodsItemProvider GetProvider() => (CUSFINGoodsItemProvider)dataProvider;
	}
}
