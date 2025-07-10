using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusClassPartPivotAddInfoLookupsTest : AUAddInfoLookupsTest
	{
		public void TestProduceTypeList()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.NEXDOC_OTH, Core.Constants.CountryCodes.Australia, ZDateTime.Today, true))
			{
				AssertEquals(10, Pivot.AddInfo.Lookups.ProduceTypeList.Count);
				Assert(Pivot.AddInfo.Lookups.ProduceTypeList.ContainsCode(EXDOCCommodityCodes.Codes.OtherGoods));
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.NEXDOC_OTH, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				AssertEquals(9, Pivot.AddInfo.Lookups.ProduceTypeList.Count);
				Assert(!Pivot.AddInfo.Lookups.ProduceTypeList.ContainsCode(EXDOCCommodityCodes.Codes.OtherGoods));
			}
		}

		public void TestProductList()
		{
			AssertEquals(typeof(EXDOCProductTypeCollection), Pivot.AddInfo.Lookups.ProductList.GetType());
		}

		public void TestCategoryCodetList()
		{
			AssertEquals(typeof(NEXDOCCategoryCodesCollection), Pivot.AddInfo.Lookups.CategoryCodes.GetType());
		}

		public void TestSupplementaryCodesList()
		{
			AssertEquals(typeof(EXDOCSupplementaryCodeCollection), Pivot.AddInfo.Lookups.SupplementaryCodesList.GetType());
		}

		public void TestCutCodesList()
		{
			AssertEquals(typeof(EXDOCCutCodeCollection), Pivot.AddInfo.Lookups.CutCodesList.GetType());
		}

		CusClassPartPivot fPivot;
		CusClassPartPivot Pivot
		{
			get
			{
				if (fPivot == null)
				{
					fPivot = Factory.New<CusClassPartPivot>();
					fPivot.CI_CC = Class.PK;
					fPivot.CI_OP = Product.PK;
				}
				return fPivot;
			}
		}

		Classification fClass;
		Classification Class
		{
			get
			{
				if (fClass == null)
				{
					fClass = Factory.New<Classification>();
				}
				return fClass;
			}
		}

		AUOrgSupplierPart fProduct;
		AUOrgSupplierPart Product
		{
			get
			{
				if (fProduct == null)
				{
					fProduct = Factory.New<AUOrgSupplierPart>();
					fProduct.OP_PartNum = fProduct.PK.ToString().Replace("-", "");
				}
				return fProduct;
			}
		}
	}
}
