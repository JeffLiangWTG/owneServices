using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(PreviousDocument))]
	public class PreviousDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<PreviousDocument>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().PreviousDocuments.AddNew();
		}

		public void TestSetDefaultValues()
		{
			var supporting = Factory.New<PreviousDocument>();
			AssertEquals(CusSupportingInfoTypeList.Codes.PreviousDocument, supporting.CSI_Type);
			AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, supporting.CSI_ParentTableCode);
		}

		public void TestItemNumberMaxLength()
		{
			var supporting = Factory.New<PreviousDocument>();
			AssertEquals(5, supporting.CSI_ItemNumberInfo.MaxLength);
		}

		protected override IEnumerable<PreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var cusSupporting = factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().PreviousDocuments.AddNew();
			cusSupporting.CSI_ReferenceNumber = "1";
			cusSupporting.CSI_LineNo = 1;
			yield return cusSupporting;
		}
	}
}
