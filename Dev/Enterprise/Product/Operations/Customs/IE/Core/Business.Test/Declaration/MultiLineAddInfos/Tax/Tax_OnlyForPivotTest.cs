using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.MasterFiles;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using OrgSupplierPart = Enterprise.Customs.IE.Business.MasterFiles.OrgSupplierPart;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(Tax_OnlyForPivot))]
	class Tax_OnlyForPivotTest : BusinessObjectBaseTestCase
	{
		public void TestLookups()
		{
			AssertType<AddInfoTaxLookups>(pivot.Taxes.AddNew().Data.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject() => pivot.Taxes.AddNew().Data;

		protected override void SetUp()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var product = (OrgSupplierPart)Enterprise.MasterFiles.Business.OrgSupplierPart.New(Factory);
			product.OP_PartNum = "PRODUCT";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relationship.OU_OH = orgHeader.PK;
			pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Common.ClassificationType.Both;
		}
		CusClassPartPivot pivot;
	}
}
