using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(PreApproval))]
	sealed class PreApprovalTest : CusSupportingInfoTest<PreApproval>
	{
		protected override BusinessObject GetNewBusinessObject() => preApproval;

		protected override IEnumerable<PreApproval> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var invoiceLine = factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var preApprovalCollection = new PreApprovalCollection(invoiceLine);
			yield return preApprovalCollection.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var businessObj = (PreApproval)base.GetBusinessObjectForFetchForLoad();
			businessObj.CSI_Type = CusSupportingInfoTypeList.Codes.PreApproval;
			return businessObj;
		}

		public void TestValidation()
		{
			var supportingInfo = Factory.New<PreApproval>();
			AssertEquals(typeof(PreApprovalValidation), supportingInfo.Validation.GetType());
		}

		public void TestParent()
		{
			AssertEquals(invoiceLine, preApproval.Parent);
		}

		protected override void SetUp()
		{
			invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var preApprovalCollection = new PreApprovalCollection(invoiceLine);
			preApproval = preApprovalCollection.AddNew();
		}
		PreApproval preApproval;
		JobComInvoiceLine invoiceLine;
	}
}
