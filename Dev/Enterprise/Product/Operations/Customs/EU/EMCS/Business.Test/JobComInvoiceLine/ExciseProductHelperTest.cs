using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	class ExciseProductHelperTest : TestCaseWithFactory
	{
		public void TestGetExciseProduct_InvoiceLineIsNull()
		{
			invoiceLine = null;
			AssertNull(invoiceLine.GetExciseProduct());
		}

		public void TestGetExciseProduct_ExciseProductCodeIsEmpty()
		{
			invoiceLine.ZG_ExciseProductCode = ZString.Empty;
			AssertNull(invoiceLine.GetExciseProduct());
		}

		public void TestGetExciseProduct()
		{
			AssertNotNull(invoiceLine.GetExciseProduct());
		}

		public void TestHasExciseProductAttribute_InvoiceLineIsNull()
		{
			invoiceLine = null;
			CombineAssertions(() =>
			{
				AssertEquals("AlcoholicStrength", false, invoiceLine.HasExciseProductAttribute(ExciseProductCodeAttribute.AlcoholicStrength));
				AssertEquals("AlcoholicStrength with value Y", false, invoiceLine.HasExciseProductAttribute(ExciseProductCodeAttribute.AlcoholicStrength, Customs.Business.YesNoList.Codes.Yes));
			});
		}

		public void TestHasExciseProductAttribute()
		{
			CombineAssertions(() =>
			{
				AssertEquals("AlcoholicStrength", true, invoiceLine.HasExciseProductAttribute(ExciseProductCodeAttribute.AlcoholicStrength));
				AssertEquals("DegreePlato", false, invoiceLine.HasExciseProductAttribute(ExciseProductCodeAttribute.DegreePlato));
				AssertEquals("AlcoholicStrength with value Y", true, invoiceLine.HasExciseProductAttribute(ExciseProductCodeAttribute.AlcoholicStrength, Customs.Business.YesNoList.Codes.Yes));
				AssertEquals("AlcoholicStrength with value N", false, invoiceLine.HasExciseProductAttribute(ExciseProductCodeAttribute.AlcoholicStrength, Customs.Business.YesNoList.Codes.No));
			});
		}

		public void TestGetExciseProductAttributeValues_InvoiceLineIsNull()
		{
			invoiceLine = null;
			AssertEquals(0, invoiceLine.GetExciseProductAttributeValues(ExciseProductCodeAttribute.RelTrf).Count());
		}

		public void TestGetExciseProductAttributeValues()
		{
			AssertContainsExactElementsInAnyOrder(new[] { "20180802", "20180801" }, invoiceLine.GetExciseProductAttributeValues(ExciseProductCodeAttribute.RelTrf));
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunGrouping);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(nameof(ExciseProductCodeAttribute.AlcoholicStrength), "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(nameof(ExciseProductCodeAttribute.RelTrf), "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var cusCodeList = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "AX", new ZDateTime(1990, 01, 01), ZDateTime.Now.AddYears(1));
			cusCodeList.Attributes.AddNew(nameof(ExciseProductCodeAttribute.AlcoholicStrength), Customs.Business.YesNoList.Codes.Yes);
			cusCodeList.Attributes.AddNew(nameof(ExciseProductCodeAttribute.RelTrf), "20180802");
			cusCodeList.Attributes.AddNew(nameof(ExciseProductCodeAttribute.RelTrf), "20180801");
			Factory.Save();

			invoiceLine = Factory.New<EMCSJobDeclaration>().InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine.ZG_ExciseProductCode = "AX";
		}

		EMCSJobComInvoiceLine invoiceLine;
	}
}
