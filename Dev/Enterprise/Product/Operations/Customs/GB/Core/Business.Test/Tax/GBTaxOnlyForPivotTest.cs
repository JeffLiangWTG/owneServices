using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.MasterFiles;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Integration.Customs.GB;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(GBTaxOnlyForPivot))]
	public class GBTaxOnlyForPivotTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<GBTaxOnlyForPivot>
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

		protected override IEnumerable<GBTaxOnlyForPivot> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var product = (OrgSupplierPart)Enterprise.Customs.EU.Business.MasterFiles.OrgSupplierPart.New(factory);
			product.OP_PartNum = "POOPY2";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = "BTH";
			relationship.OU_OH = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Customs.Business.BaseCusClassification.ClassificationType.Both;
			yield return pivot.Taxes.AddNew();
		}

		protected override void SetUp()
		{
			org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "DJC123";
			var product = (OrgSupplierPart)Enterprise.Customs.EU.Business.MasterFiles.OrgSupplierPart.New(Factory);
			product.OP_PartNum = "POOPY";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = "BTH";
			relationship.OU_OH = org.PK;
			pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Customs.Business.BaseCusClassification.ClassificationType.Both;
		}

		public void TestIGBTaxOnlyForPivotImplements()
		{
			var tax = pivot.Taxes.AddNew();
			AssertSame(tax.Data, ((IGBTaxOnlyForPivot)tax).Data);
		}

		OrgHeader org;
		CusClassPartPivot pivot;
	}
}
