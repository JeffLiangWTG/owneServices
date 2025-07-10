using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tcl;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE801LineProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE801LineProvider(null));
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
				line.FiscalMarkUsedFlagValueSpecified = false;
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
				line.AlcoholicStrengthByVolumeInPercentageValueSpecified = false;
				AssertEquals("AlcoholicStrength is not Specified", 0m, dataProvider.AlcoholicStrength);
			});
		}

		public void TestDegreePlato()
		{
			CombineAssertions(() =>
			{
				AssertEquals(400.4m, dataProvider.DegreePlato);
				line.DegreePlatoValueSpecified = false;
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
				line.DensityValueSpecified = false;
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
			AssertEquals("10Months", dataProvider.MaturationPeriodOrAgeOfProducts);
		}

		public void TestIndependentSmallProducersDeclaration()
		{
			AssertEquals("Independent Small Producers Declaration", dataProvider.IndependentSmallProducersDeclaration);
		}

		protected override void SetUp()
		{
			base.SetUp();

			line = new BodyEadEsadType
			{
				BodyRecordUniqueReference = "1",
				ExciseProductCode = "B000",
				CnCode = "22030001100",
				FiscalMarkUsedFlagValueSpecified = true,
				FiscalMarkUsedFlag = Flag.Item1,
				FiscalMark = new LsdFiscalMarkType
				{
					Value = "FM001",
					Language = "en",
				},
				DesignationOfOrigin = new LsdDesignationOfOriginType
				{
					Value = "AU",
					Language = "en",
				},
				CommercialDescription = new LsdCommercialDescriptionType
				{
					Value = "C Description 1",
					Language = "en",
				},
				BrandNameOfProducts = new LsdBrandNameOfProductsType
				{
					Value = "Brand Name 1",
					Language = "en",
				},
				Quantity = 20m,
				GrossMass = 300.2m,
				NetMass = 100.1m,
				AlcoholicStrengthByVolumeInPercentage = 200.2m,
				AlcoholicStrengthByVolumeInPercentageValueSpecified = true,
				DegreePlato = 400.4m,
				DegreePlatoValueSpecified = true,
				SizeOfProducer = "30",
				Density = 500.5m,
				DensityValueSpecified = true,
				Package = new Collection<PackageType>
				{
					new PackageType { }, new PackageType { },
				},
				WineProduct = new WineProductType
				{
					WineGrowingZoneCode = "Z1",
					WineProductCategory = CategoryOfWineProduct.Item1,
					ThirdCountryOfOrigin = "NZ",
					OtherInformation = new LsdOtherInformationType
					{
						Value = "Other Info 001",
						Language = "en",
					},
					WineOperation = new Collection<WineOperationType>
					{
						new WineOperationType
						{
							WineOperationCode = "WOC001",
						},
						new WineOperationType
						{
							WineOperationCode = "WOC002",
						},
					}
				},
				MaturationPeriodOrAgeOfProducts = new LsdMaturationPeriodOrAgeOfProductsType
				{
					Language = "en",
					Value = "10Months"
				},
				IndependentSmallProducersDeclaration = new LsdIndependentSmallProducersDeclarationType
				{
					Language = "en",
					Value = "Independent Small Producers Declaration"
				}
			};
			dataProvider = new IE801LineProvider(line);
		}
		IIE801Line dataProvider;
		BodyEadEsadType line;
	}
}
