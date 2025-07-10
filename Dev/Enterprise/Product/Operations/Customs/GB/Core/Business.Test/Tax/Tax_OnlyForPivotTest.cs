using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.MasterFiles;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(Tax_OnlyForPivot))]
	public class Tax_OnlyForPivotTest : BusinessObjectBaseTestCase
	{
		public void TestLookups()
		{
			var tax = pivot.Taxes.AddNew().Data;
			AssertType<GBAddInfoTaxLookups>(tax.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return pivot.Taxes.AddNew().Data;
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
