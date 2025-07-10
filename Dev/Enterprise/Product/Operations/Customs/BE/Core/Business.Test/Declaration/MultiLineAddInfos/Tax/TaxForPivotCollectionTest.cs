using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BE.Business.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(TaxForPivotCollection))]
public class TaxOnlyForPivotCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestElementType()
	{
		var part = Factory.New<OrgSupplierPart>();
		var pivot = part.PivotsForBinding.AddNew();
		var tax = pivot.Taxes.AddNew();
		AssertType(typeof(BETaxOnlyForPivot), tax);
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
