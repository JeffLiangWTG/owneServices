using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.MasterFiles;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(DETax_OnlyForPivot))]
	public class DETaxTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<DETax_OnlyForPivot>
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

		protected override IEnumerable<DETax_OnlyForPivot> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
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
