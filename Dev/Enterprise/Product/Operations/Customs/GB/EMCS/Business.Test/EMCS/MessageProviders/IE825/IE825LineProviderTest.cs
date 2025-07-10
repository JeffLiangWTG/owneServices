using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE825LineProvider))]
	sealed class IE825LineProviderTest : LineProviderProviderAbstractTest<IE825LineProvider>
	{
		public void TestBodyRecordUniqueReference()
		{
			AssertEquals("1", LineProvider.BodyRecordUniqueReference);
		}

		public void TestCnCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for empty", string.Empty, LineProvider.CnCode);
				emcsInvoiceLine.JI_Tariff = "22042926";
				AssertEquals("Returns Correct value", "22042926", LineProvider.CnCode);
			});
		}

		public void TestQuantity()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for Zero", decimal.Zero, LineProvider.Quantity);
				emcsInvoiceLine.JI_CustomsQuantity = 1.13m;
				AssertEquals("Returns value", 1.13m, LineProvider.Quantity);
				emcsInvoiceLine.JI_CustomsQuantity = 2.00m;
				AssertEquals("Returns Integer", "2", LineProvider.Quantity.ToString());
				emcsInvoiceLine.JI_CustomsQuantity = 10.2000m;
				AssertEquals("Value is normalized", "10.2", LineProvider.Quantity.ToString());
			});
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
				AssertEquals("Using the convert to KGM property", 10.636741m, LineProvider.NetWeight);

				emcsInvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
				emcsInvoiceLine.JI_NetWeight = 200.200m;
				AssertEquals("Normalized", "200.2", LineProvider.NetWeight.ToString());
				emcsInvoiceLine.JI_NetWeight = 200.000m;
				AssertEquals("Normalized", "200", LineProvider.NetWeight.ToString());
			});
		}

		public void TestFiscalMark()
		{
			emcsInvoiceLine.ZG_FiscalMark = "FISCAL MARK TEXT";
			AssertEquals("Returns Correct value", "FISCAL MARK TEXT", LineProvider.FiscalMark.Text);
		}

		public void TestFiscalMarkUsedFlag()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Fiscal mark not used", false, LineProvider.FiscalMarkUsedFlag);
				emcsInvoiceLine.ZG_FiscalMarkUsed = true;
				AssertEquals("Fiscal mark is used", true, LineProvider.FiscalMarkUsedFlag);
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

		public void TestDensitySpecified()
		{
			CombineAssertions(() =>
			{
				emcsInvoiceLine.ZG_Density = 0.0m;
				AssertEquals("Density not specified", false, LineProvider.DensitySpecified);
				emcsInvoiceLine.ZG_Density = 0.3m;
				AssertEquals("Density is specified", true, LineProvider.DensitySpecified);
			});
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

		protected override IE825LineProvider GetLineProvider() => new IE825LineProvider(emcsInvoiceLine);

		protected override IE825LineProvider GetProvider()
		{
			emcsInvoiceLine.ZG_FiscalMark = "FISCAL MARK TEXT";
			emcsInvoiceLine.JI_NDescription = "DESCRIPTION TEXT";
			emcsInvoiceLine.JI_BrandName = "BMW";
			emcsInvoiceLine.ZG_ExciseProductCode = EMCSJobComInvoiceLine.ExciseProductCode_W200;
			emcsInvoiceLine.ZG_WineCategory = EMCSWineCategoryList.Codes.ImportedWine;
			return new IE825LineProvider(emcsInvoiceLine);
		}

		protected override IEnumerable<Expression<System.Func<IE825LineProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.FiscalMark;
			yield return x => x.CommercialDescription;
			yield return x => x.BrandNameOfProducts;
			yield return x => x.Packages;
		}
	}
}
