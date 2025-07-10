using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business.MasterFiles;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using OrgSupplierPart = Enterprise.Customs.BE.Business.MasterFiles.OrgSupplierPart;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(BETaxOnlyForPivot))]
public class BETaxOnlyForPivotTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<BETaxOnlyForPivot>
{
	public void TestData()
	{
		var addINfoTax = pivot.Taxes.AddNew();
		AssertType<Tax_OnlyForPivot>(addINfoTax.Data);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return pivot.Taxes.AddNew();
	}

	protected override IEnumerable<BETaxOnlyForPivot> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var product = (OrgSupplierPart)Enterprise.MasterFiles.Business.OrgSupplierPart.New(factory);
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
		org.OH_Code = "DJC123";
		var product = (OrgSupplierPart)Enterprise.MasterFiles.Business.OrgSupplierPart.New(Factory);
		product.OP_PartNum = "POOPY";
		var relationship = product.RelatedOrganisations.AddNew();
		relationship.OU_Relationship = "BTH";
		relationship.OU_OH = org.PK;
		pivot = product.PivotsForBinding.AddNew();
		pivot.CI_ChildType = Common.ClassificationType.Both;
	}

	OrgHeader org;
	CusClassPartPivot pivot;
}
