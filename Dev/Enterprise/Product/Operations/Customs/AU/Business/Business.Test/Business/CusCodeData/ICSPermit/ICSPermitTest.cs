using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ICSPermit))]
	public class ICSPermitTest : CusCodeDataTest<ICSPermit>
	{
		public void TestSetDefaultValues()
		{
			var customsManifestLineSequence = Factory.New<ICSPermit>();
			CombineAssertions(() =>
			{
				AssertEquals("CY_Type defaults to PER", CusCodeDataTypeList.Codes.ICSPermit, customsManifestLineSequence.CY_Type);
				AssertEquals("CY_Code defaults to PER", CusCodeDataTypeList.Codes.ICSPermit, customsManifestLineSequence.CY_Code);
			});
		}

		public void TestValidation()
		{
			AssertType<ICSPermitValidation>(Factory.New<ICSPermit>().Validation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().ICSPermits.AddNew();
		}

		protected override IEnumerable<ICSPermit> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().ICSPermits.AddNew();

			var classification = factory.New<Classification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = Classification.ClassificationType.Both;
			var part = factory.New<AUOrgSupplierPart>();
			part.OP_PartNum = part.PK.ToString().Replace("-", "");
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_CC = classification.PK;
			yield return pivot.ICSPermits.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().ICSPermits.AddNew();
		}
	}
}
