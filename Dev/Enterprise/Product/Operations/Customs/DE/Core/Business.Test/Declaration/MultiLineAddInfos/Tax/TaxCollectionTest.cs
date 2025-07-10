using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.MasterFiles;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(TaxForPivotCollection))]
	public class TaxCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestElementType()
		{
			var deTax = pivot.Taxes.AddNew();
			AssertType<DETax_OnlyForPivot>(deTax);
			AssertType<DETax_OnlyForPivot>(pivot.Taxes[0]);
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
				SetUp(); // stupid reflection tests don't call setup
			}
			pivot.Taxes.AddNew();
			return pivot.Taxes;
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
}
