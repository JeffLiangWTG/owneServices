using System;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4.Testing
{
	[TestedType(typeof(ED801LineProvider))]
	class ED801LineProviderTest : InboundDataProviderTestCase<IED801Line, ED801LineProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED801LineProvider(null));
		}

		public void TestBodyRecordUniqueReference()
		{
			AssertEquals(1, dataProvider.BodyRecordUniqueReference);
		}

		public void TestExciseProductCode()
		{
			AssertEquals("B000", dataProvider.ExciseProductCode);
		}

		public void TestCnCode()
		{
			AssertEquals("22030001100", dataProvider.CnCode);
		}

		public void TestFiscalMarkUsedFlag()
		{
			CombineAssertions(() =>
			{
				AssertEquals(true, dataProvider.FiscalMarkUsedFlag);
				line.FiscalMarkUsedFlagSpecified = false;
				AssertEquals("FiscalMarkUsedFlag is not specified", false, dataProvider.FiscalMarkUsedFlag);
			});
		}
		public void TestFiscalMark()
		{
			AssertEquals("FM001", dataProvider.FiscalMark);
		}

		public void TestDesignationOfOrigin()
		{
			AssertEquals("AU", dataProvider.DesignationOfOrigin);
		}

		public void TestCommercialDescription()
		{
			AssertEquals("C Description 1", dataProvider.CommercialDescription);
		}

		public void TestBrandNameOfProducts()
		{
			AssertEquals("Brand Name 1", dataProvider.BrandNameOfProducts);
		}

		public void TestQuantity()
		{
			AssertEquals(20m, dataProvider.Quantity);
		}

		public void TestGrossWeight()
		{
			AssertEquals(300.2m, dataProvider.GrossWeight);
		}

		public void TestNetWeight()
		{
			AssertEquals(100.1m, dataProvider.NetWeight);
		}

		public void TestAlcoholicStrength()
		{
			CombineAssertions(() =>
			{
				AssertEquals(200.2m, dataProvider.AlcoholicStrength);
				line.AlcoholicStrengthSpecified = false;
				AssertEquals("AlcoholicStrength is not Specified", 0m, dataProvider.AlcoholicStrength);
			});
		}

		public void TestDegreePlato()
		{
			CombineAssertions(() =>
			{
				AssertEquals(400.4m, dataProvider.DegreePlato);
				line.DegreePlatoSpecified = false;
				AssertEquals("DegreePlato is not Specified", 0m, dataProvider.DegreePlato);
			});
		}

		public void TestSizeOfProducer()
		{
			AssertEquals(30m, dataProvider.SizeOfProducer);
		}

		public void TestSizeOfProducer_Null()
		{
			line.SizeOfProducer = null;
			AssertEquals(0m, dataProvider.SizeOfProducer);
		}

		public void TestDensity()
		{
			CombineAssertions(() =>
			{
				AssertEquals(500.5m, dataProvider.Density);
				line.DensitySpecified = false;
				AssertEquals("Density is not Specified", 0m, dataProvider.Density);
			});
		}

		public void TestPackages()
		{
			CombineAssertions(() =>
			{
				var packages = dataProvider.Packages;
				AssertEquals("Cached", packages, dataProvider.Packages);
				AssertEquals("Have 2 packages", 2, packages.Count);
			});
		}

		public void TestWineGrowingZoneCode()
		{
			AssertEquals("Z1", dataProvider.WineGrowingZoneCode);
		}

		public void TestWineProductCategory()
		{
			AssertEquals("1", dataProvider.WineProductCategory);
		}

		public void TestWineProductThirdCountryOfOrigin()
		{
			AssertEquals("NZ", dataProvider.WineProductThirdCountryOfOrigin);
		}

		public void TestWineProductOtherInfo()
		{
			AssertEquals("Other Info 001", dataProvider.WineProductOtherInfo);
		}

		public void TestWineOperationCodes()
		{
			CombineAssertions(() =>
			{
				var wineOperationCodes = dataProvider.WineOperationCodes;
				AssertEquals("Cached", wineOperationCodes, dataProvider.WineOperationCodes);
				AssertContainsExactElementsInAnyOrder("WineOperationCodes", new[] { "WOC001", "WOC002" }, wineOperationCodes);
			});
		}

		public void TestWineProductIsNull()
		{
			line.WineProduct = null;
			CombineAssertions(() =>
			{
				AssertEquals("WineGrowingZoneCode", ZString.Empty, dataProvider.WineGrowingZoneCode);
				AssertEquals("WineProductCategory", ZString.Empty, dataProvider.WineProductCategory);
				AssertEquals("WineProductThirdCountryOfOrigin", ZString.Empty, dataProvider.WineProductThirdCountryOfOrigin);
				AssertEquals("WineProductOtherInfo", ZString.Empty, dataProvider.WineProductOtherInfo);
				AssertEquals("WineOperationCodes", Array.Empty<ZString>(), dataProvider.WineOperationCodes);
			});
		}

		public void TestWineOperationIsNull()
		{
			line.WineProduct.WineOperation = null;
			AssertEquals(Array.Empty<ZString>(), dataProvider.WineOperationCodes);
		}

		public void TestMaturationPeriodOrAgeOfProducts()
		{
			AssertExceptionThrown<NotImplementedException>(() => _ = dataProvider.MaturationPeriodOrAgeOfProducts);
		}

		protected override void SetUp()
		{
			base.SetUp();

			line = new ED801DBodyEadContainerBodyEad
			{
				BodyRecordUniqueReference = "1",
				ExciseProductCode = "B000",
				CnCode = "22030001100",
				FiscalMarkUsedFlagSpecified = true,
				FiscalMarkUsedFlag = ED801DBodyEadContainerBodyEadFiscalMarkUsedFlag.Item1,
				FiscalMark = "FM001",
				DesignationOfOrigin = "AU",
				CommercialDescription = "C Description 1",
				BrandNameOfProducts = "Brand Name 1",
				Quantity = 20m,
				GrossWeight = 300.2m,
				NetWeight = 100.1m,
				AlcoholicStrength = 200.2m,
				AlcoholicStrengthSpecified = true,
				DegreePlato = 400.4m,
				DegreePlatoSpecified = true,
				SizeOfProducer = "30",
				Density = 500.5m,
				DensitySpecified = true,
				Package = new ED801DBodyEadContainerBodyEadPackage[2]
				{
					new ED801DBodyEadContainerBodyEadPackage
					{
					},
					new ED801DBodyEadContainerBodyEadPackage
					{
					},
				},
				WineProduct = new ED801DBodyEadContainerBodyEadWineProduct
				{
					WineGrowingZoneCode = "Z1",
					WineProductCategory = ED801DBodyEadContainerBodyEadWineProductWineProductCategory.Item1,
					ThirdCountryOfOrigin = "NZ",
					OtherInformation = "Other Info 001",
					WineOperation = new ED801DBodyEadContainerBodyEadWineProductWineOperation[2]
					{
						new ED801DBodyEadContainerBodyEadWineProductWineOperation
						{
							WineOperationCode = "WOC001",
						},
						new ED801DBodyEadContainerBodyEadWineProductWineOperation
						{
							WineOperationCode = "WOC002",
						},
					}
				}
			};
			dataProvider = new ED801LineProvider(line);
		}
		IED801Line dataProvider;
		ED801DBodyEadContainerBodyEad line;

		protected override ED801LineProvider GetProvider() => (ED801LineProvider)dataProvider;
	}
}
