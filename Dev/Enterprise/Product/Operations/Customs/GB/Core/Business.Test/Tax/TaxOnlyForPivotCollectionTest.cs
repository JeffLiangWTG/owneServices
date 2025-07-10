using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(TaxOnlyForPivotCollection))]
	public class TaxOnlyForPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestElementType()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			var tax = pivot.Taxes.AddNew();
			AssertType(typeof(GBTaxOnlyForPivot), tax);
			AssertType(typeof(Tax_OnlyForPivot), tax.Data);
		}

		public new void TestReintroducedAddNewRemovedForGenericCollection()
		{
			Assert(true);
		}

		public new void TestReintroducedIndexerRemovedForGenericCollection()
		{
			Assert(true);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			return pivot.Taxes;
		}

		public void TestIGBTaxOnlyForPivotImplements()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			var tax = ((Integration.Customs.GB.ICusClassPartPivot)pivot).Taxes.AddNew();
			AssertSame(pivot.Taxes[0], ((Integration.Customs.GB.ICusClassPartPivot)pivot).Taxes[0]);
		}
	}
}
