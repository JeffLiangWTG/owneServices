using System;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSFSTGoodsItemProvider))]
	sealed class CUSFSTGoodsItemProviderTest : InboundDataProviderTestCase<IUnderCustomsControlGoodsItem, CUSFSTGoodsItemProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSFSTGoodsItemProvider(null));
		}

		[ExpectNoExceptions]
		public void TestSequenceNumber()
		{
			goodsItem.SequenceNumber = "11";
			NUnit.Framework.Assert.That(dataProvider.SequenceNumber, Is.EqualTo("11").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCustodianReferenceNumber()
		{
			goodsItem.Storer.Identification.ReferenceNumber = "DE8999783";
			NUnit.Framework.Assert.That(dataProvider.CustodianReferenceNumber, Is.EqualTo("DE8999783").Using(CustomComparers.TypeComparison));

			goodsItem.Storer.Identification = null;
			NUnit.Framework.Assert.That(dataProvider.CustodianReferenceNumber, Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestCustodianSubsidiaryNumber()
		{
			goodsItem.Storer.Identification.SubsidiaryNumber = "0001";
			NUnit.Framework.Assert.That(dataProvider.CustodianSubsidiaryNumber, Is.EqualTo("0001").Using(CustomComparers.TypeComparison));

			goodsItem.Storer.Identification = null;
			NUnit.Framework.Assert.That(dataProvider.CustodianSubsidiaryNumber, Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestDisposalEntitledTraderReferenceNumber()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.DisposalEntitledTraderReferenceNumber, Is.EqualTo(ZString.Empty), "No Disposal Entitled Trader");
				goodsItem.DisposalEntitledTrader = new SCFSTFGoodsItemDisposalEntitledTrader
				{
					Identification = new SCFSTFGoodsItemDisposalEntitledTraderIdentification
					{
						ReferenceNumber = "GR1-Z"
					}
				};
				NUnit.Framework.Assert.That(dataProvider.DisposalEntitledTraderReferenceNumber, Is.EqualTo("GR1-Z").Using(CustomComparers.TypeComparison), "Refernce Number");

				goodsItem.DisposalEntitledTrader.Identification = null;
				NUnit.Framework.Assert.That(dataProvider.DisposalEntitledTraderReferenceNumber, Is.EqualTo(ZString.Empty), "Refernce Number");
			});
		}

		[ExpectNoExceptions]
		public void TestDisposalEntitledTraderSubsidiaryNumber()
		{
			NUnit.Framework.Assert.That(dataProvider.DisposalEntitledTraderReferenceNumber, Is.EqualTo(ZString.Empty), "No Disposal Entitled Trader");
			goodsItem.DisposalEntitledTrader = new SCFSTFGoodsItemDisposalEntitledTrader
			{
				Identification = new SCFSTFGoodsItemDisposalEntitledTraderIdentification
				{
					SubsidiaryNumber = "0104"
				}
			};
			NUnit.Framework.Assert.That(dataProvider.DisposalEntitledTraderSubsidiaryNumber, Is.EqualTo("0104").Using(CustomComparers.TypeComparison));

			goodsItem.DisposalEntitledTrader.Identification = null;
			NUnit.Framework.Assert.That(dataProvider.DisposalEntitledTraderSubsidiaryNumber, Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestOwnerReferenceType()
		{
			goodsItem.ClassificationKeyKind = "ZZZ";
			NUnit.Framework.Assert.That(dataProvider.OwnerReferenceType, Is.EqualTo("ZZZ").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestOwnerReferenceNumber()
		{
			goodsItem.ClassificationKeyNumber = "NCTS-PosNr: 00011;";
			NUnit.Framework.Assert.That(dataProvider.OwnerReferenceNumber, Is.EqualTo("NCTS-PosNr: 00011;").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLocationOfGoods()
		{
			goodsItem.StoragePlaceCode = "1";
			NUnit.Framework.Assert.That(dataProvider.LocationOfGoods, Is.EqualTo("1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGoodsDescription()
		{
			goodsItem.GoodsDescription = "elektronische Untersuchungsgeräte";
			NUnit.Framework.Assert.That(dataProvider.GoodsDescription, Is.EqualTo("elektronische Untersuchungsgeräte").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPackageType()
		{
			goodsItem.Package.Kind = "CS";
			NUnit.Framework.Assert.That(dataProvider.PackageType, Is.EqualTo("CS").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCustomsGoodsStatus()
		{
			goodsItem.CustomsGoodsStatus = "C";
			NUnit.Framework.Assert.That(dataProvider.CustomsGoodsStatus, Is.EqualTo("C").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPackageQty()
		{
			CombineAssertions(() =>
			{
				goodsItem.Package.Quantity = "A";
				NUnit.Framework.Assert.That(dataProvider.PackageQty, Is.EqualTo(0).Using(CustomComparers.TypeComparison), "Character is not numeric");
				goodsItem.Package.Quantity = "2.3";
				NUnit.Framework.Assert.That(dataProvider.PackageQty, Is.EqualTo(0).Using(CustomComparers.TypeComparison), "Decimal is not integer");
				goodsItem.Package.Quantity = "12";
				NUnit.Framework.Assert.That(dataProvider.PackageQty, Is.EqualTo(12).Using(CustomComparers.TypeComparison), "Parsed as ZInt");
			});
		}

		[ExpectNoExceptions]
		public void TestGrossWeight()
		{
			goodsItem.GrossMassMeasure = 5645.45m;
			NUnit.Framework.Assert.That(dataProvider.GrossWeight, Is.EqualTo(5645.45m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLimitDate()
		{
			NUnit.Framework.Assert.That(dataProvider.LimitDate, Is.EqualTo(ZDate.Empty));
		}

		protected override void SetUp()
		{
			base.SetUp();
			goodsItem = new SCFSTFGoodsItem()
			{
				Storer = new SCFSTFGoodsItemStorer()
				{
					Identification = new SCFSTFGoodsItemStorerIdentification()
				},
				Package = new SCFSTFGoodsItemPackage()
			};
			dataProvider = new CUSFSTGoodsItemProvider(goodsItem);
		}
		SCFSTFGoodsItem goodsItem;
		IUnderCustomsControlGoodsItem dataProvider;

		protected override CUSFSTGoodsItemProvider GetProvider() => (CUSFSTGoodsItemProvider)dataProvider;
	}
}
