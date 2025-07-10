using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(TaxForPivotCollection))]
	class TaxOnlyForPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestIndexType()
		{
			pivot.Taxes.AddNew();
			AssertType<TaxOnlyForPivot>(pivot.Taxes[0]);
		}

		public void TestAddNewType()
		{
			AssertType<TaxOnlyForPivot>(pivot.Taxes.AddNew());
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
			if (pivot == null)
			{
				SetUp();
			}
			return pivot.Taxes;
		}

		protected override void SetUp()
		{
			var part = Factory.New<OrgSupplierPart>();
			pivot = part.PivotsForBinding.AddNew();
		}
		CusClassPartPivot pivot;
	}
}
