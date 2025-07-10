using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.MasterFiles;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(Tax_OnlyForPivot))]
	public class TaxTest : BusinessObjectBaseTestCase
	{
		public void TestLookups()
		{
			var tax = pivot.Taxes.AddNew().Data;
			AssertType<DEAddInfoTaxLookups>(tax.Lookups);
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
			pivot.CI_ChildType = BaseCusClassification.ClassificationType.Both;
		}
		OrgHeader org;
		CusClassPartPivot pivot;
	}
}
