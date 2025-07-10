using System;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	[TestedType(typeof(CUSTSTGoodsItemProvider))]
	sealed class CUSTSTGoodsItemProviderTest : InboundDataProviderTestCase<ICUSTSTGoodsItem, CUSTSTGoodsItemProvider>
	{
		public void TestConstructorThrowsArgumentException()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSTSTGoodsItemProvider(null));
		}

		[ExpectNoExceptions]
		public void TestSequenceNumber()
		{
			goodsItem.SequenceNumber = "1";
			NUnit.Framework.Assert.That(dataProvider.SequenceNumber, Is.EqualTo("1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCustodianReferenceNumber()
		{
			CombineAssertions(() =>
			{
				goodsItem.Custodian.Identification.ReferenceNumber = "DE8999783";
				NUnit.Framework.Assert.That(dataProvider.CustodianReferenceNumber, Is.EqualTo("DE8999783").Using(CustomComparers.TypeComparison));

				goodsItem.Custodian.Identification = null;
				NUnit.Framework.Assert.That(dataProvider.CustodianReferenceNumber, Is.EqualTo(ZString.Empty));

				goodsItem.Custodian = null;
				NUnit.Framework.Assert.That(dataProvider.CustodianReferenceNumber, Is.EqualTo(ZString.Empty));
			});
		}

		[ExpectNoExceptions]
		public void TestCustodianSubsidiaryNumber()
		{
			CombineAssertions(() =>
			{
				goodsItem.Custodian.Identification.SubsidiaryNumber = "0001";
				NUnit.Framework.Assert.That(dataProvider.CustodianSubsidiaryNumber, Is.EqualTo("0001").Using(CustomComparers.TypeComparison));

				goodsItem.Custodian.Identification = null;
				NUnit.Framework.Assert.That(dataProvider.CustodianSubsidiaryNumber, Is.EqualTo(ZString.Empty));

				goodsItem.Custodian = null;
				NUnit.Framework.Assert.That(dataProvider.CustodianSubsidiaryNumber, Is.EqualTo(ZString.Empty));
			});
		}

		[ExpectNoExceptions]
		public void TestCustodianName()
		{
			CombineAssertions(() =>
			{
				goodsItem.Custodian.Name = "Name 1";
				NUnit.Framework.Assert.That(dataProvider.CustodianName, Is.EqualTo("Name 1").Using(CustomComparers.TypeComparison));

				goodsItem.Custodian = null;
				NUnit.Framework.Assert.That(dataProvider.CustodianName, Is.EqualTo(ZString.Empty));
			});
		}

		[ExpectNoExceptions]
		public void TestCustodianAddress()
		{
			CombineAssertions(() =>
			{
				var custodianAddress = dataProvider.CustodianAddress;
				NUnit.Framework.Assert.That(typeof(IUnderCustomsControlGoodsItemAddress).IsAssignableFrom(custodianAddress.GetType()), Is.EqualTo(true), "Is IUnderCustomsControlGoodsItemAddress");
				NUnit.Framework.Assert.That(dataProvider.CustodianAddress, Is.SameAs(custodianAddress), "Cached");
			});
		}

		[ExpectNoExceptions]
		public void TestCustodyPlaceCode()
		{
			goodsItem.CustodyPlaceCode = "1";
			NUnit.Framework.Assert.That(dataProvider.CustodyPlaceCode, Is.EqualTo("1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCustodyPlaceInformation()
		{
			CombineAssertions(() =>
			{
				goodsItem.CustodyPlace.Information = "Information 1";
				NUnit.Framework.Assert.That(dataProvider.CustodyPlaceInformation, Is.EqualTo("Information 1").Using(CustomComparers.TypeComparison));

				goodsItem.CustodyPlace = null;
				NUnit.Framework.Assert.That(dataProvider.CustodyPlaceInformation, Is.EqualTo(ZString.Empty));
			});
		}

		[ExpectNoExceptions]
		public void TestCustodyPlaceAddress()
		{
			CombineAssertions(() =>
			{
				var custodyPlaceAddress = dataProvider.CustodyPlaceAddress;
				NUnit.Framework.Assert.That(typeof(IUnderCustomsControlGoodsItemAddress).IsAssignableFrom(custodyPlaceAddress.GetType()), Is.EqualTo(true), "Is IUnderCustomsControlGoodsItemAddress");
				NUnit.Framework.Assert.That(dataProvider.CustodyPlaceAddress, Is.SameAs(custodyPlaceAddress), "Cached");
			});
		}

		[ExpectNoExceptions]
		public void TestDisposalEntitledTraderReferenceNumber()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.DisposalEntitledTraderReferenceNumber, Is.EqualTo(ZString.Empty), "No Disposal Entitled Trader");
				goodsItem.DisposalEntitledTrader = new SCTSTJGoodsItemDisposalEntitledTrader
				{
					Identification = new SCTSTJGoodsItemDisposalEntitledTraderIdentification
					{
						ReferenceNumber = "DE8999715"
					}
				};
				NUnit.Framework.Assert.That(dataProvider.DisposalEntitledTraderReferenceNumber, Is.EqualTo("DE8999715").Using(CustomComparers.TypeComparison));

				goodsItem.DisposalEntitledTrader.Identification = null;
				NUnit.Framework.Assert.That(dataProvider.DisposalEntitledTraderReferenceNumber, Is.EqualTo(ZString.Empty));
			});
		}

		[ExpectNoExceptions]
		public void TestDisposalEntitledTraderSubsidiaryNumber()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.DisposalEntitledTraderReferenceNumber, Is.EqualTo(ZString.Empty), "No Disposal Entitled Trader");
				goodsItem.DisposalEntitledTrader = new SCTSTJGoodsItemDisposalEntitledTrader
				{
					Identification = new SCTSTJGoodsItemDisposalEntitledTraderIdentification
					{
						SubsidiaryNumber = "0000"
					}
				};
				NUnit.Framework.Assert.That(dataProvider.DisposalEntitledTraderSubsidiaryNumber, Is.EqualTo("0000").Using(CustomComparers.TypeComparison));

				goodsItem.DisposalEntitledTrader.Identification = null;
				NUnit.Framework.Assert.That(dataProvider.DisposalEntitledTraderReferenceNumber, Is.EqualTo(ZString.Empty));
			});
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
			goodsItem.ClassificationKeyNumber = "TEST01";
			NUnit.Framework.Assert.That(dataProvider.OwnerReferenceNumber, Is.EqualTo("TEST01").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLocationOfGoods()
		{
			goodsItem.CustodyPlaceCode = "2";
			NUnit.Framework.Assert.That(dataProvider.LocationOfGoods, Is.EqualTo("2").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGoodsDescription()
		{
			goodsItem.GoodsDescription = "Holzspielzeug";
			NUnit.Framework.Assert.That(dataProvider.GoodsDescription, Is.EqualTo("Holzspielzeug").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPackageType()
		{
			goodsItem.Package.Kind = "PC";
			NUnit.Framework.Assert.That(dataProvider.PackageType, Is.EqualTo("PC").Using(CustomComparers.TypeComparison));
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
				goodsItem.Package.Quantity = "10";
				NUnit.Framework.Assert.That(dataProvider.PackageQty, Is.EqualTo(10).Using(CustomComparers.TypeComparison), "Parsed as ZInt");
			});
		}

		[ExpectNoExceptions]
		public void TestGrossWeight()
		{
			goodsItem.GrossMassMeasure = 10.15m;
			NUnit.Framework.Assert.That(dataProvider.GrossWeight, Is.EqualTo(10.15m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLimitDate()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(dataProvider.LimitDate, Is.EqualTo(ZDate.Empty), "Limit Date not specified");
				goodsItem.DeclarationLimitDateSpecified = true;
				goodsItem.DeclarationLimitDate = new DateTime(2019, 5, 27);
				NUnit.Framework.Assert.That(dataProvider.LimitDate, Is.EqualTo(new ZDate(2019, 5, 27)), "Limit Date specified");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			goodsItem = new SCTSTJGoodsItem()
			{
				Custodian = new SCTSTJGoodsItemCustodian()
				{
					Identification = new SCTSTJGoodsItemCustodianIdentification(),
					Address = new SCTSTJGoodsItemCustodianAddress()
				},
				Package = new SCTSTJGoodsItemPackage(),
				CustodyPlace = new SCTSTJGoodsItemCustodyPlace()
				{
					Address = new SCTSTJGoodsItemCustodyPlaceAddress()
				}
			};
			dataProvider = new CUSTSTGoodsItemProvider(goodsItem);
		}
		SCTSTJGoodsItem goodsItem;
		ICUSTSTGoodsItem dataProvider;

		protected override CUSTSTGoodsItemProvider GetProvider() => (CUSTSTGoodsItemProvider)dataProvider;
	}
}
