using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Business.MasterFiles;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using OrgSupplierPart = Enterprise.Customs.ES.Business.MasterFiles.OrgSupplierPart;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(ESTaxOnlyForPivot))]
	public class ESTaxOnlyForPivotTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<ESTaxOnlyForPivot>
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

		protected override IEnumerable<ESTaxOnlyForPivot> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
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
			org.OH_Code = "DJC123";
			var product = (OrgSupplierPart)Enterprise.Customs.EU.Business.MasterFiles.OrgSupplierPart.New(Factory);
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
}
