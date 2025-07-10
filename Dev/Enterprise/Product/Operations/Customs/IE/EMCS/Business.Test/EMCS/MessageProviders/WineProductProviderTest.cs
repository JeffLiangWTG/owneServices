using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	sealed class WineProductProviderTest : Customs.Business.Testing.DataProviderTestCase<WineProductProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new WineProductProvider(null));
		}

		public void TestGrowingZoneCode()
		{
			emcsInvoiceLine.ZG_GrowingZone = EMCSGrowingZoneList.Codes.Cii;
			AssertEquals(EMCSGrowingZoneList.Codes.Cii, dataProvider.GrowingZoneCode);
		}

		public void TestProductCategory()
		{
			emcsInvoiceLine.ZG_WineCategory = EMCSWineCategoryList.Codes.ImportedWine;
			AssertEquals(EMCSWineCategoryList.Codes.ImportedWine, dataProvider.ProductCategory);
		}

		public void TestThirdCountryOfOrigin()
		{
			emcsInvoiceLine.ZG_WineCountryOrigin = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, dataProvider.ThirdCountryOfOrigin);
		}

		public void TestOtherInformation()
		{
			AssertEquals("RED SILKY VELVET WITH HEAPS OF BLACKBERRIES", dataProvider.OtherInformation.Text);
		}

		public void TestOperationCodesNull()
		{
			AssertEquals("IEnumerables don't return null", false, dataProvider.OperationCodes.Any());
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
			AssertContainsExactElementsInAnyOrder("Matching elements", expectedOperationCodes, dataProvider.OperationCodes);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			emcsInvoiceLine = emcsDeclaration.InvoiceHeader.InvoiceLines.AddNew();
			emcsInvoiceLine.JI_WineDetailsComments = "RED SILKY VELVET WITH HEAPS OF BLACKBERRIES";
			dataProvider = new WineProductProvider(emcsInvoiceLine);
		}
		EMCSJobComInvoiceLine emcsInvoiceLine;
		IWineProduct dataProvider;

		protected override WineProductProvider GetProvider() => (WineProductProvider)dataProvider;

		protected override IEnumerable<Expression<Func<WineProductProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.OtherInformation;
		}
	}
}
