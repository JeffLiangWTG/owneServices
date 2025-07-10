using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class WineProductProviderHelperTest : TestCaseWithFactory
	{
		public void TestGrowingZoneCode()
		{
			emcsInvoiceLine.ZG_GrowingZone = EMCSGrowingZoneList.Codes.Cii;
			AssertEquals(EMCSGrowingZoneList.Codes.Cii, helper.GrowingZoneCode);
		}

		public void TestProductCategory()
		{
			emcsInvoiceLine.ZG_WineCategory = EMCSWineCategoryList.Codes.ImportedWine;
			AssertEquals(EMCSWineCategoryList.Codes.ImportedWine, helper.ProductCategory);
		}

		public void TestThirdCountryOfOrigin()
		{
			emcsInvoiceLine.ZG_WineCountryOrigin = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, helper.ThirdCountryOfOrigin);
		}

		public void TestOperationCodesNull()
		{
			AssertEquals("IEnumerables don't return null", false, helper.OperationCodes.Any());
		}

		public void TestOperationCodes()
		{
			var expectedOperationCodes = new List<string>();
			for (var i = 1; i < 11; i++)
			{
				var operationCodes = emcsInvoiceLine.OperationCodeDataCollection.AddNew();
				var code = "OCD" + i;
				operationCodes.CY_Code = code;
				expectedOperationCodes.Add(code);
			}
			AssertContainsExactElementsInAnyOrder("Matching elements", expectedOperationCodes, helper.OperationCodes);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			emcsInvoiceLine = emcsDeclaration.InvoiceHeader.InvoiceLines.AddNew();
			helper = new WineProductProviderHelper(emcsInvoiceLine);
		}
		EMCSJobComInvoiceLine emcsInvoiceLine;
		WineProductProviderHelper helper;
	}
}
