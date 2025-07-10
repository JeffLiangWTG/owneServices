using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(IE815LineProvider))]
	public class IE815LineProviderTest : LineProviderAbstractTest<IE815LineProvider>
	{
		public void TestTariff()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for empty", string.Empty, LineProvider.Tariff);
				emcsInvoiceLine.JI_Tariff = "22042926";
				AssertEquals("Returns Correct value", "22042926", LineProvider.Tariff);
			});
		}

		public void TestQuantity()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for empty", decimal.Zero, LineProvider.Quantity);
				emcsInvoiceLine.JI_CustomsQuantity = 3.13m;
				AssertEquals("Returns value", 3.13m, LineProvider.Quantity);
				emcsInvoiceLine.JI_CustomsQuantity = 3.00m;
				AssertEquals("Returns Integer", "3", LineProvider.Quantity.ToString());
				emcsInvoiceLine.JI_CustomsQuantity = 10.2000m;
				AssertEquals("Value is normalized", "10.2", LineProvider.Quantity.ToString());
			});
		}

		public void TestFiscalMarkUsedFlag()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for empty", false, LineProvider.FiscalMarkUsedFlag);
				emcsInvoiceLine.ZG_FiscalMarkUsed = true;
				AssertEquals("Returns Correct value", true, LineProvider.FiscalMarkUsedFlag);
			});
		}

		public void TestFiscalMark()
		{
			emcsInvoiceLine.ZG_FiscalMark = "FISCAL MARK TEXT";
			AssertEquals("Returns Correct value", "FISCAL MARK TEXT", LineProvider.FiscalMark.Text);
		}

		public void TestDesignationOfOrigin()
		{
			emcsInvoiceLine.ZG_Origin = "DESIGNATION OF ORIGIN TEXT";
			AssertEquals("Returns Correct value", "DESIGNATION OF ORIGIN TEXT", LineProvider.DesignationOfOrigin.Text);
		}

		public void TestCommercialDescription()
		{
			emcsInvoiceLine.JI_NDescription = "DESCRIPTION TEXT";
			AssertEquals("Returns Correct value", "DESCRIPTION TEXT", LineProvider.CommercialDescription.Text);
		}

		public void TestBrandNameOfProducts()
		{
			emcsInvoiceLine.JI_BrandName = "BMW";
			AssertEquals("Returns Correct value", "BMW", LineProvider.BrandNameOfProducts.Text);
		}

		public void TestGrossWeight()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for Zero", decimal.Zero, LineProvider.GrossWeight);
				emcsInvoiceLine.JI_Weight = 123.89m;
				AssertEquals("Returns Correct value", 123.89m, LineProvider.GrossWeight);
				emcsInvoiceLine.JI_Weight = 123.00m;
				AssertEquals("Value is integer", "123", LineProvider.GrossWeight.ToString());
				emcsInvoiceLine.JI_Weight = 123.500m;
				AssertEquals("Value is normalized", "123.5", LineProvider.GrossWeight.ToString());
			});
		}

		public void TestNetWeight()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for Zero", decimal.Zero, LineProvider.NetWeight);
				emcsInvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				emcsInvoiceLine.JI_NetWeight = 23.45m;
				AssertEquals("Returns Correct value", 23.45m, LineProvider.NetWeight);
				emcsInvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Pounds;
				AssertEquals("Using the convert to KGM proeprty", 10.636741m, LineProvider.NetWeight);

				emcsInvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				emcsInvoiceLine.JI_NetWeight = 200.200m;
				AssertEquals("Normalized", "200.2", LineProvider.NetWeight.ToString());
				emcsInvoiceLine.JI_NetWeight = 200.000m;
				AssertEquals("Normalized", "200", LineProvider.NetWeight.ToString());
			});
		}

		public void TestAlcoholicStrength()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for Zero", decimal.Zero, LineProvider.AlcoholicStrength);
				emcsInvoiceLine.ZG_AlcoholicStrength = 12.3m;
				AssertEquals("Returns Correct value", 12.3m, LineProvider.AlcoholicStrength);

				emcsInvoiceLine.ZG_AlcoholicStrength = 13.300m;
				AssertEquals("Normalized", "13.3", LineProvider.AlcoholicStrength.ToString());
				emcsInvoiceLine.ZG_AlcoholicStrength = 10.000m;
				AssertEquals("Normalized", "10", LineProvider.AlcoholicStrength.ToString());
			});
		}

		public void TestDegreePlato()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for Zero", decimal.Zero, LineProvider.DegreePlato);
				emcsInvoiceLine.ZG_DegreePlato = 5.1m;
				AssertEquals("Returns Correct value", 5.1m, LineProvider.DegreePlato);

				emcsInvoiceLine.ZG_DegreePlato = 13.300m;
				AssertEquals("Normalized", "13.3", LineProvider.DegreePlato.ToString());
				emcsInvoiceLine.ZG_DegreePlato = 10.000m;
				AssertEquals("Normalized", "10", LineProvider.DegreePlato.ToString());
			});
		}

		public void TestSizeOfProducer()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for Zero", decimal.Zero, LineProvider.SizeOfProducer);
				emcsInvoiceLine.ZG_SizeOfProducer = 10000m;
				AssertEquals("Returns Correct value", 10000m, LineProvider.SizeOfProducer);
			});
		}

		public void TestDensity()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for Zero", decimal.Zero, LineProvider.Density);
				emcsInvoiceLine.ZG_Density = 2.3m;
				AssertEquals("Returns Correct value", 2.3m, LineProvider.Density);

				emcsInvoiceLine.ZG_Density = 13.300m;
				AssertEquals("Normalized", "13.3", LineProvider.Density.ToString());
				emcsInvoiceLine.ZG_Density = 10.000m;
				AssertEquals("Normalized", "10", LineProvider.Density.ToString());
			});
		}

		public void TestPackages_Null()
		{
			AssertEquals("IEnumerable does not return null", false, LineProvider.Packages.Any());
		}

		public void TestPackages()
		{
			for (var i = 0; i < 5; i++)
			{
				emcsDeclaration.EMCSPackages.AddNew();
			}
			emcsInvoiceLine.EMCSPackagePivots[0].IsForInvoiceLine = true;
			emcsInvoiceLine.EMCSPackagePivots[1].IsForInvoiceLine = true;
			CombineAssertions(() =>
			{
				AssertEquals("Number of packages on declaration", 5, emcsDeclaration.EMCSPackages.Count);
				AssertEquals("Number of mapped line packages", 2, LineProvider.Packages.Count);
			});
		}

		public void TestMaturationPeriodOrAgeOfProducts()
		{
			emcsInvoiceLine.ZG_MaturationPeriodOrAgeOfProducts = "Maturation Period Or Age Of Products Text";
			AssertEquals("Returns Correct value", "Maturation Period Or Age Of Products Text", LineProvider.MaturationPeriodOrAgeOfProducts.Text);
		}

		public void TestIndependentSmallProducersDeclaration()
		{
			emcsInvoiceLine.ZG_IndependentSmallProducersDeclaration = "Independent Small Producers Declaration Text";
			AssertEquals("Returns Correct value", "Independent Small Producers Declaration Text", LineProvider.IndependentSmallProducersDeclaration.Text);
		}

		public void TestWineProduct()
		{
			emcsInvoiceLine.ZG_WineCategory = EMCSWineCategoryList.Codes.ImportedWine;
			AssertNull("not a wine product", LineProvider.WineProduct);
		}

		public void TestWineProductWhenW200AndJI_WineCategoryNotEmpty()
		{
			emcsInvoiceLine.ZG_ExciseProductCode = EMCSJobComInvoiceLine.ExciseProductCode_W200;
			emcsInvoiceLine.ZG_WineCategory = EMCSWineCategoryList.Codes.ImportedWine;
			AssertNotNull("WineProduct Exists", LineProvider.WineProduct);
		}

		public void TestWineProductNullWhenJI_WineCategoryEmpty()
		{
			emcsInvoiceLine.ZG_ExciseProductCode = EMCSJobComInvoiceLine.ExciseProductCode_W200;
			AssertNull("Wine category is empty", LineProvider.WineProduct);
		}

		protected override IE815LineProvider GetLineProvider() => new IE815LineProvider(emcsInvoiceLine);

		protected override IE815LineProvider GetProvider()
		{
			emcsInvoiceLine.ZG_FiscalMark = "FISCAL MARK TEXT";
			emcsInvoiceLine.ZG_Origin = "DESIGNATION OF ORIGIN TEXT";
			emcsInvoiceLine.JI_NDescription = "DESCRIPTION TEXT";
			emcsInvoiceLine.JI_BrandName = "BMW";
			emcsInvoiceLine.ZG_MaturationPeriodOrAgeOfProducts = "Maturation Period Or Age Of Products Text";
			emcsInvoiceLine.ZG_ExciseProductCode = EMCSJobComInvoiceLine.ExciseProductCode_W200;
			emcsInvoiceLine.ZG_WineCategory = EMCSWineCategoryList.Codes.ImportedWine;
			return new IE815LineProvider(emcsInvoiceLine);
		}

		protected override IEnumerable<Expression<System.Func<IE815LineProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.FiscalMark;
			yield return x => x.DesignationOfOrigin;
			yield return x => x.CommercialDescription;
			yield return x => x.BrandNameOfProducts;
			yield return x => x.MaturationPeriodOrAgeOfProducts;
		}
	}
}
