using System;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSSTAGoodsItemProvider))]
	sealed class CUSSTAGoodsItemProviderTest : InboundDataProviderTestCase<ICUSSTAGoodsItem, CUSSTAGoodsItemProvider>
	{
		[ExpectNoExceptions]
		public void TestMRN() => CombineAssertions(() =>
		{
			goodsItem.IdentificationByRegistration.MRN = "24DE12345678901234";
			NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo("24DE12345678901234"));
			goodsItem.IdentificationByRegistration = null;
			NUnit.Framework.Assert.That(dataProvider.MRN, Is.EqualTo(default(string)));
		});

		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSSTAGoodsItemProvider(null));
		}

		[ExpectNoExceptions]
		public void TestReferencedRegistrationNumber() => CombineAssertions(() =>
		{
			goodsItem.IdentificationByRegistration.ReferencedRegistrationNumber = "ATB150001400420195875";
			NUnit.Framework.Assert.That(dataProvider.ReferencedRegistrationNumber, Is.EqualTo("ATB150001400420195875").Using(CustomComparers.TypeComparison), "ReferencedRegistrationNumber is set");
			goodsItem.IdentificationByRegistration = null;
			NUnit.Framework.Assert.That(dataProvider.ReferencedRegistrationNumber, Is.EqualTo(ZString.Empty), "IdentificationByRegistration is null");
		});

		[ExpectNoExceptions]
		public void TestReferencedSequenceNumber() => CombineAssertions(() =>
		{
			goodsItem.IdentificationByRegistration.ReferencedSequenceNumber = "1";
			NUnit.Framework.Assert.That(dataProvider.ReferencedSequenceNumber, Is.EqualTo("1").Using(CustomComparers.TypeComparison), "ReferencedSequenceNumber is set");

			goodsItem.IdentificationByRegistration = null;
			NUnit.Framework.Assert.That(dataProvider.ReferencedSequenceNumber, Is.EqualTo(ZString.Empty), "IdentificationByRegistration is null");
		});

		[ExpectNoExceptions]
		public void TestQuantity()
		{
			goodsItem.Quantity = "98";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.Quantity, Is.EqualTo(98).Using(CustomComparers.TypeComparison), "Valid integer");
				goodsItem.Quantity = "9ABC8";
				NUnit.Framework.Assert.That(dataProvider.Quantity, Is.EqualTo(0).Using(CustomComparers.TypeComparison), "invalid integer");
			});
		}
		[ExpectNoExceptions]
		public void TestCancellationFlag()
		{
			goodsItem.CancellationFlag = "J";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.CancellationFlag, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "canellation has a value");
				goodsItem.CancellationFlag = ZString.Empty;
				NUnit.Framework.Assert.That(dataProvider.CancellationFlag, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "canellation has no value");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			goodsItem = new SCSTABSummaryDeclarationGoodsItem()
			{
				IdentificationByRegistration = new SCSTABSummaryDeclarationGoodsItemIdentificationByRegistration()
			};
			dataProvider = new CUSSTAGoodsItemProvider(goodsItem);
		}
		SCSTABSummaryDeclarationGoodsItem goodsItem;
		ICUSSTAGoodsItem dataProvider;

		protected override CUSSTAGoodsItemProvider GetProvider() => (CUSSTAGoodsItemProvider)dataProvider;
	}
}
