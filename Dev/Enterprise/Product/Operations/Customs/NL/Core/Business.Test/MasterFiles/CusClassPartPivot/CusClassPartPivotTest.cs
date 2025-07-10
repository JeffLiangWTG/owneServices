using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.MasterFiles.Testing;

[TestedType(typeof(CusClassPartPivot))]
public class CusClassPartPivotTest : EnterpriseBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject()
	{
		return pivot;
	}

	public void TestSupportingDocuments()
	{
		var product = Factory.New<OrgSupplierPart>();
		product.OP_PartNum = "POOPY";
		var relationship = product.RelatedOrganisations.AddNew();
		relationship.OU_Relationship = "BTH";
		relationship.OU_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
		var pivot = product.PivotsForBinding.AddNew();
		pivot.CI_ChildType = Enterprise.Customs.Business.BaseCusClassification.ClassificationType.Both;
		AssertType<SupportingDocumentCollection>(pivot.SupportingDocuments);
	}

	protected override void SetUp()
	{
		org = Factory.NewWithValidTestData<OrgHeader>();
		var product = (OrgSupplierPart)Enterprise.Customs.EU.Business.MasterFiles.OrgSupplierPart.New(Factory);
		product.OP_PartNum = "POOPY";
		var relationship = product.RelatedOrganisations.AddNew();
		relationship.OU_Relationship = "BTH";
		relationship.OU_OH = org.PK;
		pivot = (CusClassPartPivot)product.PivotsForBinding.AddNew();
		pivot.CI_ChildType = BaseCusClassification.ClassificationType.Both;
	}
	OrgHeader org;
	CusClassPartPivot pivot;
}
