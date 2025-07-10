using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(NonGADetail))]
	sealed class NonGADetailTest : CusSupportingInfoTest<NonGADetail>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().NonGADetailCollection.AddNew();
		}

		protected override IEnumerable<NonGADetail> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().NonGADetailCollection.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var businessObj = (NonGADetail)base.GetBusinessObjectForFetchForLoad();
			businessObj.CSI_Type = CusSupportingInfoTypeList.Codes.NonGADetail;
			return businessObj;
		}

		public void TestValidation()
		{
			var supportingInfo = Factory.New<NonGADetail>();
			AssertEquals(typeof(NonGADetailValidation), supportingInfo.Validation.GetType());
		}

		public void TestParent()
		{
			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var supportingInfo = invoiceLine.NonGADetailCollection.AddNew();
			AssertEquals(invoiceLine, supportingInfo.Parent);
		}

		public void TestParentIsValidationEnabled()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var entryLine = declaration.ActiveEntryHeaders.AddNew().MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var nonGADetail1 = invoiceLine.NonGADetailCollection.AddNew();
			var nonGADetail2 = entryLine.NonGADetailCollection.AddNew();

			Assert(nonGADetail1.Parent.IsValidationEnabled);
			Assert(!nonGADetail2.Parent.IsValidationEnabled);
		}
	}
}
