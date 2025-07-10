using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using OrgSupplierPart = Enterprise.Customs.IE.Business.MasterFiles.OrgSupplierPart;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class AddInfoTaxLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestParent()
		{
			AssertType<Tax_OnlyForPivot>(lookups.Parent);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var product = (OrgSupplierPart)Enterprise.MasterFiles.Business.OrgSupplierPart.New(Factory);
			product.OP_PartNum = "PRODUCT";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relationship.OU_OH = orgHeader.PK;
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Common.ClassificationType.Both;
			lookups = new AddInfoTaxLookups(pivot.Taxes.AddNew().Data);
		}
		AddInfoTaxLookups lookups;
	}
}
