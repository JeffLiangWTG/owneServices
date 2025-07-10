using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(ITTaxOnlyForPivot))]
sealed class ITTaxOnlyForPivotTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<ITTaxOnlyForPivot>
{
	public void TestData()
	{
		var addInfoTax = pivot.Taxes.AddNew();
		AssertNotNull("Data", addInfoTax.Data);
	}
	protected override BusinessObject GetNewBusinessObject()
	{
		return pivot.Taxes.AddNew();
	}

	protected override IEnumerable<ITTaxOnlyForPivot> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var product = (OrgSupplierPart)Enterprise.Customs.EU.Business.MasterFiles.OrgSupplierPart.New(factory);
		product.OP_PartNum = "POOPY2";
		var relationship = product.RelatedOrganisations.AddNew();
		relationship.OU_Relationship = "BTH";
		relationship.OU_OH = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
		var pivot = product.PivotsForBinding.AddNew();
		pivot.CI_ChildType = Common.ClassificationType.Both;
		yield return pivot.Taxes.AddNew();
	}

	protected override void SetUp()
	{
		org = Factory.NewWithValidTestData<OrgHeader>();
		org.OH_Code = "LQO123";
		var product = (OrgSupplierPart)Enterprise.Customs.EU.Business.MasterFiles.OrgSupplierPart.New(Factory);
		product.OP_PartNum = "FUFFY";
		var relationship = product.RelatedOrganisations.AddNew();
		relationship.OU_Relationship = "BTH";
		relationship.OU_OH = org.PK;
		pivot = product.PivotsForBinding.AddNew();
		pivot.CI_ChildType = Common.ClassificationType.Both;
	}

	OrgHeader org;
	CusClassPartPivot pivot;
}
