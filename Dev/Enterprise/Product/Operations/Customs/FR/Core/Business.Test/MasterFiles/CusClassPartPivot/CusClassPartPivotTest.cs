using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MasterFiles.Testing
{
	[TestedType(typeof(CusClassPartPivot))]
	public class CusClassPartPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAdditionalInfoCorrectType()
		{
			AssertType<AdditionalInfoCollection>(pivot.AdditionalInfos);
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
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "POOPY";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = "BTH";
			relationship.OU_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Common.ClassificationType.Both;
		}
		CusClassPartPivot pivot;
	}

	public class CusClassPartPivotAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
	}
}
