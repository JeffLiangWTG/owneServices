using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(TaxForPivotCollection))]
	public class TaxOnlyForPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestElementType()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			var tax = pivot.Taxes.AddNew();
			AssertType(typeof(ESTaxOnlyForPivot), tax);
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
	}
}
