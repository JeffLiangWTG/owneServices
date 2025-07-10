using System;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSNOAGoodsItemProvider))]
	sealed class CUSNOAGoodsItemProviderTest : InboundDataProviderTestCase<ICUSNOAGoodsItem, CUSNOAGoodsItemProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSNOAGoodsItemProvider(null));
		}

		[ExpectNoExceptions]
		public void TestSequenceNumber()
		{
			goodsItem.SequenceNumber = "1";
			NUnit.Framework.Assert.That(dataProvider.SequenceNumber, Is.EqualTo("1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDocumentType_DocumentIsNull()
		{
			NUnit.Framework.Assert.That(dataProvider.DocumentType, Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestDocumentType()
		{
			goodsItem.Document = new GCNOADGoodsItemDocument() { Type = "I004" };
			NUnit.Framework.Assert.That(dataProvider.DocumentType, Is.EqualTo("I004").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDocumentReference_DocumentIsNull()
		{
			NUnit.Framework.Assert.That(dataProvider.DocumentReference, Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestDocumentReference()
		{
			goodsItem.Document = new GCNOADGoodsItemDocument() { ReferenceNumber = "80553609" };
			NUnit.Framework.Assert.That(dataProvider.DocumentReference, Is.EqualTo("80553609").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCancellationWriteOffFlag_DocumentIsNull()
		{
			NUnit.Framework.Assert.That(dataProvider.CancellationWriteOffFlag, Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestCancellationWriteOffFlag()
		{
			goodsItem.Document = new GCNOADGoodsItemDocument() { CancellationWriteOffFlag = "N" };
			NUnit.Framework.Assert.That(dataProvider.CancellationWriteOffFlag, Is.EqualTo("N").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();

			goodsItem = new GCNOADGoodsItem();
			dataProvider = new CUSNOAGoodsItemProvider(goodsItem);
		}
		GCNOADGoodsItem goodsItem;
		ICUSNOAGoodsItem dataProvider;

		protected override CUSNOAGoodsItemProvider GetProvider() => (CUSNOAGoodsItemProvider)dataProvider;
	}
}
