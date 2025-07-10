using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(PreviousExpDecLine))]
	sealed class PreviousExpDecLineTest : CusSupportingInfoTest<PreviousExpDecLine>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().PreviousExpDecLineCollection.AddNew();
		}

		protected override IEnumerable<PreviousExpDecLine> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().PreviousExpDecLineCollection.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var businessObj = (PreviousExpDecLine)base.GetBusinessObjectForFetchForLoad();
			businessObj.CSI_Type = CusSupportingInfoTypeList.Codes.PreviousExpDecLine;
			return businessObj;
		}

		public void TestValidation()
		{
			var supportingInfo = Factory.New<PreviousExpDecLine>();
			AssertEquals(typeof(PreviousExpDecLineValidation), supportingInfo.Validation.GetType());
		}

		public void TestParent()
		{
			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			var supportingInfo = invoiceLine.PreviousExpDecLineCollection.AddNew();
			AssertEquals(invoiceLine, supportingInfo.Parent);
		}

		public void TestParentIsValidationEnabled()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var entryLine = declaration.ActiveEntryHeaders.AddNew().MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var previousExpDecLine1 = invoiceLine.PreviousExpDecLineCollection.AddNew();
			var previousExpDecLine2 = entryLine.PreviousExpDecLineCollection.AddNew();

			Assert(previousExpDecLine1.Parent.IsValidationEnabled);
			Assert(!previousExpDecLine2.Parent.IsValidationEnabled);
		}
	}
}
