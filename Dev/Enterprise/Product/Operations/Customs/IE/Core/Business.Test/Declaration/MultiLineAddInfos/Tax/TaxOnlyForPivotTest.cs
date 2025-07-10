using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.MasterFiles;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using OrgSupplierPart = Enterprise.Customs.IE.Business.MasterFiles.OrgSupplierPart;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(TaxOnlyForPivot))]
	class TaxOnlyForPivotTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<TaxOnlyForPivot>
	{
		public void TestData()
		{
			AssertType<Tax_OnlyForPivot>(pivot.Taxes.AddNew().Data);
		}

		protected override BusinessObject GetNewBusinessObject() => pivot.Taxes.AddNew();

		protected override IEnumerable<TaxOnlyForPivot> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var product = (OrgSupplierPart)Enterprise.MasterFiles.Business.OrgSupplierPart.New(factory);
			product.OP_PartNum = "PRODUCT2";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relationship.OU_OH = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Common.ClassificationType.Both;
			yield return pivot.Taxes.AddNew();
		}

		protected override void SetUp()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var product = (OrgSupplierPart)Enterprise.MasterFiles.Business.OrgSupplierPart.New(Factory);
			product.OP_PartNum = "PRODUCT1";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			relationship.OU_OH = orgHeader.PK;
			pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Common.ClassificationType.Both;
		}
		CusClassPartPivot pivot;
	}
}
