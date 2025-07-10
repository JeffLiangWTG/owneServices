using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(CusClassPartPivot))]
sealed class CusClassPartPivotTest : ZArchitecture.Business.Testing.EnterpriseBusinessObjectTestCase
{
	public void TestPreviousDocumentsCorrectType()
	{
		AssertType<PreviousDocumentCollection>(pivot.PreviousDocuments);
	}

	public void TestSupportingDocumentsCorrectType()
	{
		AssertType<SupportingDocumentCollection>(pivot.SupportingDocuments);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return pivot;
	}

	protected override void SetUp()
	{
		org = Factory.NewWithValidTestData<OrgHeader>();
		var product = Factory.New<OrgSupplierPart>();
		product.OP_PartNum = "POOPY";
		var relationship = product.RelatedOrganisations.AddNew();
		relationship.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
		relationship.OU_OH = org.PK;
		pivot = product.PivotsForBinding.AddNew();
		pivot.CI_ChildType = Customs.Business.BaseCusClassification.ClassificationType.Both;
	}
	OrgHeader org;
	CusClassPartPivot pivot;
}
