using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(TaxForPivotCollection))]
sealed class TaxOnlyForPivotCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestElementType()
	{
		var itTax = pivot.Taxes.AddNew();
		AssertType<ITTaxOnlyForPivot>("New ", itTax);
		AssertType<ITTaxOnlyForPivot>(pivot.Taxes[0]);
	}

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		if (pivot == null)
		{
			SetUp();
		}
		pivot.Taxes.AddNew();
		return pivot.Taxes;
	}

	public new void TestReintroducedAddNewRemovedForGenericCollection()
	{
		Assert("Need AddNew() returns casted object", true);
	}

	public new void TestReintroducedIndexerRemovedForGenericCollection()
	{
		Assert("Need Indexer returns casted object", true);
	}

	protected override void SetUp()
	{
		org = Factory.NewWithValidTestData<OrgHeader>();
		var product = (OrgSupplierPart)Enterprise.Customs.EU.Business.MasterFiles.OrgSupplierPart.New(Factory);
		product.OP_PartNum = "POOPY";
		var relationship = product.RelatedOrganisations.AddNew();
		relationship.OU_Relationship = "BTH";
		relationship.OU_OH = org.PK;
		pivot = product.PivotsForBinding.AddNew();
		pivot.CI_ChildType = Customs.Business.BaseCusClassification.ClassificationType.Both;
	}

	OrgHeader org;
	CusClassPartPivot pivot;
}
